using System;
using System.Threading.Tasks;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;
using MauiTime.Models;

namespace MauiTime.Services;

public class NotificationService
{
    public async Task ProgramarRecordatorio(Evento evento)
    {
        if (evento == null) return;

        DateTime ahora = DateTime.Now;
        DateTime horaExactaEvento = evento.FechaHora;

        // 🎯 REGLA DE ORO 1: El Radar de aviso previo se calcula exactamente 30 minutos antes
        DateTime inicioIntervaloAviso = horaExactaEvento.AddMinutes(-30);

        // Si el evento ya pasó por completo de la hora actual, abortamos
        if (horaExactaEvento <= ahora) return;

        // Mapeo básico de ciclos recurrentes
        var tipoRepeticion = evento.Frecuencia switch
        {
            Evento.FrecuenciaEvento.Diario => NotificationRepeat.Daily,
            Evento.FrecuenciaEvento.Semanal => NotificationRepeat.Weekly,
            Evento.FrecuenciaEvento.Mensual => NotificationRepeat.Monthly,
            Evento.FrecuenciaEvento.Anual => NotificationRepeat.TimeInterval,
            _ => NotificationRepeat.No
        };

        // Cancelamos registros previos enlazados al mismo ID para evitar ecos
        LocalNotificationCenter.Current.Cancel(evento.Id);

        // =========================================================================
        // CIRCUITO 1: NOTIFICACIÓN DE PRE-AVISO (30 MINUTOS)
        // =========================================================================
        DateTime tiempoDisparoPreAviso = (ahora >= inicioIntervaloAviso && ahora < horaExactaEvento)
            ? ahora.AddSeconds(1)
            : inicioIntervaloAviso;

        if (tiempoDisparoPreAviso > ahora)
        {
            var requestPreAviso = new NotificationRequest
            {
                NotificationId = evento.Id + 100000,
                Title = $"📢 INFILTRACIÓN INMINENTE: {evento.Titulo.ToUpper()}",
                Description = $"PLAZO LÍMITE EN MENOS DE 30 MINUTOS ({horaExactaEvento:HH:mm}). PREPARA TU EQUIPO.",
                Schedule = new NotificationRequestSchedule
                {
                    NotifyTime = tiempoDisparoPreAviso,
                    RepeatType = tipoRepeticion
                }
            };
#if ANDROID
            requestPreAviso.Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
            {
                ChannelId = "mauitime_misiones",
                LaunchApp = new Plugin.LocalNotification.AndroidOption.AndroidLaunchApp { ForceAppWindowToForeground = true }
            };
#endif
            await LocalNotificationCenter.Current.Show(requestPreAviso);
        }

        // =========================================================================
        // CIRCUITO 2: ALARMA FUEGO (HORA EXACTA) - ¿AGRESIVA O BANNER?
        // =========================================================================
        var requestHoraExacta = new NotificationRequest
        {
            NotificationId = evento.Id, // ID puro de SQLite
            Title = $"🎯 OBJETIVO AVISTADO: {evento.Titulo.ToUpper()}",
            
            // 🎯 REPARACIÓN CRÍTICA EN WINDOWS: Si el switch está APAGADO, inyectamos la marca que frena el modal rojo en App.xaml.cs
            Description = evento.EsAlarmaAgresiva 
                ? evento.Descripcion.ToUpper() 
                : "[BANNER] " + evento.Descripcion.ToUpper(),

            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = horaExactaEvento, // Clavado en el minuto exacto elegido (ej: 11:00)
                RepeatType = tipoRepeticion,
                NotifyRepeatInterval = evento.Frecuencia == Evento.FrecuenciaEvento.Anual ? TimeSpan.FromDays(365) : null
            }
        };

#if ANDROID
        // Exigimos el derecho de hardware para la alarma exacta en Android 14+
        await LocalNotificationCenter.Current.RequestNotificationPermission(new NotificationPermission
        {
            RequestPermissionToScheduleExactAlarm = true
        });

        // 🛡️ BIFURCACIÓN DE HARDWARE EN ANDROID: Evaluamos la decisión del botón del usuario
        if (evento.EsAlarmaAgresiva)
        {
            requestHoraExacta.Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
            {
                ChannelId = "mauitime_alarma_critica", // Canal de importancia Máxima
                Ongoing = true, // No se puede quitar barriendo la pantalla
                LaunchApp = new Plugin.LocalNotification.AndroidOption.AndroidLaunchApp 
                { 
                    ForceAppWindowToForeground = true 
                }
            };
        }
        else
        {
            requestHoraExacta.Android = new Plugin.LocalNotification.AndroidOption.AndroidOptions
            {
                ChannelId = "mauitime_misiones",
                LaunchApp = new Plugin.LocalNotification.AndroidOption.AndroidLaunchApp { ForceAppWindowToForeground = true }
            };
        }
#endif

        await LocalNotificationCenter.Current.Show(requestHoraExacta);
        Console.WriteLine($"[SISTEMA DOBLE CARRIEL] Sincronizadas alertas para ID {evento.Id}");
    }

    public void CancelarRecordatorio(int eventoId)
    {
        LocalNotificationCenter.Current.Cancel(eventoId);
        LocalNotificationCenter.Current.Cancel(eventoId + 100000); // Borramos también el pre-aviso
    }
}
