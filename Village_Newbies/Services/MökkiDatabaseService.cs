using DatabaseConnection;  // Make sure to import the DatabaseConnection namespace
using Village_Newbies.Models;
using Dapper;
using MySqlConnector; // Added for MySqlException
using System; // Added for Exception
using System.Collections.Generic; // Added for List
using System.Linq; // Added for ToList()
using System.Threading.Tasks; // Added for Task
using Microsoft.Maui.Controls; // Added for Application.Current.MainPage.DisplayAlert
using System.Diagnostics; // Added for Debug.WriteLine


namespace Village_Newbies.Services
{
    public class MokkiDatabaseService
    {
        private readonly DatabaseConnector _databaseConnector;

        public MokkiDatabaseService()
        {
            _databaseConnector = new DatabaseConnector(); // Use the DatabaseConnector to create the connection
        }
        // Constructor that accepts DatabaseConnector
        public MokkiDatabaseService(DatabaseConnector databaseConnector)
        {
            _databaseConnector = databaseConnector;
        }

        /// <summary>
        /// Lisää uuden mökin tietokantaan.
        /// </summary>
        /// <param name="mokki">Lisättävä Mokki-olio.</param>
        /// <returns>Lisättyjen rivien määrä (pitäisi olla 1 onnistuessa) tai 0 virheen sattuessa.</returns>
        public async Task<int> CreateMokkiAsync(Mokki mokki)
        {
            try
            {
                using (var conn = _databaseConnector._getConnection()) // Get the connection from DatabaseConnector
                {
                    await conn.OpenAsync();  // Open the connection asynchronously

                    string query = @"INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu)
                                     VALUES (@alue_id, @Postinro, @Mokkinimi, @Katuosoite, @Hinta, @Kuvaus, @Henkilomaara, @Varustelu)";

                    return await conn.ExecuteAsync(query, mokki); // Execute the query asynchronously using Dapper
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe mökin luonnissa: {ex.Message}");
                // Käsittele erityyppisiä MySQL-virheitä tarvittaessa
                await NaytaIlmoitus("Tietokantavirhe", $"Mökin luominen epäonnistui: {ex.Message}");
                return 0; // Palauta 0 virheen sattuessa
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Yleinen virhe mökin luonnissa: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Mökin luominen epäonnistui odottamattoman virheen vuoksi: {ex.Message}");
                return 0; // Palauta 0 virheen sattuessa
            }
        }

        /// <summary>
        /// Hakee kaikki alueet tietokannasta.
        /// </summary>
        /// <returns>Lista Alue-olioita.</returns>
        public async Task<List<Alue>> GetAllAlueAsync()
        {
             try
            {
                // Käytä olemassa olevaa _databaseConnector-instanssia
                AlueDatabaseService aluePalvelu = new AlueDatabaseService(_databaseConnector);
                List<Alue> alueet = await aluePalvelu.HaeKaikki();
                return alueet;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe alueiden haussa: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Alueiden lataaminen epäonnistui: {ex.Message}");
                return new List<Alue>(); // Palauta tyhjä lista virheen sattuessa
            }
        }


        /// <summary>
        /// Hakee kaikki mökit tietokannasta.
        /// </summary>
        /// <returns>Kokoelma Mokki-olioita.</returns>
        public async Task<IEnumerable<Mokki>> GetAllMokkisAsync()
        {
            try
            {
                using (var conn = _databaseConnector._getConnection())
                {
                    await conn.OpenAsync();  // Open the connection asynchronously

                    string query = "SELECT * FROM mokki"; // Simple SELECT query
                    return await conn.QueryAsync<Mokki>(query); // Execute the query asynchronously and return the results
                }
            }
             catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe mökkien haussa: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Mökkien lataaminen epäonnistui: {ex.Message}");
                return Enumerable.Empty<Mokki>(); // Palauta tyhjä kokoelma virheen sattuessa
            }
        }

        /// <summary>
        /// Hakee kaikki mökit tietyltä alueelta.
        /// </summary>
        /// <param name="alueId">Alueen ID.</param>
        /// <returns>Lista Mokki-olioita.</returns>
        public async Task<List<Mokki>> GetAllMokkiInAlue(int alueId) // Lisätty laskujenhallintaa varten, JaniV
        {
            try
            {
                using (var conn = _databaseConnector._getConnection())
                {
                    await conn.OpenAsync();

                    string query = "SELECT * FROM mokki WHERE alue_id = @AlueId";
                    var mokit = await conn.QueryAsync<Mokki>(query, new { AlueId = alueId });

                    return mokit.ToList();
                }
            }
             catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe mökkien haussa alueelta {alueId}: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Mökkien lataaminen alueelta epäonnistui: {ex.Message}");
                return new List<Mokki>(); // Palauta tyhjä lista virheen sattuessa
            }
        }

