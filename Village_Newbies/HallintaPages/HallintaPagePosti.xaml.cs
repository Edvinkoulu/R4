namespace Village_Newbies.HallintaPages;

using Village_Newbies.ViewModels;
using Village_Newbies.Services;

public partial class HallintaPagePosti : ContentPage
{
    private readonly PostiViewModel _vm;

    public HallintaPagePosti()
    {
        InitializeComponent();
        _vm = new PostiViewModel(new PostiDatabaseService());
        BindingContext = _vm;
    }
}