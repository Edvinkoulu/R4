namespace Village_Newbies.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Village_Newbies.Models;
    using Village_Newbies.Services;

    public class PalveluViewModel : BindableObject
    {
        private readonly PalveluDatabaseService _palveluDatabaseService;
        private ObservableCollection<Palvelu> _palvelut;
        private ObservableCollection<Alue> _alueList;
        private Palvelu _newPalvelu;

        public Palvelu NewPalvelu
        {
            get => _newPalvelu;
            set
            {
                _newPalvelu = value;
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

        public ObservableCollection<Palvelu> Palvelut
        {
            get => _palvelut;
            set
            {
                _palvelut = value;
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

                // Update NewPalvelu's alue_id when user selects a new Alue
                if (_selectedAlue != null)
                {
                    NewPalvelu.alue_id = (int)_selectedAlue.alue_id; // Cast to int
                    OnPropertyChanged(nameof(NewPalvelu)); // Optional, in case the UI isn't updating
                }
            }
        }

        private Palvelu _selectedPalveluForEdit;
        public Palvelu SelectedPalveluForEdit
        {
            get => _selectedPalveluForEdit;
            set
            {
                _selectedPalveluForEdit = value;
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

        public ICommand AddPalveluCommand { get; }
        public ICommand DeletePalveluCommand { get; }
        public ICommand LoadPalveluForEditCommand { get; }
        public ICommand ConfirmUpdatePalveluCommand { get; }

        public PalveluViewModel()
        {
            _palveluDatabaseService = new PalveluDatabaseService(); // Initialize the service
            NewPalvelu = new Palvelu(); // Create a new instance of Palvelu

            // Define commands
            AddPalveluCommand = new Command(async () => await AddPalveluAsync());
            DeletePalveluCommand = new Command<Palvelu>(async (palvelu) => await DeletePalveluAsync(palvelu));
            LoadPalveluForEditCommand = new Command<Palvelu>((palvelu) => LoadPalveluForEdit(palvelu));
            ConfirmUpdatePalveluCommand = new Command(async () => await UpdatePalveluAsync());
            
            // Load initial data
            LoadAlueList(); // Get all Alue list
            LoadPalvelut();   // Get all Palvelu list
        }

        private async Task LoadAlueList()
        {
            // Fetch all alue records from the database
            var alueList = await _palveluDatabaseService.GetAllAlueAsync();
            AlueList = new ObservableCollection<Alue>(alueList);
        }

        private async void LoadPalvelut()
        {
            // Get all Palvelut from the database
            var palvelut = await _palveluDatabaseService.GetAllPalvelutAsync();
            // Update the ObservableCollection to reflect the changes
            Palvelut = new ObservableCollection<Palvelu>(palvelut);
        }

        private async Task AddPalveluAsync()
        {
            if (NewPalvelu == null)
                return;

            // Validation before adding the service
            if (string.IsNullOrEmpty(NewPalvelu.Nimi) || NewPalvelu.Hinta <= 0)
            {
                // Add some user feedback here (e.g., show an alert)
                return;
            }

            // Call the service method to insert the new Palvelu
            int rowsAffected = await _palveluDatabaseService.CreatePalveluAsync(NewPalvelu);

            // Provide feedback to the user about success or failure
            if (rowsAffected > 0)
            {
                // Successfully added
                NewPalvelu = new Palvelu(); // Reset the form
                OnPropertyChanged(nameof(NewPalvelu));
            }
            else
            {
                // Error handling: show an alert, log, etc.
            }
        }

        private async Task DeletePalveluAsync(Palvelu palvelu)
        {
            if (palvelu == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete '{palvelu.Nimi}'?",
                "Yes",
                "No");

            if (confirm)
            {
                await _palveluDatabaseService.DeletePalveluAsync(palvelu.palvelu_id);
                Palvelut.Remove(palvelu);
            }
        }

        private void LoadPalveluForEdit(Palvelu palvelu)
        {
            if (palvelu == null) return;

            SelectedPalveluForEdit = palvelu;

            // Copy fields from selected palvelu into NewPalvelu
            NewPalvelu = new Palvelu
            {
                palvelu_id = palvelu.palvelu_id,
                alue_id = palvelu.alue_id,
                Nimi = palvelu.Nimi,
                Kuvaus = palvelu.Kuvaus,
                Hinta = palvelu.Hinta,
                Alv = palvelu.Alv
            };

            SelectedAlue = AlueList.FirstOrDefault(a => a.alue_id == palvelu.alue_id);

            IsEditing = true; // This makes the button appear
            IsEditing2 = false;
        }

        private async Task UpdatePalveluAsync()
        {
            IsEditing = false;
            IsEditing2 = true;
            if (NewPalvelu == null || NewPalvelu.palvelu_id <= 0)
                return;

            await _palveluDatabaseService.UpdatePalveluAsync(NewPalvelu);
            LoadPalvelut();

            // Clear form if needed
            NewPalvelu = new Palvelu();
            SelectedAlue = null;
        }
    }
}