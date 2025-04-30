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
        private readonly MokkiDatabaseService _mokkiDatabaseService = new();
        public ObservableCollection<MajoitusRaportti> MajoitusRaporttiLista { get; } = new();
        private string _hakuehdotYhteenveto = string.Empty;
        private string _raporttiLaskuri;
        private bool isLisapalvelutVisible;
        private bool isMajoittumisetVisible;

        public bool IsLisapalvelutVisible
        {
            get => isLisapalvelutVisible;
            set
            {
                isLisapalvelutVisible = value;
                OnPropertyChanged(nameof(IsLisapalvelutVisible));  
            }
        }

        public bool IsMajoittumisetVisible
        {
            get => isMajoittumisetVisible;
            set
            {
                isMajoittumisetVisible = value;
                OnPropertyChanged(nameof(IsMajoittumisetVisible));  
            }
        }

        public string RaporttiLaskuri
        {
            get => _raporttiLaskuri;
            set
            {
                _raporttiLaskuri = value;
                OnPropertyChanged(nameof(RaporttiLaskuri));  
            }
        }

        public string HakuehdotYhteenveto
        {
            get => _hakuehdotYhteenveto;
            set
            {
                _hakuehdotYhteenveto = value;
                OnPropertyChanged(nameof(HakuehdotYhteenveto)); 
            }
        }

        private async Task LataaRaporttiAsync()
        {
            MajoitusRaporttiLista.Clear();

            if (SelectedRaporttiTyyppi == "Majoittumiset" && SelectedAlue != null)
            {
                var raportti = await _mokkiDatabaseService.HaeMajoitusRaportti(ValittuAlkuPvm, ValittuLoppuPvm, (int)SelectedAlue.alue_id);
                foreach (var item in raportti)
                {
                    MajoitusRaporttiLista.Add(item);
                }
            }
        }

        public RaportointiViewModel(PalveluDatabaseService dbService)
        {
            _databaseService = dbService;

            LoadAlueetCommand = new Command(async () => await LoadAlueet());
            LoadRaporttiCommand = new Command(async () => await LoadRaportti());

            RaporttiTyypit = new ObservableCollection<string> { "Lisäpalvelut", "Majoittumiset" };
            SelectedRaporttiTyyppi = "Lisäpalvelut";

            ValittuAlkuPvm = DateTime.Today;
            ValittuLoppuPvm = DateTime.Today.AddMonths(+4);

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
                OnPropertyChanged(nameof(SelectedRaporttiTyyppi));  // Käytetään oikeaa propertyn nimeä
            }
        }

        private ObservableCollection<Alue> _alueet = new();
        public ObservableCollection<Alue> Alueet
        {
            get => _alueet;
            set
            {
                _alueet = value;
                OnPropertyChanged(nameof(Alueet));  // Käytetään oikeaa propertyn nimeä
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
            HakuehdotYhteenveto = $"Raportti: {SelectedRaporttiTyyppi}, ajanjaksolla: {ValittuAlkuPvm:dd.MM.yyyy} – {ValittuLoppuPvm:dd.MM.yyyy}, Alueella: {SelectedAlue?.alue_nimi}";

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
                    IsLisapalvelutVisible = true;
                    IsMajoittumisetVisible = false;

                    var maara = Raportti.Count;
                    HakuehdotYhteenveto = $"Raportti: {SelectedRaporttiTyyppi}, ajanjaksolla: {ValittuAlkuPvm:dd.MM.yyyy} – {ValittuLoppuPvm:dd.MM.yyyy}, Alueella: {SelectedAlue?.alue_nimi}, Määrä: {maara}";

                    if (maara == 0)
                    {
                        await Application.Current.MainPage.DisplayAlert("Ei tuloksia", "Raportilla ei ole tuloksia.", "OK");
                    }
                }
                else if (SelectedRaporttiTyyppi == "Majoittumiset")
                {
                    await LataaRaporttiAsync();
                    IsMajoittumisetVisible = true;
                    IsLisapalvelutVisible = false;

                    var maara = MajoitusRaporttiLista.Count;
                    HakuehdotYhteenveto = $"Raportti: {SelectedRaporttiTyyppi}, ajanjaksolla: {ValittuAlkuPvm:dd.MM.yyyy} – {ValittuLoppuPvm:dd.MM.yyyy}, Alueella: {SelectedAlue?.alue_nimi}, Määrä: {maara}";

                    if (maara == 0)
                    {
                        await Application.Current.MainPage.DisplayAlert("Ei tuloksia", "Raportilla ei ole tuloksia.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe", $"Tapahtui virhe: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Muutettu OnPropertyChanged-metodia niin, että se saa parametrin, joka kertoo mikä property on muuttunut
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}