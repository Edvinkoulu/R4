namespace Village_Newbies.ViewModels;

using Village_Newbies.Models;
using Village_Newbies.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Linq;
using System;

// Laskujen hallinnassa ei voi tehdä uusia laskuja toistaiseksi.
// Laskujen lisääminen kuitenkin pitäisi onnistua laskudatabase servicen kautta tarvittaessa.

public class LaskuViewModel : INotifyPropertyChanged
{
    // =========================================
    // Kentät ja konstruktorit
    // =========================================
    private readonly LaskuDatabaseService _laskuDb = new();
    private readonly MokkiDatabaseService _mokkiDb = new();
    private readonly AlueDatabaseService _alueDb = new();

    public ObservableCollection<Lasku> Laskut { get; } = new();
    private ObservableCollection<Lasku> _kaikkiLaskut = new(); 
    public ObservableCollection<Varaus> Varaukset { get; } = new();
    public ObservableCollection<Alue> Alueet { get; } = new();
    public ObservableCollection<Mokki> Mokit { get; } = new();
    public ObservableCollection<Asiakas> Asiakkaat { get; } = new();
    public LaskuViewModel()
    {
        HaeDataCommand = new Command(async () => await LoadData());
        TallennaLaskuCommand = new Command(async () => await SaveLasku());
        PoistaLaskuCommand = new Command<Lasku>(async l => await DeleteLasku(l));
        ClearValittuLaskuCommand = new Command(() => ValittuLasku = null);
        HaeSuodatetutLaskutCommand = new Command(async () => await HaeSuodatetutLaskut());
        ValitseLaskuCommand = new Command<Lasku>(l => ValittuLasku = l);
        TyhjennaValinnatCommand = new Command(async () => await TyhjennaValinnat());
        SuodataMaksamattomatCommand = new Command(SuodataMaksamattomat);
    }
    public async Task InitializeAsync()
    {
        await LoadData();
    } 
    // =========================================
    // ========= Propertyt: Valinnat =========== Nämä siis pitävät kirjaa mitä käyttäjä on valinnut käyttöliittymästä
    // =========================================
    private Alue _selectedAlue;
    public Alue ValittuAlue
    {
        get => _selectedAlue;
        set => SetProperty(ref _selectedAlue, value, async () => await HaeMokit());
    }
    private Mokki _selectedMokki;
    public Mokki ValittuMokki
    {
        get => _selectedMokki;
        set => SetProperty(ref _selectedMokki, value);
    }
    private Asiakas _selectedAsiakas;
    public Asiakas ValittuAsiakas
    {
        get => _selectedAsiakas;
        set => SetProperty(ref _selectedAsiakas, value);
    }
    private Lasku _selectedLasku;
    public Lasku ValittuLasku
    {
        get => _selectedLasku;
        set => SetProperty(ref _selectedLasku, value, () =>
        {
            IsEditing = value != null;
            UpdateInputFromValittuLasku();
        });
    }
    private Varaus _selectedVaraus;
    public Varaus ValittuVaraus
    {
        get => _selectedVaraus;
        set => SetProperty(ref _selectedVaraus, value, UpdateLaskuDisplayProperties);
    }
    // =========================================
    // ========== Propertyt: Syötteet ========== Näitä käytetään tietojen muokkaamiseen
    // =========================================

    public int UusiVarausId
    {
        get => _uusiVarausId;
        set => SetProperty(ref _uusiVarausId, value);
    }
    public double UusiSumma
    {
        get => _uusiSumma;
        set => SetValidatedProperty(ref _uusiSumma, value, nameof(UusiSumma));
    }
    public double UusiAlv
    {
        get => _uusiAlv;
        set => SetValidatedProperty(ref _uusiAlv, value, nameof(UusiAlv));
    }
    public bool UusiMaksettu
    {
        get => _uusiMaksettu;
        set => SetValidatedProperty(ref _uusiMaksettu, value, nameof(UusiMaksettu));
    }
    private int _uusiVarausId;
    private double _uusiSumma;
    private double _uusiAlv;
    private bool _uusiMaksettu;
    // =========================================
    // ========= Propertyt: Näyttö ============= Infoa käyttäjälle
    // =========================================
    public string VarausInfo => ValittuVaraus == null ? "" : $"Varaus: {ValittuVaraus.varaus_id}, Asiakas: {VarausAsiakas}, Mökki: {VarausMokki}, {VarausPvm}";

