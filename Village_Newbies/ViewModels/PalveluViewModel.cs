using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies.ViewModels
{
    public class PalveluViewModel : INotifyPropertyChanged
    {
        private readonly PalveluDatabaseService _databaseService;

        private ObservableCollection<Palvelu> _palvelut = new();
        private ObservableCollection<Alue> _alueList = new();
        private Palvelu? _selectedPalvelu;
        private Palvelu _uusiPalvelu = new();
        private Alue? _selectedAlue;
        private bool _isEditing;
        private string _hakusana = string.Empty;

        public PalveluViewModel(PalveluDatabaseService dbService)
        {
            _databaseService = dbService;

            LoadPalvelutCommand = new Command(async () => await LoadPalvelut());
            AddPalveluCommand = new Command(async () => await AddPalvelu());
            UpdatePalveluCommand = new Command(async () => await UpdatePalvelu());
            DeletePalveluCommand = new Command<Palvelu>(async (palvelu) => await DeletePalvelu(palvelu));
            LoadPalveluForEditCommand = new Command<Palvelu>((palvelu) => LoadPalveluForEdit(palvelu));
            SearchPalvelutCommand = new Command(async () => await SearchPalvelut());
            ClearFormCommand = new Command(ClearForm);

            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            await LoadPalvelut();
            await LoadAlueet();
        }

        public ObservableCollection<Palvelu> Palvelut
        {
            get => _palvelut;
            set
            {
                _palvelut = value;
                OnPropertyChanged(nameof(Palvelut));
            }
        }

        public ObservableCollection<Alue> AlueList
        {
            get => _alueList;
            set
            {
                _alueList = value;
                OnPropertyChanged(nameof(AlueList));
            }
        }

        public Palvelu? SelectedPalvelu
        {
            get => _selectedPalvelu;
            set
            {
                _selectedPalvelu = value;
                OnPropertyChanged(nameof(SelectedPalvelu));
            }
        }

        public Palvelu UusiPalvelu
        {
            get => _uusiPalvelu;
            set
            {
                _uusiPalvelu = value;
                OnPropertyChanged(nameof(UusiPalvelu));
            }
        }

        public Alue? SelectedAlue
        {
            get => _selectedAlue;
            set
            {
                _selectedAlue = value;
                if (_selectedAlue != null)
                {
                    UusiPalvelu.alue_id = (int)_selectedAlue.alue_id;
                }
                OnPropertyChanged(nameof(SelectedAlue));
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged(nameof(IsEditing));
                OnPropertyChanged(nameof(IsAdding));
            }
        }

        public bool IsAdding => !IsEditing;

        public string Hakusana
        {
            get => _hakusana;
            set
            {
                _hakusana = value;
                OnPropertyChanged(nameof(Hakusana));
            }
        }

        public ICommand LoadPalvelutCommand { get; }
        public ICommand AddPalveluCommand { get; }
        public ICommand UpdatePalveluCommand { get; }
        public ICommand DeletePalveluCommand { get; }
        public ICommand LoadPalveluForEditCommand { get; }
        public ICommand SearchPalvelutCommand { get; }
        public ICommand ClearFormCommand { get; }

        private async Task LoadPalvelut()
        {
            var lista = await _databaseService.GetAllPalvelutAsync();
            Palvelut = new ObservableCollection<Palvelu>(lista);
        }

        private async Task LoadAlueet()
        {
            var alueet = await _databaseService.GetAllAlueAsync();
            AlueList = new ObservableCollection<Alue>(alueet);
        }

        private async Task AddPalvelu()
        {
            System.Diagnostics.Debug.WriteLine(
                $"ID: {UusiPalvelu.palvelu_id}, " +
                $"Nimi: {UusiPalvelu.Nimi}, " +
                $"Kuvaus: {UusiPalvelu.Kuvaus}, " +
                $"AlueId: {UusiPalvelu.alue_id}, " +
                $"Hinta: {UusiPalvelu.Hinta}, " +
                $"ALV: {UusiPalvelu.Alv}"
            );

            if (string.IsNullOrWhiteSpace(UusiPalvelu?.Nimi) || UusiPalvelu.Hinta <= 0 || UusiPalvelu.Alv <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe", "Kaikki kentät täytyy täyttää oikein.", "OK");
                return;
            }

            try
            {
                // Yritetään luoda uusi palvelu tietokantaan
                int result = await _databaseService.CreatePalveluAsync(UusiPalvelu);

                // Tarkastetaan, mitä tietokanta palautti
                System.Diagnostics.Debug.WriteLine($"Tietokannan vastaus: {result}");

                if (result > 0)
                {
                    await LoadPalvelut();
                    ClearForm();
                }
                else
                {
                    // Jos vastaus ei ollut odotettu (esim. 0 tai negatiivinen)
                    await Application.Current.MainPage.DisplayAlert("Virhe", "Palvelua ei voitu lisätä.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Jos tulee virhe tietokannan käsittelyssä, tulostetaan se Debug-logiin
                System.Diagnostics.Debug.WriteLine($"Virhe lisäyksessä: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Virhe", $"Tietokantavirhe: {ex.Message}", "OK");
            }
        }

        private void LoadPalveluForEdit(Palvelu palvelu)
        {
            if (palvelu == null) return;

            SelectedPalvelu = palvelu;
            UusiPalvelu = new Palvelu
            {
                palvelu_id = palvelu.palvelu_id,
                alue_id = palvelu.alue_id,
                Nimi = palvelu.Nimi,
                Kuvaus = palvelu.Kuvaus,
                Hinta = palvelu.Hinta,
                Alv = palvelu.Alv
            };

            SelectedAlue = AlueList.FirstOrDefault(a => a.alue_id == palvelu.alue_id);
            IsEditing = true;
        }

        private async Task UpdatePalvelu()
        {
            System.Diagnostics.Debug.WriteLine(
            $"ID: {UusiPalvelu.palvelu_id}, " +
            $"Nimi: {UusiPalvelu.Nimi}, " +
            $"Kuvaus: {UusiPalvelu.Kuvaus}, " +
            $"AlueId: {UusiPalvelu.alue_id}, " +
            $"Hinta: {UusiPalvelu.Hinta}, " +
            $"ALV: {UusiPalvelu.Alv}"
            );


            try
            {
                // Yritetään päivittää palvelu tietokantaan
                int result = await _databaseService.UpdatePalveluAsync(UusiPalvelu);

                if (result > 0)
                {
                    await LoadPalvelut();
                    ClearForm();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Virhe", "Palvelun päivittäminen epäonnistui.", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Virhe muokkauksessa: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Virhe", $"Tietokantavirhe: {ex.Message}", "OK");
            }
        }

        private async Task DeletePalvelu(Palvelu palvelu)
        {
            if (palvelu == null)
                return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Vahvista poisto",
                $"Haluatko varmasti poistaa palvelun '{palvelu.Nimi}'?",
                "Kyllä", "Peruuta");

            if (confirm)
            {
                try
                {
                    await _databaseService.DeleteViittauksetPalveluista(palvelu.palvelu_id);
                    await _databaseService.DeletePalveluAsync(palvelu.palvelu_id);
                    await LoadPalvelut();

                    await Application.Current.MainPage.DisplayAlert("Poisto onnistui",
                        $"Palvelu '{palvelu.Nimi}' on poistettu.", "OK");
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Virhe",
                        $"Poisto epäonnistui: {ex.Message}", "OK");
                }
            }
        }

        private async Task SearchPalvelut()
        {
            var kaikki = await _databaseService.GetAllPalvelutAsync();

            if (string.IsNullOrWhiteSpace(Hakusana))
            {
                Palvelut = new ObservableCollection<Palvelu>(kaikki);
            }
            else
            {
                var suodatetut = kaikki
                    .Where(p => p.Nimi.Contains(Hakusana, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                Palvelut = new ObservableCollection<Palvelu>(suodatetut);
            }
        }

        private void ClearForm()
        {
            UusiPalvelu = new Palvelu();
            SelectedAlue = null;
            IsEditing = false;
            Hakusana = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}