namespace Village_Newbies.Interfacet;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Village_Newbies.Models;

// Tein interfacen laskuViewModelille koska sekin on todella iso.
// Kenties helpottaa toiminnallisuuden hahmottamista, veikkei tätä muualla hyödynnetä.
public interface ILaskuViewModel
{
    ObservableCollection<Lasku> Laskut { get; }
    ObservableCollection<Varaus> Varaukset { get; }
    ObservableCollection<Alue> Alueet { get; }
    ObservableCollection<Mokki> Mokit { get; }
    ObservableCollection<Asiakas> Asiakkaat { get; }
    Lasku ValittuLasku { get; set; }
    Alue ValittuAlue { get; set; }
    Mokki ValittuMokki { get; set; }
    Asiakas ValittuAsiakas { get; set; }
    Varaus ValittuVaraus { get; set; }
    int UusiVarausId { get; set; }
    double UusiSumma { get; set; }
    double UusiAlv { get; set; }
    bool UusiMaksettu { get; set; }
    string VarausAsiakas { get; }
    string VarausMokki { get; }
    string VarausPvm { get; }
    bool CanSave { get; }
    bool IsNotEditing { get; }
    string VarausIdVirhe { get; }
    string SummaVirhe { get; }
    string AlvVirhe { get; }
    bool IsEditing { get; }
    ICommand HaeDataCommand { get; }
    ICommand TallennaLaskuCommand { get; }
    ICommand PoistaLaskuCommand { get; }
    ICommand ClearValittuLaskuCommand { get; }
    ICommand HaeSuodatetutLaskutCommand { get; }
    ICommand ValitseLaskuCommand { get; }
    ICommand TyhjennaValinnatCommand { get; }
    ICommand SuodataMaksamattomatCommand { get; }
    ICommand TulostaLaskuCommand { get; }
    ICommand LahetaLaskuEmailillaCommand { get; }
    ICommand TulostaMaksumuistutusCommand { get; }
    ICommand LahetaMaksumuistutusEmailillaCommand { get; }

    Task InitializeAsync();
    uint HaeAsiakasId(uint varausId);
    string HaeAsiakkaanKokoNimi(uint varausId);
    string HaeAsiakkaanOsoite(uint varausId);
    string HaeAsiakkaanEmail(uint varausId);
    string HaeAsiakkaanPuhNro(uint varausId);
    Asiakas? HaeAsiakas(uint varausId);
}