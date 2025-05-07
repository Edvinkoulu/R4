namespace Village_Newbies.Services;
using Village_Newbies.Interfacet;
using DatabaseConnection;
using MySqlConnector;
using System.Data;
using System.Threading.Tasks;


// Tätä ei ole tarkoitus luoda missään vaan käyttää periytymisen yhteydessä kun teet tietovarastoja.
// Kaikki tietovarastot tarvitsee yhteyden tietokantaan ja tämä luokka sitten toteuttaa ne metodit.
// Eipähän tarvitse tehdä samaa viittä kertaa. JaniV

public abstract class DatabaseService : IDataHaku, IDisposable
{

    protected readonly DatabaseConnector dbConnector;
    private MySqlConnection _connection; // Luo yhteys, mutta ei avata vielä. Tällä tavalla vasta luodun ID:n hakeminen tietokannasta on helpompaa.
    // Aiemmin HaeData ja Suorita komento metodit avasivat ja sulkivat yhteyden heti käytön jälkeen. 
    // Tämän takia vasta lisätyn ID:n haku ei onnistutun lauseella "SELECT LAST_INSERT_ID();" - JaniV

    //Muodostimet
    public DatabaseService()
    {
        dbConnector = new DatabaseConnector();
        _connection = dbConnector._getConnection();
    }
    public DatabaseService(DatabaseConnector databaseConnector)
    {
        dbConnector = databaseConnector;
        _connection = databaseConnector._getConnection();
    }
    protected async Task AvaaYhteys()
    {
        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }
    }
    protected async Task SuljeYhteys()
    {
        if (_connection.State == ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
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
    public virtual async Task<DataTable> HaeData(string sql, params (string, object)[] parameters)
    {
        using (var connection = dbConnector._getConnection()) // Uusi yhteys
        {
            await connection.OpenAsync();
            using var cmd = new MySqlCommand(sql, connection);
            foreach (var (nimi, arvo) in parameters)
                cmd.Parameters.AddWithValue(nimi, arvo);
            var table = new DataTable();
            using var adapter = new MySqlDataAdapter(cmd);
            await Task.Run(() => adapter.Fill(table));
            return table;
        }
    }
    public virtual async Task<int> SuoritaKomento(string sql, params (string, object)[] parameters)
    {
        using (var connection = dbConnector._getConnection()) // Uusi yhteys
        {
            await connection.OpenAsync();
            using var cmd = new MySqlCommand(sql, connection);
            foreach (var (nimi, arvo) in parameters)
                cmd.Parameters.AddWithValue(nimi, arvo);
            return await cmd.ExecuteNonQueryAsync();
        }
    }
    public void Dispose()
    {
        SuljeYhteysAsync().Wait();
        _connection?.Dispose();
    }
    protected virtual async Task SuljeYhteysAsync()
    {
        if (_connection.State == ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
    }
}
