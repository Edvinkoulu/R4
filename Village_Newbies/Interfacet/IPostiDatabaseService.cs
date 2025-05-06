
using Village_Newbies.Models;

namespace Village_Newbies.Services;

public interface IPostiDatabaseService
{

    Task<bool> LisaaTaiPaivitaAsync(Posti p);


    Task<List<Posti>> HaeKaikkiAsync();
}