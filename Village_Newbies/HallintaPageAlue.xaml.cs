namespace Village_Newbies;
using Village_Newbies.Services;
using Village_Newbies.ViewModels;
using Microsoft.Maui.Controls;

public partial class HallintaPageAlue : ContentPage
{
    private AlueViewModel _viewModel;

    public HallintaPageAlue()
    {
        InitializeComponent();
        _viewModel = new AlueViewModel(new AlueDatabaseService());
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}