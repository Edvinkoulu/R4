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

        public ObservableCollection<Varaus> Varaukset { get; set; } = new();

        public ICommand HaeKaikkiVarauksetCommand { get; }
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

        public MajoitusVarausViewModel()
        {
            HaeKaikkiVarauksetCommand = new Command(async () => await HaeKaikkiVaraukset());
            PoistaVarausCommand = new Command(async () => await PoistaValittuVaraus());
            _ = HaeKaikkiVaraukset();
        }

        private async Task HaeKaikkiVaraukset()
        {
            var lista = await _varausService.HaeKaikki();
            Varaukset.Clear();
            foreach (var v in lista)
                Varaukset.Add(v);
        }

        private async Task PoistaValittuVaraus()
        {
            if (ValittuVaraus != null)
            {
                await _varausService.Poista((int)ValittuVaraus.varaus_id);
                await HaeKaikkiVaraukset();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
