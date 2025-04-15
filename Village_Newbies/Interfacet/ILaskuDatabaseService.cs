namespace Village_Newbies.Interfacet;
using Village_Newbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILaskuDatabaseService
{
    Task<Lasku> Hae(int id);
    Task<List<Lasku>> HaeKaikki();
    Task Lisaa(Lasku lasku);
    Task Muokkaa(Lasku lasku);
    Task Poista(int id);
}