using MauiTime.Services;
using MauiTime.Models;

namespace MauiTime.Services;

public class DiagnosticService
{
    private readonly DatabaseService _db;
    private readonly NotificationService _notification;

    public DiagnosticService(DatabaseService db, NotificationService notification)
    {
        _db = db;
        _notification = notification;
    }

    public async Task RunDiagnostic()
{
    try
    {
        var testEvent = new Evento { Titulo = "Test DB", FechaHora = DateTime.Now.AddDays(1) };
        await _db.GuardarEventoAsync(testEvent);
        var eventos = await _db.ObtenerEventosAsync();
        
        MainThread.BeginInvokeOnMainThread(async () => {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
                await page.DisplayAlertAsync("Diagnóstico", $"Éxito: DB funciona. Eventos: {eventos.Count}", "OK");
        });
    }
    catch (Exception ex)
    {
        MainThread.BeginInvokeOnMainThread(async () => {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page != null)
                await page.DisplayAlertAsync("Error", $"Fallo: {ex.Message}", "Cerrar");
        });
    }
}
}