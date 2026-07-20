using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using MauiTime.Models;
using MauiTime.Services;
using static MauiTime.Models.Evento; // Mapeo de tus Enums auténticos

namespace MauiTime.Views
{
    public partial class NuevaTareaPopup : ContentPage
    {
        private readonly DatabaseService _dbService;
        private DateTime _fechaSeleccionada;
        private DateTime _mesVisualizado;

        public NuevaTareaPopup(DateTime fechaInicial, DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            _fechaSeleccionada = DateTime.Now.Date;
            _mesVisualizado = new DateTime(_fechaSeleccionada.Year, _fechaSeleccionada.Month, 1);

            // 1. Inicializamos el selector de fecha en el día actual del sistema
            PickerFecha.Date = _fechaSeleccionada;

            // 2. Seteamos por defecto la hora actual del sistema en formato limpio
            PickerHora.Time = DateTime.Now.TimeOfDay;

            PickerFrecuencia.SelectedIndex = 0; // "Ninguna" por defecto
            LabelFecha.Text = $"NUEVO OBJETIVO: {fechaInicial:dd / MM / yyyy}";

            ActualizarCabeceraMes();
            ConstruirCuadriculaCalendario();
        }

        private void ActualizarCabeceraMes()
        {
            var culturaEsp = CultureInfo.GetCultureInfo("es-ES");
            string mesTexto = _mesVisualizado.ToString("MMMM", culturaEsp).ToUpperInvariant();
            LabelMesAno.Text = $"{mesTexto} DE {_mesVisualizado.Year}";
        }

        private void ConstruirCuadriculaCalendario()
        {
            GridDiasCalendario.Children.Clear();

            DateTime primerDiaDelMes = new DateTime(_mesVisualizado.Year, _mesVisualizado.Month, 1);
            int diasEnMes = DateTime.DaysInMonth(_mesVisualizado.Year, _mesVisualizado.Month);
            int columnaInicio = ((int)primerDiaDelMes.DayOfWeek + 6) % 7;

            for (int celda = 0; celda < 42; celda++)
            {
                int fila = celda / 7;
                int columna = celda % 7;
                int numeroDia = celda - columnaInicio + 1;
                bool esMesActual = numeroDia >= 1 && numeroDia <= diasEnMes;

                var botonDia = CrearBotonDiaCalendario(numeroDia, esMesActual);
                GridDiasCalendario.Add(botonDia, columna, fila);
            }
        }

        private Microsoft.Maui.Controls.Button CrearBotonDiaCalendario(int numeroDia, bool esMesActual)
        {
            var botonDia = new Microsoft.Maui.Controls.Button
            {
                FontFamily = "Impact",
                FontSize = 16,
                CornerRadius = 10,
                HeightRequest = 48,
                WidthRequest = 48,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Padding = new Thickness(0),
                BorderWidth = 0,
                BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent,
                TextColor = Microsoft.Maui.Graphics.Colors.Black
            };

            if (!esMesActual)
            {
                botonDia.IsVisible = false;
                botonDia.IsEnabled = false;
                botonDia.Text = string.Empty;
                return botonDia;
            }

            var fechaActual = new DateTime(_mesVisualizado.Year, _mesVisualizado.Month, numeroDia);
            botonDia.Text = numeroDia.ToString();
            botonDia.BindingContext = fechaActual;

            if (fechaActual.Date == _fechaSeleccionada)
            {
                botonDia.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#E31D26");
                botonDia.TextColor = Microsoft.Maui.Graphics.Color.FromArgb("#FFFFFF");
                botonDia.Rotation = -4;
            }
            else
            {
                botonDia.BackgroundColor = Microsoft.Maui.Graphics.Colors.Transparent;
                botonDia.TextColor = Microsoft.Maui.Graphics.Colors.Black;
                botonDia.Rotation = 0;
            }

            botonDia.Clicked += OnDiaCalendarioClicked;
            return botonDia;
        }

        private void OnMesAnteriorClicked(object? sender, EventArgs e)
        {
            _mesVisualizado = _mesVisualizado.AddMonths(-1);
            ActualizarCabeceraMes();
            ConstruirCuadriculaCalendario();
        }

        private void OnMesSiguienteClicked(object? sender, EventArgs e)
        {
            _mesVisualizado = _mesVisualizado.AddMonths(1);
            ActualizarCabeceraMes();
            ConstruirCuadriculaCalendario();
        }

        private async void OnDiaCalendarioClicked(object? sender, EventArgs e)
        {
            if (sender is Microsoft.Maui.Controls.Button boton && boton.BindingContext is DateTime fecha)
            {
                _fechaSeleccionada = fecha.Date;
                PickerFecha.Date = _fechaSeleccionada;
                LabelFecha.Text = $"NUEVO OBJETIVO: {_fechaSeleccionada:dd / MM / yyyy}";
                ConstruirCuadriculaCalendario();
            }
        }

        private async void OnAceptarCalendarioHud(object? sender, EventArgs e)
        {
            try
            {
                await this.ScaleToAsync(0.95, 120, Microsoft.Maui.Easing.SpringOut);
                await this.ScaleToAsync(1.0, 180, Microsoft.Maui.Easing.BounceOut);
            }
            catch { /* Animaciones no críticas, fail-safe */ }

            await Navigation.PopModalAsync(true);
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
