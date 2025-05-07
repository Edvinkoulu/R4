using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;
using MySqlConnector;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Village_Newbies.ViewModels
{
    public class MajoitusVarausViewModel : INotifyPropertyChanged
    {
        private readonly VarausDatabaseService _varausService = new();
        private readonly MokkiDatabaseService _mokkiService = new();
        private readonly AsiakasDatabaseService _asiakasService = new();

        public ObservableCollection<Varaus> Varaukset { get; set; } = new();
        public ObservableCollection<Mokki> Mokit { get; set; } = new();
        public ObservableCollection<Asiakas> Asiakkaat { get; set; } = new();

        public ICommand ValitseVarausCommand { get; }
        public ICommand LisaaVarausCommand { get; }
        public ICommand PoistaVarausCommand { get; }
        public ICommand PaivitaVarausCommand { get; }

        public MajoitusVarausViewModel()
        {
            ValittuAlkuPv = DateTime.Now.Date;
            ValittuLoppuPv = DateTime.Now.Date.AddDays(1);
            ValitseVarausCommand = new Command<object>(async (varaus) => await ValitseVaraus(varaus));
            LisaaVarausCommand = new Command(async () => await LisaaVaraus());
            PoistaVarausCommand = new Command(async (varaus) => await PoistaVaraus(varaus));
            PaivitaVarausCommand = new Command(async () => await PaivitaVaraus());
            _ = LataaData();
        }

        private Varaus? _valittuVaraus;
        public Varaus? ValittuVaraus
        {
            get => _valittuVaraus;
            set
            {
                if (_valittuVaraus != value)
                {
                    _valittuVaraus = value;
                    OnPropertyChanged(nameof(ValittuVaraus));

                    if (_valittuVaraus != null)
                    {
                        ValittuAlkuPv = _valittuVaraus.varattu_alkupvm.Date;
                        ValittuLoppuPv = _valittuVaraus.varattu_loppupvm.Date;

                        ValittuAsiakas = Asiakkaat.FirstOrDefault(a => a.asiakas_id == _valittuVaraus.asiakas_id);
                        ValittuMokki = Mokit.FirstOrDefault(m => m.mokki_id == _valittuVaraus.mokki_id);
                    }
                    else // Resetoi UI jos valittu varaus on null
                    {
                        ValittuAlkuPv = DateTime.Now.Date;
                        ValittuLoppuPv = DateTime.Now.Date.AddDays(1);
                        ValittuAsiakas = null;
                        ValittuMokki = null;
                    }
                }
            }
        }
        
        private Mokki? _valittuMokki;
        public Mokki? ValittuMokki
        {
            get => _valittuMokki;
            set
            {
                _valittuMokki = value;
                OnPropertyChanged(nameof(ValittuMokki));
            }
        }

        private Asiakas? _valittuAsiakas;
        public Asiakas? ValittuAsiakas
        {
            get => _valittuAsiakas;
            set
            {
                _valittuAsiakas = value;
                OnPropertyChanged(nameof(ValittuAsiakas));
            }
        }
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

        private async Task LataaData()
        {
            Varaukset.Clear();
            foreach (var v in await _varausService.HaeKaikki())
                Varaukset.Add(v);

            Mokit.Clear();
            foreach (var m in await _mokkiService.GetAllMokkisAsync())
                Mokit.Add(m);

            Asiakkaat.Clear();
            foreach (var a in await _asiakasService.HaeKaikki())
                Asiakkaat.Add(a);
        }
        private async Task ValitseVaraus(object _valittuVaraus)
        {
            if (_valittuVaraus is Varaus varaus)
            {
                ValittuVaraus = varaus;
            }
        }
        private async Task LisaaVaraus()
        {
            if (ValittuAsiakas == null || ValittuMokki == null)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe lisättäessä varausta", "Valitse asiakas ja mökki.", "OK");
            }
            else if (ValittuLoppuPv <= ValittuAlkuPv)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe lisättäessä varausta", "Loppupäivämäärän on oltava alkupäivämäärää myöhempi.", "OK");
            }
            else
            {
                var uusi = new Varaus
                {
                    asiakas_id = ValittuAsiakas.asiakas_id,
                    mokki_id = (uint)ValittuMokki.mokki_id,
                    varattu_pvm = DateTime.Now,
                    vahvistus_pvm = DateTime.Now,
                    varattu_alkupvm = ValittuAlkuPv,
                    varattu_loppupvm = ValittuLoppuPv
                };
                await _varausService.Lisaa(uusi);
                await LataaData();
            }
        }

        private async Task PaivitaVaraus()
        {
            if (ValittuVaraus == null || ValittuAsiakas == null || ValittuMokki == null)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe muokattaessa varausta", "Valitse varaus, asiakas ja mökki.", "OK");
                return;
            }
            else if (ValittuLoppuPv <= ValittuAlkuPv)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe muokattaessa varausta", "Loppupäivämäärän on oltava alkupäivämäärää myöhempi.", "OK");
                return;
            }
            else
            {
                ValittuVaraus.asiakas_id = ValittuAsiakas.asiakas_id;
                ValittuVaraus.mokki_id = (uint)ValittuMokki.mokki_id;
                ValittuVaraus.varattu_alkupvm = ValittuAlkuPv;
                ValittuVaraus.varattu_loppupvm = ValittuLoppuPv;
                await _varausService.Muokkaa(ValittuVaraus);
                await LataaData();
            }
        }

        private async Task PoistaVaraus(object _poistettavaVaraus)
        {
            if (_poistettavaVaraus is not Varaus varaus) return;

            bool vahvistus = await Application.Current.MainPage.DisplayAlert(
                "Varmista poisto",
                $"Haluatko varmasti poistaa varauksen {varaus.varaus_id}?",
                "Kyllä",
                "Ei"
            );

            if (!vahvistus) return;
            try
            {
                await _varausService.Poista((int)varaus.varaus_id);
                await LataaData();
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe","Varaukseen liittyy muita tietoja (esim. laskuja tai palveluita), eikä sitä voida poistaa.","OK"
                );
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Virhe",$"Varauksen poisto epäonnistui: {ex.Message}","OK"
                );
            }

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
