namespace Village_Newbies.Services;

using Village_Newbies.Interfacet;
using Village_Newbies.Models;
using DatabaseConnection;
using MySqlConnector;
using System.Data;
using System.Diagnostics;

public class LaskuDatabaseService : DatabaseService, ILaskuDatabaseService
{
    public LaskuDatabaseService() { }
    public LaskuDatabaseService(DatabaseConnector connection) : base(connection) { }

    // ===================== HAKU =======================

    public async Task<Lasku> Hae(int id)
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

    // ==================== CRUD =======================

    public async Task Lisaa(Lasku lasku)
    {
        var sql = "INSERT INTO lasku (varaus_id, summa, alv, maksettu) VALUES (@varausId, @summa, @alv, @maksettu)";
        await SuoritaKomento(sql,
            ("@varausId", lasku.varaus_id),
            ("@summa", lasku.summa),
            ("@alv", lasku.alv),
            ("@maksettu", lasku.maksettu));
    }

    public async Task Muokkaa(Lasku lasku)
    {
        var sql = "UPDATE lasku SET varaus_id = @varausId, summa = @summa, alv = @alv, maksettu = @maksettu WHERE lasku_id = @laskuId";
        await SuoritaKomento(sql,
            ("@laskuId", lasku.lasku_id),
            ("@varausId", lasku.varaus_id),
            ("@summa", lasku.summa),
            ("@alv", lasku.alv),
            ("@maksettu", lasku.maksettu));
    }

    public async Task Poista(int id)
    {
        await SuoritaKomento("DELETE FROM lasku WHERE lasku_id = @laskuId", ("@laskuId", id));
    }

    // ==================== MUUT HAKUTOIMINNOT =======================

    public async Task<List<Varaus>> HaeKaikkiVaraukset()
    {
        var varaukset = new List<Varaus>();
        try
        {
            using var conn = HaeYhteysTietokantaan();
            using var cmd = new MySqlCommand("SELECT * FROM varaus", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                varaukset.Add(new Varaus
                {
                    varaus_id = (uint)reader.GetInt32(0),
                    asiakas_id = (uint)reader.GetInt32(1),
                    mokki_id = (uint)reader.GetInt32(2),
                    varattu_pvm = reader.GetDateTime(3),
                    vahvistus_pvm = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                    varattu_alkupvm = reader.GetDateTime(5),
                    varattu_loppupvm = reader.GetDateTime(6)
                });
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Virhe varauksia haettaessa", ex);
        }
        return varaukset;
    }

    public async Task<List<Asiakas>> HaeKaikkiAsiakkaat()
    {
        var data = await HaeData("SELECT * FROM asiakas");
        return data.AsEnumerable()
            .Select(row => LuoAsiakasOlio(row))
            .ToList();
    }

    // ==================== APUTOIMINNOT =======================

    private List<Lasku> LuoLaskuLista(DataTable data)
        => data.AsEnumerable().Select(row => LuoLaskuOlio(row)).ToList();

    private Lasku LuoLaskuOlio(DataRow row)
    {
        var laskuId = Convert.ToInt32(row["lasku_id"]);
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

    // ==================== OVERRIDET =======================

    public override async Task<DataTable> HaeData(string sql, params (string, object)[] parameters)
    {
        using var conn = HaeYhteysTietokantaan();
        using var cmd = new MySqlCommand(sql, conn);
        foreach (var (nimi, arvo) in parameters) cmd.Parameters.AddWithValue(nimi, arvo);

        var table = new DataTable();
        using var adapter = new MySqlDataAdapter(cmd);
        await Task.Run(() => adapter.Fill(table));
        return table;
    }

    public override async Task<int> SuoritaKomento(string sql, params (string, object)[] parameters)
    {
        using var conn = HaeYhteysTietokantaan();
        using var cmd = new MySqlCommand(sql, conn);
        foreach (var (nimi, arvo) in parameters) cmd.Parameters.AddWithValue(nimi, arvo);

        return await cmd.ExecuteNonQueryAsync();
    }
}
