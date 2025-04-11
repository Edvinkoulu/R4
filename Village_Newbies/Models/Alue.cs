namespace Village_Newbies.Models;
using System.Diagnostics;

// Tämä määrittelee mikä on alue ja mitä se sisältää. JaniV

public class Alue
{
    private const int NIMI_MAX_PITUUS = 40; //Maksimi pituus alueen nimelle. Tämä oli määritelty tietokannan luonti skriptissä.
    private uint alueID; // Vaihdettu UINT muotoon koska niin myös tietokannassa(?)
    private string? alueNimi;

    //Constructorit
    public Alue()
    { }
    public Alue(string _alue_nimi)
    {
        alueNimi = _alue_nimi;
    }

    // Properties
    public uint alue_id
    {
        get => alueID;
        set => alueID = SyoteValidointi.TarkistaIDuint(value);
    }

    public string? alue_nimi
    {
        get
        {
            if (alueNimi == null || alueNimi == "")
            {
                Debug.WriteLine("Hallinta: Alue.cs: Alueen nimi on null tai tyhjä.");
            }
            return alueNimi;
        }
        set => alueNimi = SyoteValidointi.TarkistaString(value, NIMI_MAX_PITUUS);
    }

    //overridet
    public override string ToString()
    {
        return $"id: {alue_id}, nimi: {alue_nimi}";
    }
}
