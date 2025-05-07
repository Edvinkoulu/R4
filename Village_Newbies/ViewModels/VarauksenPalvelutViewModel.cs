using System.Collections.ObjectModel;
using System.Windows.Input;
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies.ViewModels
{
    public class VarauksenPalvelutViewModel : BindableObject
    {
        private readonly Varauksen_palvelutDatabaseService _vpService;
        private readonly PalveluDatabaseService _palveluService;
        private readonly VarausDatabaseService _varausService;

        public ObservableCollection<Palvelu> Palvelut { get; set; }
        public ObservableCollection<Varaus> Varaukset { get; set; }

        private Palvelu _selectedPalvelu;
        public Palvelu SelectedPalvelu
        {
            get => _selectedPalvelu;
            set
            {
                _selectedPalvelu = value;
                OnPropertyChanged();
            }
        }

        private Varaus _selectedVaraus;
        public Varaus SelectedVaraus
        {
            get => _selectedVaraus;
            set
            {
                _selectedVaraus = value;
                OnPropertyChanged();
            }
        }

        private int _maara = 1;
        public int Maara
        {
            get => _maara;
            set
            {
                _maara = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddPalveluToVarausCommand { get; }

        public VarauksenPalvelutViewModel()
{
    _vpService = new Varauksen_palvelutDatabaseService();
    _palveluService = new PalveluDatabaseService();         // same as in PalveluViewModel
    _varausService = new VarausDatabaseService();           // same as in MajoitusVarausViewModel

    Palvelut = new ObservableCollection<Palvelu>();
    Varaukset = new ObservableCollection<Varaus>();

    AddPalveluToVarausCommand = new Command(async () => await AddPalveluToVarausAsync(), CanAdd);
    
    _ = LoadPalvelutAsync();   // Use same logic as PalveluViewModel
    _ = LoadVarauksetAsync();  // Use same logic as MajoitusVarausViewModel
}

private async Task LoadPalvelutAsync()
{
    var palvelut = await _palveluService.GetAllPalvelutAsync();
    Palvelut.Clear();
    foreach (var p in palvelut)
        Palvelut.Add(p);
}

private async Task LoadVarauksetAsync()
{
    var varaukset = await _varausService.HaeKaikki();
    Varaukset.Clear();
    foreach (var v in varaukset)
        Varaukset.Add(v);
}


        private bool CanAdd()
        {
            return SelectedPalvelu != null && SelectedVaraus != null && Maara > 0;
        }

        private async Task AddPalveluToVarausAsync()
{
    if (!CanAdd()) return;

    var vp = new VarauksenPalvelu
    {
        VarausId = (uint)SelectedVaraus.varaus_id,
        PalveluId = (uint)SelectedPalvelu.palvelu_id,
        Lkm = Maara
    };

    int rowsAffected = await _vpService.CreateAsync(vp);

    if (rowsAffected > 0)
    {
        await Shell.Current.DisplayAlert("Onnistui", "Palvelu lisätty varaukseen.", "OK");
        SelectedPalvelu = null;
        SelectedVaraus = null;
        Maara = 1;
        OnPropertyChanged(nameof(SelectedPalvelu));
        OnPropertyChanged(nameof(SelectedVaraus));
        OnPropertyChanged(nameof(Maara));
    }
    else
        {
        await Shell.Current.DisplayAlert("Virhe", "Lisäys epäonnistui.", "OK");
        }
    }

    }
}
