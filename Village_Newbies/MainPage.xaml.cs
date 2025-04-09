namespace Village_Newbies;
using DatabaseConnection;
using MySqlConnector;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	private async void AsiakasVaraus(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AsiakasVarausPage());
    }

	private async void Hallinta(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HallintaPage());
    }

	private async void OnDatabaseClicked(object sender, EventArgs e)
	{
		DatabaseConnector dbc = new DatabaseConnector();
		try
		{
			var conn = dbc._getConnection();
			conn.Open();
			await DisplayAlert("Onnistui", "Tietokanta yhteysaukesi", "OK");
		}
		catch (MySqlException ex)
		{
			await DisplayAlert("Failure", ex.Message, "OK");
		}
	}
}

