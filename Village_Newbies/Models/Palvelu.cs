namespace Village_Newbies.Models
{
    public class Palvelu
    {
        public int palvelu_id { get; set; }  // Corresponds to palvelu_id in the database
        public int alue_id { get; set; }     // Corresponds to alue_id in the database
        public string Nimi { get; set; }     // Corresponds to nimi in the database
        public string Kuvaus { get; set; }   // Corresponds to kuvaus in the database
        public double Hinta { get; set; }    // Corresponds to hinta in the database
        public double Alv { get; set; }      // Corresponds to alv in the database
    }
}