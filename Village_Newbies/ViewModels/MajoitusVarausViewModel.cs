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
        private readonly VarausDatabaseService _varausService = new VarausDatabaseService();
        private readonly MokkiDatabaseService _mokkiService = new MokkiDatabaseService();
        private readonly AsiakasDatabaseService _asiakasService = new AsiakasDatabaseService();

        public ObservableCollection<Varaus> Varaukset { get; set; } = new();
        public ObservableCollection<Mokki> Mokit { get; set; } = new();
        public ObservableCollection<Asiakas> Asiakkaat { get; set; } = new();

        public ICommand LisaaVarausCommand { get; }
        public ICommand PoistaVarausCommand { get; }

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

        public MajoitusVarausViewModel()
        {
            LisaaVarausCommand = new Command(async () => await LisaaVaraus());
            PoistaVarausCommand = new Command(async () => await PoistaVaraus());

            _ = LataaData();
        }

        private async Task LataaData()
        {
            var varaukset = await _varausService.HaeKaikki();
            Varaukset.Clear();
            foreach (var v in varaukset)
                Varaukset.Add(v);

            var mokit = await _mokkiService.GetAllMokkisAsync();
            Mokit.Clear();
            foreach (var m in mokit)
                Mokit.Add(m);

            var asiakkaat = await _asiakasService.HaeKaikki();
            Asiakkaat.Clear();
            foreach (var a in asiakkaat)
                Asiakkaat.Add(a);
        }

        private async Task LisaaVaraus()
        {
            if (ValittuAsiakas == null || ValittuMokki == null)
                return;

            ValittuVaraus ??= new Varaus();

            ValittuVaraus.asiakas_id = (uint)ValittuAsiakas.asiakas_id;
            ValittuVaraus.mokki_id = (uint)ValittuMokki.mokki_id;
            ValittuVaraus.varattu_pvm = DateTime.Now;
            ValittuVaraus.vahvistus_pvm = DateTime.Now;

            await _varausService.Lisaa(ValittuVaraus);
            await LataaData();
        }

        private async Task PoistaVaraus()
        {
            if (ValittuVaraus != null)
            {
                await _varausService.Poista((int)ValittuVaraus.varaus_id);  // castattu
                await LataaData();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
