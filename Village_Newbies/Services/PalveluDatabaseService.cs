using DatabaseConnection;  // Importoi DatabaseConnection namespace
using Village_Newbies.Models;
using Dapper;

namespace Village_Newbies.Services
{
    public class PalveluDatabaseService
    {
        private readonly DatabaseConnector _databaseConnector;

        public PalveluDatabaseService()
        {
            _databaseConnector = new DatabaseConnector();
        }

        public PalveluDatabaseService(DatabaseConnector databaseConnector)
        {
            _databaseConnector = databaseConnector;
        }

        // Create: Lisää uusi palvelu
        public async Task<int> CreatePalveluAsync(Palvelu palvelu)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = @"IINSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv)
                VALUES (@AlueId, @Nimi, @Kuvaus, @Hinta, @Alv)";

                return await conn.ExecuteAsync(query, palvelu);
            }
        }

        // Read: Hae kaikki palvelut
        public async Task<IEnumerable<Palvelu>> GetAllPalvelutAsync()
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT * FROM palvelu";
                return await conn.QueryAsync<Palvelu>(query);
            }
        }

        // Read: Hae palvelu ID:n perusteella
        public async Task<Palvelu> GetPalveluByIdAsync(int palveluId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT * FROM palvelu WHERE palvelu_id = @PalveluId";
                return await conn.QueryFirstOrDefaultAsync<Palvelu>(query, new { PalveluId = palveluId });
            }
        }

        // Update: Päivitä olemassa oleva palvelu
        public async Task<int> UpdatePalveluAsync(Palvelu palvelu)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = @"UPDATE palvelu
                                 SET alue_id = @AlueId, nimi = @Nimi, kuvaus = @Kuvaus,
                                     hinta = @Hinta, alv = @Alv
                                 WHERE palvelu_id = @PalveluId";

                return await conn.ExecuteAsync(query, palvelu);
            }
        }

        // Delete: Poista palvelu ID:n perusteella
        public async Task<int> DeletePalveluAsync(int palveluId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = "DELETE FROM palvelu WHERE palvelu_id = @PalveluId";
                return await conn.ExecuteAsync(query, new { PalveluId = palveluId });
            }
        }
         // Read: Hae kaikki alueet
        public async Task<IEnumerable<Alue>> GetAllAlueAsync()
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = "SELECT * FROM alue";  // Assuming there is a table named 'alue'
                return await conn.QueryAsync<Alue>(query);
            }
        }
    }
}