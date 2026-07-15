using System;
using Microsoft.Maui.Controls;
using MauiTime.Models;
using MauiTime.Services;
using static MauiTime.Models.Evento; // Mapeo de tus Enums auténticos

namespace MauiTime.Views
{
    public partial class NuevaTareaPopup : ContentPage
    {
        private readonly DatabaseService _dbService;


        public NuevaTareaPopup(DateTime fechaInicial, DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            // 1. Inicializamos el selector de fecha con el día que pulsaste en el calendario
            PickerFecha.Date = fechaInicial;

            // 2. Seteamos por defecto la hora actual del sistema en formato limpio
            PickerHora.Time = DateTime.Now.TimeOfDay;

            PickerFrecuencia.SelectedIndex = 0; // "Ninguna" por defecto
            LabelFecha.Text = $"NUEVO OBJETIVO: {fechaInicial:dd / MM / yyyy}";
        }

        // 💡 SOLUCIÓN: Cambiamos 'object sender' a 'object? sender' para limpiar el warning CS8622
        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            // Validación ágil de campo obligatorio
            if (string.IsNullOrWhiteSpace(EntryTitulo.Text))
            {
                await DisplayAlertAsync("ALERTA", "El evento requiere un título obligatorio.", "ENTENDIDO");
                return;
            }

            // 🛡️ CORRECCIÓN: Quitados los ?? null ya que Date y Time en MAUI nunca son nulos
            TimeSpan horaSeleccionada = PickerHora.Time.GetValueOrDefault();
            DateTime fechaDatePicker = PickerFecha.Date.GetValueOrDefault();

            // Construimos la fecha pura elegida por el usuario en el formulario
            DateTime fechaHoraFinal = new DateTime(
                fechaDatePicker.Year,
                fechaDatePicker.Month,
                fechaDatePicker.Day,
                horaSeleccionada.Hours,
                horaSeleccionada.Minutes,
                0
            );

            string seleccionTexto = PickerFrecuencia.SelectedItem?.ToString() ?? "Ninguna";
            FrecuenciaEvento frecuenciaEnum = seleccionTexto switch
            {
                "Diario" => FrecuenciaEvento.Diario,
                "Semanal" => FrecuenciaEvento.Semanal,
                "Mensual" => FrecuenciaEvento.Mensual,
                "Anual" => FrecuenciaEvento.Anual,
                _ => FrecuenciaEvento.Ninguna
            };

            // Creamos el objeto con los datos limpios en frío y estética mayúsculas
            var nuevoEvento = new Evento
            {
                Titulo = EntryTitulo.Text.Trim().ToUpper(),
                Descripcion = EditorDescripcion.Text?.Trim() ?? "SIN DETALLES ADICIONALES.",
                FechaHora = fechaHoraFinal,
                Frecuencia = frecuenciaEnum,
                DuracionAvisoTicks = TimeSpan.Zero.Ticks, // Suena a la hora exacta elegida

                // 🎯 INYECTA ESTA NUEVA LÍNEA: Conecta el objeto con el interruptor del formulario
                EsAlarmaAgresiva = SwitchAlarmaAgresiva.IsToggled
            };


            // 1. Calculas la fecha correcta en memoria pasándola al futuro de ser necesario
            nuevoEvento.CalcularProximoAviso();

            // 2. ⚡ CRÍTICO: Lo grabas físicamente en la base de datos de SQLite
            await _dbService.GuardarEventoAsync(nuevoEvento);

            // =========================================================================
            // 🎯 NUEVO REQUISITO: INYECCIÓN DE ALERTA EN ANDROID Y WINDOWS
            // =========================================================================
            try
            {
                // Instanciamos el servicio y agendamos la notificación de forma asíncrona
                var notifier = new MauiTime.Services.NotificationService();
                await notifier.ProgramarRecordatorio(nuevoEvento);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NOTIFICACIÓN POPUP ERROR] {ex.Message}");
            }

            // Cerramos la vista modal limpiamente
            await Navigation.PopModalAsync();
        }



        // 💡 SOLUCIÓN: Cambiamos 'object sender' a 'object? sender'
        private async void OnCancelarClicked(object? sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }


    }
}
