namespace Village_Newbies.Models
{
    public class PalveluRaportti
    {
        public string PalveluNimi { get; set; }
        public string PalveluKuvaus { get; set; }
        public decimal PalveluHinta { get; set; }
        public DateTime VarattuAlkuPvm { get; set; }
        public DateTime VarattuLoppuPvm { get; set; }
        public string AsiakasNimi { get; set; }

        // Laskettu kesto (päivinä)
        public int KestoPaivina => (VarattuLoppuPvm - VarattuAlkuPvm).Days + 1;

        // Laskettu hinta yhteensä (Hinta × päivien määrä)
        public decimal HintaYhteensa => KestoPaivina * PalveluHinta;
    }
}