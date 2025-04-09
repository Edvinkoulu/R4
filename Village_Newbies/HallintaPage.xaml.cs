namespace Village_Newbies;
using Microsoft.Maui.Controls;
using Village_Newbies.ViewModels;


public partial class HallintaPage : ContentPage
{
    public HallintaPage()
    {
        InitializeComponent();
        BindingContext = new MokkiViewModel();
    }

            // Avaa palveluiden lisäys sivun
        private async void OnLisaysButtonClicked(object sender, EventArgs e)
        {
            // Siirrytään palvelu sivulle
            await Navigation.PushAsync(new PalvelunLisaysPage());
        }



            // Avaa palveluiden lisäys sivun
        private async void OnLisaysButtonClicked(object sender, EventArgs e)
        {
            // Siirrytään palvelu sivulle
            await Navigation.PushAsync(new PalvelunLisaysPage());
        }


