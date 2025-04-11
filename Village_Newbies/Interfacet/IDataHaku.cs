namespace Village_Newbies.Interfacet;
using System.Data;
using System.Threading.Tasks;

// Tämä interface pakottaa toteuttamaan metodit, jotka hakevat dataa ja suorittavat komentoja tietokannassa.
// Käytetään tietovarasto abstrakti luokassa.
// HaeData: Suorittaa SQL-kyselyn, joka palauttaa dataa, ja palauttaa sen DataTable-oliona.
// SuoritaKomento: Suorittaa SQL-komennon (INSERT, UPDATE, DELETE) ja palauttaa muutettujen rivien määrän.
// params (string, object)[] parameters: Mahdollistaa parametrien välittämisen SQL-kyselyihin
// JaniV

public interface IDataHaku
{
    Task<DataTable> HaeData(string sql, params (string, object)[] parameters);
    Task<int> SuoritaKomento(string sql, params (string, object)[] parameters);
}
