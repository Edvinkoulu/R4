using Village_Newbies.Models;

public interface IAlueDatabaseService
{
    // dbServicen oma interface joka pakottaa tarvittavan CRUD toiminnallisuuden alueelle.
    Task<Alue> Hae(uint id);
    Task<List<Alue>> HaeKaikki();
    Task Lisaa(Alue alue);
    Task Muokkaa(Alue alue);
    Task Poista(uint id);
}