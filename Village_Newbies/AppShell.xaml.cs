namespace Village_Newbies;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// Rekisteröidään reitti MokkiPage-näkymälle
        Routing.RegisterRoute("MokkiPage", typeof(Village_Newbies.HallintaPages.HallintaPageMokki));
		Routing.RegisterRoute("AluePage", typeof(Village_Newbies.HallintaPages.HallintaPageAlue));
		Routing.RegisterRoute("LaskuPage", typeof(Village_Newbies.HallintaPages.HallintaPageLasku));
		Routing.RegisterRoute("PalveluPage", typeof(Village_Newbies.HallintaPages.HallintaPagePalvelu));
    
	}
	
}
