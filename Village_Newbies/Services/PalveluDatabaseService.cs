using DatabaseConnection;  // Importoi DatabaseConnection namespace
using Village_Newbies.Models;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Village_Newbies.Services
{
    public class PalveluDatabaseService
    {
        private readonly DatabaseConnector _databaseConnector;

        // Default-konstruktori
        public PalveluDatabaseService()
        {
            _databaseConnector = new DatabaseConnector();
        }

        // Konstruktori, jossa voidaan käyttää mukautettua DatabaseConnectoria
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

        string query = @"INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv)
                         VALUES (@AlueId, @Nimi, @Kuvaus, @Hinta, @Alv)";

        // Parametrin nimeä vastaa luokan kenttä.
        return await conn.ExecuteAsync(query, new { AlueId = palvelu.alue_id, Nimi = palvelu.Nimi, Kuvaus = palvelu.Kuvaus, Hinta = palvelu.Hinta, Alv = palvelu.Alv });
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

        // Päivitetään palvelu tietokannassa
        string query = @"UPDATE palvelu
                         SET alue_id = @AlueId, nimi = @Nimi, kuvaus = @Kuvaus, 
                             hinta = @Hinta, alv = @Alv
                         WHERE palvelu_id = @PalveluId";

        // Varmistetaan, että kaikki kentät ovat mukana
        var parameters = new 
        {
            AlueId = palvelu.alue_id,
            Nimi = palvelu.Nimi,
            Kuvaus = palvelu.Kuvaus,
            Hinta = palvelu.Hinta,
            Alv = palvelu.Alv,
            PalveluId = palvelu.palvelu_id
        };

        return await conn.ExecuteAsync(query, parameters);
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

        // Delete: Poista viittaukset varauksen_palvelut-taulusta, jossa palvelu_id on viitattu
        public async Task DeleteViittauksetPalveluista(int palveluId)
        {
            using (var conn = _databaseConnector._getConnection())
            {
                await conn.OpenAsync();

                string query = "DELETE FROM varauksen_palvelut WHERE palvelu_id = @PalveluId";
                await conn.ExecuteAsync(query, new { PalveluId = palveluId });
            }
        }

        // Read: Hae kaikki alueet
        public async Task<List<Alue>> GetAllAlueAsync()
        {
            AlueDatabaseService aluePalvelu = new AlueDatabaseService(_databaseConnector);
            List<Alue> alueet = await aluePalvelu.HaeKaikki();
            return alueet;
        }
public async Task<IEnumerable<PalveluRaportti>> GetOstetutLisapalvelutRaportti(DateTime alkuPvm, DateTime loppuPvm, List<int> alueet)
{
    using var conn = _databaseConnector._getConnection();
    await conn.OpenAsync();

    // SQL-kysely palveluiden hakemiseksi
string query = @"
    SELECT 
        p.nimi AS PalveluNimi,
        p.kuvaus AS PalveluKuvaus,
        p.hinta AS PalveluHinta,
        v.varattu_alkupvm AS VarattuAlkuPvm,
        v.varattu_loppupvm AS VarattuLoppuPvm,
        CONCAT(a.etunimi, ' ', a.sukunimi) AS AsiakasNimi
    FROM 
        varauksen_palvelut vp
    JOIN 
        varaus v ON vp.varaus_id = v.varaus_id
    JOIN 
        palvelu p ON vp.palvelu_id = p.palvelu_id
    JOIN 
        asiakas a ON v.asiakas_id = a.asiakas_id
    WHERE 
        v.varattu_alkupvm BETWEEN @AlkuPvm AND @LoppuPvm
        AND p.alue_id IN @Alueet
";

    var parameters = new 
    {
        AlkuPvm = alkuPvm,
        LoppuPvm = loppuPvm,
        Alueet = alueet
    };

    // Suorita kysely ja palauta tulokset
    return await conn.QueryAsync<PalveluRaportti>(query, parameters);
}

    }

    
}