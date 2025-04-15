
namespace Village_Newbies.Models
{
    public class Lasku
    {
        public const double ARVONLISAVEROKANTA = 25.5;
        private readonly int _laskuId;
        private readonly uint _varausId;
        private double _alv;
        private double _summa;
        private bool _maksettu = false;

        // Muoodostimet
        private Lasku() { }
        public Lasku(int laskuid, uint varausid)
        {
            _laskuId = laskuid;
            _varausId = varausid;
            _alv = ARVONLISAVEROKANTA;
        }
        public Lasku(int laskuid, uint varausid, double summa, bool maksettu = false)
        {
            _laskuId = laskuid;
            _varausId = varausid;
            _summa = summa;
            _maksettu = maksettu;
            _alv = ARVONLISAVEROKANTA;
        }
        public Lasku(int laskuid, uint varausid, double summa, double alv, bool maksettu = false)
        {
            _laskuId = laskuid;
            _varausId = varausid;
            _summa = summa;
            _maksettu = maksettu;
            _alv = alv;
        }
        //Propertyt
        public int lasku_id
        {
            get => _laskuId;
        }
        public uint varaus_id
        {
            get => _varausId;
        }
        public double alv
        {
            get => _alv;
            set => _alv = SyoteValidointi.TarkistaDouble(value, 0, 100);
        }
        public double summa
        {
            get => _summa;
            set => _summa = SyoteValidointi.TarkistaDouble(value, 0, double.MaxValue);
        }
        public bool maksettu
        {
            get => _maksettu;
            set => _maksettu = value;
        }
    }
}