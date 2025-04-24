namespace Village_Newbies;

using Village_Newbies.Models;
using Village_Newbies.Services;
using Microsoft.Maui.Controls;
using System.Collections.Generic;

public partial class LaskutusPage : ContentPage
{
    private readonly LaskutusService _laskutusService = new LaskutusService();

    public LaskutusPage()
    {
        InitializeComponent();

    }

    // Tämä on vain testaamista varten. Siirretään vaikka asiakasvarauksen loppuun painikkeeseen "Tee lasku".
    // Tulostaa PDF tiedoston "Tiedostot / MyDocuments kansioon.
    private async void OnLuoLaskuButtonClicked(object sender, EventArgs e)
    {
        // Dummy-data laskua varten
        var lasku = new Lasku(1, 101, 100.00, 25.5); // lasku_id, varaus_id, summa, ALV
        var varaus = new Varaus { varaus_id = 101, asiakas_id = 1, mokki_id = 1, varattu_alkupvm = DateTime.Now.AddDays(1), varattu_loppupvm = DateTime.Now.AddDays(3) };
        var asiakas = new Asiakas
        {
            etunimi = "Testi",
            sukunimi = "Asiakas",
            lahiosoite = "Testikatu 1",
            email = "testi@example.com",
            puhelinnro = "0401234567",
            postinro = "12345"

        };
        var mokki = new Mokki { Mokkinimi = "Testimökki" };
        var valitutPalvelut = new List<Palvelu>
        {
            new Palvelu { Nimi = "Dummy Palvelu 1", Hinta = 10.00, Alv = 10 },
            new Palvelu { Nimi = "Dummy Palvelu 2", Hinta = 20.00, Alv = 24 }
        };

        _laskutusService.LuoLaskuPdf(lasku, varaus, asiakas, mokki, valitutPalvelut);
    }
    // Tällä testataan sähköpostilaskun lähetystä.
    private async void OnLahetaSahkopostiLaskuButtonClicked(object sender, EventArgs e)
    {
        // Dummy-data laskua varten
        var lasku = new Lasku(2, 102, 320.00, 25.5); // lasku_id, varaus_id, summa, ALV
        var varaus = new Varaus { varaus_id = 102, asiakas_id = 2, mokki_id = 1, varattu_alkupvm = DateTime.Now.AddDays(5), varattu_loppupvm = DateTime.Now.AddDays(8) };
        var asiakas = new Asiakas
        {
            etunimi = "Matti",
            sukunimi = "Meikäläinen",
            lahiosoite = "Kauppakatu 100",
            email = "mattimeikalainen@example.com",
            puhelinnro = "0407654321",
            postinro = "70100"

        };
        var mokki = new Mokki { Mokkinimi = "Testimökki" };
        var valitutPalvelut = new List<Palvelu>
        {
            new Palvelu { Nimi = "Dummy Palvelu 1", Hinta = 9.99, Alv = 25.5 },
            new Palvelu { Nimi = "Dummy Palvelu 2", Hinta = 19.99, Alv = 25.5 },
            new Palvelu { Nimi = "Dummy Palvelu 3", Hinta = 29.99, Alv = 25.5 },
            new Palvelu { Nimi = "Dummy Palvelu 4", Hinta = 39.99, Alv = 25.5 }
        };
        _laskutusService.LuoJaLahetaEmailLasku(lasku, varaus, asiakas, mokki, valitutPalvelut);
    }
}