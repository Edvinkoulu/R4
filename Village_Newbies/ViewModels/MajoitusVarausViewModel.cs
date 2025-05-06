using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;
using Microsoft.Maui.Controls;

namespace Village_Newbies.ViewModels
{
    public class MajoitusVarausViewModel : INotifyPropertyChanged
    {
        private readonly VarausDatabaseService _varausService = new VarausDatabaseService();
        private readonly AsiakasDatabaseService _asiakasService = new AsiakasDatabaseService();
        private readonly MokkiDatabaseService _mokkiService = new MokkiDatabaseService();

        public ObservableCollection<Varaus> Varaukset { get; set; } = new();
        public ObservableCollection<Asiakas> Asiakkaat { get; set; } = new();
        public ObservableCollection<Mokki> Mokit { get; set; } = new();

        public ICommand LisaaVarausCommand { get; }
        public ICommand PoistaVarausCommand { get; }
        public ICommand MuokkaaVarausCommand { get; }

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

        public MajoitusVarausViewModel()
        {
            LisaaVarausCommand = new Command(async () => await LisaaVaraus());
            PoistaVarausCommand = new Command(async () => await PoistaVaraus());
            MuokkaaVarausCommand = new Command(async () => await MuokkaaVaraus());

            _ = LataaData();
        }

        private async Task LataaData()
        {
            // Lataa varaukset
            var varaukset = await _varausService.HaeKaikki();
            Varaukset.Clear();
            foreach (var v in varaukset)
                Varaukset.Add(v);

            // Lataa asiakkaat
            var asiakkaat = await _asiakasService.HaeKaikki();
            Asiakkaat.Clear();
            foreach (var a in asiakkaat)
                Asiakkaat.Add(a);

            // Lataa mökit
            var mokit = await _mokkiService.GetAllMokkisAsync();
            Mokit.Clear();
            foreach (var m in mokit)
                Mokit.Add(m);
        }

        private async Task LisaaVaraus()
        {
            if (ValittuAsiakas == null || ValittuMokki == null || ValittuVaraus == null)
                return;

            ValittuVaraus.asiakas_id = (uint)ValittuAsiakas.asiakas_id;
            ValittuVaraus.mokki_id = (uint)ValittuMokki.mokki_id;
            ValittuVaraus.varattu_pvm = DateTime.Now;
            ValittuVaraus.vahvistus_pvm = DateTime.Now;

            await _varausService.Lisaa(ValittuVaraus);
            await LataaData();
        }

        private async Task PoistaVaraus()
        {
            if (ValittuVaraus == null) return;
            await _varausService.Poista((int)ValittuVaraus.varaus_id);
            await LataaData();
        }

        private async Task MuokkaaVaraus()
        {
            if (ValittuVaraus == null) return;
            await _varausService.Muokkaa(ValittuVaraus);
            await LataaData();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
