namespace Village_Newbies.Models
{
    public class MajoitusRaportti
    {
    public string MokkiNimi { get; set; }
    public DateTime VarattuAlkuPvm { get; set; }
    public DateTime VarattuLoppuPvm { get; set; }
    public string AsiakasNimi { get; set; }
    public int KestoPaivina { get; set; }
    public decimal HintaYhteensa { get; set; }
    }
}

