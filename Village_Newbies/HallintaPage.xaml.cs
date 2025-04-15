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
    private async void OnLisaysButtonClicked(object sender, EventArgs e)
    {
        // Siirrytään palvelu sivulle
        await Navigation.PushAsync(new PalvelunLisaysPage());
    }

    private async void SiirryAlueHallintaan_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HallintaPageAlue());
    }
    private async void SiirryLaskuHallintaan_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HallintaPageLasku());
    }
}