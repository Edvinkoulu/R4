namespace Village_Newbies.Services;
using Village_Newbies.Models;
using Village_Newbies.Interfacet;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using DatabaseConnection;

// Tämä on nyt sitten se varsinainen luokka joka käsittelee alueita ja suorittaa CRUD toiminnallisuutta.
// Periytetty Tietovarasto luokalta joka sisältää IDataHaku interfacen. JaniV

public class AlueDatabaseService : DatabaseService, IAlueDatabaseService
{
    // Muodostimet
    public AlueDatabaseService()
    {
    }
    public AlueDatabaseService(DatabaseConnector connection) : base(connection)
    {
    }

    // CRUD toiminnallisuus
    public async Task<Alue> Hae(uint id)
    {
        string sql = "SELECT alue_id, nimi FROM alue WHERE alue_id = @alueId";
        DataTable dataTable = await HaeData(sql, ("@alueId", id));
        if (dataTable.Rows.Count > 0)
        {
            return LuoAlueOlio(dataTable.Rows[0]);
        }
        return null;
    }

    public async Task<List<Alue>> HaeKaikki()
    {
        string sql = "SELECT alue_id, nimi FROM alue";
        DataTable dataTable = await HaeData(sql);
        List<Alue> alueet = new List<Alue>();
        foreach (DataRow row in dataTable.Rows)
        {
            alueet.Add(LuoAlueOlio(row));
        }
        return alueet;
    }

    public async Task Lisaa(Alue alue)
    {
        string sql = "INSERT INTO alue (nimi) VALUES (@nimi)";
        await SuoritaKomento(sql, ("@nimi", alue.alue_nimi));
    }

    public async Task Muokkaa(Alue alue)
    {
        string sql = "UPDATE alue SET nimi = @nimi WHERE alue_id = @alueId";
        await SuoritaKomento(sql, ("@nimi", alue.alue_nimi), ("@alueId", alue.alue_id));
    }

    public async Task Poista(uint id)
    {
        string sql = "DELETE FROM alue WHERE alue_id = @alueId";
        await SuoritaKomento(sql, ("@alueId", id));
    }

    private Alue LuoAlueOlio(DataRow row)
    {
        uint alueId;

        // Tarkistetaan sarakkeen tyyppi ja muunnetaan tarvittaessa int -> uint. Ehkä turhaa koodia?
        if (row["alue_id"] is int intValue)
        {
            if (intValue < 0)
            {
                throw new ArgumentOutOfRangeException($"Int32-arvo {intValue} ei saa olla negatiivinen alue_id-kentässä.");
            }
            alueId = (uint)intValue;
        }
        else if (row["alue_id"] is uint uintValue)
        {
            alueId = uintValue;
        }
        else
        {
            throw new InvalidCastException($"Odottamaton tietotyyppi alue_id-kentässä: {row["alue_id"].GetType()}");
        }

        return new Alue(alueId, row["nimi"].ToString());
    }

    public override async Task<DataTable> HaeData(string sql, params (string, object)[] parameters)
    {
        using (var yhteys = HaeYhteysTietokantaan())
        using (MySqlCommand komento = new MySqlCommand(sql, yhteys))
        {
            foreach (var param in parameters)
            {
                komento.Parameters.AddWithValue(param.Item1, param.Item2);
            }
            using (MySqlDataAdapter adapteri = new MySqlDataAdapter(komento))
            {
                DataTable dataTaulu = new DataTable();
                await Task.Run(() => adapteri.Fill(dataTaulu));
                return dataTaulu;
            }
        }
    }

    public override async Task<int> SuoritaKomento(string sql, params (string, object)[] parameters)
    {
        using (var yhteys = HaeYhteysTietokantaan())
        using (MySqlCommand komento = new MySqlCommand(sql, yhteys))
        {
            foreach (var param in parameters)
            {
                komento.Parameters.AddWithValue(param.Item1, param.Item2);
            }
            return await komento.ExecuteNonQueryAsync();
        }
    }
}
