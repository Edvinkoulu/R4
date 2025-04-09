namespace Village_Newbies.Models
{
    public class Mokki
    {
        public int mokki_id { get; set; }  // Corresponds to mokki_id in the database
        public int alue_id { get; set; }   // Corresponds to alue_id in the database
        public string Postinro { get; set; }  // Corresponds to postinro in the database
        public string Mokkinimi { get; set; }  // Corresponds to mokkinimi in the database
        public string Katuosoite { get; set; }  // Corresponds to katuosoite in the database
        public double Hinta { get; set; }  // Corresponds to hinta in the database
        public string Kuvaus { get; set; }  // Corresponds to kuvaus in the database
        public int? Henkilomaara { get; set; }  // Corresponds to henkilomaara in the database
        public string Varustelu { get; set; }  // Corresponds to varustelu in the database
    }
}
