namespace Village_Newbies;

public partial class HallintaPage : ContentPage
{
    public HallintaPage()
    {
        InitializeComponent();
    }

            // Avaa palveluiden lisäys sivun
        private async void OnLisaysButtonClicked(object sender, EventArgs e)
        {
            // Siirrytään palvelu sivulle
            await Navigation.PushAsync(new PalvelunLisaysPage());
        }

}