        /// <summary>
        /// Hakee mökin ID:n perusteella.
        /// </summary>
        /// <param name="mokkiId">Mökin ID.</param>
        /// <returns>Mokki-olio tai null, jos ei löydy.</returns>
        public async Task<Mokki?> GetMokkiByIdAsync(int mokkiId)
        {
            try
            {
                using (var conn = _databaseConnector._getConnection())
                {
                    await conn.OpenAsync();  // Open the connection asynchronously

                    string query = "SELECT * FROM mokki WHERE mokki_id = @MokkiId";  // Query to get a Mokki by ID
                    return await conn.QueryFirstOrDefaultAsync<Mokki>(query, new { MokkiId = mokkiId }); // Execute query and return the first match (or null)
                }
            }
             catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe mökin {mokkiId} haussa: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Mökin tietojen lataaminen epäonnistui: {ex.Message}");
                return null; // Palauta null virheen sattuessa
            }
        }

        /// <summary>
        /// Päivittää olemassa olevan mökin tiedot tietokantaan.
        /// </summary>
        /// <param name="mokki">Päivitettävä Mokki-olio.</param>
        /// <returns>Päivitettyjen rivien määrä (pitäisi olla 1 onnistuessa) tai 0 virheen sattuessa.</returns>
        public async Task<int> UpdateMokkiAsync(Mokki mokki)
        {
            try
            {
                using (var conn = _databaseConnector._getConnection())
                {
                    await conn.OpenAsync();  // Open the connection asynchronously

                    string query = @"UPDATE mokki
                                     SET alue_id = @alue_id, postinro = @Postinro, mokkinimi = @Mokkinimi, katuosoite = @Katuosoite,
                                         hinta = @Hinta, kuvaus = @Kuvaus, henkilomaara = @Henkilomaara, varustelu = @Varustelu
                                     WHERE mokki_id = @mokki_id"; // Query to update a Mokki by ID

                    return await conn.ExecuteAsync(query, mokki); // Execute the update query
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe mökin {mokki?.mokki_id} päivityksessä: {ex.Message}");
                 // Tarkista vierasavaimen rajoitus (esim. alue_id tai postinro ei löydy)
                if (ex.Number == 1452) // MySQL:n virhekoodi vierasavaimen puuttumiselle (Cannot add or update a child row...)
                {
                     await NaytaIlmoitus("Tietokantavirhe", $"Mökin päivitys epäonnistui: Valittua aluetta tai postinumeroa ei löydy.");
                }
                else
                {
                    await NaytaIlmoitus("Tietokantavirhe", $"Mökin päivitys epäonnistui: {ex.Message}");
                }
                return 0; // Palauta 0 virheen sattuessa
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Yleinen virhe mökin {mokki?.mokki_id} päivityksessä: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Mökin päivitys epäonnistui odottamattoman virheen vuoksi: {ex.Message}");
                return 0; // Palauta 0 virheen sattuessa
            }
        }

        /// <summary>
        /// Poistaa mökin tietokannasta ID:n perusteella.
        /// </summary>
        /// <param name="mokkiId">Poistettavan mökin ID.</param>
        /// <returns>True, jos poisto onnistui; false, jos epäonnistui (esim. FK-rajoitus tai muu virhe).</returns>
        public async Task<bool> DeleteMokkiAsync(int mokkiId)
        {
            try
            {
                using (var conn = _databaseConnector._getConnection())
                {
                    await conn.OpenAsync();  // Open the connection asynchronously

                    string query = "DELETE FROM mokki WHERE mokki_id = @MokkiId";  // Query to delete a Mokki by ID
                    int rowsAffected = await conn.ExecuteAsync(query, new { MokkiId = mokkiId }); // Execute the delete query

                    // Tarkista, poistettiinko yksikään rivi
                    if (rowsAffected > 0)
                    {
                         Debug.WriteLine($"MokkiDatabaseService: Mökki {mokkiId} poistettu onnistuneesti.");
                         return true; // Poisto onnistui
                    }
                    else
                    {
                         // Mökkiä ei löytynyt tällä ID:llä
                         Debug.WriteLine($"MokkiDatabaseService: Mökkiä ID:llä {mokkiId} ei löytynyt poistettavaksi.");
                         await NaytaIlmoitus("Huomautus", $"Mökkiä ID:llä {mokkiId} ei löytynyt poistettavaksi.");
                         return false; // Ei poistettu, koska ei löytynyt
                    }
                }
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe mökin {mokkiId} poistossa: {ex.Message}");
                // Tarkista vierasavaimen rajoitus (esim. varaukset viittaavat mökkiin)
                if (ex.Number == 1451) // MySQL:n virhekoodi vierasavaimen rajoitusrikkomukselle (Cannot delete or update a parent row...)
                {
                    await NaytaIlmoitus("Poisto estetty", $"Mökkiä ID:llä {mokkiId} ei voida poistaa, koska siihen liittyy varauksia.");
                }
                else
                {
                    await NaytaIlmoitus("Tietokantavirhe", $"Mökin poistaminen epäonnistui: {ex.Message}");
                }
                return false; // Palauta false virheen sattuessa
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Yleinen virhe mökin {mokkiId} poistossa: {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Mökin poistaminen epäonnistui odottamattoman virheen vuoksi: {ex.Message}");
                return false; // Palauta false virheen sattuessa
            }
        }

        /// <summary>
        /// Hakee majoitusraportin tiedot tietokannasta tietyllä ajanjaksolla ja alueella.
        /// </summary>
        /// <param name="alkuPvm">Raportin alkupäivämäärä.</param>
        /// <param name="loppuPvm">Raportin loppupäivämäärä.</param>
        /// <param name="alueId">Alueen ID.</param>
        /// <returns>Lista MajoitusRaportti-olioita.</returns>
        public async Task<List<MajoitusRaportti>> HaeMajoitusRaportti(DateTime alkuPvm, DateTime loppuPvm, int alueId)
        {
            try
            {
                using (var conn = _databaseConnector._getConnection())
                {
                    await conn.OpenAsync();

                    string query = @"
                        SELECT
                            m.mokkinimi AS MokkiNimi,
                            v.varattu_alkupvm AS VarattuAlkuPvm,
                            v.varattu_loppupvm AS VarattuLoppuPvm,
                            CONCAT(a.etunimi, ' ', a.sukunimi) AS AsiakasNimi,
                            DATEDIFF(v.varattu_loppupvm, v.varattu_alkupvm) AS KestoPaivina,
                            DATEDIFF(v.varattu_loppupvm, v.varattu_alkupvm) * m.hinta AS HintaYhteensa
                        FROM
                            varaus v
                        JOIN
                            mokki m ON v.mokki_id = m.mokki_id
                        JOIN
                            asiakas a ON v.asiakas_id = a.asiakas_id
                        WHERE
                            v.varattu_alkupvm BETWEEN @AlkuPvm AND @LoppuPvm
                            AND m.alue_id = @AlueId
                    ";

                    var tulokset = await conn.QueryAsync<MajoitusRaportti>(query, new
                    {
                        AlkuPvm = alkuPvm,
                        LoppuPvm = loppuPvm,
                        AlueId = alueId
                    });

                    return tulokset.ToList();
                }
            }
             catch (Exception ex)
            {
                Debug.WriteLine($"MokkiDatabaseService: Virhe majoitusraportin haussa (AlueId: {alueId}, {alkuPvm:d}-{loppuPvm:d}): {ex.Message}");
                await NaytaIlmoitus("Virhe", $"Majoitusraportin lataaminen epäonnistui: {ex.Message}");
                return new List<MajoitusRaportti>(); // Palauta tyhjä lista virheen sattuessa
            }
        }

        /// <summary>
        /// Hakee kaikki mökit tietokannasta (käytetään ViewModelissä).
        /// </summary>
        /// <returns>Lista Mokki-olioita.</returns>
        public async Task<List<Mokki>> HaeKaikki()
        {
            // Kutsu olemassa olevaa GetAllMokkisAsync metodia
            var mokit = await GetAllMokkisAsync();
            return mokit.ToList();
        }

        /// <summary>
        /// Näyttää ilmoituksen käyttäjälle UI-säikeessä.
        /// </summary>
        private async Task NaytaIlmoitus(string otsikko, string viesti, string kuittaus = "OK")
        {
            // Tarkista, että Application.Current ja MainPage eivät ole null
            if (Application.Current?.MainPage != null)
            {
                // Dispatch to the UI thread to show the alert
                await Application.Current.MainPage.Dispatcher.DispatchAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(otsikko, viesti, kuittaus);
                });
            }
            else
            {
                // Jos UI-kontekstia ei ole, kirjoita virhe Debug-ulostuloon
                Debug.WriteLine($"NaytaIlmoitus (ei UI-kontekstia): {otsikko} - {viesti}");
            }
        }
    }
}
