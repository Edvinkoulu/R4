using System.Data;
using System.Diagnostics;
using MySqlConnector;
using Village_Newbies.Models;
using DatabaseConnection;
using Dapper;

namespace Village_Newbies.Services
{
    public class VarausDatabaseService : DatabaseService
    {
        private readonly LaskuDatabaseService _laskuService = new LaskuDatabaseService();
        private readonly MokkiDatabaseService _mokkiService = new MokkiDatabaseService();
        private readonly Varauksen_palvelutDatabaseService _vpService = new Varauksen_palvelutDatabaseService();
        public VarausDatabaseService() { }
        public VarausDatabaseService(DatabaseConnector connector) : base(connector) { }

        // ==================== CRUD =======================

        public async Task<List<Varaus>> HaeKaikki()
        {
            var data = await HaeData("SELECT * FROM varaus");
            return LuoLista(data);
        }

        public async Task<Varaus?> Hae(int id)
        {
            var data = await HaeData("SELECT * FROM varaus WHERE varaus_id = @id", ("@id", id));
            return data.Rows.Count > 0 ? LuoOlio(data.Rows[0]) : null;
        }
        public async Task Lisaa(Varaus varaus)
        {
            await LisaaVaraus(varaus);
        }

        public async Task<uint> Lisaa2(Varaus varaus)
        {
            return await LisaaVaraus(varaus);
        }
        private async Task<uint> LisaaVaraus(Varaus varaus)
        {
            if (await OnkoVarausPaallekkain((int)varaus.mokki_id, varaus.varattu_alkupvm, varaus.varattu_loppupvm))
            {
                await NaytaIlmoitus("Virhe lisättäessä varausta", "Mökki ei ollut vapaana valitulle ajankohdalle.");
                return 0;
            }

            try
            {
                var sqlInsertAndGetId = @"INSERT INTO varaus
                                (asiakas_id, mokki_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm)
                                VALUES
                                (@asiakas_id, @mokki_id, @varattu_pvm, @vahvistus_pvm, @varattu_alkupvm, @varattu_loppupvm);
                                SELECT LAST_INSERT_ID();";

                var idData = await HaeData(sqlInsertAndGetId,
                    ("@asiakas_id", varaus.asiakas_id),
                    ("@mokki_id", varaus.mokki_id),
                    ("@varattu_pvm", varaus.varattu_pvm),
                    ("@vahvistus_pvm", varaus.vahvistus_pvm ?? (object)DBNull.Value),
                    ("@varattu_alkupvm", varaus.varattu_alkupvm),
                    ("@varattu_loppupvm", varaus.varattu_loppupvm));

                if (idData.Rows.Count > 0 && idData.Rows[0][0] != DBNull.Value)
                {
                    uint uusiVarausId = Convert.ToUInt32(idData.Rows[0][0]);
                    varaus.varaus_id = uusiVarausId;

                    if (varaus.varaus_id > 0)
                    {
                        await KasitteleLaskuVaraukselle(varaus, true);
                        return uusiVarausId;
                    }
                    else
                    {
                        Debug.WriteLine("VarausDatabaseService: Varauksen lisäys onnistui, mutta uusi varaus ID oli virheellinen (0). Laskua ei voitu luoda.");
                        await NaytaIlmoitus("Virhe laskun luonnissa", $"Varaus (Asiakas: {varaus.asiakas_id}, Mökki: {varaus.mokki_id}) luotiin, mutta varaus ID oli virheellinen. Laskua ei voitu luoda automaattisesti.");
                        return 0;
                    }
                }
                else
                {
                    Debug.WriteLine("VarausDatabaseService: Varauksen lisäys onnistui, mutta uuden varaus ID:n haku epäonnistui.");
                    await NaytaIlmoitus("Varaus lisätty osittain", $"Varaus (Asiakas: {varaus.asiakas_id}, Mökki: {varaus.mokki_id}) luotiin ajalle {varaus.varattu_alkupvm:d} - {varaus.varattu_loppupvm:d}, mutta sen ID:n haku epäonnistui. Laskua ei voitu luoda automaattisesti.");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VarausDatabaseService: Varauksen lisäys epäonnistui (poikkeus): \n{ex.Message}");
                await NaytaIlmoitus("Virhe", $"Varauksen tallennus epäonnistui: {ex.Message}");
                return 0;
            }
        }

        public async Task Muokkaa(Varaus varaus)
        {
            // 1. Päällekkäisyyden tarkistus
            if (await OnkoVarausPaallekkain((int)varaus.mokki_id, varaus.varattu_alkupvm, varaus.varattu_loppupvm, varaus.varaus_id))
            {
                await NaytaIlmoitus("Virhe varausta muokattaessa", "Mökki ei ollut vapaana valitulle ajankohdalle.");
                return;
            }

            int muutetutRivit = 0;
            try
            {
                // 2. Varauksen päivitys tietokantaan
                var sql = @"UPDATE varaus
                            SET asiakas_id = @asiakas_id,
                                mokki_id = @mokki_id,
                                varattu_pvm = @varattu_pvm,
                                vahvistus_pvm = @vahvistus_pvm,
                                varattu_alkupvm = @varattu_alkupvm,
                                varattu_loppupvm = @varattu_loppupvm
                            WHERE varaus_id = @varaus_id";
                muutetutRivit = await SuoritaKomento(sql,
                    ("@asiakas_id", varaus.asiakas_id),
                    ("@mokki_id", varaus.mokki_id),
                    ("@varattu_pvm", varaus.varattu_pvm),
                    ("@vahvistus_pvm", varaus.vahvistus_pvm ?? (object)DBNull.Value),
                    ("@varattu_alkupvm", varaus.varattu_alkupvm),
                    ("@varattu_loppupvm", varaus.varattu_loppupvm),
                    ("@varaus_id", varaus.varaus_id));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VarausDatabaseService: Varauksen muokkaus epäonnistui (SuoritaKomento poikkeus): \n{ex.Message}");
                await NaytaIlmoitus("Virhe", $"Varauksen muokkaus epäonnistui: {ex.Message}");
                return; // Kriittinen virhe, keskeytä suoritus
            }

            if (muutetutRivit > 0)
            {
                // 3. Laskun käsittely (päivitys)
                await KasitteleLaskuVaraukselle(varaus, false);
            }
            else
            {
                await NaytaIlmoitus("Huomautus", $"Varausta (ID: {varaus.varaus_id}) ei löytynyt tai tietoja ei muutettu tietokannassa.");
            }
        }
        public async Task<bool> Poista(int varausId)
        {
            if (varausId <= 0)
            {
                await NaytaIlmoitus("Virhe poistettaessa", "Virheellinen varaus ID.");
                return false;
            }

            try
            {
                await _vpService.DeleteByVarausIdAsync((uint)varausId);
                Debug.WriteLine($"VarausDatabaseService: Poistettu varauksen (ID: {varausId}) palvelut.");

                // 2. Poista varaukseen liittyvät laskut
                 try
                 {
                     var lasku = await _laskuService.HaeVarauksenLasku((uint)varausId);
                     if (lasku != null)
                     {
                         bool laskuPoistettu = await _laskuService.Poista(lasku.lasku_id);

                         // Tarkista, onnistuiko laskun poisto (käyttäjä vahvisti)
                         if (!laskuPoistettu) // Jos laskua EI poistettu (käyttäjä peruutti tai tapahtui virhe)
                         {
                             // Näytä ilmoitus peruutuksesta ja keskeytä varauksen poisto
                             await NaytaIlmoitus("Poisto keskeytetty", $"Laskua ID:llä {lasku.lasku_id} ei poistettu. Varauksen poisto keskeytetään.");
                             return false; // Keskeytä varauksen poisto
                         }
                         Debug.WriteLine($"VarausDatabaseService: Poistettu varauksen (ID: {varausId}) lasku.");
                     }
                     else
                     {
                          Debug.WriteLine($"VarausDatabaseService: Varauksella (ID: {varausId}) ei ollut laskua poistettavaksi.");
                          // Jatka varauksen poistoon, koska laskua ei ollut
                     }
                 }
                 catch (Exception laskuEx)
                 {
                     Debug.WriteLine($"VarausDatabaseService: Virhe poistettaessa varauksen (ID: {varausId}) laskua: {laskuEx.Message}");
                     await NaytaIlmoitus("Virhe laskua poistaessa", $"Varauksen laskun poisto epäonnistui. Varauksen poisto keskeytetään. Virhe: \n{laskuEx.Message}.");
                     return false;
                 }
                // 3. Poista itse varaus
                string sql = "DELETE FROM varaus WHERE varaus_id = @varaus_id";
                int rowsAffected = await SuoritaKomento(sql, ("@varaus_id", varausId));

                if (rowsAffected == 0)
                {
                    // Heitä poikkeus tai näytä ilmoitus, jos varausta ei löytynyt poistettavaksi
                    await NaytaIlmoitus("Huomautus", $"Varausta ID:llä {varausId} ei löytynyt poistettavaksi (mahdollisesti jo poistettu?).");
                    return false; // Ei löytynyt poistettavaa -> ei onnistunut
                }
                else
                {
                    await NaytaIlmoitus("Onnistui", $"Varaus (ID: {varausId}) poistettu onnistuneesti.");
                    return true; // Varaus poistettu onnistuneesti
                }
            }
            catch (MySqlException ex) when (ex.Number == 1451) // Foreign key constraint (should ideally not happen after deleting services/invoices)
            {
                Debug.WriteLine($"VarausDatabaseService: Varauksen (ID: {varausId}) poisto epäonnistui viite-eheysrajoituksen vuoksi (odotusarvoisesti ei pitäisi tapahtua): {ex.Message}");
                await NaytaIlmoitus("Virhe poistettaessa", "Varaukseen liittyy yhä muita tietoja (esim. laskuja tai palveluita), eikä sitä voida poistaa. Tarkista tietokannan eheys.");
                return false; // Poisto epäonnistui FK-rajoitteen vuoksi
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VarausDatabaseService: Virhe poistettaessa varausta {varausId}: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Virhe poistettaessa varausta {varausId}: {ex.Message}");
                return false; // Yleinen virhe poistossa
            }
        }

        // ==================== APUMETODIT =======================

        private Varaus LuoOlio(DataRow r)
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
        // Tämä metodi estää varausten tekemisen päällekkäin
        // Isompi pulma oli sallia varauksen ajankohdan muokkaaminen, jos se oli päällekkäin edellisen ajankohdan kanssa
        // Ratkaistu syöttämällä varauksen ID metodille joka sitten jättää "oman varauksen" huomiotta.
        public async Task<bool> OnkoVarausPaallekkain(int mokkiId, DateTime alkuPvm, DateTime loppuPvm, uint? VarausId = null)
        {
            // Poista aikakomponentti, jos se on mukana
            alkuPvm = alkuPvm.Date;
            loppuPvm = loppuPvm.Date;

            // Tarkista että päivämäärät ovat loogisia
            if (loppuPvm <= alkuPvm)
            {
                throw new ArgumentException("Loppupäivämäärän on oltava alkupäivämäärää myöhempi.");
            }

            // SQL-kysely päällekkäisten varausten tarkistamiseen
            var sql = @"
                        SELECT COUNT(*) 
                        FROM varaus 
                        WHERE mokki_id = @mokkiId 
                        AND NOT (varattu_loppupvm <= @alkuPvm OR varattu_alkupvm >= @loppuPvm)";

            // Poissulje nykyinen varaus päivitystilanteessa
            if (VarausId.HasValue)
            {
                sql += " AND varaus_id != @VarausId";
            }
            var parameters = new List<(string, object)>
            {
                ("@mokkiId", mokkiId),
                ("@alkuPvm", alkuPvm),
                ("@loppuPvm", loppuPvm)
            };

            if (VarausId.HasValue)
            {
                sql += " AND varaus_id != @VarausId";
                parameters.Add(("@VarausId", VarausId.Value));
            }

            // Suorita kysely
            var data = await HaeData(sql, parameters.ToArray());

            // Palauta true, jos löytyy päällekkäinen varaus
            return data.Rows.Count > 0 && Convert.ToInt64(data.Rows[0][0]) > 0;
        }
        // Laskun käsittely metodi. Tehty erilliseksi jotta olisi vähemmän toistoa ja koodi olisi selkeämpi.
        private async Task KasitteleLaskuVaraukselle(Varaus varaus, bool onUusiVaraus)
        {
            try
            {
                var mokki = await _mokkiService.GetMokkiByIdAsync((int)varaus.mokki_id);
                if (mokki == null)
                {
                    await NaytaIlmoitus("Virhe laskunkäsittelyssä", $"Mökin (ID: {varaus.mokki_id}) tietoja ei löytynyt. Laskua ei {(onUusiVaraus ? "luotu" : "päivitetty")}.");
                    return;
                }

                // Lasketaan varattujen vuorokausien määrä
                TimeSpan varauksenKesto = varaus.varattu_loppupvm.Date - varaus.varattu_alkupvm.Date;
                int vuorokaudet = (int)varauksenKesto.TotalDays;

                // Lasketaan kokonaissumma
                double summa = mokki.Hinta * vuorokaudet;

                if (onUusiVaraus)
                {
                    Lasku uusiLasku = new Lasku(varaus.varaus_id, summa, maksettu: false);
                    await _laskuService.Lisaa(uusiLasku);
                    await NaytaIlmoitus("Toiminto onnistui", $"Varaus (ID: {varaus.varaus_id}) ja lasku luotu onnistuneesti.\nAjalle: {varaus.varattu_alkupvm:d} - {varaus.varattu_loppupvm:d}\nAsiakas: {varaus.asiakas_id}, Mökki: {varaus.mokki_id}\nLaskun summa (ilman ALV): {uusiLasku.summa:C}");
                }
                else // Päivitetään olemassa olevaa laskua
                {
                    Lasku? lasku = await _laskuService.HaeVarauksenLasku(varaus.varaus_id);
                    if (lasku != null)
                    {
                        lasku.summa = summa;
                        await _laskuService.Muokkaa(lasku);
                        await NaytaIlmoitus("Toiminto onnistui", $"Varaus (ID: {varaus.varaus_id}) ja lasku päivitetty onnistuneesti.\nAjalle: {varaus.varattu_alkupvm:d} - {varaus.varattu_loppupvm:d}\nLaskun summa (ilman ALV): {lasku.summa:C}");
                    }
                    else
                    {
                        // Varaus päivitettiin, mutta laskua ei löytynyt.
                        await NaytaIlmoitus("Huomautus laskunkäsittelyssä", $"Varaus (ID: {varaus.varaus_id}) päivitettiin, mutta siihen liittyvää laskua ei löytynyt päivitettäväksi. Harkitse laskun luomista manuaalisesti.");
                    }
                }
            }
            catch (MySqlException dbEx)
            {
                Debug.WriteLine($"VarausDatabaseService: Tietokantavirhe laskua käsiteltäessä varaukselle {varaus.varaus_id}: {dbEx.Message}");
                await NaytaIlmoitus("Tietokantavirhe laskua käsiteltäessä", $"Laskun {(onUusiVaraus ? "luonti" : "päivitys")} epäonnistui varaukselle ID {varaus.varaus_id}: {dbEx.Message}.");
            }
            catch (Exception exLasku)
            {
                Debug.WriteLine($"VarausDatabaseService: Yleinen virhe laskua käsiteltäessä varaukselle {varaus.varaus_id}: {exLasku.Message}");
                await NaytaIlmoitus("Virhe laskua käsiteltäessä", $"Laskun {(onUusiVaraus ? "luonti" : "päivitys")} epäonnistui varaukselle ID {varaus.varaus_id}: {exLasku.Message}.");
            }
        }
        // Näytä ilmoitus otettu erikseen (DRY)
        private async Task NaytaIlmoitus(string otsikko, string viesti, string kuittaus = "OK")
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Dispatcher.DispatchAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(otsikko, viesti, kuittaus);
                });
            }
            else
            {
                Debug.WriteLine($"NaytaIlmoitus (ei UI-kontekstia): {otsikko} - {viesti}");
            }
        }
        private List<Varaus> LuoLista(DataTable taulu)
            => taulu.AsEnumerable().Select(r => LuoOlio(r)).ToList();
    }
}