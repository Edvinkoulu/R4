namespace Village_Newbies.Services;

using Village_Newbies.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class LaskutusService
{
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
        decimal kokonaissumma = LaskeKokonaissumma(lasku, valitutPalvelut);

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

    private decimal LaskeKokonaissumma(Lasku lasku, List<Palvelu> palvelut)
    {
        decimal perushinta = (decimal)lasku.summa + palvelut.Sum(p => (decimal)p.Hinta);
        return perushinta * (1 + (decimal)lasku.alv / 100);
    }
}
