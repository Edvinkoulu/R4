namespace Village_Newbies.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;

public class MokkiViewModel : BindableObject
{
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
                NewMokki.alue_id = _selectedAlue.alue_id;
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
    private bool _isEditing2;
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
        if (NewMokki == null)
            return;

        // You can add validation here before sending data to the service
        if (string.IsNullOrEmpty(NewMokki.Postinro) || string.IsNullOrEmpty(NewMokki.Mokkinimi))
        {
            // Add some user feedback here (e.g., show an alert)
            return;
        }

        // Call the service method to insert the new Mokki
        int rowsAffected = await _mokkiDatabaseService.CreateMokkiAsync(NewMokki);

        // Provide feedback to the user about success or failure
        if (rowsAffected > 0)
        {
            // Successfully added
            NewMokki = new Mokki(); // Reset the form
            OnPropertyChanged(nameof(NewMokki));
        }
        else
        {
            // Error handling: show an alert, log, etc.
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