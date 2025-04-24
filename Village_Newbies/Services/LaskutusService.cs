namespace Village_Newbies.Services;
using Village_Newbies.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

// Lähetä lasku sähköpostiin kutsumalla LahetaEmailLasku tai tulosta paperilasku LuoLaskuPdf metodilla
// Anna parametreiksi tietokannasta kaivetut: lasku, varaus, asiakas, mökki ja lista asiakkaan valitsemista palveluista.
// PDF tiedosto tallennetaan MyDocuments (tai Tiedostot) kansioon.

public class LaskutusService
{
    public async void LuoJaLahetaEmailLasku(Lasku lasku, Varaus varaus, Asiakas asiakas, Mokki mokki, List<Palvelu> valitutPalvelut)
    {
        LuoLaskuPdf(lasku, varaus, asiakas, mokki, valitutPalvelut);
        {
            if (asiakas != null && !string.IsNullOrEmpty(asiakas.email))
            {
                string subject = $"Uusi Lasku - Varaus {varaus.varaus_id}";
                string body = $"Hyvä {asiakas.etunimi} {asiakas.sukunimi},\n\n" +
                              "Liitteenä löydät laskun varauksestanne.\n\n" +
                              "Kiitos varauksestanne!\n\n" +
                              "Ystävällisin terveisin,\n" +
                              "Village Newbies";

                await Application.Current.MainPage.DisplayAlert("Sähköposti Luonnosteltu", $"Vastaanottaja: {asiakas.email}\nAihe: {subject}\nSisältö: {body}\n\n(Liite: Lasku_{lasku.lasku_id}_{asiakas.sukunimi}.pdf - (simuloitu lähetys)", "OK");
                // Tähän sähköpostin lähetys koodit.
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Virhe", "Asiakkaan sähköpostiosoitetta ei löydy.", "OK");
            }
        }
    }

    public void LuoLaskuPdf(Lasku lasku, Varaus varaus, Asiakas asiakas, Mokki mokki, List<Palvelu> valitutPalvelut)
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
                    .Text("Lasku").FontSize(20).Bold().FontColor(Colors.Black);

                page.Content().Padding(10)
                    .Column(column =>
                    {
                        LisaaAsiakastiedot(column, asiakas);
                        LisaaMokinTiedot(column, mokki);
                        LisaaVaraustiedot(column, varaus);
                        LisaaLaskutaulukko(column, lasku, varaus, valitutPalvelut);
                    });

                page.Footer().Height(30).Background(Colors.Grey.Lighten1).Padding(10)
                    .AlignCenter()
                    .Text($"Lasku luotu: {DateTime.Now:d}").FontSize(8).FontColor(Colors.Black);
            });
        });

        string fileName = $"Lasku_{lasku.lasku_id}_{asiakas.sukunimi}.pdf";
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
        document.GeneratePdf(filePath);

        Console.WriteLine($"PDF-lasku luotu polkuun: {filePath}");
        Application.Current.MainPage.DisplayAlert("Valmis", $"PDF-lasku tallennettu polkuun: {filePath}", "OK");
    }

    private void LisaaAsiakastiedot(ColumnDescriptor column, Asiakas asiakas)
    {
        column.Item().PaddingBottom(5).Text("Asiakkaan tiedot").Bold();
        column.Item().Text($"{asiakas.etunimi} {asiakas.sukunimi}");
        column.Item().Text(asiakas.lahiosoite);
        column.Item().Text(asiakas.postinro);
        column.Item().Text($"Sähköposti: {asiakas.email}");
    }

    private void LisaaMokinTiedot(ColumnDescriptor column, Mokki mokki)
    {
        column.Item().Text($"Mökki: {mokki.Mokkinimi}");
        column.Spacing(15);
    }

    private void LisaaVaraustiedot(ColumnDescriptor column, Varaus varaus)
    {
        column.Item().PaddingBottom(5).Text("Varaustiedot").Bold();
        column.Item().Text($"Varausnumero: {varaus.varaus_id}");
        column.Item().Text($"Saapuminen: {varaus.varattu_alkupvm:d}");
        column.Item().Text($"Lähtö: {varaus.varattu_loppupvm:d}");
        column.Item().PaddingTop(10);
    }

    private void LisaaLaskutaulukko(ColumnDescriptor column, Lasku lasku, Varaus varaus, List<Palvelu> valitutPalvelut)
    {
        double kokonaissumma = LaskeKokonaissumma(lasku, valitutPalvelut);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Kuvaus").Bold();
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Määrä").Bold().AlignRight();
                header.Cell().BorderBottom(1).PaddingBottom(5).Text("Hinta (€)").Bold().AlignRight();
            });

            table.Cell().Text($"Majoitus ajalla {varaus.varattu_alkupvm:d} - {varaus.varattu_loppupvm:d}");
            table.Cell().Text("1").AlignRight();
            table.Cell().Text($"{lasku.summa:F2}").AlignRight();

            foreach (var palvelu in valitutPalvelut)
            {
                table.Cell().Text(palvelu.Nimi);
                table.Cell().Text("1").AlignRight();
                table.Cell().Text($"{palvelu.Hinta:F2}").AlignRight();
            }

            table.Cell().ColumnSpan(3).Text("");

            // ALV-rivi
            table.Cell().ColumnSpan(2).BorderTop(1).PaddingTop(5).Text("ALV").Bold().AlignRight();
            table.Cell().BorderTop(1).PaddingTop(5).Text($"{lasku.alv:F2} %").AlignRight();

            // Yhteensä-rivi
            table.Cell().ColumnSpan(2).BorderTop(1).PaddingTop(5).Text("Yhteensä").Bold().AlignRight();
            table.Cell().BorderTop(1).PaddingTop(5).Text($"{kokonaissumma:F2} €").Bold().AlignRight();
        });
    }

    private double LaskeKokonaissumma(Lasku lasku, List<Palvelu> palvelut)
    {
        double kokonaissumma = (double)lasku.summa * (1 + (double)lasku.alv / 100); // Lasketaan majoituksen osuus ALV:lla

        foreach (var palvelu in palvelut)
        {
            try
            {
                SyoteValidointi.TarkistaDouble(palvelu.Hinta, 0, double.MaxValue);
                SyoteValidointi.TarkistaDouble(palvelu.Alv, 0, 100);
                kokonaissumma += (double)palvelu.Hinta * (1 + (double)palvelu.Alv / 100);
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Virhe laskun summassa", $"{ex.Message}, Palvelun tiedot: {palvelu.Nimi}, {palvelu.Alv}, {palvelu.Hinta}. Luo lasku uudelleen korjausten jälkeen.", "Jatka");
                Debug.WriteLine($"Virheellinen syöte palvelua {palvelu.Nimi}, {palvelu.Alv}, {palvelu.Hinta} laskettaessa: {ex.Message}");
                // Laskenta jatkuu, mutta virhe on ilmoitettu.
            }
        }
        try
        {
            return SyoteValidointi.TarkistaDouble(kokonaissumma, 0, double.MaxValue);
        }
        catch (Exception ex)
        {
            Application.Current.MainPage.DisplayAlert("Virhe", $"{ex.Message}", "OK");
            Debug.WriteLine($"Virheellinen kokonaissumma: {ex.Message}");
            return 0; // Palautetaan 0 virheen sattuessa kokonaissummassa.
        }
    }
}
