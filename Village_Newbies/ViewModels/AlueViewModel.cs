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

    private async Task DeleteAlue(Alue alue) // Virheenkäsittelyä ei vielä testattu.
    {
        if (alue != null)
        {
            try
            {
                await databaseService.Poista(alue.alue_id);
                await LoadAlueet();
            }
            catch (MySqlException ex)
            {
                // Tarkistetaan, onko virhe vierasavaimen rajoituksesta johtuva
                if (ex.Number == 1451) // MySQL:n virhekoodi vierasavaimen rajoitusrikkomukselle
                {
                    // Näytetään käyttäjälle ilmoitus
                    bool vastaus = await Application.Current.MainPage.DisplayAlert(
                        "Poisto estetty",
                        $"Aluetta '{alue.alue_nimi}' ei voida poistaa, koska se on käytössä mökeissä.",
                        "OK",
                        "Peruuta");

                    // Jos käyttäjä painaa "Peruuta" (tai sulkee ilmoituksen OK:lla), ei tehdä mitään.
                    // Jos painetaan "OK", voidaan mahdollisesti tehdä lisätoimenpiteitä (esim. ohjata käyttäjä muokkaamaan mökkejä).
                    if (vastaus)
                    {
                        // Tässä kohtaa voitaisiin mahdollisesti navigoida käyttäjä mökkien hallintaan
                        // tai näyttää viestiä siitä, miten alue saadaan poistettua (esim. poistamalla ensin siihen liittyvät mökit).
                        Debug.WriteLine($"Käyttäjä painoi OK poistoviestissä alueelle '{alue.alue_nimi}'.");
                    }
                    else
                    {
                        Debug.WriteLine($"Käyttäjä peruutti poistoyrityksen alueelle '{alue.alue_nimi}'.");
                    }
                }
                else
                {
                    // Jos kyseessä on jokin muu tietokantavirhe, näytetään yleisluontoisempi virheilmoitus
                    await Application.Current.MainPage.DisplayAlert(
                        "Virhe poistettaessa",
                        $"Alueen '{alue.alue_nimi}' poistamisessa tapahtui virhe: {ex.Message}",
                        "OK");
                    Debug.WriteLine($"Tietokantavirhe poistettaessa aluetta '{alue.alue_nimi}': {ex}");
                }
            }
            catch (Exception ex)
            {
                // Yleinen virheenkäsittely muiden poikkeusten varalle
                await Application.Current.MainPage.DisplayAlert(
                    "Yleinen virhe",
                    $"Alueen '{alue.alue_nimi}' poistamisessa tapahtui odottamaton virhe: {ex.Message}",
                    "OK");
                Debug.WriteLine($"Yleinen virhe poistettaessa aluetta '{alue.alue_nimi}': {ex}");
            }
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