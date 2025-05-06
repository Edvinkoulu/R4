using System.Data;
using DatabaseConnection;                // projektissa jo oleva https-kerros
using MySqlConnector;
using Village_Newbies.Models;

namespace Village_Newbies.Services;

public class PostiDatabaseService : DatabaseService, IPostiDatabaseService
{
    public PostiDatabaseService() { }
    public PostiDatabaseService(DatabaseConnector c) : base(c) { }

    public async Task<bool> LisaaTaiPaivitaAsync(Posti p)
    {
        const string sql = @"
            INSERT INTO posti (postinro, toimipaikka)
            VALUES (@code, @city)
            ON DUPLICATE KEY UPDATE toimipaikka = VALUES(toimipaikka);";

        var rows = await SuoritaKomento(sql,
                     ("@code", p.Postinumero.Trim()),
                     ("@city", p.Toimipaikka.Trim()));
        return rows > 0;  // 1 = insert, 2 = update
    }

    public async Task<List<Posti>> HaeKaikkiAsync()
    {
        const string sql = "SELECT postinro, toimipaikka FROM posti ORDER BY postinro";
        var dt = await HaeData(sql);
        var list = new List<Posti>(dt.Rows.Count);
        foreach (DataRow r in dt.Rows)
        {
            list.Add(new Posti
            {
                Postinumero = r["postinro"] as string ?? string.Empty,
                Toimipaikka = r["toimipaikka"] as string ?? string.Empty
            });
        }
        return list;
    }
}
