using DatabaseConnection; //jos tarviitestata databasea 
using MySqlConnector; //jos tarviitestata databasea 
using Village_Newbies.Models;
using Village_Newbies.Services;
using System.Diagnostics;

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
        this.BindingContext = this;
        LoadPalvelut();
        LoadMokit();
        ValittuAlkuPv = DateTime.Today;
        ValittuLoppuPv = DateTime.Today.AddDays(1);
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

            uint varausId = await _varausService.Lisaa2(uusiVaraus);

            if (varausId > 0) //Lisaa2 palauttaa 0 jos tulee virhe.
            {
                var selectedPalvelu = PalveluPicker.SelectedItem as Palvelu;

                if (selectedPalvelu == null)
                {
                    Debug.WriteLine("Palvelua ei valittu varaukselle. Varaus tallennettu ilman palvelua.");
                }
                else if (!int.TryParse(LkmEntry.Text, out int lkm) || lkm <= 0)
                {
                    await DisplayAlert("Huomautus", "Varauksen palvelun määrä oli virheellinen. Varaus tallennettu ilman palvelua.", "OK");
                    Debug.WriteLine($"Varauksen palvelun määrä virheellinen: '{LkmEntry.Text}'");
                }
                else
                {
                    var uusiVP = new VarauksenPalvelu
                    {
                        VarausId = varausId,
                        PalveluId = (uint)selectedPalvelu.palvelu_id,
                        Lkm = lkm
                    };

                    int rowsAffectedVP = await _vpService.CreateAsync(uusiVP);

                    if (rowsAffectedVP > 0)
                    {
                        Debug.WriteLine($"Varauksen palvelu (ID: {selectedPalvelu.palvelu_id}, Määrä: {lkm}) lisätty varaukseen ID: {varausId}.");
                    }
                    else
                    {
                        Debug.WriteLine($"Varauksen palvelun (ID: {selectedPalvelu.palvelu_id}) lisäys varaukseen ID: {varausId} ei vaikuttanut riveihin.");
                        await DisplayAlert("Huomautus", "Valittua palvelua ei voitu liittää varaukseen.", "OK");
                    }
                }

                EtunimiEntry.Text = "";
                SukunimiEntry.Text = "";
                EmailEntry.Text = "";
                PuhelinEntry.Text = "";
                OsoiteEntry.Text = "";
                PostinroEntry.Text = "";
                PalveluPicker.SelectedIndex = -1;
                LkmEntry.Text = "";

                VarausLomake.IsVisible = false;
            }
            else
            {
                Debug.WriteLine("Varauksen lisäys epäonnistui (ID 0). Palvelua ei tallennettu.");
            }
        }
        catch (FormatException)
        {
            await DisplayAlert("Virhe syötteessä", "Määrä-kentän tulee olla numero.", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MainPage TallennaVaraus Exception: {ex.Message}");
            await DisplayAlert("Virhe tallennuksessa", $"Varauksen tallennus epäonnistui: {ex.Message}", "OK");
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