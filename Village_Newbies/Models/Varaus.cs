public class Varaus
{
    public uint varaus_id {get;set;}
    public uint asiakas_id {get;set;}
    public uint mokki_id {get;set;}
    public DateTime varattu_pvm {get;set;}
    public DateTime? vahvistus_pvm {get;set;}
    public DateTime varattu_alkupvm {get;set;}
    public DateTime varattu_loppupvm{get;set;}
}