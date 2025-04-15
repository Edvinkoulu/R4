using Village_Newbies;
public class Asiakas
{
    public readonly uint asiakasId;
    private string postinumero;
    public string etunimi { get; set; }
    public string sukunimi { get; set; }
    public string lahiosoite { get; set; }
    public string email { get; set; }
    public string puhelinnro { get; set; }

    public Asiakas() { }
    public Asiakas(string etunimi, string sukunimi, string lahiosoite, string email, string puhelinnro, string postinro)
    {
        this.etunimi = etunimi;
        this.sukunimi = sukunimi;
        this.lahiosoite = lahiosoite;
        this.email = email;
        this.puhelinnro = puhelinnro;
        this.postinro = postinro;
    }
    public Asiakas(uint asiakasId, string etunimi, string sukunimi, string lahiosoite, string email, string puhelinnro, string postinro)
    {
        this.asiakasId = asiakasId;
        this.etunimi = etunimi;
        this.sukunimi = sukunimi;
        this.lahiosoite = lahiosoite;
        this.email = email;
        this.puhelinnro = puhelinnro;
        this.postinro = postinro;
    }
    public uint asiakas_id
    {
        get => asiakasId;
    }
    public string postinro
    {
        get => postinumero;
        set => postinumero = SyoteValidointi.TarkistaPostinumero(value);
    }
}