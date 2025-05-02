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
    public AlueDatabaseService() { }
    public AlueDatabaseService(DatabaseConnector connection) : base(connection) { }

    public async Task<Alue> Hae(uint id)
    {
        string sql = "SELECT alue_id, nimi FROM alue WHERE alue_id = @alueId";
        return await HaeYksi(sql, LuoAlueOlio, ("@alueId", id));
    }
    public async Task<List<Alue>> HaeKaikki()
    {
        string sql = "SELECT alue_id, nimi FROM alue";
        return await HaeLista(sql, LuoAlueOlio);
    }
    public async Task Lisaa(Alue alue)
    {
        string sql = "INSERT INTO alue (nimi) VALUES (@nimi)";
        await SuoritaKomento(sql, ("@nimi", alue.alue_nimi));
    }
    public async Task Muokkaa(Alue alue)
    {
        if (alue != null)
        {
            var nykyinenAlue = await Hae(alue.alue_id);
            if (nykyinenAlue != null)
            {
                bool vahvistus = await VahvistaToiminto("Muokkaus", $"Haluatko varmasti vaihtaa alueen '{nykyinenAlue.alue_nimi}' nimeksi '{alue.alue_nimi}'?");
                if (vahvistus)
                {
                    string sql = "UPDATE alue SET nimi = @nimi WHERE alue_id = @alueId";
                    await SuoritaKomento(sql, ("@nimi", alue.alue_nimi), ("@alueId", alue.alue_id));
                }
            }
        }
    }
    public async Task Poista(uint id)
    {
        var alue = await Hae(id);
        if (alue != null)
        {
            bool vahvistus = await VahvistaToiminto("Poisto", $"Haluatko varmasti poistaa alueen '{alue.alue_nimi}'?");
            if (vahvistus)
            {
                string sql = "DELETE FROM alue WHERE alue_id = @alueId";
                await SuoritaKomento(sql, ("@alueId", id));
            }
        }
    }
    private Alue LuoAlueOlio(DataRow row)
    {
        uint alueId = Convert.ToUInt32(row["alue_id"]);
        return new Alue(alueId, row["nimi"].ToString());
    }
}