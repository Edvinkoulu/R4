namespace Village_Newbies.ViewModels;
using MySqlConnector;
using Village_Newbies.Models;
using Village_Newbies.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;



/// <summary>
/// CRUD-logiikka asiakkaille.  
/// Sitoutuu HallintaPageAsiakas.xaml:iin.
/// </summary>
public sealed class AsiakasHallintaViewModel : BindableObject
{
    // ───── Kentät & ominaisuudet ────────────────────────────────────────────
    private readonly IAsiakasDatabaseService _db = new AsiakasDatabaseService();
    private readonly IPostiDatabaseService _posti = new PostiDatabaseService();
    public ObservableCollection<Asiakas> Asiakkaat { get; } = new();

    private Asiakas _valittu = new();      // kaksi-suuntainen sidonta
    public Asiakas Valittu
    {
        get => _valittu;
        set { _valittu = value; OnPropertyChanged(); }
    }
    private string _hakusana;
    public string Hakusana
    {
        get => _hakusana;
        set { _hakusana = value; OnPropertyChanged(); }
    }
    private string _uusiToimipaikka = string.Empty;
    public string UusiToimipaikka
    {
    get => _uusiToimipaikka;
    set { _uusiToimipaikka = value; OnPropertyChanged(); }
    }
    private List<Posti> _postiCache = new();
public bool NäytäToimipaikka => !_postiCache.Any(p => p.Postinumero == Valittu.postinro);
private async Task LataaPostitAsync()
{
    _postiCache = await _posti.HaeKaikkiAsync();
    OnPropertyChanged(nameof(NäytäToimipaikka));
}
    // ───── Komennot ─────────────────────────────────────────────────────────
    public ICommand LataaCommand     { get; }
    public ICommand UusiCommand      { get; }
    public ICommand TallennaCommand  { get; }
    public ICommand PoistaCommand    { get; }
    public ICommand HaeCommand { get; }

    // ───── Konstruktori ─────────────────────────────────────────────────────
    public AsiakasHallintaViewModel()
    {
        LataaCommand    = new Command(async () => await LataaAsync());
        UusiCommand     = new Command(() => Valittu = new Asiakas());
        TallennaCommand = new Command(async () => await TallennaAsync());
        PoistaCommand   = new Command(async () => await PoistaAsync());
        HaeCommand = new Command(async () => await HaeAsync());

        // lataus heti käynnistyessä
        _ = LataaAsync();
    }

    // ───── Yksityiset metodit ───────────────────────────────────────────────
    private async Task HaeAsync()
    {
        Asiakkaat.Clear();
        var tulos = await _db.Hae(Hakusana);
        foreach (var a in tulos) Asiakkaat.Add(a);
    }
    private async Task LataaAsync()
    {
        Asiakkaat.Clear();
        foreach (var a in await _db.HaeKaikki())
            Asiakkaat.Add(a);
    }
    
    async Task TallennaAsync()
{
    try
    {
        SyoteValidointi.TarkistaPostinumero(Valittu.postinro);

        // 1)  Lisätään puuttuva postinumero, jos tarpeen
        if (!_postiCache.Any(p => p.Postinumero == Valittu.postinro))
        {
            // varmistetaan että käyttäjä antoi toimipaikan
            if (string.IsNullOrWhiteSpace(UusiToimipaikka))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Puuttuva tieto",
                    "Anna toimipaikka uudelle postinumerolle.",
                    "OK");
                return;
            }

            await _posti.LisaaTaiPaivitaAsync(
                new Posti { Postinumero = Valittu.postinro, Toimipaikka = UusiToimipaikka });

            // päivitetään välimuisti, ettei hetkeä myöhemmin näy puuttuvaksi
            _postiCache.Add(new Posti { Postinumero = Valittu.postinro, Toimipaikka = UusiToimipaikka });
        }

        // 2)  Asiakkaan lisäys / muokkaus
        if (Valittu.asiakas_id == 0)
            await _db.Lisaa(Valittu);
        else
            await _db.Muokkaa(Valittu);

        // 3)  Siivoa lomake
        await HaeAsync();
        Valittu = new Asiakas();
        UusiToimipaikka = string.Empty;
    }
    catch (MySqlException ex) when (ex.Number == 1062)
    {
        await Application.Current.MainPage.DisplayAlert(
            "Virhe", "Sähköposti on jo käytössä.", "OK");
    }
}

    private async Task PoistaAsync()
    {
        if (Valittu.asiakas_id == 0) return;   // uusi, ei poistettavaa
        await _db.Poista(Valittu.asiakas_id);
        await LataaAsync();
        Valittu = new Asiakas();
    }
}

