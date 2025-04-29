using System.Diagnostics;

namespace Village_Newbies;

// Tämän luokan idea on koota kaikki syötteiden tarkistukset yhteen paikkaan. Tänne vain lisää metodeja jos tarttee.
// tehtävä on varmistaa, että sovellukseen syötetty data on tiettyjen sääntöjen mukaista ennen kuin se yritetään tallentaa tietokantaan.
// saattaa myös olla ihan rikki, ei testattu!

public static class SyoteValidointi
{
    // ==================================================================
    // ================== Perusmuuttujien tarkistukset ================== 
    // ==================================================================

    public static string TarkistaString(string nimi, int maxMerkkia)
    {
        if (nimi.Length > maxMerkkia)
        {
            throw new ArgumentException($"Nimi ei voi olla yli {maxMerkkia} merkkiä pitkä");
        }
        return nimi;
    }
    public static double TarkistaDouble(double luku, double min, double max)
    {
        if (luku > double.MaxValue || luku < double.MinValue)
        {
            throw new ArgumentException($"Luku ei voi olla suurempi kuin {double.MaxValue} tai pienempi kuin {double.MinValue}");
        }
        else if (luku < min || luku > max)
        {
            throw new ArgumentException($"Luku ei voi olla pienempi kuin {min} tai suurempi kuin {max}");
        }
        return luku;
    }
    public static int TarkistaInt(int luku, int min, int max)
    {
        if (luku > int.MaxValue || luku < int.MinValue)
        {
            throw new ArgumentException($"Luku ei voi olla suurempi kuin {int.MaxValue} tai pienempi kuin {int.MinValue}");
        }
        else if (luku < min || luku > max)
        {
            throw new ArgumentException($"Luku ei voi olla pienempi kuin {min} tai suurempi kuin {max}");
        }
        return luku;
    }

    // ==================================================================
    // ================== Päivämäärien tarkistukset =====================
    // ==================================================================

    public static DateTime TarkistaPvmVaraus(DateTime varausPvm)
    {
        // Varausta ei voi tehdä menneisuuteen. 
        if (varausPvm < DateTime.Now)
        {
            throw new ArgumentException("Varauksen päivämäärä ei voi olla menneissyydessä");
        }
        return varausPvm;
    }

    public static DateTime TarkistaPvmVahvistus(DateTime vahvistusPvm, DateTime varausPvm)
    {
        // Vahvistuksen päivämäärän tulee olla varauksen päivämäärää myöhemmin tai sama.
        // Saa olla null jos vahvistusta ei ole vielä tehty.
        if (vahvistusPvm < varausPvm)
        {
            throw new ArgumentException("Vahvistuksen päivämäärä ei voi olla ennen varauksen päivämäärää");
        }
        else if (vahvistusPvm > DateTime.Now)
        {
            // Poistetaan tarvittaessa, jos pitää pystyä vahvistamaan tulevaisuuteen
            throw new ArgumentException("Vahvistuksen päivämäärä ei voi olla tulevaisuudessa");
        }
        return vahvistusPvm;
    }

    public static DateTime TarkistaPvmVarattuAlku(DateTime varattuAlkuPvm, DateTime varattuLoppuPvm)
    {
        // Varauksen alkupäivämäärän pittää olla tulevaisuudessa tai nykyisyydessä.
        if (varattuAlkuPvm < DateTime.Now)
        {
            throw new ArgumentException("Varauksen alkupäivämäärä ei voi olla menneisyydessä");
        }
        else if (varattuAlkuPvm > varattuLoppuPvm)
        {
            throw new ArgumentException("Varauksen alkupäivämäärä ei voi olla myöhemmin kuin loppupäivämäärä");
        }
        return varattuAlkuPvm;
    }

    public static DateTime TarkistaPvmVarattuLoppu(DateTime varattuAlkuPvm, DateTime varattuLoppuPvm)
    {
        //varauksen loppu pitää olla alkupäivämäärää myöhemmin tai sama
        if (varattuLoppuPvm < varattuAlkuPvm)
        {
            throw new ArgumentException("Varauksen loppupäivämäärä ei voi olla ennen alkupäivämäärää");
        }
        return varattuLoppuPvm;
    }

    // ==================================================================
    // ==================  Muita tarkistus metodeja ===================== 
    // ==================================================================  

    public static string TarkistaPostinumero(string postinumero)
    {
        if (string.IsNullOrEmpty(postinumero))
        {
            throw new ArgumentException("Postinumero ei voi olla tyhjää tai null");
        }
        else if (postinumero.Length != 5)
        {
            
        }

        foreach (char merkki in postinumero)
        {
            if (!char.IsDigit(merkki))
            {
                throw new ArgumentException("Postinumeron voi sisältää vain numeroita");
            }
        }
        return postinumero;
    }
    public static uint TarkistaIDuint(uint id)
    {

        if (id < uint.MinValue || id > uint.MaxValue)
        {
            throw new OverflowException(nameof(id));
        }
        return id;
    }

    public static int TarkistaIDint(int id)
    {
        if (id < 0)
        {
            throw new ArgumentException("ID ei voi olla pienempi kuin 0");
        }
        else if (id > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(id));
        }
        return id;
    }
}
