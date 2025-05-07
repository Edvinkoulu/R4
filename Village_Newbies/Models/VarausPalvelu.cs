namespace Village_Newbies.Models
{
    public class VarauksenPalvelu
    {
        public uint VarausId { get; set; }
        public uint PalveluId { get; set; }
        public int Lkm { get; set; }

        // Optional navigation properties if you plan to use them
        public Varaus? Varaus { get; set; }
        public Palvelu? Palvelu { get; set; }
    }
}
