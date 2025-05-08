namespace Village_Newbies.ViewModels;
using Village_Newbies.Services;
using Village_Newbies.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using MySqlConnector;

// Tämä luokka kertoo käyttöliittymälle mitä sen pitäisi näyttää ja mitä se voi tehdä.

public class AlueViewModel : INotifyPropertyChanged
{
    private AlueDatabaseService databaseService;
    private ObservableCollection<Alue> alueet;
    private Alue _valittuAlue;
    private string _uusiAlueNimi;
    private bool _isEditing = false;
    private bool _onkoUusiAlueNimiValid = false;


    //Muodostimet

    public AlueViewModel()
    {
        databaseService = new AlueDatabaseService();
        alueet = new ObservableCollection<Alue>();
        LoadAlueetCommand = new Command(async () => await LoadAlueet());
        AddAlueCommand = new Command(async () => await AddAlue());
        UpdateAlueCommand = new Command(async () => await UpdateAlue());
        DeleteAlueCommand = new Command<Alue>(async (alue) => await DeleteAlue(alue));
        AsetaValittuAlueNullCommand = new Command(AsetaValittuAlueNull);
        SelectedAlueCommand = new Command<Alue>(ValitseAlueMuokkaustaVarten);
    }
    public AlueViewModel(AlueDatabaseService dbService)
    {
        databaseService = dbService;
        alueet = new ObservableCollection<Alue>();
        LoadAlueetCommand = new Command(async () => await LoadAlueet());
        AddAlueCommand = new Command(async () => await AddAlue());
        UpdateAlueCommand = new Command(async () => await UpdateAlue());
        DeleteAlueCommand = new Command<Alue>(async (alue) => await DeleteAlue(alue));
        AsetaValittuAlueNullCommand = new Command(AsetaValittuAlueNull);
        SelectedAlueCommand = new Command<Alue>(ValitseAlueMuokkaustaVarten);
    }

    public async Task InitializeAsync()
    {
        await LoadAlueet();
    }

    public Alue SelectedAlue
    {
        get => _valittuAlue;
        set
        {
            _valittuAlue = value;
            if (_valittuAlue != null)
            {
                UusiAlueNimi = _valittuAlue.alue_nimi;
                IsEditing = true;
            }
            else
            {
                UusiAlueNimi = string.Empty;
                IsEditing = false;
            }
            OnPropertyChanged(nameof(SelectedAlue));
        }
    }

    public ObservableCollection<Alue> Alueet
    {
        get => alueet;
        set
        {
            alueet = value;
            OnPropertyChanged(nameof(Alueet));
        }
    }

    public bool OnkoUusiAlueNimiValid
    {
        get => _onkoUusiAlueNimiValid;
        set
        {
            _onkoUusiAlueNimiValid = value;
            OnPropertyChanged(nameof(OnkoUusiAlueNimiValid));
        }
    }

    public string UusiAlueNimi
    {
        get => _uusiAlueNimi;
        set
        {
            _uusiAlueNimi = value;
            OnPropertyChanged(nameof(UusiAlueNimi));
            OnkoUusiAlueNimiValid = !string.IsNullOrWhiteSpace(_uusiAlueNimi);
            OnPropertyChanged(nameof(OnkoUusiAlueNimiValid));
        }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(IsNotEditing));
        }
    }
    //Komentoja
    public bool IsNotEditing => !IsEditing;
    public ICommand SelectedAlueCommand { get; }
    public ICommand LoadAlueetCommand { get; }
    public ICommand AddAlueCommand { get; }
    public ICommand UpdateAlueCommand { get; }
    public ICommand DeleteAlueCommand { get; }
    public ICommand AsetaValittuAlueNullCommand { get; }

    private void AsetaValittuAlueNull()
    {
        SelectedAlue = null;
    }
    private async Task LoadAlueet()
    {
        var alueet = await databaseService.HaeKaikki();
        Alueet = new ObservableCollection<Alue>(alueet);
    }

    private async Task AddAlue()
    {
        if (!string.IsNullOrWhiteSpace(UusiAlueNimi))
        {
            var uusiAlue = new Alue { alue_nimi = UusiAlueNimi };
            await databaseService.Lisaa(uusiAlue);
            await LoadAlueet(); // Päivitä lista
            UusiAlueNimi = string.Empty;
        }
    }

    private async Task UpdateAlue()
    {
        if (SelectedAlue != null && !string.IsNullOrWhiteSpace(UusiAlueNimi))
        {
            SelectedAlue.alue_nimi = UusiAlueNimi;
            await databaseService.Muokkaa(SelectedAlue);
            await LoadAlueet();
            SelectedAlue = null; // Postaa valinnat
            UusiAlueNimi = string.Empty;
        }
    }

    private async Task DeleteAlue(Alue alue)
    {
        // Tarkista, että alue-olio ei ole null
        if (alue == null)
        {
            Debug.WriteLine("DeleteAlue: Yritettiin poistaa null-aluetta.");
            return;
        }

        // Vahvistuskysely ennen poistoa
        bool vahvistus = await Application.Current.MainPage.DisplayAlert(
            "Poista alue",
            $"Haluatko varmasti poistaa alueen '{alue.alue_nimi}'?",
            "Kyllä",
            "Ei");

        if (!vahvistus)
        {
            Debug.WriteLine($"DeleteAlue: Alueen '{alue.alue_nimi}' poisto peruutettu käyttäjän toimesta.");
            return; // Palataan koska käyttäjä peruutti poiston
        }

        try
        {
            await databaseService.Poista(alue.alue_id);
            await LoadAlueet();
            await Application.Current.MainPage.DisplayAlert( 
                "Poisto onnistui",
                $"Alue '{alue.alue_nimi}' poistettiin onnistuneesti.",
                "OK"); // Näytä ilmoitus onnistuneesta poistosta
        }
        catch (MySqlException ex)
        {
            // Tarkistetaan, onko virhe vierasavaimen rajoituksesta johtuva (esim. mökit viittaavat alueeseen)
            if (ex.Number == 1451) // MySQL:n virhekoodi vierasavaimen rajoitusrikkomukselle
            {
                // Näytetään käyttäjälle selkeä ilmoitus siitä, miksi poisto estyi
                await Application.Current.MainPage.DisplayAlert(
                    "Poisto estetty",
                    $"Aluetta '{alue.alue_nimi}' ei voida poistaa, koska se on käytössä mökeissä tai muissa tiedoissa. Poista tai muokkaa ensin kaikki tähän alueeseen liittyvät mökit ja palvelut.",
                    "OK");
                Debug.WriteLine($"Tietokantavirhe (FK-rajoitus) poistettaessa aluetta '{alue.alue_nimi}': {ex.Message}");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Tietokantavirhe",
                    $"Alueen '{alue.alue_nimi}' poistamisessa tapahtui tietokantavirhe: {ex.Message}",
                    "OK");
                Debug.WriteLine($"Muu tietokantavirhe poistettaessa aluetta '{alue.alue_nimi}': {ex}");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Yleinen virhe",
                $"Alueen '{alue.alue_nimi}' poistamisessa tapahtui odottamaton virhe: {ex.Message}",
                "OK");
            Debug.WriteLine($"Yleinen virhe poistettaessa aluetta '{alue.alue_nimi}': {ex}");
        }
    }
    private void ValitseAlueMuokkaustaVarten(Alue alue)
    {
        if (alue != null)
        {
            SelectedAlue = alue; // Tämä asettaa UusiAlueNimi-kentän ja IsEditing-tilan
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}