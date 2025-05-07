using DatabaseConnection; //jos tarviitestata databasea 
using MySqlConnector; //jos tarviitestata databasea 
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies;

public partial class MainPage : TabbedPage
{
    private AsiakasDatabaseService asiakasService = new AsiakasDatabaseService();
    private PalveluDatabaseService _palveluService = new PalveluDatabaseService();
    private MokkiDatabaseService _mokkiService = new MokkiDatabaseService();
    private VarausDatabaseService _varausService = new VarausDatabaseService();
    
    private Varauksen_palvelutDatabaseService _vpService = new();

    private DateTime _valittuAlkuPv;
        public DateTime ValittuAlkuPv
        {
            get => _valittuAlkuPv;
            set
            {
                if (_valittuAlkuPv.Date != value.Date)
                {
                    _valittuAlkuPv = value.Date;
                    OnPropertyChanged(nameof(ValittuAlkuPv));
                }
            }
        }

        private DateTime _valittuLoppuPv;
        public DateTime ValittuLoppuPv
        {
            get => _valittuLoppuPv;
            set
            {
                if (_valittuLoppuPv.Date != value.Date)
                {
                    _valittuLoppuPv = value.Date;
                    OnPropertyChanged(nameof(ValittuLoppuPv));
                }
            }
        }
    public MainPage()
    {
        InitializeComponent();
        LoadPalvelut();
        LoadMokit();
    }

    private async void LoadPalvelut()
{
    var palvelut = await _palveluService.GetAllPalvelutAsync();
    PalveluPicker.ItemsSource = palvelut.ToList(); // Explicit IList
    PalveluPicker.ItemDisplayBinding = new Binding("Nimi");

}

    private async void LoadMokit()
{
    var mokit = await _mokkiService.GetAllMokkisAsync();
    MokkiPicker.ItemsSource = mokit.ToList(); // Explicit IList
    MokkiPicker.ItemDisplayBinding = new Binding("Mokkinimi");
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

            uint asiakasId = await asiakasService.Lisaa(uusiAsiakas);
            var selectedMokki = MokkiPicker.SelectedItem as Mokki; // Ensure that selectedMokki is of type Mokki

            if (selectedMokki == null)
            {
                await DisplayAlert("Virhe", "Mökki ei ole valittu", "OK");
                return;
            }

            var uusiVaraus = new Varaus
            {
                asiakas_id = asiakasId,
                mokki_id = (uint)selectedMokki.mokki_id,
                varattu_pvm = DateTime.Now,
                vahvistus_pvm = DateTime.Now,
                varattu_alkupvm = ValittuAlkuPv,
                varattu_loppupvm = ValittuLoppuPv
            };

            uint varausId =  await _varausService.Lisaa(uusiVaraus);

            if (int.TryParse(LkmEntry.Text, out int lkm) &&
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