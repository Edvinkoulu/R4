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

        public Palvelu(){}
        public Palvelu(int _palvelu_id, int _alue_id, string _Nimi, string _Kuvaus, double _Hinta, double _Alv) //Lisätty laskutusta varten JaniV
        {
            this.palvelu_id = _palvelu_id;
            this.alue_id = _alue_id;
            this.Nimi = _Nimi;
            this.Kuvaus = _Kuvaus;
            this.Hinta = _Hinta;
            this.Alv = _Alv;
        }
    }
}
