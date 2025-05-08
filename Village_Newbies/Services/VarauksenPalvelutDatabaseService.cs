using DatabaseConnection;
using Dapper;
using Village_Newbies.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Village_Newbies.Services
{
    public class Varauksen_palvelutDatabaseService
    {
        private readonly DatabaseConnector _databaseConnector;

        public Varauksen_palvelutDatabaseService()
        {
            _databaseConnector = new DatabaseConnector();
        }

        public Varauksen_palvelutDatabaseService(DatabaseConnector databaseConnector)
        {
            _databaseConnector = databaseConnector;
        }
        public async Task<int> CreateAsync(VarauksenPalvelu vp)
        {
            using (var conn = _databaseConnector._getConnection()) // Get the connection from DatabaseConnector
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = @"
                INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm)
                VALUES (@VarausId, @PalveluId, @Lkm);";
                
                return await conn.ExecuteAsync(query, vp); // Execute the query asynchronously using Dapper
            }
        }

        public async Task<IEnumerable<VarauksenPalvelu>> GetAllAsync()
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = "SELECT * FROM varauksen_palvelut"; // Simple SELECT query
                return await conn.QueryAsync<VarauksenPalvelu>(query); // Execute the query asynchronously and return the results
            }
        }

        public async Task<VarauksenPalvelu?> GetAsync(uint varausId, uint palveluId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = @"
                SELECT varaus_id AS VarausId, palvelu_id AS PalveluId, lkm AS Lkm
                FROM varauksen_palvelut
                WHERE varaus_id = @VarausId AND palvelu_id = @PalveluId;";
                return await conn.QueryFirstOrDefaultAsync<VarauksenPalvelu>(query, new { VarausId = varausId, PalveluId = palveluId }); // Execute the query asynchronously and return the results
            }
        }

        public async Task<int> UpdateAsync(VarauksenPalvelu vp)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = @"
                UPDATE varauksen_palvelut
                SET lkm = @Lkm
                WHERE varaus_id = @VarausId AND palvelu_id = @PalveluId;";

                return await conn.ExecuteAsync(query, vp); // Execute the update query
            }
        }
        public async Task<int> DeleteAsync(uint varausId, uint palveluId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();  // Open the connection asynchronously

                string query = @"
                DELETE FROM varauksen_palvelut
                WHERE varaus_id = @VarausId AND palvelu_id = @PalveluId;";
                return await conn.ExecuteAsync(query, new { VarausId = varausId, PalveluId = palveluId }); // Execute the delete query
            }
        }
        public async Task<int> DeleteByVarausIdAsync(uint varausId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync(); // Open the connection asynchronously

                string query = @"
                DELETE FROM varauksen_palvelut
                WHERE varaus_id = @VarausId;";

                // Execute the delete query and return the number of affected rows
                return await conn.ExecuteAsync(query, new { VarausId = varausId });
            }
        }
        public async Task PoistaVarauksenPalvelutAsync(uint varausId)
        {
            if (varausId == 0)
            {
                // Ei voida poistaa palveluita varaukselta, jonka ID on 0
                await Shell.Current.DisplayAlert("Virhe", "Virheellinen varaus-ID (0).", "OK");
                return;
            }

            try
            {
                int deletedCount = await DeleteByVarausIdAsync(varausId);

                if (deletedCount > 0)
                {
                    await Shell.Current.DisplayAlert("Onnistui", $"Poistettiin {deletedCount} palvelua varaukselta ID {varausId}.", "OK");
                }
                else
                {
                    // Vaikka poisto onnistui teknisesti, yhtään palvelua ei löytynyt tälle varaukselle
                    await Shell.Current.DisplayAlert("Huomio", $"Varauksella ID {varausId} ei ollut palveluita poistettavaksi.", "OK");
                }
            }
            catch (MySqlException ex)
            {
                 // Käsittele mahdolliset tietokantavirheet poiston aikana
                 await Shell.Current.DisplayAlert("Tietokantavirhe", $"Palveluiden poisto varaukselta {varausId} epäonnistui: {ex.Message}", "OK");
            }
            catch (Exception ex)
            {
                // Käsittele muut mahdolliset virheet
                await Shell.Current.DisplayAlert("Virhe", $"Palveluiden poisto varaukselta {varausId} epäonnistui: {ex.Message}", "OK");
            }
        }
    }
}
