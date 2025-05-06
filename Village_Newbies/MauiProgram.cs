using Microsoft.Extensions.Logging;

namespace Village_Newbies;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

			// Ohjelma kaatuu jos tämä rivi puuttuu ja käytetään QuestPDF:ää
			QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
			
#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
