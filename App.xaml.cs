namespace MauiTime;

using MauiTime.Services;

public partial class App : Application
{
    public App() // Constructor simple, sin inyección
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        // Resolución directa de servicios
        var diagnostic = IPlatformApplication.Current?.Services.GetService<DiagnosticService>();

        // Ejecutamos todo en una sola secuencia lógica
        Task.Run(async () =>
        {
            var dbService = IPlatformApplication.Current?.Services.GetService<DatabaseService>();
            if (dbService != null)
            {
                // Solo resetea si realmente necesitas limpiar todo en cada inicio.
                // Si quieres persistencia, comenta la línea de abajo.
                await dbService.ResetDatabaseAsync();

                // Ahora cargamos los datos
                await dbService.SeedDataAsync();
            }
        });


        /*
        if (diagnostic != null)
        {
            Task.Run(async () => {
                await Task.Delay(1000);
                await diagnostic.RunDiagnostic();
            });
        }
        */

        return window;
    }
}