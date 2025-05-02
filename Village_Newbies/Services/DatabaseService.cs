namespace Village_Newbies.Services;
using Village_Newbies.Interfacet;
using DatabaseConnection;
using MySqlConnector;
using System.Data;
using System.Threading.Tasks;


// Tätä ei ole tarkoitus luoda missään vaan käyttää periytymisen yhteydessä kun teet tietovarastoja.
// Kaikki tietovarastot tarvitsee yhteyden tietokantaan ja tämä luokka sitten toteuttaa ne metodit.
// Eipähän tarvitse tehdä samaa viittä kertaa. JaniV

public abstract class DatabaseService : IDataHaku
{

    protected readonly DatabaseConnector dbConnector;

    //Muodostimet
    public DatabaseService()
    {
        dbConnector = new DatabaseConnector();
    }
    public DatabaseService(DatabaseConnector databaseConnector)
    {
        dbConnector = databaseConnector;
    }
    protected MySqlConnection HaeYhteysTietokantaan()
    {
        var connection = dbConnector._getConnection();
        connection.Open();
        return connection;
    }
    protected async Task<bool> VahvistaToiminto(string toiminto, string viesti)
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert($"Vahvista '{toiminto}", viesti, "Kyllä", "Ei");
        }
        // Jos MainPage on null, palautetaan false, koska vahvistusta ei voida näyttää.
        return false;
    }
    protected async Task<T> HaeYksi<T>(string sql, Func<DataRow, T> luonti, params (string, object)[] parametrit)
    {
        var data = await HaeData(sql, parametrit);
        return data.Rows.Count > 0 ? luonti(data.Rows[0]) : default;
    }
    protected async Task<List<T>> HaeLista<T>(string sql, Func<DataRow, T> luonti, params (string, object)[] parametrit)
    {
        var data = await HaeData(sql, parametrit);
        return data.AsEnumerable().Select(luonti).ToList();
    }
    // HaeData: Suorittaa SQL-kyselyn, joka palauttaa dataa, ja palauttaa sen DataTable-oliona.
    // SuoritaKomento: Suorittaa SQL-komennon (INSERT, UPDATE, DELETE) ja palauttaa muutettujen rivien määrän.
    public virtual async Task<DataTable> HaeData(string sql, params (string, object)[] parameters)
    {
        using var conn = HaeYhteysTietokantaan();
        using var cmd = new MySqlCommand(sql, conn);
        foreach (var (nimi, arvo) in parameters)
            cmd.Parameters.AddWithValue(nimi, arvo);

        var table = new DataTable();
        using var adapter = new MySqlDataAdapter(cmd);
        await Task.Run(() => adapter.Fill(table));
        return table;
    }
    public virtual async Task<int> SuoritaKomento(string sql, params (string, object)[] parameters)
    {
        using var conn = HaeYhteysTietokantaan();
        using var cmd = new MySqlCommand(sql, conn);
        foreach (var (nimi, arvo) in parameters)
            cmd.Parameters.AddWithValue(nimi, arvo);

        return await cmd.ExecuteNonQueryAsync();
    }
}
