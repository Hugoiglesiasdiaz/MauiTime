namespace MauiTime;

using MauiTime.Services;
using System;
using System.Threading.Tasks;

public partial class App : Application
{
    public App() 
    {
        InitializeComponent();

        // 🚨 EL CAPTURADOR SUPREMO DE CRASHES:
        // Si algo volviese a fallar en los hilos de Windows, nos guardará el chivato en el Disco D
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            var ex = (Exception)error.ExceptionObject;
            try
            {
                System.IO.File.WriteAllText(@"D:\Error_Crash_Maui.txt", ex.ToString());
            }
            catch { }
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Creamos la ventana de forma nativa e impecable
        var window = new Window(new AppShell());

        // EVITAMOS TASK.RUN: Usamos el Dispatcher de Windows para inicializar la base de datos
        // de forma segura en el segundo frame, cuando la ventana ya está asentada.
        window.Dispatcher.Dispatch(async () =>
        {
            var dbService = IPlatformApplication.Current?.Services.GetService<DatabaseService>();
            if (dbService != null)
            {
                // Inicializa la base de datos en su propio hilo asíncronizado limpio
                await dbService.ResetDatabaseAsync();
                await dbService.SeedDataAsync();
            }
        });

        return window;
    }
}
