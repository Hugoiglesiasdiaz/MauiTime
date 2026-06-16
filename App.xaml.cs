using MauiTime;
using MauiTime.Services;

namespace MauiTime;
public partial class App : Application
{
    private readonly DiagnosticService _diagnostic;

    public App(DiagnosticService diagnostic)
    {
        InitializeComponent();
        _diagnostic = diagnostic;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Definimos la ventana
        var window = new Window(new AppShell());

        /*
        // Disparamos el diagnóstico una vez creada la ventana
        Task.Run(async () => {
            await Task.Delay(1000); // Pequeña pausa para asegurar carga de UI
            await _diagnostic.RunDiagnostic();
        });*/

        return window;
    }
}