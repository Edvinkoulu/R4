namespace Village_Newbies.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;

public class MokkiViewModel : BindableObject
{
    public PostiViewModel PostiVM { get; set; }
    private readonly MokkiDatabaseService _mokkiDatabaseService;
    private ObservableCollection<Mokki> _mokkis;
    private ObservableCollection<Alue> _alueList;
    private Mokki _newMokki;

    public Mokki NewMokki
    {
        get => _newMokki;
        set
        {
            _newMokki = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Alue> AlueList
        {
            get => _alueList;
            set
            {
                _alueList = value;
                OnPropertyChanged();
            }
        }

    public ObservableCollection<Mokki> Mokkis
    {
        get => _mokkis;
        set
            {
                _mokkis = value;
                OnPropertyChanged(); // Notify the UI that the property has changed
            }
    }

    private Alue _selectedAlue;
    public Alue SelectedAlue
    {
        get => _selectedAlue;
        set
        {
            _selectedAlue = value;
            OnPropertyChanged();

            // Update NewMokki's alue_id when user selects a new Alue
            if (_selectedAlue != null)
            {
                NewMokki.alue_id = (int)_selectedAlue.alue_id; // Lisätty cast (int). En koskenut muuhun. Alue käyttää nyt uint tyyppistä muuttujaa id:ssä koska se muuten aiheutti ongelmia... JaniV
                OnPropertyChanged(nameof(NewMokki)); // Optional, in case the UI isn't updating
            }
        }
    }

    private Mokki _selectedMokkiForEdit;
    public Mokki SelectedMokkiForEdit
    {
        get => _selectedMokkiForEdit;
        set
        {
            _selectedMokkiForEdit = value;
            OnPropertyChanged();
        }
    }
    private bool _isEditing;
    private bool _isEditing2 = true;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            OnPropertyChanged();
        }
    }

    public bool IsEditing2
    {
        get => _isEditing2;
        set
        {
            _isEditing2 = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddMokkiCommand { get; }
    public ICommand DeleteMokkiCommand { get; }
    public ICommand LoadMokkiForEditCommand { get; }
    public ICommand ConfirmUpdateMokkiCommand { get; }

    public MokkiViewModel()
    {
        var postiService = new PostiDatabaseService(); // or however your actual implementation is named
        PostiVM = new PostiViewModel(postiService);

    PostiVM.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(PostiViewModel.Postinumero))
        {
            if (NewMokki != null)
                NewMokki.Postinro = PostiVM.Postinumero;

        }
    };

        _mokkiDatabaseService = new MokkiDatabaseService(); // Initialize the service
        NewMokki = new Mokki(); // Create a new instance of Mokki

        // Define command to add a new Mokki
        AddMokkiCommand = new Command(async () => await AddMokkiAsync());
        DeleteMokkiCommand = new Command<Mokki>(async (mokki) => await DeleteMokkiAsync(mokki));
        LoadMokkiForEditCommand = new Command<Mokki>((mokki) => LoadMokkiForEdit(mokki));
        ConfirmUpdateMokkiCommand = new Command(async () => await UpdateMokkiAsync());
        
        // Load initial data
        LoadAlueList(); // Get all Alue list
        LoadMokkis();   // Get all Mokki list
    }

    private async Task LoadAlueList()
        {
            // Fetch all alue records from the database
            var alueList = await _mokkiDatabaseService.GetAllAlueAsync();
            AlueList = new ObservableCollection<Alue>(alueList);
        }

        private async void LoadMokkis()
        {
            // Get all Mokkis from the database
            var mokkis = await _mokkiDatabaseService.GetAllMokkisAsync();
            // Update the ObservableCollection to reflect the changes
            Mokkis = new ObservableCollection<Mokki>(mokkis);
        }

    private async Task AddMokkiAsync()
{
    // Try saving postal info first
    await PostiVM.TallennaAsync();

    if (NewMokki == null)
    {
        await Shell.Current.DisplayAlert("Virhe", "Mökin tiedot puuttuvat.", "OK");
        return;
    }
    // Validate required fields
    if (string.IsNullOrEmpty(NewMokki.Postinro))
    {
        await Shell.Current.DisplayAlert("Virhe", "Postinumero puuttuu.", "OK");
        return;
    }

    if (string.IsNullOrWhiteSpace(NewMokki.Mokkinimi))
    {
        await Shell.Current.DisplayAlert("Virhe", "Mökin nimi puuttuu.", "OK");
        return;
    }

    // Try saving the Mokki
    int rowsAffected = await _mokkiDatabaseService.CreateMokkiAsync(NewMokki);

    if (rowsAffected > 0)
    {
        await Shell.Current.DisplayAlert("Onnistui", "Mökki lisättiin onnistuneesti.", "OK");

        NewMokki = new Mokki(); // Reset form
        OnPropertyChanged(nameof(NewMokki));
        LoadMokkis();           // Refresh list
    }
    else
    {
        await Shell.Current.DisplayAlert("Virhe", "Mökin lisääminen epäonnistui. Yritä uudelleen.", "OK");
    }
}


    private async Task DeleteMokkiAsync(Mokki mokki)
    {
    if (mokki == null) return;

    bool confirm = await Application.Current.MainPage.DisplayAlert(
        "Confirm Delete",
        $"Are you sure you want to delete '{mokki.Mokkinimi} ja {mokki.mokki_id}'?",
        "Yes",
        "No");

    if (confirm)
    {
        await _mokkiDatabaseService.DeleteMokkiAsync(mokki.mokki_id);
        Mokkis.Remove(mokki);
    }
    }

    private void LoadMokkiForEdit(Mokki mokki)
    {
    if (mokki == null) return;

        SelectedMokkiForEdit = mokki;

        // Copy fields from selected mokki into NewMokki
        NewMokki = new Mokki
        {
            mokki_id = mokki.mokki_id,
            alue_id = mokki.alue_id,
            Mokkinimi = mokki.Mokkinimi,
            Katuosoite = mokki.Katuosoite,
            Kuvaus = mokki.Kuvaus,
            Hinta = mokki.Hinta,
            Henkilomaara = mokki.Henkilomaara,
            Varustelu = mokki.Varustelu,
            Postinro = mokki.Postinro
        };

        SelectedAlue = AlueList.FirstOrDefault(a => a.alue_id == mokki.alue_id);

        IsEditing = true; // <- This makes the button appear
        IsEditing2 = false;
    }

    private async Task UpdateMokkiAsync()
    {
        IsEditing = false;
        IsEditing2 = true;
    if (NewMokki == null || NewMokki.mokki_id <= 0)
        return;

    await _mokkiDatabaseService.UpdateMokkiAsync(NewMokki);
    LoadMokkis();

    // Clear form if needed
    NewMokki = new Mokki();
    SelectedAlue = null;
    }

}