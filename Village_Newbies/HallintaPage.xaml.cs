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
}