using DatabaseConnection;
using MySqlConnector;
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies;

public partial class MainPage : TabbedPage
{
    private AsiakasDatabaseService asiakasService = new AsiakasDatabaseService();
    private PalveluDatabaseService _palveluService = new PalveluDatabaseService();
    private Varauksen_palvelutDatabaseService _vpService = new();

    public MainPage()
    {
        InitializeComponent();
        LoadPalvelut();
    }

    private async void LoadPalvelut()
{
    var palvelut = await _palveluService.GetAllPalvelutAsync();
    PalveluPicker.ItemsSource = palvelut.ToList(); // Explicit IList
    PalveluPicker.ItemDisplayBinding = new Binding("Nimi");

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

            if (uint.TryParse(VarausIdEntry.Text, out uint varausId) &&
            int.TryParse(LkmEntry.Text, out int lkm) &&
            PalveluPicker.SelectedItem is Palvelu selectedPalvelu)
            {
                var uusiVP = new VarauksenPalvelu
                {
                    VarausId = varausId,
                    PalveluId = (uint)selectedPalvelu.palvelu_id,
                    Lkm = lkm
                };

                await _vpService.CreateAsync(uusiVP);
            }
            await DisplayAlert("Onnistui", "Asiakas tallennettu!", "OK");

            EtunimiEntry.Text = "";
            SukunimiEntry.Text = "";
            EmailEntry.Text = "";
            PuhelinEntry.Text = "";
            OsoiteEntry.Text = "";
            PostinroEntry.Text = "";
            PalveluPicker.SelectedIndex = -1;

            VarausLomake.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Virhe", ex.Message, "OK");
        }
    }
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
    }
}