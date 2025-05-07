using System.Data;
using System.Diagnostics;
using MySqlConnector;
using Village_Newbies.Models;
using Village_Newbies.Interfacet;
using Village_Newbies.Services;
using DatabaseConnection;


namespace Village_Newbies.Services
{
    public class VarausDatabaseService : DatabaseService
    {
        private readonly LaskuDatabaseService _laskuService;
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
            try
            {
                if (await OnkoVarausPaallekkain((int)varaus.mokki_id, varaus.varattu_alkupvm, varaus.varattu_loppupvm))
                {
                    await Application.Current.MainPage.DisplayAlert("Virhe lisättäessä varausta", "Mökki ei ollut vapaana", "OK");
                }
                else
                {
                    var sql = @"INSERT INTO varaus 
                        (asiakas_id, mokki_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm)
                        VALUES 
                        (@asiakas_id, @mokki_id, @varattu_pvm, @vahvistus_pvm, @varattu_alkupvm, @varattu_loppupvm)";
                    await SuoritaKomento(sql,
                        ("@asiakas_id", varaus.asiakas_id),
                        ("@mokki_id", varaus.mokki_id),
                        ("@varattu_pvm", varaus.varattu_pvm),
                        ("@vahvistus_pvm", varaus.vahvistus_pvm ?? (object)DBNull.Value),
                        ("@varattu_alkupvm", varaus.varattu_alkupvm),
                        ("@varattu_loppupvm", varaus.varattu_loppupvm));
                    await Application.Current.MainPage.DisplayAlert("Uusi varaus lisätty", $"Ajalle: {varaus.varattu_alkupvm} - {varaus.varattu_loppupvm} \nAsiakas: {varaus.asiakas_id}, Mokki {varaus.mokki_id}", "OK");
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Virhe", $"Varauksen lisäys epäonnistui \n{ex.Message}", "OK");
            }


        }
        public async Task Poista(int varausId)
        {
            try
            {
                string sql = "DELETE FROM varaus WHERE varaus_id = @varaus_id";
                int rowsAffected = await SuoritaKomento(sql, ("@varaus_id", varausId));

                if (rowsAffected == 0)
                {
                    throw new Exception($"Varausta ID:llä {varausId} ei löytynyt.");
                }
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                throw new Exception("Varaukseen liittyy muita tietoja (esim. laskuja tai palveluita), eikä sitä voida poistaa.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Virhe poistettaessa varausta {varausId}: {ex.Message}", ex);
            }
        }

        public async Task Muokkaa(Varaus varaus)
        {
            try
            {
                if (await OnkoVarausPaallekkain((int)varaus.mokki_id, varaus.varattu_alkupvm, varaus.varattu_loppupvm, varaus.varaus_id))
                {
                    await Application.Current.MainPage.DisplayAlert("Virhe varausta muokattaessa", "Mökki ei ollut vapaana", "OK");
                }
                else
                {
                    var sql = @"UPDATE varaus
                SET asiakas_id = @asiakas_id,
                    mokki_id = @mokki_id,
                    varattu_pvm = @varattu_pvm,
                    vahvistus_pvm = @vahvistus_pvm,
                    varattu_alkupvm = @varattu_alkupvm,
                    varattu_loppupvm = @varattu_loppupvm
                WHERE varaus_id = @varaus_id";

                    await SuoritaKomento(sql,
                        ("@asiakas_id", varaus.asiakas_id),
                        ("@mokki_id", varaus.mokki_id),
                        ("@varattu_pvm", varaus.varattu_pvm),
                        ("@vahvistus_pvm", varaus.vahvistus_pvm ?? (object)DBNull.Value),
                        ("@varattu_alkupvm", varaus.varattu_alkupvm),
                        ("@varattu_loppupvm", varaus.varattu_loppupvm),
                        ("@varaus_id", varaus.varaus_id));
                    await Application.Current.MainPage.DisplayAlert("Varausta muokattu", $"Ajalle: {varaus.varattu_alkupvm} - {varaus.varattu_loppupvm} \nAsiakas: {varaus.asiakas_id}, Mokki {varaus.mokki_id}", "OK");
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Virhe", $"Varauksen muokkaus epäonnistui \n{ex.Message}", "OK");
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
                sql += " AND varaus_id != @currentVarausId";
            }
            var parameters = new List<(string, object)>
            {
                ("@mokkiId", mokkiId),
                ("@alkuPvm", alkuPvm),
                ("@loppuPvm", loppuPvm)
            };

            if (VarausId.HasValue)
            {
                sql += " AND varaus_id != @currentVarausId";
                parameters.Add(("@currentVarausId", VarausId.Value));
            }

            // Suorita kysely
            var data = await HaeData(sql, parameters.ToArray());

            // Palauta true, jos löytyy päällekkäinen varaus
            return data.Rows.Count > 0 && Convert.ToInt64(data.Rows[0][0]) > 0;
        }

        private List<Varaus> LuoLista(DataTable taulu)
            => taulu.AsEnumerable().Select(r => LuoOlio(r)).ToList();
    }
}