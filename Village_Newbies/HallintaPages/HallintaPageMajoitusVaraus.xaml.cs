using System;
using Microsoft.Maui.Controls;
using Village_Newbies.Models;
using Village_Newbies.Services;

namespace Village_Newbies.HallintaPages
{
    public partial class HallintaPageMajoitusVaraus : ContentPage
    {
        public HallintaPageMajoitusVaraus()
        {
            InitializeComponent();
        }

        private void OnSearchClicked(object sender, EventArgs e)
        {
            string alue = AreaPicker.SelectedItem?.ToString() ?? "Kaikki";
            DateTime alku = StartDatePicker.Date;
            DateTime loppu = EndDatePicker.Date;

            string minHinta = MinPriceEntry.Text;
            string maxHinta = MaxPriceEntry.Text;
            bool vainVapaat = OnlyAvailableCheckBox.IsChecked;

            Console.WriteLine($"Hakualue: {alue}");
            Console.WriteLine($"Ajanjakso: {alku.ToShortDateString()} - {loppu.ToShortDateString()}");
            Console.WriteLine($"Hintahaarukka: {minHinta} - {maxHinta}");
            Console.WriteLine($"Näytä vain vapaat: {vainVapaat}");
        }

        private async void OnSaveReservationClicked(object sender, EventArgs e)
        {
            string nimi = CustomerNameEntry.Text;
            string puhelin = PhoneEntry.Text;
            string email = EmailEntry.Text;
            string lisapalvelut = ServicesEntry.Text;

            var uusiVaraus = new Varaus
            {
                asiakas_id = 1, // kovakoodattu testiasiakas
                mokki_id = 1,   // kovakoodattu testimökki
                varattu_pvm = DateTime.Now,
                vahvistus_pvm = DateTime.Now,
                varattu_alkupvm = StartDatePicker.Date,
                varattu_loppupvm = EndDatePicker.Date
            };

            try
            {
                var varausService = new VarausDatabaseService();
                await varausService.Lisaa(uusiVaraus);

                await DisplayAlert("Varaus", "Varaus tallennettu tietokantaan!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Tallennuksessa tapahtui virhe: {ex.Message}", "OK");
            }
        }
    }
}
