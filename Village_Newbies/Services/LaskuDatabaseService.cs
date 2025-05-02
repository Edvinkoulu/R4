namespace Village_Newbies.Services;

using Village_Newbies.Interfacet;
using Village_Newbies.Models;
using DatabaseConnection;
using MySqlConnector;
using System.Data;

public class LaskuDatabaseService : DatabaseService, ILaskuDatabaseService
{
    VarausDatabaseService varausService = new VarausDatabaseService();
    public LaskuDatabaseService() { }
    public LaskuDatabaseService(DatabaseConnector connection) : base(connection) { }
    // ===================== HAKU =======================
    public async Task<Lasku> Hae(uint id)
    {
        var sql = "SELECT lasku_id, varaus_id, summa, alv, maksettu FROM lasku WHERE lasku_id = @laskuId";
        var data = await HaeData(sql, ("@laskuId", id));
        return data.Rows.Count > 0 ? LuoLaskuOlio(data.Rows[0]) : null;
    }

    public async Task<List<Lasku>> HaeKaikki()
    {
        var data = await HaeData("SELECT lasku_id, varaus_id, summa, alv, maksettu FROM lasku");
        return LuoLaskuLista(data);
    }
    public async Task<List<Lasku>> HaeSuodatetutLaskut(
        int? alueId = null, int? mokkiId = null, int? asiakasId = null,
        DateTime? varausAlku = null, DateTime? varausLoppu = null,
        bool? maksettu = null, string asiakasNimi = null)
    {
        var sql = @"
            SELECT l.lasku_id, l.varaus_id, l.summa, l.alv, l.maksettu
            FROM lasku l
            JOIN varaus v ON l.varaus_id = v.varaus_id
            JOIN mokki m ON v.mokki_id = m.mokki_id
            JOIN asiakas a ON v.asiakas_id = a.asiakas_id
            WHERE 1 = 1";
        var parametrit = new List<(string, object)>();

        // Supdattimet
        if (alueId.HasValue) { sql += " AND m.alue_id = @alueId"; parametrit.Add(("@alueId", alueId.Value)); }
        if (mokkiId.HasValue) { sql += " AND m.mokki_id = @mokkiId"; parametrit.Add(("@mokkiId", mokkiId.Value)); }
        if (asiakasId.HasValue) { sql += " AND a.asiakas_id = @asiakasId"; parametrit.Add(("@asiakasId", asiakasId.Value)); }
        if (varausAlku.HasValue) { sql += " AND v.varattu_alkupvm >= @varausAlku"; parametrit.Add(("@varausAlku", varausAlku.Value)); }
        if (varausLoppu.HasValue) { sql += " AND v.varattu_loppupvm <= @varausLoppu"; parametrit.Add(("@varausLoppu", varausLoppu.Value)); }
        if (maksettu.HasValue) { sql += " AND l.maksettu = @maksettu"; parametrit.Add(("@maksettu", maksettu.Value ? 1 : 0)); }
        if (!string.IsNullOrWhiteSpace(asiakasNimi))
        {
            sql += " AND (a.etunimi LIKE @nimi OR a.sukunimi LIKE @nimi)";
            parametrit.Add(("@nimi", $"%{asiakasNimi}%"));
        }

        return LuoLaskuLista(await HaeData(sql, parametrit.ToArray()));
    }
    public async Task<List<Palvelu>> HaeLaskunPalvelut(Lasku lasku)
    {
        uint laskuId = lasku.lasku_id;
        var sql = @"
                SELECT p.palvelu_id AS PalveluId,
                       p.alue_id AS AlueId,
                       p.nimi AS Nimi,
                       p.kuvaus AS Kuvaus,
                       p.hinta AS Hinta,
                       p.alv AS Alv
                FROM lasku l
                JOIN varaus v ON l.varaus_id = v.varaus_id
                JOIN varauksen_palvelut vp ON v.varaus_id = vp.varaus_id
                JOIN palvelu p ON vp.palvelu_id = p.palvelu_id
                WHERE l.lasku_id = @LaskuId;";

        return (await HaeData(sql, ("@LaskuId", laskuId)))
            .AsEnumerable()
            .Select(row => new Palvelu(
                Convert.ToInt32(row["PalveluId"]),
                Convert.ToInt32(row["AlueId"]),
                row["Nimi"]?.ToString(),
                row["Kuvaus"]?.ToString(),
                Convert.ToDouble(row["Hinta"]),
                Convert.ToDouble(row["Alv"])
            )).ToList();
    }
    // =============== READ UPDATE DELETE ===============
    public async Task Lisaa(Lasku lasku)
    {
        try { await TarkistaLasku(lasku); }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Virhe laskua lisättäessä", ex.Message, "OK");
            return; // Ei viedä huonoa dataa tietokantaan.
        }
        var sql = "INSERT INTO lasku (varaus_id, summa, alv, maksettu) VALUES (@varausId, @summa, @alv, @maksettu)";
        await SuoritaKomento(sql,
            ("@varausId", lasku.varaus_id),
            ("@summa", lasku.summa),
            ("@alv", lasku.alv),
            ("@maksettu", lasku.maksettu));
    }
    public async Task Muokkaa(Lasku lasku)
    {
        if (lasku != null)
        {
            Lasku? nykyinenLasku = await Hae(lasku.lasku_id);
            if (nykyinenLasku != null)
            {
                bool vahvistus = await VahvistaToiminto(
                    "Muokkaa laskua",
                    $"Haluatko varmasti muokata laskua ID:llä {nykyinenLasku.lasku_id} (varaus ID: {nykyinenLasku.varaus_id})?"
                );
                if (vahvistus)
                {
                    try { await TarkistaLasku(lasku); }
                    catch (Exception ex)
                    {
                        await Application.Current.MainPage.DisplayAlert("Virhe laskua muokattaessa", ex.Message, "OK");
                        return; // Ei viedä huonoa dataa tietokantaan.
                    }
                    var sql = "UPDATE lasku SET varaus_id = @varausId, summa = @summa, alv = @alv, maksettu = @maksettu WHERE lasku_id = @laskuId";
                    await SuoritaKomento(sql,
                        ("@laskuId", lasku.lasku_id),
                        ("@varausId", lasku.varaus_id),
                        ("@summa", lasku.summa),
                        ("@alv", lasku.alv),
                        ("@maksettu", lasku.maksettu));
                }
            }
        }
    }
    public async Task Poista(uint id)
    {
        Lasku? poistettavaLasku = await Hae(id);
        if (poistettavaLasku != null)
        {
            bool vahvistus = await VahvistaToiminto(
                "Poista lasku",
                $"Haluatko varmasti poistaa laskun ID:llä {poistettavaLasku.lasku_id} (varaus ID: {poistettavaLasku.varaus_id})?"
            );
            if (vahvistus)
            {
                await SuoritaKomento("DELETE FROM lasku WHERE lasku_id = @laskuId", ("@laskuId", id));
            }
        }
    }
    public async Task TarkistaLasku(Lasku lasku)
    {
        SyoteValidointi.TarkistaDouble(lasku.alv, 0, 100);
        SyoteValidointi.TarkistaDouble(lasku.summa, 0, double.MaxValue);
    }
    // ==================== MUUT HAKUTOIMINNOT =======================
    public async Task<Varaus> HaeVaraus(uint id)
    {
        var sql = "SELECT * FROM varaus WHERE varaus_id = @varausId";
        var data = await HaeData(sql, ("@varausId", id));
        return data.Rows.Count > 0 ? LuoVarausOlio(data.Rows[0]) : null;
    }
    public async Task<MokkiUint> HaeMokki(uint id)
    {
        var sql = "SELECT * FROM mokki WHERE mokki_id = @mokkiId";
        var data = await HaeData(sql, ("@mokkiId", id));
        return data.Rows.Count > 0 ? LuoMokkiOlio(data.Rows[0]) : null;
    }
    public async Task<Asiakas?> HaeAsiakas(uint id)
    {
        var sql = "SELECT * FROM asiakas WHERE asiakas_id = @asiakasId";
        var data = await HaeData(sql, ("@asiakasId", id));
        return data.Rows.Count > 0 ? LuoAsiakasOlio(data.Rows[0]) : null;
    }
    public async Task<List<Varaus>> HaeKaikkiVaraukset()
    {
        var sql = "SELECT * FROM varaus";
        var data = await HaeData(sql);
        return data.AsEnumerable().Select(LuoVarausOlio).ToList();
    }
    public async Task<List<Asiakas>> HaeKaikkiAsiakkaat()
    {
        var data = await HaeData("SELECT * FROM asiakas");
        return data.AsEnumerable().Select(LuoAsiakasOlio).ToList();
    }
    // ==================== APUTOIMINNOT =======================
    private List<Lasku> LuoLaskuLista(DataTable data)
        => data.AsEnumerable().Select(row => LuoLaskuOlio(row)).ToList();

    private Lasku LuoLaskuOlio(DataRow row)
    {
        var laskuId = Convert.ToUInt32(row["lasku_id"]);
        var varausId = Convert.ToUInt32(row["varaus_id"]);
        var summa = double.TryParse(row["summa"]?.ToString(), out var s) ? s : 0;
        var alv = double.TryParse(row["alv"]?.ToString(), out var a) ? a : 0;

        bool maksettu = row["maksettu"] switch
        {
            byte b => b != 0,
            bool b => b,
            int i => i != 0,
            sbyte sb => sb != 0,
            _ => false
        };

        return new Lasku(laskuId, varausId, summa, alv, maksettu);
    }
    private Asiakas LuoAsiakasOlio(DataRow row)
    {
        uint id = row["asiakas_id"] is int i ? (uint)i : Convert.ToUInt32(row["asiakas_id"]);
        return new Asiakas(
            id,
            row["etunimi"]?.ToString(),
            row["sukunimi"]?.ToString(),
            row["lahiosoite"]?.ToString(),
            row["email"]?.ToString(),
            row["puhelinnro"]?.ToString(),
            row["postinro"]?.ToString()
        );
    }
    private MokkiUint LuoMokkiOlio(DataRow row)
    {
        return new MokkiUint
        {
            mokki_id = (uint)row["mokki_id"],
            alue_id = (uint)row["alue_id"],
            Postinro = row["postinro"]?.ToString(),
            Mokkinimi = row["mokkinimi"]?.ToString(),
            Katuosoite = row["katuosoite"]?.ToString(),
            Hinta = Convert.ToDouble(row["hinta"]),
            Kuvaus = row["kuvaus"]?.ToString(),
            Henkilomaara = Convert.ToInt32(row["henkilomaara"]),
            Varustelu = row["varustelu"]?.ToString()
        };
    } /* Tätä metodia pitäisi käyttää, mutta mokki_id ja alue_id int tyyppisenä aiheutti ongelmia. Aika vähissä, joten nopea ratkaisu meni hyvän edelle.
    private Mokki LuoMokkiOlio(DataRow row)
    {
        return new Mokki
        {
            mokki_id = (int)row["mokki_id"],
            alue_id = (int)row["alue_id"],
            Postinro = row["postinro"]?.ToString(),
            Mokkinimi = row["mokkinimi"]?.ToString(),
            Katuosoite = row["katuosoite"]?.ToString(),
            Hinta = Convert.ToDouble(row["hinta"]),
            Kuvaus = row["kuvaus"]?.ToString(),
            Henkilomaara = Convert.ToInt32(row["henkilomaara"]),
            Varustelu = row["varustelu"]?.ToString()
        };
    } */
    private Varaus LuoVarausOlio(DataRow r)
    {
        return new Varaus
        {
            varaus_id = Convert.ToUInt32(r["varaus_id"]),
            asiakas_id = Convert.ToUInt32(r["asiakas_id"]),
            mokki_id = Convert.ToUInt32(r["mokki_id"]),
            varattu_pvm = Convert.ToDateTime(r["varattu_pvm"]),
            vahvistus_pvm = r["vahvistus_pvm"] is DBNull ? null : Convert.ToDateTime(r["vahvistus_pvm"]),
            varattu_alkupvm = Convert.ToDateTime(r["varattu_alkupvm"]),
            varattu_loppupvm = Convert.ToDateTime(r["varattu_loppupvm"])
        };
    }
}
public class MokkiUint
{
    // Kierran int ja uint ongelmaa mökki olioiden luonnissa nopeesti tekemällä uuden luokan joka hyödyntää uint tyyppistä muuttujaa.
    // Välttää koodin uudelleen kirjoittamista, koska aika käy vähiin. 
    public uint mokki_id { get; set; }
    public uint alue_id { get; set; }
    public string Postinro { get; set; }
    public string Mokkinimi { get; set; }
    public string Katuosoite { get; set; }
    public double Hinta { get; set; }
    public string Kuvaus { get; set; }
    public int? Henkilomaara { get; set; }
    public string Varustelu { get; set; }
}