namespace Village_Newbies.ViewModels;

using Village_Newbies.Models;
using Village_Newbies.Services;
using Village_Newbies.Interfacet;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using System.Globalization;
using System.Linq;
using System;

// Laskujen hallinnassa ei voi tehdä uusia laskuja. Lasku pitää tehdä asiakasvarauksen täytön yhteydessä
// Laskujen lisääminen onnistuu laskudatabasea hyödyntämällä.

public class LaskuViewModel : INotifyPropertyChanged, ILaskuViewModel
{
    // =========================================
    // Kentät ja konstruktorit
    // =========================================
    private readonly LaskuDatabaseService _laskuDb = new();
    private readonly MokkiDatabaseService _mokkiDb = new();
    private readonly AlueDatabaseService _alueDb = new();
    private readonly VarausDatabaseService _varausDb = new();
    private readonly LaskutusService _laskutusService;

    public ObservableCollection<Lasku> Laskut { get; } = new();
    private ObservableCollection<Lasku> _kaikkiLaskut = new();
    public ObservableCollection<Varaus> Varaukset { get; } = new();
    public ObservableCollection<Alue> Alueet { get; } = new();
    public ObservableCollection<Mokki> Mokit { get; } = new();
    public ObservableCollection<Asiakas> Asiakkaat { get; } = new();
    private ObservableCollection<Asiakas> _kaikkiAsiakkaat = new();
    private ObservableCollection<Palvelu> _laskunPalvelut = new();
    public LaskuViewModel()
    {
        _laskutusService = new LaskutusService(_laskuDb);
        InitializeCommands();
    }
    public async Task InitializeAsync()
    {
        await LoadData();
    }
    private void InitializeCommands()
    {
        HaeDataCommand = new Command(async () => await LoadData());
        TallennaLaskuCommand = new Command(async () => await SaveLasku());
        PoistaLaskuCommand = new Command<Lasku>(async l => await DeleteLasku(l));
        ClearValittuLaskuCommand = new Command(() => ValittuLasku = null);
        HaeSuodatetutLaskutCommand = new Command(async () => await HaeSuodatetutLaskut());
        ValitseLaskuCommand = new Command<Lasku>(l => ValittuLasku = l);
        TyhjennaValinnatCommand = new Command(async () => await TyhjennaValinnat());
        SuodataMaksamattomatCommand = new Command(SuodataMaksamattomat);
        TulostaLaskuCommand = new Command(async () => await KasitteleLasku(false, false));
        LahetaLaskuEmailillaCommand = new Command(async () => await KasitteleLasku(false, true));
        TulostaMaksumuistutusCommand = new Command(async () => await KasitteleLasku(true, false));
        LahetaMaksumuistutusEmailillaCommand = new Command(async () => await KasitteleLasku(true, true));
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
        set => SetProperty(ref _selectedMokki, value, async () => await PaivitaAsiakasListaa());

    }
    private Asiakas _selectedAsiakas;
    public Asiakas ValittuAsiakas
    {
        get => _selectedAsiakas;
        set => SetProperty(ref _selectedAsiakas, value);
    }
    public Lasku ValittuLasku
    {
        get => _selectedLasku;
        set => SetProperty(ref _selectedLasku, value, async () =>
        {
            IsEditing = value != null;
            UpdateInputFromValittuLasku();
            OnkoMuutoksia = false;
            ValittuVaraus = Varaukset.FirstOrDefault(v => v.varaus_id == value?.varaus_id);

            if (ValittuVaraus?.mokki_id != null)
            {
                var mokkiId = (int)ValittuVaraus.mokki_id;
                if (!Mokit.Any(m => m.mokki_id == mokkiId))
                {
                    var mokki = await _mokkiDb.GetMokkiByIdAsync(mokkiId);
                    if (mokki != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => Mokit.Add(mokki));
                    }
                }
            }
            if (value != null)
            {
                LaskunPalvelut.Clear();
                var palvelut = await _laskuDb.HaeLaskunPalvelut(value);
                foreach (var palvelu in palvelut)
                {
                    LaskunPalvelut.Add(palvelu);
                }
            }
            else
            {
                LaskunPalvelut.Clear();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PalveluidenSummaIlmanVeroa)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KokonaissummaVerojenKanssa)));

        });
    }
    private void UpdateInputFromValittuLasku()
    {
        if (ValittuLasku != null)
        {
            UusiVarausId = (int)ValittuLasku.varaus_id;
            UusiSumma = ValittuLasku.summa;
            UusiAlv = ValittuLasku.alv;
            UusiMaksettu = ValittuLasku.maksettu;
        }
    }
    private void UpdateLaskuDisplayProperties()
    {
        if (ValittuVaraus != null)
        {
            // Varmistetaan, että mokki lisätään Mokit-listaan jos puuttuu
            if (ValittuVaraus.mokki_id != null && !Mokit.Any(m => m.mokki_id == ValittuVaraus.mokki_id))
            {
                Task.Run(async () =>
                {
                    var mokki = await _mokkiDb.GetMokkiByIdAsync((int)ValittuVaraus.mokki_id);
                    if (mokki != null)
                    {
                        MainThread.BeginInvokeOnMainThread(() => Mokit.Add(mokki));
                        // Pakotetaan PropertyChanged MokkiNimiin ja VarausMokkiin
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MokkiNimi)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VarausMokki)));
                    }
                });
            }
            VarausAsiakas = HaeAsiakkaanKokoNimi(ValittuVaraus.varaus_id);
            VarausMokki = MokkiNimi;

            string varattuPvmTeksti = ValittuVaraus.varattu_pvm is DateTime vp ? vp.ToString("d") : (ValittuVaraus.varattu_pvm != null ? $"EI DateTime ({ValittuVaraus.varattu_pvm})" : "Ei asetettu");
            string vahvistusPvmTeksti = ValittuVaraus.vahvistus_pvm is DateTime vvp ? vvp.ToString("d") : (ValittuVaraus.vahvistus_pvm != null ? $"EI DateTime ({ValittuVaraus.vahvistus_pvm})" : "Ei asetettu");

            VarausPvm = $"{varattuPvmTeksti} - {vahvistusPvmTeksti}";
        }
        else
        {
            VarausAsiakas = "Tyhjä";
            VarausMokki = "Tyhjä";
            VarausPvm = "Tyhjä";
        }
    }
    private Lasku _selectedLasku;
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
        set => SetProperty(ref _uusiVarausId, value, () => ValidateVarausId());
    }
    public double UusiSumma
    {
        get => _uusiSumma;
        set => SetProperty(ref _uusiSumma, value, () => ValidateDouble(_uusiSumma.ToString(CultureInfo.InvariantCulture), nameof(UusiSumma)));
    }
    public double UusiAlv
    {
        get => _uusiAlv;
        set => SetProperty(ref _uusiAlv, value, () => ValidateDouble(_uusiAlv.ToString(CultureInfo.InvariantCulture), nameof(UusiAlv)));
    }
    public bool UusiMaksettu
    {
        get => _uusiMaksettu;
        set => SetProperty(ref _uusiMaksettu, value, () => OnkoMuutoksia = true);
    }
    private int _uusiVarausId;
    private double _uusiSumma;
    private double _uusiAlv;
    private bool _uusiMaksettu;
    // =========================================
    // ========= Propertyt: Näyttö ============= Infoa käyttäjälle
    // =========================================
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
    private bool _onkoMuutoksia = false;
    public bool OnkoMuutoksia
    {
        get => _onkoMuutoksia;
        private set => SetProperty(ref _onkoMuutoksia, value, () => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSave))));
    }
    public ObservableCollection<Palvelu> LaskunPalvelut
    {
        get => _laskunPalvelut;
        private set => SetProperty(ref _laskunPalvelut, value, () =>
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PalveluidenSummaIlmanVeroa)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KokonaissummaVerojenKanssa)));
        });
    }
    private string _varausAsiakas = "Tyhjä";
    private string _varausMokki = "Tyhjä";
    private string _varausPvm = "Tyhjä";
    public double PalveluidenSummaIlmanVeroa => LaskunPalvelut?.Sum(p => p.Hinta) ?? 0;
    public double KokonaissummaVerojenKanssa
    {
        get
        {
            if (ValittuLasku == null) return 0;
            return _laskutusService.LaskeKokonaissumma(ValittuLasku, LaskunPalvelut?.ToList() ?? new List<Palvelu>());
        }
    }
    // =========================================
    // ========= Propertyt: Validointi========== Jos käyttäjä tekee virheellisiä syötteitä, nämä sitten tekevät virheilmoituksen käyttöliittymään
    // =========================================
    public bool CanSave => ValittuVaraus != null && string.IsNullOrEmpty(SummaVirhe) && string.IsNullOrEmpty(AlvVirhe) && string.IsNullOrEmpty(VarausIdVirhe) && OnkoMuutoksia;
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
    public ICommand HaeDataCommand { get; private set; }
    public ICommand TallennaLaskuCommand { get; private set; }
    public ICommand PoistaLaskuCommand { get; private set; }
    public ICommand ClearValittuLaskuCommand { get; private set; }
    public ICommand HaeSuodatetutLaskutCommand { get; private set; }
    public ICommand ValitseLaskuCommand { get; private set; }
    public ICommand TyhjennaValinnatCommand { get; private set; }
    public ICommand SuodataMaksamattomatCommand { get; private set; }
    public ICommand TulostaLaskuCommand { get; private set; }
    public ICommand LahetaLaskuEmailillaCommand { get; private set; }
    public ICommand TulostaMaksumuistutusCommand { get; private set; }
    public ICommand LahetaMaksumuistutusEmailillaCommand { get; private set; }
    // =========================================
    // ========= Tietojen haku ================= Nämä metodit käyttää databaseservicejä tietojen hakemiseen
    // =========================================
    private async Task LoadData()
    {
        try
        {
            await Task.WhenAll(
                LoadCollectionAsync(Alueet, _alueDb.HaeKaikki, "Virhe ladattaessa alueita"),
                LoadCollectionAsync(_kaikkiLaskut, _laskuDb.HaeKaikki, "Virhe ladattaessa laskuja"),
                LoadCollectionAsync(Varaukset, _laskuDb.HaeKaikkiVaraukset, "Virhe ladattaessa varauksia"),
                LoadCollectionAsync(_kaikkiAsiakkaat, _laskuDb.HaeKaikkiAsiakkaat, "Virhe ladattaessa asiakkaita")
            );
            PaivitaNaytettavatLaskut(_kaikkiLaskut.ToList());
            await PaivitaAsiakasListaKaikista();
            await HaeMokit();
            if (Laskut.FirstOrDefault() != null)
            {
                ValittuLasku = Laskut.First();
            }
        }
        catch (Exception ex)
        {
            await ShowAlert("Virhe ladattaessa tietoja", ex.Message);
        }
    }
    private async Task LoadCollectionAsync<T>(ObservableCollection<T> collection, Func<Task<List<T>>> fetchData, string errorMessage)
    {
        try
        {
            collection.Clear();
            var data = await fetchData();
            foreach (var item in data)
            {
                collection.Add(item);
            }
        }
        catch (Exception ex)
        {
            await ShowAlert(errorMessage, ex.Message);
        }
    }
    private async Task HaeMokit()
    {
        Mokit.Clear();
        var mokit = ValittuAlue != null
            ? await _mokkiDb.GetAllMokkiInAlue((int)ValittuAlue.alue_id)
            : await _mokkiDb.GetAllMokkisAsync();
        mokit.OrderBy(m => m.Mokkinimi).ToList().ForEach(Mokit.Add);
        if (ValittuMokki != null) await PaivitaAsiakasListaa();
        else await PaivitaAsiakasListaKaikista();
    }
    private async Task PaivitaAsiakasListaKaikista() =>
    await LoadCollectionAsync(Asiakkaat, async () => _kaikkiAsiakkaat.OrderBy(a => a.sukunimi).DistinctBy(a => a.asiakas_id).ToList(), "Virhe päivittäessä asiakaslistaa");

    private async Task PaivitaAsiakasListaa() =>
    await LoadCollectionAsync(Asiakkaat, async () => ValittuMokki != null
        ? (await HaeAsiakkaatJoillaOnVarausMokkiin(ValittuMokki.mokki_id)).OrderBy(a => a.sukunimi).DistinctBy(a => a.asiakas_id).ToList()
        : _kaikkiAsiakkaat.OrderBy(a => a.sukunimi).DistinctBy(a => a.asiakas_id).ToList(), "Virhe päivittäessä asiakaslistaa");

    private async Task<List<Asiakas>> HaeAsiakkaatJoillaOnVarausMokkiin(int mokkiId)
    {
        var asiakkaat = new List<Asiakas>();
        var varaukset = Varaukset.Where(v => v.mokki_id == mokkiId).ToList();
        var asiakasIdt = varaukset.Select(v => v.asiakas_id).Distinct().ToList();

        foreach (var id in asiakasIdt)
        {
            var asiakas = _kaikkiAsiakkaat.FirstOrDefault(a => a.asiakas_id == id); // Haetaan välimuistista
            if (asiakas != null)
            {
                asiakkaat.Add(asiakas);
            }
        }
        return asiakkaat;
    }
    private async Task HaeSuodatetutLaskut()
    {
        try
        {
            var suodatetut = await _laskuDb.HaeSuodatetutLaskut(
                alueId: (int?)ValittuAlue?.alue_id,
                mokkiId: ValittuMokki?.mokki_id,
                asiakasId: (int?)ValittuAsiakas?.asiakas_id);

            PaivitaNaytettavatLaskut(suodatetut);
        }
        catch (Exception ex)
        {
            await ShowAlert("Virhe suodatettaessa laskuja", ex.Message);
        }
    }
    private void PaivitaNaytettavatLaskut(List<Lasku> laskut)
    {
        Laskut.Clear();
        foreach (var lasku in laskut)
        {
            lasku.ViewModel = this; // Aseta ViewModel-viittaus, tarvitaan asiakkaiden tietojen näyttämiseen suodatettujen laskujen listassa.
            Laskut.Add(lasku);
        } // Päivitetään ValittuLasku, jotta palvelut latautuvat uudelleen tarvittaessa
        if (ValittuLasku != null && laskut.Any(l => l.lasku_id == ValittuLasku.lasku_id))
        {
            ValittuLasku = laskut.First(l => l.lasku_id == ValittuLasku.lasku_id);
        }
        else if (laskut.Any())
        {
            ValittuLasku = laskut.First();
        }
        else
        {
            ValittuLasku = null;
        }
    }
    private async void SuodataMaksamattomat()
    {
        List<Lasku> suodatetutLaskut = _kaikkiLaskut.Where(l => !l.maksettu).ToList();

        if (ValittuAlue != null)
        {
            suodatetutLaskut = suodatetutLaskut.Where(l => Varaukset.Any(v => v.varaus_id == l.varaus_id && Mokit.Any(m => m.mokki_id == v.mokki_id && m.alue_id == ValittuAlue.alue_id))).ToList();
        }

        if (ValittuMokki != null)
        {
            suodatetutLaskut = suodatetutLaskut.Where(l => Varaukset.Any(v => v.varaus_id == l.varaus_id && v.mokki_id == ValittuMokki.mokki_id)).ToList();
        }

        if (ValittuAsiakas != null)
        {
            suodatetutLaskut = suodatetutLaskut.Where(l => Varaukset.Any(v => v.varaus_id == l.varaus_id && v.asiakas_id == ValittuAsiakas.asiakas_id)).ToList();
        }

        PaivitaNaytettavatLaskut(suodatetutLaskut);
    }
    public Asiakas? HaeAsiakas(uint varausId)
    {
        var varaus = Varaukset.FirstOrDefault(v => v.varaus_id == varausId);
        var asiakas = Asiakkaat.FirstOrDefault(a => a.asiakas_id == varaus?.asiakas_id);
        return asiakas;
    }
    private Asiakas? HaeAsiakasByVarausId(uint varausId)
    {
        var varaus = Varaukset.FirstOrDefault(v => v.varaus_id == varausId);
        return Asiakkaat.FirstOrDefault(a => a.asiakas_id == varaus?.asiakas_id);
    }
    public uint HaeAsiakasId(uint varausId) => HaeAsiakasByVarausId(varausId)?.asiakas_id ?? 0;
    public string HaeAsiakkaanKokoNimi(uint varausId) => HaeAsiakasByVarausId(varausId)?.kokoNimi ?? "Tuntematon";
    public string HaeAsiakkaanOsoite(uint varausId) => HaeAsiakasByVarausId(varausId)?.lahiosoite ?? "Tuntematon";
    public string HaeAsiakkaanEmail(uint varausId) => HaeAsiakasByVarausId(varausId)?.email ?? "Tuntematon";
    public string HaeAsiakkaanPuhNro(uint varausId) => HaeAsiakasByVarausId(varausId)?.puhelinnro ?? "Tuntematon";
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
        LaskunPalvelut.Clear();
        await LoadData();
    }
    private async Task<Tuple<Varaus, Asiakas, Mokki>> HaeLaskunTiedot()
    {
        Asiakas asiakas = null;
        Mokki mokki = null;
        Varaus varaus = null;
        if (ValittuLasku == null)
        {
            await ShowAlert("Virhe", "Valitse ensin lasku.");
            return null;
        }
        varaus = await _varausDb.Hae((int)ValittuLasku.varaus_id);
        if (varaus != null)
        {
            asiakas = _kaikkiAsiakkaat.FirstOrDefault(a => a.asiakas_id == varaus.asiakas_id);
        }

        if (varaus?.mokki_id != null)
        {
            mokki = await _mokkiDb.GetMokkiByIdAsync((int)varaus.mokki_id);
        }

        if (varaus == null)
        {
            await ShowAlert("Virhe", "Laskulle ei löytynyt vastaavaa varausta.");
            return null;
        }
        if (asiakas == null)
        {
            await ShowAlert("Virhe", "Laskulle ei löytynyt vastaavaa asiakasta.");
            return null;
        }
        if (mokki == null)
        {
            await ShowAlert("Virhe", "Laskulle ei löytynyt mökkiä.");
            return null;
        }
        return Tuple.Create(varaus, asiakas, mokki);
    }
    private async Task KasitteleLasku(bool onMaksumuistutus, bool lahetaEmaililla)
    {
        var tiedot = await HaeLaskunTiedot();
        if (tiedot != null)
        {
            if (lahetaEmaililla)
            {
                await _laskutusService.LuoJaLahetaEmailLasku(ValittuLasku, tiedot.Item1, tiedot.Item2, tiedot.Item3, onMaksumuistutus);
            }
            else
            {
                await _laskutusService.LuoLaskuPdf(ValittuLasku, tiedot.Item1, tiedot.Item2, tiedot.Item3, onMaksumuistutus);
            }
        }
    }
    // =========================================
    // ========== Tietokantaoperaatiot ========= Kommunikoi databaseservicen kanssa, välittää käyttäjän syötteet
    // ========================================= Luo uusia lasku olioita
    private async Task SaveLasku()
    {
        if (!CanSave)
        {
            await ShowAlert("Virhe", "Tarkista kentät.");
            return;
        }
        try
        {
            var lasku = new Lasku(ValittuLasku?.lasku_id ?? 0, (uint)UusiVarausId, UusiSumma, UusiAlv, UusiMaksettu);
            if (IsEditing) await _laskuDb.Muokkaa(lasku);
            else await _laskuDb.Lisaa(lasku);
            OnkoMuutoksia = false;
            ShowAlert("Tallennus onnistui", "");
            await TyhjennaValinnat();
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
            await _laskuDb.Poista(lasku.lasku_id);
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
    private bool ValidateVarausId()
    {
        if (!int.TryParse(UusiVarausId.ToString(), out _))
        {
            VarausIdVirhe = "Varaus ID on virheellinen.";
            return false;
        }
        try { SyoteValidointi.TarkistaInt(UusiVarausId, 1, int.MaxValue); VarausIdVirhe = ""; return true; }
        catch (Exception ex) { VarausIdVirhe = ex.Message; return false; }
    }
    private bool ValidateDouble(string value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
        {
            if (propertyName == nameof(UusiSumma)) SummaVirhe = "Syötä summa.";
            else if (propertyName == nameof(UusiAlv)) AlvVirhe = "Syötä ALV.";
            return false;
        }
        if (!double.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double doubleValue))
        {
            string virheviesti = "Syötä kelvollinen desimaaliluku.";
            if (propertyName == nameof(UusiSumma)) SummaVirhe = virheviesti;
            else if (propertyName == nameof(UusiAlv)) AlvVirhe = virheviesti;
            return false;
        }
        if (propertyName == nameof(UusiSumma))
        {
            try { SyoteValidointi.TarkistaDouble(doubleValue, 0, double.MaxValue); SummaVirhe = ""; return true; }
            catch (Exception ex) { SummaVirhe = ex.Message; return false; }
        }
        else if (propertyName == nameof(UusiAlv))
        {
            try { SyoteValidointi.TarkistaDouble(doubleValue, 0, 100); AlvVirhe = ""; return true; }
            catch (Exception ex) { AlvVirhe = ex.Message; return false; }
        }
        return true;
    }
    // =========================================
    // ======== Apu- ja ilmoitusmetodit ========
    // =========================================
    public event PropertyChangedEventHandler PropertyChanged;
    private async Task ShowAlert(string title, string message) =>
        await Application.Current.MainPage.DisplayAlert(title, message, "OK");

    private void SetProperty<T>(ref T storage, T value, Action onChanged = null, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(storage, value)) return;
        storage = value;
        onChanged?.Invoke();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (new string[] { nameof(UusiVarausId), nameof(UusiSumma), nameof(UusiAlv), nameof(UusiMaksettu) }.Contains(propertyName))
        {
            OnkoMuutoksia = true;
        }
        if (new string[] { nameof(UusiVarausId), nameof(UusiSumma), nameof(UusiAlv), nameof(ValittuVaraus), nameof(OnkoMuutoksia), nameof(SummaVirhe), nameof(AlvVirhe), nameof(VarausIdVirhe) }.Contains(propertyName))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSave)));
        }
        if (propertyName == nameof(ValittuLasku))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PalveluidenSummaIlmanVeroa)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KokonaissummaVerojenKanssa)));
        }
    }
}