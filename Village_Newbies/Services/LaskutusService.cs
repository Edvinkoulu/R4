namespace Village_Newbies.Services;
using Village_Newbies.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

// Lähetä lasku sähköpostiin kutsumalla LahetaEmailLasku tai tulosta paperilasku LuoLaskuPdf metodilla
// Anna parametreiksi tietokannasta kaivetut: lasku, varaus, asiakas, mökki ja lista asiakkaan valitsemista palveluista.
// PDF tiedosto tallennetaan MyDocuments (tai Tiedostot) kansioon.

public class LaskutusService
{
    public const int ERAPAIVA = 14; //Kuinka monen päivän päästä on laskun eräpäivä.
    private readonly LaskuDatabaseService laskuDatabaseService;
    string laskutFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Village_Newbies", "Laskut");
    public LaskutusService()
    {
        laskuDatabaseService = new LaskuDatabaseService();
        LuoLaskutKansio();
    }
    public LaskutusService(LaskuDatabaseService _laskuDatabaseService)
    {
        laskuDatabaseService = _laskuDatabaseService;
        LuoLaskutKansio();
    }
    private void LuoLaskutKansio()
    {
        if (!Directory.Exists(laskutFolderPath))
        {
            Directory.CreateDirectory(laskutFolderPath);
            Console.WriteLine($"Luotu laskukansio polkuun: {laskutFolderPath}");
        }
        else
        {
            Console.WriteLine($"Laskukansio on jo olemassa polussa: {laskutFolderPath}");
        }
    }
    public async Task LuoJaLahetaEmailLasku(Lasku lasku, Varaus varaus, Asiakas asiakas, Mokki mokki, bool onMaksumuistutus = false)
    {
        List<Palvelu> valitutPalvelut = await laskuDatabaseService.HaeLaskunPalvelut(lasku);
        LuoLasku(lasku, varaus, asiakas, mokki, valitutPalvelut, onMaksumuistutus);
        {
            if (asiakas != null && !string.IsNullOrEmpty(asiakas.email))
            {
                string subject = $"{(onMaksumuistutus ? "Maksumuistutus" : "Uusi Lasku")} - Varaus {varaus.varaus_id}";
                string body = $"Hyvä {asiakas.etunimi} {asiakas.sukunimi},\n\n" +
                                    $"Liitteenä löydät {(onMaksumuistutus ? "maksumuistutuksen" : "laskun")} varauksestanne.\n\n" +
                                    "Kiitos varauksestanne!\n\n" +
                                    "Ystävällisin terveisin,\n" +
                                    "Village Newbies";
                await Application.Current.MainPage.DisplayAlert("Sähköposti Luonnosteltu (simuloitu lähetys)", $"Vastaanottaja: {asiakas.email}\nAihe: {subject}\nSisältö: {body}\n\n(Liite: {(onMaksumuistutus ? "Maksumuistutus" : $"Lasku")}_{lasku.lasku_id}_{asiakas.sukunimi}.pdf", "OK");
                // Tähän sähköpostin lähetys koodit.
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Virhe", "Asiakkaan sähköpostiosoitetta ei löydy.", "OK");
            }
        }
    }
    public async Task LuoLaskuPdf(Lasku lasku, Varaus varaus, Asiakas asiakas, Mokki mokki, bool onMaksumuistutus = false)
    {
        var valitutPalvelut = await laskuDatabaseService.HaeLaskunPalvelut(lasku);
        LuoLasku(lasku, varaus, asiakas, mokki, valitutPalvelut, onMaksumuistutus);
    }
    private async Task LuoLasku(Lasku lasku, Varaus varaus, Asiakas asiakas, Mokki mokki, List<Palvelu> valitutPalvelut, bool onMaksumuistutus)
    {
        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Height(50).Background(Colors.Grey.Lighten1).Padding(10)
                        .AlignCenter()
                        .Text(onMaksumuistutus ? "Maksumuistutus" : "Lasku").FontSize(20).Bold().FontColor(Colors.Black);

                    page.Content().Padding(10)
                        .Column(column =>
                        {
                            LisaaYrityksenTiedot(column);
                            LisaaAsiakastiedot(column, asiakas);
                            LisaaVaraustiedot(column, varaus, mokki);
                            LisaaLaskutaulukko(column, lasku, varaus, valitutPalvelut, onMaksumuistutus ? "HETI" : $"{DateTime.Now.AddDays(ERAPAIVA):d}");
                        });

                    page.Footer().Height(30).Background(Colors.Grey.Lighten1).Padding(10)
                        .AlignCenter()
                        .Text($"{(onMaksumuistutus ? "Maksumuistutus luotu" : "Lasku luotu")}: {DateTime.Now:d}").FontSize(8).FontColor(Colors.Black);
                });
            });
            string fileName = $"{(onMaksumuistutus ? "Maksumuistutus" : $"Lasku")}_{lasku.lasku_id}_{asiakas.sukunimi}.pdf";
            string filePath = Path.Combine(laskutFolderPath, fileName);

            document.GeneratePdf(filePath);
            Console.WriteLine($"PDF {(onMaksumuistutus ? "maksumuistutus" : "lasku")} luotu polkuun: {filePath}");
            await Application.Current.MainPage.DisplayAlert("Valmis", $"PDF {(onMaksumuistutus ? "maksumuistutus" : "lasku")} tallennettu polkuun: {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Virhe", ex.Message, "Ok");
        }
    }
    private void LisaaYrityksenTiedot(ColumnDescriptor column)
    {
        column.Item().Text(YrityksenTiedot.Nimi).Bold();
        column.Item().Text(YrityksenTiedot.Osoite);
        column.Item().Text($"{YrityksenTiedot.Postinro} {YrityksenTiedot.Postitoimipaikka}");
        column.Item().Text($"Y-tunnus: {YrityksenTiedot.YTunnus}");
        column.Item().PaddingTop(15);
    }
    private void LisaaAsiakastiedot(ColumnDescriptor column, Asiakas asiakas)
    {
        column.Item().PaddingBottom(10).Text("Asiakkaan tiedot").Bold();
        column.Item().Text($"{asiakas.etunimi} {asiakas.sukunimi}");
        column.Item().Text(asiakas.lahiosoite);
        column.Item().Text(asiakas.postinro);
        column.Item().Text($"Sähköposti: {asiakas.email}");
    }
    private void LisaaVaraustiedot(ColumnDescriptor column, Varaus varaus, Mokki mokki)
    {
        column.Item().PaddingBottom(10).Text("Varaustiedot").Bold();
        column.Item().Text($"Varausnumero: {varaus.varaus_id}");
        column.Item().Text($"Mökki: {mokki.Mokkinimi}");
        column.Item().Text($"Saapuminen: {varaus.varattu_alkupvm:d}");
        column.Item().Text($"Lähtö: {varaus.varattu_loppupvm:d}");
        column.Item().PaddingTop(10);
    }
    private void LisaaLaskutaulukko(ColumnDescriptor column, Lasku lasku, Varaus varaus, List<Palvelu> valitutPalvelut, string eräpäiväTeksti)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Kuvaus
                columns.RelativeColumn(1); // Määrä
                columns.RelativeColumn(1); // Hinta (€)
                columns.RelativeColumn(1); // ALV (%)
                columns.RelativeColumn(1); // Yhteensä (€)
            });

            table.Header(header =>
            {
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Kuvaus").Bold();
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Määrä").Bold().AlignRight();
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Hinta (€)").Bold().AlignRight();
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("ALV (%)").Bold().AlignRight();
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Yhteensä (€)").Bold().AlignRight();
            });

            // Majoitusrivi
            double majoitusHintaAlv = lasku.summa * (1 + (lasku.alv / 100));
            table.Cell().Text($"Majoitus ajalla {varaus.varattu_alkupvm:d} - {varaus.varattu_loppupvm:d}");
            table.Cell().Text("1").AlignRight();
            table.Cell().Text($"{lasku.summa:F2}").AlignRight();
            table.Cell().Text($"{lasku.alv:F2}").AlignRight();
            table.Cell().Text($"{majoitusHintaAlv:F2}").AlignRight();

            // Palvelurivit
            foreach (var palvelu in valitutPalvelut)
            {
                double palveluHintaAlv = palvelu.Hinta * (1 + (palvelu.Alv / 100));
                table.Cell().Text(palvelu.Nimi);
                table.Cell().Text("1").AlignRight();
                table.Cell().Text($"{palvelu.Hinta:F2}").AlignRight();
                table.Cell().Text($"{palvelu.Alv:F2}").AlignRight();
                table.Cell().Text($"{palveluHintaAlv:F2}").AlignRight();
            }

            table.Cell().ColumnSpan(5).PaddingTop(10).BorderTop(1).Text(""); // Tyhjä rivi ennen kokonaissummaa

            // Kokonaissumma-rivi
            double kokonaissumma = LaskeKokonaissumma(lasku, valitutPalvelut);
            table.Cell().ColumnSpan(4).BorderTop(1).PaddingTop(5).Text("Yhteensä").Bold().AlignRight();
            table.Cell().BorderTop(1).PaddingTop(5).Text($"{kokonaissumma:F2} €").Bold().AlignRight();
            table.Cell().ColumnSpan(5).PaddingTop(5).AlignLeft()
            .Column(x =>
            {
                x.Item().Text($"Tilinumero: {YrityksenTiedot.tilinumero}");
                x.Item().Text($"Eräpäivä: {eräpäiväTeksti}").Bold();
            });
        });
    }
    public double LaskeKokonaissumma(Lasku lasku, List<Palvelu> palvelut)
    {
        double majoitusHintaAlv = lasku.summa * (1 + (lasku.alv / 100));
        double palveluidenHintaAlv = 0;

        foreach (var palvelu in palvelut)
        {
            try
            {
                SyoteValidointi.TarkistaDouble(palvelu.Hinta, 0, double.MaxValue);
                SyoteValidointi.TarkistaDouble(palvelu.Alv, 0, 100);
                palveluidenHintaAlv += palvelu.Hinta * (1 + (palvelu.Alv / 100));
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Virhe palvelun tiedoissa", $"{ex.Message}, Palvelu: {palvelu.Nimi}", "OK");
                Debug.WriteLine($"Virheellinen palvelun tieto: {ex.Message}, Palvelu: {palvelu.Nimi}");
                // Laskenta jatkuu, mutta virhe on ilmoitettu.
            }
        }

        try
        {
            return SyoteValidointi.TarkistaDouble(majoitusHintaAlv + palveluidenHintaAlv, 0, double.MaxValue);
        }
        catch (Exception ex)
        {
            Application.Current.MainPage.DisplayAlert("Virhe kokonaissummassa", $"{ex.Message}", "OK");
            Debug.WriteLine($"Virheellinen kokonaissumma: {ex.Message}");
            return 0;
        }
    }
    // OVERLOADIT, purkkaliima jesari ratkaisu. Idea on vaihtaa mokkiUint luokan -> Mokki luokaksi jos kutsuva luokka käytti pikakorjausta.
    // Tämä on nyt pysyvä väliaikaisratkaisu olioiden luonti ongelmiin. Tein koska tämä oli nopea ratkaisu ja aika vähissä.
    // TODO: nämä voi poistaa kunhan koodi on siistitty.
    public async Task LuoLaskuPdf(Lasku lasku, Varaus varaus, Asiakas asiakas, MokkiUint mokkiUint, bool onkoMaksumuistutus = false)
    {
        Mokki mokki = WorkaroundMokki(mokkiUint);
        await LuoLaskuPdf(lasku, varaus, asiakas, mokki, onkoMaksumuistutus);
    }
    public async Task LuoJaLahetaEmailLasku(Lasku lasku, Varaus varaus, Asiakas asiakas, MokkiUint mokkiUint, bool onkoMaksumuistutus = false)
    {
        Mokki mokki = WorkaroundMokki(mokkiUint);
        await LuoJaLahetaEmailLasku(lasku, varaus, asiakas, mokki, onkoMaksumuistutus);
    }
    private Mokki WorkaroundMokki(MokkiUint mokkiUint)
    {
        Mokki mokki = new Mokki
        {
            mokki_id = (int)mokkiUint.mokki_id,
            alue_id = (int)mokkiUint.alue_id,
            Postinro = mokkiUint.Postinro,
            Mokkinimi = mokkiUint.Mokkinimi,
            Katuosoite = mokkiUint.Katuosoite,
            Hinta = mokkiUint.Hinta,
            Kuvaus = mokkiUint.Kuvaus,
            Henkilomaara = mokkiUint.Henkilomaara,
            Varustelu = mokkiUint.Varustelu
        };
        return mokki;
    }
}