    public string VarausAsiakas
    {
        get => _varausAsiakas;
        private set => SetProperty(ref _varausAsiakas, value);
    }
    public string VarausMokki
    {
        get => _varausMokki;
        private set => SetProperty(ref _varausMokki, value);
    }
    public string VarausPvm
    {
        get => _varausPvm;
        private set => SetProperty(ref _varausPvm, value);
    }
    public string MokkiNimi
    {
        get
        {
            if (ValittuVaraus == null || ValittuVaraus.mokki_id == null)
                return "Ei mökkiä";

            var mokki = Mokit.FirstOrDefault(m => m.mokki_id == ValittuVaraus.mokki_id);
            return mokki != null ? mokki.Mokkinimi : "Ei mökkiä";
        }
    }
    private string _varausAsiakas = "Tyhjä";
    private string _varausMokki = "Tyhjä";
    private string _varausPvm = "Tyhjä";
    // =========================================
    // ========= Propertyt: Validointi========== Jos käyttäjä tekee virheellisiä syötteitä, nämä sitten tekevät virheilmoituksen käyttöliittymään
    // =========================================
    public bool CanSave => ValittuVaraus != null && ValidateSumma() && ValidateAlv();
    public bool IsNotEditing => !IsEditing;
    public string VarausIdVirhe
    {
        get => _varausIdVirhe;
        private set => SetProperty(ref _varausIdVirhe, value);
    }
    public string SummaVirhe
    {
        get => _summaVirhe;
        private set => SetProperty(ref _summaVirhe, value);
    }
    public string AlvVirhe
    {
        get => _alvVirhe;
        private set => SetProperty(ref _alvVirhe, value);
    }
    private string _varausIdVirhe = "";
    private string _summaVirhe = "";
    private string _alvVirhe = "";
    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        private set => SetProperty(ref _isEditing, value);
    }
    // =========================================
    // ============= Komennot ==================
    // =========================================
    public ICommand HaeDataCommand { get; }
    public ICommand TallennaLaskuCommand { get; }
    public ICommand PoistaLaskuCommand { get; }
    public ICommand ClearValittuLaskuCommand { get; }
    public ICommand HaeSuodatetutLaskutCommand { get; }
    public ICommand ValitseLaskuCommand { get; }
    public ICommand TyhjennaValinnatCommand { get; }
    public ICommand SuodataMaksamattomatCommand { get; }
    // =========================================
    // ========= Tietojen haku ================= Nämä metodit käyttää databaseservicejä tietojen hakemiseen
    // =========================================
    private async Task LoadData()
    {
        try
        {
            await Task.WhenAll(
                Load(Alueet, _alueDb.HaeKaikki),
                Load(_kaikkiLaskut, _laskuDb.HaeKaikki), 
                Load(Varaukset, _laskuDb.HaeKaikkiVaraukset)
            );
            PaivitaNaytettavatLaskut(_kaikkiLaskut.ToList());
            await HaeMokit();
            await HaeAsiakkaat();
        }
        catch (Exception ex)
        {
            await ShowAlert("Virhe ladattaessa tietoja", ex.Message);
        }
    }
    private async Task Load<T>(ObservableCollection<T> collection, Func<Task<List<T>>> fetch)
    {
        collection.Clear();
        (await fetch()).ForEach(collection.Add);
    }
    private async Task HaeMokit()
    {
        Mokit.Clear();
        var mokit = ValittuAlue != null
            ? await _mokkiDb.GetAllMokkiInAlue((int)ValittuAlue.alue_id)
            : await _mokkiDb.GetAllMokkisAsync();

        foreach (var mokki in mokit.OrderBy(m => m.Mokkinimi))
            Mokit.Add(mokki);
    }
    private async Task HaeAsiakkaat()
    {
        Asiakkaat.Clear();
        var asiakkaat = await _laskuDb.HaeKaikkiAsiakkaat();
        foreach (var asiakas in asiakkaat.OrderBy(a => a.sukunimi))
            Asiakkaat.Add(asiakas);
    }
    private async Task HaeSuodatetutLaskut()
    {
        try
        {
            var suodatetut = await _laskuDb.HaeSuodatetutLaskut(
                alueId: (int?)ValittuAlue?.alue_id,
                mokkiId: ValittuMokki?.mokki_id,
                asiakasId: (int?)ValittuAsiakas?.asiakas_id
            );

            Laskut.Clear();
            foreach (var lasku in suodatetut)
                Laskut.Add(lasku);
        }
        catch (Exception ex)
        {
            await ShowAlert("Virhe suodatuksessa", ex.Message);
        }
    }    
    private void PaivitaNaytettavatLaskut(List<Lasku> laskut)
    {
        Laskut.Clear();
        foreach (var lasku in laskut)
        {
            Laskut.Add(lasku);
        }
    }
    private void SuodataMaksamattomat()
    {
        var maksamattomat = _kaikkiLaskut.Where(l => !l.maksettu).ToList();
        PaivitaNaytettavatLaskut(maksamattomat);
    }
    // =========================================
    // ============== Tapahtumat =============== Näitä käytetään käyttöliittymän päivittämiseen kun sitä tarvitaan
    // =========================================
    private async Task TyhjennaValinnat()
    {
        ValittuAlue = null;
        ValittuMokki = null;
        ValittuAsiakas = null;
        ValittuLasku = null;
        Laskut.Clear();
        await LoadData();
    }
    private void UpdateInputFromValittuLasku()
    {
        if (ValittuLasku == null)
        {
            UusiVarausId = 0;
            UusiSumma = 0;
            UusiAlv = 0;
            UusiMaksettu = false;
            ValittuVaraus = null;
        }
        else
        {
            UusiVarausId = (int)ValittuLasku.varaus_id;
            UusiSumma = ValittuLasku.summa;
            UusiAlv = ValittuLasku.alv;
            UusiMaksettu = ValittuLasku.maksettu;
            ValittuVaraus = Varaukset.FirstOrDefault(v => v.varaus_id == ValittuLasku.varaus_id);
        }
    }
    private async void UpdateLaskuDisplayProperties()
    {
        if (ValittuVaraus == null)
        {
            VarausAsiakas = VarausMokki = VarausPvm = "Tyhjä";
            return;
        }
        var asiakas = Asiakkaat.FirstOrDefault(a => a.asiakas_id == ValittuVaraus.asiakas_id);
        VarausAsiakas = asiakas != null ? $"{asiakas.etunimi} {asiakas.sukunimi}" : $"ID: {ValittuVaraus.asiakas_id}";

        var mokki = Mokit.FirstOrDefault(m => m.mokki_id == ValittuVaraus.mokki_id)
                    ?? await _mokkiDb.GetMokkiByIdAsync((int)ValittuVaraus.mokki_id);

        VarausMokki = mokki?.Mokkinimi ?? $"ID: {ValittuVaraus.mokki_id}";
        if (mokki != null && !Mokit.Any(m => m.mokki_id == mokki.mokki_id)) Mokit.Add(mokki);

        VarausPvm = ValittuVaraus.varattu_alkupvm != DateTime.MinValue
            ? $"Ajanjakso: {ValittuVaraus.varattu_alkupvm:d} - {ValittuVaraus.varattu_loppupvm:d}"
            : "Päivämäärätiedot puuttuvat";
    }
    // =========================================
    // ========== Tietokantaoperaatiot ========= Kommunikoi databaseservicen kanssa, välittää käyttäjän syötteet
    // ========================================= Luo uusia lasku olioita

    private async Task SaveLasku()
    {
        if (!CanSave)
        {
            ValidateAll();
            await ShowAlert("Virhe", "Tarkista kentät.");
            return;
        }

        try
        {
            var lasku = new Lasku(ValittuLasku?.lasku_id ?? 0, ValittuVaraus.varaus_id, UusiSumma, UusiAlv, UusiMaksettu);
            if (IsEditing) await _laskuDb.Muokkaa(lasku);
            else await _laskuDb.Lisaa(lasku);

            await LoadData();
            ValittuLasku = null;
        }
        catch (Exception ex)
        {
            await ShowAlert("Virhe tallennuksessa", ex.Message);
        }
    }

    private async Task DeleteLasku(Lasku lasku)
    {
        if (lasku == null) return;

        try
        {
            await _laskuDb.Poista((int)lasku.lasku_id);
            await LoadData();
        }
        catch (Exception ex)
        {
            await ShowAlert("Virhe poistossa", ex.Message);
        }
    }

    // =========================================
    // =========== Validointi ==================
    // =========================================

    private void ValidateAll()
    {
        _ = ValidateVarausId();
        _ = ValidateSumma();
        _ = ValidateAlv();
    }

    private bool ValidateVarausId()
    {
        try { SyoteValidointi.TarkistaInt(UusiVarausId, 1, int.MaxValue); VarausIdVirhe = ""; return true; }
        catch (Exception ex) { VarausIdVirhe = ex.Message; return false; }
    }

    private bool ValidateSumma()
    {
        try { SyoteValidointi.TarkistaDouble(UusiSumma, 0, double.MaxValue); SummaVirhe = ""; return true; }
        catch (Exception ex) { SummaVirhe = ex.Message; return false; }
    }

    private bool ValidateAlv()
    {
        try { SyoteValidointi.TarkistaDouble(UusiAlv, 0, 100); AlvVirhe = ""; return true; }
        catch (Exception ex) { AlvVirhe = ex.Message; return false; }
    }

    // =========================================
    // ======== Apu- ja ilmoitusmetodit ========
    // =========================================

    private async Task ShowAlert(string title, string message) =>
        await Application.Current.MainPage.DisplayAlert(title, message, "OK");

    private void SetProperty<T>(ref T storage, T value, Action onChanged = null, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return;
        storage = value;
        onChanged?.Invoke();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetValidatedProperty<T>(ref T storage, T value, string propertyName)
    {
        if (!EqualityComparer<T>.Default.Equals(storage, value))
        {
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSave)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}