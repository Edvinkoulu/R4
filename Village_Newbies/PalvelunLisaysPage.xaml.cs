using MySqlConnector;
using System.Collections.ObjectModel;

namespace Village_Newbies
{
    public partial class PalvelunLisaysPage : ContentPage
    {
        private ObservableCollection<Palvelu> palvelut = new ObservableCollection<Palvelu>();

        public PalvelunLisaysPage()
        {
            InitializeComponent();
            palvelutView.ItemsSource = palvelut;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAlueetAsync();
            await LoadPalvelutAsync();
        }

            // Tallentaa tiedot kantaan ja päivittää listan
        private async void OnTallennaButtonClicked(object sender, EventArgs e)
        {
            string nimi = nimiEntry.Text;
            string kuvaus = kuvausEntry.Text;
            string hintaText = hintaEntry.Text;
            string alvText = alvEntry.Text;

            if (string.IsNullOrWhiteSpace(nimi) || string.IsNullOrWhiteSpace(kuvaus)
                || string.IsNullOrWhiteSpace(hintaText) || string.IsNullOrWhiteSpace(alvText)
                || aluePicker.SelectedItem == null)
            {
                await DisplayAlert("Virhe", "Kaikki kentät tulee täyttää", "OK");
                return;
            }

            if (!double.TryParse(hintaText, out double hinta) ||
                !double.TryParse(alvText, out double alv))
            {
                await DisplayAlert("Virhe", "Virheellinen hinta tai ALV", "OK");
                return;
            }

            var valittuAlue = aluePicker.SelectedItem as Alue;
            if (valittuAlue == null)
            {
                await DisplayAlert("Virhe", "Valitse alue", "OK");
                return;
            }

            try
            {
                var conn = new DatabaseConnection.DatabaseConnector()._getConnection();
                await conn.OpenAsync();

                string query = "INSERT INTO palvelu (alue_id, nimi, kuvaus, hinta, alv) VALUES (@alue_id, @nimi, @kuvaus, @hinta, @alv)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@alue_id", valittuAlue.AlueId);
                cmd.Parameters.AddWithValue("@nimi", nimi);
                cmd.Parameters.AddWithValue("@kuvaus", kuvaus);
                cmd.Parameters.AddWithValue("@hinta", hinta);
                cmd.Parameters.AddWithValue("@alv", alv);

                await cmd.ExecuteNonQueryAsync();
                conn.Close();

                palvelut.Add(new Palvelu
                {
                    Nimi = nimi,
                    Kuvaus = kuvaus,
                    Hinta = hinta.ToString("F2") + " €"
                });

                nimiEntry.Text = "";
                kuvausEntry.Text = "";
                hintaEntry.Text = "";
                alvEntry.Text = "";
                aluePicker.SelectedItem = null;

                await DisplayAlert("Onnistui", "Palvelu lisätty", "OK");
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Virhe", $"Tietokantavirhe: {ex.Message}", "OK");
            }
        }

            //Lataa palvelut kannasta listaan
        private async Task LoadPalvelutAsync()
        {
            palvelut.Clear();

            try
            {
                var conn = new DatabaseConnection.DatabaseConnector()._getConnection();
                await conn.OpenAsync();

                string query = "SELECT palvelu_id, nimi, kuvaus, hinta FROM palvelu";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    palvelut.Add(new Palvelu
                    {
                        PalveluId = reader.GetInt32("palvelu_id"),
                        Nimi = reader.GetString("nimi"),
                        Kuvaus = reader.GetString("kuvaus"),
                        Hinta = reader.GetDouble("hinta").ToString("F2") + " €"
                    });
                }

                conn.Close();
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Virhe", $"Tietokantavirhe: {ex.Message}", "OK");
            }
        }


            // poistetaan palvelun kannasta ja listasta
        private async void OnPoistaButtonClicked(object sender, EventArgs e)
        {
            // Haetaan poistettavan rivin palvelu
            var palvelu = (sender as Button)?.CommandParameter as Palvelu;
            if (palvelu == null)
                return;

            bool confirm = await DisplayAlert("Vahvistus", "Oletko varma, että haluat poistaa tämän palvelun?", "Kyllä", "Ei");
            if (!confirm)
                return;

            try
            {
                var conn = new DatabaseConnection.DatabaseConnector()._getConnection();
                await conn.OpenAsync();

                string query = "DELETE FROM palvelu WHERE palvelu_id = @palvelu_id";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@palvelu_id", palvelu.PalveluId);

                await cmd.ExecuteNonQueryAsync();
                conn.Close();

                palvelut.Remove(palvelu);

                await DisplayAlert("Onnistui", "Palvelu poistettu", "OK");
            }
            catch (MySqlException ex)
            {
                await DisplayAlert("Virhe", $"Tietokantavirhe: {ex.Message}", "OK");
            }
        }

        // hakee alueet drop down valikkoon
        private async Task LoadAlueetAsync()
        {
            try
            {
                var conn = new DatabaseConnection.DatabaseConnector()._getConnection();
                await conn.OpenAsync();

                string query = "SELECT alue_id FROM alue";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                var alueet = new List<Alue>();
                while (await reader.ReadAsync())
                {
                    alueet.Add(new Alue
                    {
                        AlueId = reader.GetInt32("alue_id")
                    });
                }

                aluePicker.ItemsSource = alueet;
                conn.Close();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Virhe", $"Alueiden haku epäonnistui: {ex.Message}", "OK");
            }
        }

            //palvelu ja alue apufunktiont käsittelyä varten
        public class Palvelu
        {
            public int PalveluId { get; set; }
            public string Nimi { get; set; }
            public string Kuvaus { get; set; }
            public string Hinta { get; set; }
        }

        public class Alue
        {
            public int AlueId { get; set; }

            public override string ToString()
            {
                return $"Alue {AlueId}";
            }
        }
    }
}
