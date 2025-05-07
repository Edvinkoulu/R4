using DatabaseConnection; //jos tarviitestata databasea 
using MySqlConnector; //jos tarviitestata databasea 
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies;

public partial class MainPage : TabbedPage
{
    private AsiakasDatabaseService asiakasService = new AsiakasDatabaseService();

    public MainPage()
    {
        InitializeComponent();
    }

    private void ToggleLomake(object sender, EventArgs e)
    {
        VarausLomake.IsVisible = !VarausLomake.IsVisible;
    }

    private async void TallennaVaraus(object sender, EventArgs e)
    {
        try
        {
            var uusiAsiakas = new Asiakas
            {
                etunimi = EtunimiEntry.Text,
                sukunimi = SukunimiEntry.Text,
                email = EmailEntry.Text,
                puhelinnro = PuhelinEntry.Text,
                lahiosoite = OsoiteEntry.Text,
                postinro = PostinroEntry.Text
            };

            await asiakasService.Lisaa(uusiAsiakas);
            await DisplayAlert("Onnistui", "Asiakas tallennettu!", "OK");

            EtunimiEntry.Text = "";
            SukunimiEntry.Text = "";
            EmailEntry.Text = "";
            PuhelinEntry.Text = "";
            OsoiteEntry.Text = "";
            PostinroEntry.Text = "";

            VarausLomake.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Virhe", ex.Message, "OK");
        }
    }
    /* ONGELMIEN VARALTA TALLESSA
    private async void OnDatabaseClicked(object sender, EventArgs e)
    {
        DatabaseConnector dbc = new DatabaseConnector();
        try
        {
            var conn = dbc._getConnection();
            conn.Open();
            await DisplayAlert("Onnistui", "Tietokanta yhteys aukesi", "OK");
        }
        catch (MySqlException ex)
        {
            await DisplayAlert("Failure", ex.Message, "OK");
        }
    }*/
}