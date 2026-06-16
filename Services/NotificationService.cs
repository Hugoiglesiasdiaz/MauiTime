using Plugin.LocalNotification;
using MauiTime.Models;
using Plugin.LocalNotification.Core.Models;

namespace MauiTime.Services;

public class NotificationService
{
    public async Task ProgramarRecordatorio(Evento evento)
    {
        var fechaNotificacion = evento.CalcularProximoAviso();

        // En la v14.1, el objeto es NotificationRequest
        var request = new NotificationRequest
        {
            NotificationId = evento.Id,
            Title = "Recordatorio: " + evento.Titulo,
            Description = evento.Descripcion,
            // En v14, la programación se hace así:
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = fechaNotificacion,
                RepeatType = evento.EsAnual ? NotificationRepeat.TimeInterval : NotificationRepeat.No,
                // La propiedad correcta en v14.1 es RepeatInterval
                NotifyRepeatInterval = evento.EsAnual ? TimeSpan.FromDays(365) : null
            }
        };

        // Y para mostrarlo usamos el centro de notificaciones
        await LocalNotificationCenter.Current.Show(request);
    }
}