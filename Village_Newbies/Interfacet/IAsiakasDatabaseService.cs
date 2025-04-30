using Village_Newbies.Models;
public interface IAsiakasDatabaseService
{
    Task<Asiakas> Hae(uint id);
    Task<List<Asiakas>> HaeKaikki();
    
    Task Muokkaa(Asiakas asiakas);
    Task Poista(uint id);
    Task<List<Asiakas>> Hae(string hakusana);
    Task<uint> Lisaa(Asiakas asiakas);
}