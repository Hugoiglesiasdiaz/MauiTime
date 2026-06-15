using Microsoft.Extensions.Logging;

namespace MauiTime;

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
				fonts.AddFont("Bublegum.ttf", "Bubblegum Superstar");
				fonts.AddFont("Bubblegum.ttf", "Bubblegum");
				fonts.AddFont("HelveticaNowDisplay-ExtraBold.ttf", "HelveticaNowDisplay-ExtraBold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
