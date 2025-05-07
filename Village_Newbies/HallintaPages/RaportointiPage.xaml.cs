namespace Village_Newbies.HallintaPages;
using Village_Newbies.ViewModels;
using Village_Newbies.Services;

 public partial class RaportointiPage : ContentPage
{
    public RaportointiPage()
    {
        InitializeComponent();

        BindingContext = new RaportointiViewModel(new PalveluDatabaseService()); 
       
    }
}
