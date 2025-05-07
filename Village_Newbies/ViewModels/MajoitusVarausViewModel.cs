using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;
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

        public ICommand LisaaVarausCommand { get; }
        public ICommand PoistaVarausCommand { get; }
        public ICommand PaivitaVarausCommand { get; }

        public MajoitusVarausViewModel()
        {
            LisaaVarausCommand = new Command(async () => await LisaaVaraus());
            PoistaVarausCommand = new Command(async () => await PoistaVaraus());
            PaivitaVarausCommand = new Command(async () => await PaivitaVaraus());
            _ = LataaData();
        }

        private Varaus? _valittuVaraus;
        public Varaus? ValittuVaraus
        {
            get => _valittuVaraus;
            set
            {
                _valittuVaraus = value;
                OnPropertyChanged(nameof(ValittuVaraus));
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

        private async Task LisaaVaraus()
        {
            if (ValittuAsiakas == null || ValittuMokki == null)
                return;

            var uusi = new Varaus
            {
                asiakas_id = (uint)ValittuAsiakas.asiakas_id,
                mokki_id = (uint)ValittuMokki.mokki_id,
                varattu_pvm = DateTime.Now,
                vahvistus_pvm = DateTime.Now,
                varattu_alkupvm = ValittuVaraus?.varattu_alkupvm ?? DateTime.Now,
                varattu_loppupvm = ValittuVaraus?.varattu_loppupvm ?? DateTime.Now.AddDays(1)
            };

            await _varausService.Lisaa(uusi);
            await LataaData();
        }

        private async Task PaivitaVaraus()
        {
            if (ValittuVaraus == null || ValittuAsiakas == null || ValittuMokki == null)
                return;

            ValittuVaraus.asiakas_id = (uint)ValittuAsiakas.asiakas_id;
            ValittuVaraus.mokki_id = (uint)ValittuMokki.mokki_id;

            await _varausService.Muokkaa(ValittuVaraus);
            await LataaData();
        }

        private async Task PoistaVaraus()
        {
            if (ValittuVaraus != null)
            {
                await _varausService.Poista((int)ValittuVaraus.varaus_id);
                await LataaData();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
