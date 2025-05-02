using Village_Newbies;
namespace Village_Newbies.Models;

public class Asiakas
{
    // 1.  Poista readonly → tekee id:stä muokattavan samassa assemblyssä,
    //     mutta EI julkisesti.  Tällöin DatabaseService voi päivittää id:n
    //     LastInsertId‑arvolla lisäyksen jälkeen.
    internal uint asiakasId;

    private string postinumero;

    public string etunimi { get; set; }
    public string sukunimi { get; set; }
    public string lahiosoite { get; set; }
    public string email { get; set; }
    public string puhelinnro { get; set; }


    public Asiakas() { }


    public Asiakas(string etunimi, string sukunimi,
                   string lahiosoite, string email,
                   string puhelinnro, string postinro)
        : this(0, etunimi, sukunimi, lahiosoite, email, puhelinnro, postinro) { }

    // 2.  Uusi “luku‑/map‑konstruktori” id:llä
    public Asiakas(uint asiakasId, string etunimi, string sukunimi,
                   string lahiosoite, string email,
                   string puhelinnro, string postinro)
    {
        this.asiakasId = asiakasId;
        this.etunimi = etunimi;
        this.sukunimi = sukunimi;
        this.lahiosoite = lahiosoite;
        this.email = email;
        this.puhelinnro = puhelinnro;
        this.postinro = postinro;
    }

    // 3.  Vanha property jätetään koskematta → olemassa olevat
    //     LaskuViewModel‑ ja muut viittaukset pysyvät toimivina.
    public uint asiakas_id => asiakasId;

    //public string postinro
    //{
    //    get => postinumero;
    //  set => postinumero = SyoteValidointi.TarkistaPostinumero(value); Tämä herjaa jo täytettäessä.
    //}
    public string postinro
    {
        get => postinumero;
        set => postinumero = value;
    }
    public string kokoNimi => $"{sukunimi} {etunimi}"; // Tarvitaan laskujen hallintaa varten. JaniV
}