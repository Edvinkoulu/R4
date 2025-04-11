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

    // Override metodit, pakko toteuttaa perivässä luokassa
    
    // HaeData: Suorittaa SQL-kyselyn, joka palauttaa dataa, ja palauttaa sen DataTable-oliona.
    // SuoritaKomento: Suorittaa SQL-komennon (INSERT, UPDATE, DELETE) ja palauttaa muutettujen rivien määrän.
    public abstract Task<DataTable> HaeData(string sql, params (string, object)[] parameters);
    public abstract Task<int> SuoritaKomento(string sql, params (string, object)[] parameters);

    /* ESIM: 
    
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
    */
}
