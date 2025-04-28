using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies.ViewModels
{
    public class RaportointiViewModel : INotifyPropertyChanged
    {
        private readonly PalveluDatabaseService _databaseService;

        public RaportointiViewModel(PalveluDatabaseService dbService)
        {
            _databaseService = dbService;

            LoadAlueetCommand = new Command(async () => await LoadAlueet());
            LoadRaporttiCommand = new Command(async () => await LoadRaportti());

            RaporttiTyypit = new ObservableCollection<string> { "Lisäpalvelut", "Majoittumiset" };
            SelectedRaporttiTyyppi = "Lisäpalvelut";

            ValittuAlkuPvm = DateTime.Today.AddMonths(-1);
            ValittuLoppuPvm = DateTime.Today;

            _ = LoadAlueet();
        }

        public ObservableCollection<string> RaporttiTyypit { get; }

        private string _selectedRaporttiTyyppi;
        public string SelectedRaporttiTyyppi
        {
            get => _selectedRaporttiTyyppi;
            set
            {
                _selectedRaporttiTyyppi = value;
                OnPropertyChanged(nameof(SelectedRaporttiTyyppi));
            }
        }

        private ObservableCollection<Alue> _alueet = new();
        public ObservableCollection<Alue> Alueet
        {
            get => _alueet;
            set
            {
                _alueet = value;
                OnPropertyChanged(nameof(Alueet));
            }
        }

        private Alue? _selectedAlue;
        public Alue? SelectedAlue
        {
            get => _selectedAlue;
            set
            {
                _selectedAlue = value;
                OnPropertyChanged(nameof(SelectedAlue));
            }
        }

        public DateTime ValittuAlkuPvm { get; set; }
        public DateTime ValittuLoppuPvm { get; set; }

        private ObservableCollection<PalveluRaportti> _raportti = new();
        public ObservableCollection<PalveluRaportti> Raportti
        {
            get => _raportti;
            set
            {
                _raportti = value;
                OnPropertyChanged(nameof(Raportti));
            }
        }

        public ICommand LoadAlueetCommand { get; }
        public ICommand LoadRaporttiCommand { get; }

        private async Task LoadAlueet()
        {
            var alueet = await _databaseService.GetAllAlueAsync();
            Alueet = new ObservableCollection<Alue>(alueet);
        }
private async Task LoadRaportti()
{
    try
    {
        if (SelectedAlue == null)
        {
            await Application.Current.MainPage.DisplayAlert("Virhe", "Valitse alue.", "OK");
            return;
        }

        if (SelectedRaporttiTyyppi == "Lisäpalvelut")
        {
            var tulokset = await _databaseService.GetOstetutLisapalvelutRaportti(ValittuAlkuPvm, ValittuLoppuPvm, new List<int> { (int)SelectedAlue.alue_id });
            Raportti = new ObservableCollection<PalveluRaportti>(tulokset);
            if (Raportti.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Ei tuloksia", "Raportilla ei ole tuloksia.", "OK");
            }
        }
        else if (SelectedRaporttiTyyppi == "Majoittumiset")
        {
            await Application.Current.MainPage.DisplayAlert("Info", "Majoittumisten raportti ei ole vielä toteutettu.", "OK");
        }
    }
    catch (Exception ex)
    {
        await Application.Current.MainPage.DisplayAlert("Virhe", $"Tapahtui virhe: {ex.Message}", "OK");
    }
}
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}