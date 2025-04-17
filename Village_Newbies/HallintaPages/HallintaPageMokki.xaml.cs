namespace Village_Newbies.HallintaPages;
using Village_Newbies.ViewModels;

public partial class HallintaPageMokki : ContentPage
{
    private readonly MokkiViewModel _viewModel;

    public HallintaPageMokki()
    {
        InitializeComponent();
        _viewModel = new MokkiViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Tänne voit myöhemmin lisätä datan päivittämisen tarvittaessa
    }
}