using Plugin.LocalNotification;
using MauiTime.Models;
using Plugin.LocalNotification.Core.Models;

namespace MauiTime.Services;

public class NotificationService
{
    public async Task ProgramarRecordatorio(Evento evento)
    {
        var fechaNotificacion = evento.CalcularProximoAviso();

        if (fechaNotificacion <= DateTime.Now)
        {
            return;
        }

        var tipoRepeticion = evento.Frecuencia switch
        {
            Evento.FrecuenciaEvento.Diario => NotificationRepeat.Daily,
            Evento.FrecuenciaEvento.Semanal => NotificationRepeat.Weekly,
            Evento.FrecuenciaEvento.Mensual => NotificationRepeat.Monthly,
            Evento.FrecuenciaEvento.Anual => NotificationRepeat.TimeInterval,
            _ => NotificationRepeat.No
        };

        var request = new NotificationRequest
        {
            NotificationId = evento.Id,
            Title = "🔔 RECORDATORIO: " + evento.Titulo.ToUpper(),
            Description = evento.Descripcion,
            Sound = "default",
            
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = fechaNotificacion,
                RepeatType = tipoRepeticion,
                NotifyRepeatInterval = evento.Frecuencia == Evento.FrecuenciaEvento.Anual 
                    ? TimeSpan.FromDays(365) 
                    : null
            }
        };

        // =========================================================================
        // COMPILACIÓN CONDICIONAL: PROTEGE TU CÓDIGO EN WINDOWS
        // =========================================================================
#if ANDROID
        // Esto solo se compilará si estás corriendo la app en Android
        request.Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
        {
            Launch = new Plugin.LocalNotification.AndroidOption.AndroidLaunchOptions
            {
                OnTriggerReceiveByApp = true
            }
        };
#endif

        LocalNotificationCenter.Current.Cancel(evento.Id);
        await LocalNotificationCenter.Current.Show(request);
    }
}