namespace Village_Newbies.Services;

using Dapper;
using Village_Newbies.Models;
using Village_Newbies.Interfacet;
using DatabaseConnection;
using MySqlConnector;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;


public class AsiakasDatabaseService : DatabaseService, IAsiakasDatabaseService
{
    // ─────────────────────────  KONSTRUKTORIT ─────────────────────────
    public AsiakasDatabaseService() { }
    public AsiakasDatabaseService(DatabaseConnector connector) : base(connector) { }

    // ─────────────────────────  CRUD ‑ JULKISET ───────────────────────

    public async Task<Asiakas?> Hae(uint id)
    {
        const string sql = "SELECT * FROM asiakas WHERE asiakas_id = @id";
        var dt = await HaeData(sql, ("@id", id));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }
    public async Task<List<Asiakas>> Hae(string term)
{
    using var conn = dbConnector._getConnection();
    if (string.IsNullOrWhiteSpace(term))
        return await HaeKaikki();

    await conn.OpenAsync();

    string sql = @"SELECT * FROM asiakas
                   WHERE etunimi LIKE @T OR sukunimi LIKE @T
                      OR email LIKE @T OR puhelinnro LIKE @T
                   ORDER BY sukunimi, etunimi";

    var tulos = await conn.QueryAsync<Asiakas>(sql, new { T = $"%{term}%" });
    return tulos.ToList();
}

    public async Task<List<Asiakas>> HaeKaikki()
    {
        const string sql = "SELECT * FROM asiakas ORDER BY sukunimi, etunimi";
        var dt = await HaeData(sql);
        var list = new List<Asiakas>(dt.Rows.Count);
        foreach (DataRow r in dt.Rows) list.Add(Map(r));
        return list;
    }

    public async Task<uint> Lisaa(Asiakas a)
{
    const string sql = @"
        INSERT INTO asiakas
          (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro)
        VALUES (@postinro, @etu, @suku, @lahio, @email, @puh);
        SELECT LAST_INSERT_ID();";             // ← palauttaa uuden ID:n

    using var conn = dbConnector._getConnection();
    await conn.OpenAsync();
    a.asiakasId =  (uint) await conn.ExecuteScalarAsync<ulong>(sql, new {
        postinro = a.postinro,
        etu      = a.etunimi,
        suku     = a.sukunimi,
        lahio    = a.lahiosoite,
        email    = a.email,
        puh      = a.puhelinnro
    });
    return a.asiakasId;
}

    public async Task Muokkaa(Asiakas a)
    {
        const string sql = @"
            UPDATE asiakas SET
                postinro   = @postinro,
                etunimi    = @etu,
                sukunimi   = @suku,
                lahiosoite = @lahio,
                email      = @email,
                puhelinnro = @puh
            WHERE asiakas_id = @id";

        await SuoritaKomento(sql,
            ("@postinro", a.postinro),
            ("@etu",      a.etunimi),
            ("@suku",     a.sukunimi),
            ("@lahio",    a.lahiosoite),
            ("@email",    a.email),
            ("@puh",      a.puhelinnro),
            ("@id",       a.asiakas_id));
    }

    public async Task Poista(uint id)
    {
        const string sql = "DELETE FROM asiakas WHERE asiakas_id = @id";
        await SuoritaKomento(sql, ("@id", id));
    }

    // ─────────────────────────  IDataHaku‑OVERRIDET ──────────────────
    public override async Task<DataTable> HaeData(string sql, params (string, object)[] p)
    {
        using var conn = HaeYhteysTietokantaan();
        using var cmd  = new MySqlCommand(sql, conn);
        foreach (var (n, v) in p) cmd.Parameters.AddWithValue(n, v);

        var table = new DataTable();
        using var adapter = new MySqlDataAdapter(cmd);
        await Task.Run(() => adapter.Fill(table));
        return table;
    }

    public override async Task<int> SuoritaKomento(string sql, params (string, object)[] p)
    {
        using var conn = HaeYhteysTietokantaan();
        using var cmd  = new MySqlCommand(sql, conn);
        foreach (var (n, v) in p) cmd.Parameters.AddWithValue(n, v);
        return await cmd.ExecuteNonQueryAsync();
    }

    // ─────────────────────────  YKSITYISET APUT ──────────────────────
    private static Asiakas Map(DataRow r) => new Asiakas(
        (uint) r["asiakas_id"],
        r["etunimi"]     as string,
        r["sukunimi"]    as string,
        r["lahiosoite"]  as string,
        r["email"]       as string,
        r["puhelinnro"]  as string,
        r["postinro"]    as string);

        
}

