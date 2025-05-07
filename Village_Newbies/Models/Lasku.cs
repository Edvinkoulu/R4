namespace Village_Newbies.Models;
using Village_Newbies.ViewModels;

public class Lasku
{
    public const double ARVONLISAVEROKANTA = 25.5;
    private readonly uint _laskuId;
    private readonly uint _varausId;
    private double _alv;
    private double _summa;
    private bool _maksettu = false;
    // Seuraavat propertyt tarvitaan tietojen näyttämiseen käyttöliittymässä (Laskujen hallinta)
    // Mielestäni oli helpointa liittää muiden taulujen tiedot lasku olioon.
    public LaskuViewModel ViewModel { get; set; }
    public Asiakas? Asiakas => ViewModel?.HaeAsiakas(varaus_id);
    public uint AsiakasId => ViewModel?.HaeAsiakasId(varaus_id) ?? 0;
    public string AsiakkaanKokoNimi => Asiakas?.kokoNimi ?? "Tuntematon";
    public string AsiakkaanOsoite => Asiakas?.lahiosoite ?? "Tuntematon";
    public string AsiakkaanEmail => Asiakas?.email ?? "Tuntematon";
    public string AsiakkaanPuhNro => Asiakas?.puhelinnro ?? "Tuntematon";
    // Muoodostimet
    private Lasku() { }
    public Lasku(uint laskuid, uint varausid)
    {
        _laskuId = laskuid;
        _varausId = varausid;
        _alv = ARVONLISAVEROKANTA;
    }
    public Lasku(uint laskuid, uint varausid, double summa, bool maksettu = false)
    {
        _laskuId = laskuid;
        _varausId = varausid;
        _summa = summa;
        _maksettu = maksettu;
        _alv = ARVONLISAVEROKANTA;
    }
    public Lasku(uint varausid, double summa, bool maksettu = false)
    {
        _varausId = varausid;
        _summa = summa;
        _maksettu = maksettu;
        _alv = ARVONLISAVEROKANTA;
    }
    public Lasku(uint laskuid, uint varausid, double summa, double alv, bool maksettu = false)
    {
        _laskuId = laskuid;
        _varausId = varausid;
        _summa = summa;
        _maksettu = maksettu;
        _alv = alv;
    }
    //Propertyt
    public uint lasku_id
    {
        get => _laskuId;
    }
    public uint varaus_id
    {
        get => _varausId;
    }
    public double alv
    {
        get => _alv;
        set => _alv = SyoteValidointi.TarkistaDouble(value, 0, 100);
    }
    public double summa
    {
        get => _summa;
        set => _summa = SyoteValidointi.TarkistaDouble(value, 0, double.MaxValue);
    }
    public bool maksettu
    {
        get => _maksettu;
        set => _maksettu = value;
    }
}