
namespace Village_Newbies.HallintaPages;

using Village_Newbies.ViewModels;

public partial class HallintaPageAsiakas : ContentPage
{
    public HallintaPageAsiakas()
    {
        InitializeComponent();
        BindingContext = new AsiakasHallintaViewModel();
    }
}
