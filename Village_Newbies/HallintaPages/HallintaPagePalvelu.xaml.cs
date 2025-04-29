namespace Village_Newbies.HallintaPages;
using Village_Newbies.ViewModels;
using Village_Newbies.Services;
public partial class HallintaPagePalvelu : ContentPage
    {
        public HallintaPagePalvelu()
        {
            InitializeComponent();

            BindingContext = new PalveluViewModel(new PalveluDatabaseService());
        }
        
    }

