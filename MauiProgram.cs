using MauiTime.ViewModels;
using MauiTime.Views;
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
				fonts.AddFont("Bublegum.ttf", "Bubblegum");
				fonts.AddFont("HelveticaNowDisplay-ExtraBold.ttf", "HelveticaNowDisplay-ExtraBold");
			});
		builder.Services.AddSingleton<Services.DatabaseService>();
		builder.Services.AddSingleton<Services.NotificationService>();
		builder.Services.AddSingleton<Services.DiagnosticService>();
		builder.Services.AddTransient<App>();
		builder.Services.AddTransient<AgendaViewModel>();
		builder.Services.AddTransient<AgendaPage>();
		builder.Services.AddTransient<CalendarioPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
