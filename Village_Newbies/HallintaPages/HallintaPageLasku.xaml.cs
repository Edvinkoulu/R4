namespace Village_Newbies.HallintaPages;
using Village_Newbies.ViewModels;
using Village_Newbies.Services;

public partial class HallintaPageLasku : ContentPage
{
    private readonly LaskuViewModel _viewModel;

    public HallintaPageLasku()
    {
        InitializeComponent();
        _viewModel = new LaskuViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}