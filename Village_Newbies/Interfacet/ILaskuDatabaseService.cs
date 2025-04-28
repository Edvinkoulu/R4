namespace Village_Newbies.Interfacet;
using Village_Newbies.Models;
using Village_Newbies.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILaskuDatabaseService
{   //LaskuDatabaseService on pitkä, tässä oleellisimmat metodit
    Task<Lasku> Hae(uint id);
    Task<List<Lasku>> HaeKaikki();
    Task<List<Lasku>> HaeSuodatetutLaskut(
        int? alueId = null, int? mokkiId = null, int? asiakasId = null,
        DateTime? varausAlku = null, DateTime? varausLoppu = null,
        bool? maksettu = null, string asiakasNimi = null);
    Task<List<Palvelu>> HaeLaskunPalvelut(Lasku lasku);
    Task Lisaa(Lasku lasku);
    Task Muokkaa(Lasku lasku);
    Task Poista(uint id);
    Task<Varaus> HaeVaraus(uint id);
    Task<MokkiUint> HaeMokki(uint id);
    Task<Asiakas> HaeAsiakas(uint id);
    Task<List<Varaus>> HaeKaikkiVaraukset();
    Task<List<Asiakas>> HaeKaikkiAsiakkaat();
}