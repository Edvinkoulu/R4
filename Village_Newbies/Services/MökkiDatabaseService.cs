using DatabaseConnection;  // Make sure to import the DatabaseConnection namespace
using Village_Newbies.Models;
using Dapper;

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

        // Create: Insert a new Mokki
        public async Task<int> CreateMokkiAsync(Mokki mokki)
        {
            using (var conn = _databaseConnector._getConnection()) // Get the connection from DatabaseConnector
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = @"INSERT INTO mokki (alue_id, postinro, mokkinimi, katuosoite, hinta, kuvaus, henkilomaara, varustelu)
                                 VALUES (@alue_id, @Postinro, @Mokkinimi, @Katuosoite, @Hinta, @Kuvaus, @Henkilomaara, @Varustelu)";
                
                return await conn.ExecuteAsync(query, mokki); // Execute the query asynchronously using Dapper
            }
        }

        public async Task<List<Alue>> GetAllAlueAsync()
        {
            AlueDatabaseService aluePalvelu = new AlueDatabaseService(_databaseConnector);
            List<Alue> alueet = await aluePalvelu.HaeKaikki();
            return alueet;
        }


        // Read: Get all Mokkis
        public async Task<IEnumerable<Mokki>> GetAllMokkisAsync()
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = "SELECT * FROM mokki"; // Simple SELECT query
                return await conn.QueryAsync<Mokki>(query); // Execute the query asynchronously and return the results
            }
        }
        public async Task<List<Mokki>> GetAllMokkiInAlue(int alueId) // Lisätty laskujenhallintaa varten, JaniV
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT * FROM mokki WHERE alue_id = @AlueId";
                var mokit = await conn.QueryAsync<Mokki>(query, new { AlueId = alueId });

                return mokit.ToList();
            }
        }
        // Read: Get Mokki by ID
        public async Task<Mokki> GetMokkiByIdAsync(int mokkiId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = "SELECT * FROM mokki WHERE mokki_id = @MokkiId";  // Query to get a Mokki by ID
                return await conn.QueryFirstOrDefaultAsync<Mokki>(query, new { MokkiId = mokkiId }); // Execute query and return the first match (or null)
            }
        }

        // Update: Update an existing Mokki
        public async Task<int> UpdateMokkiAsync(Mokki mokki)
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

        // Delete: Delete Mokki by ID
        public async Task<int> DeleteMokkiAsync(int mokkiId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = "DELETE FROM mokki WHERE mokki_id = @MokkiId";  // Query to delete a Mokki by ID
                return await conn.ExecuteAsync(query, new { MokkiId = mokkiId }); // Execute the delete query
            }
        }
        public async Task<List<MajoitusRaportti>> HaeMajoitusRaportti(DateTime alkuPvm, DateTime loppuPvm, int alueId)
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
// Tämä on lisäys: antaa ViewModelin odottaman metodin majoitusvarausten hallinnassa
public async Task<List<Mokki>> HaeKaikki()
{
    var mokit = await GetAllMokkisAsync();
    return mokit.ToList();
}

    }
}
