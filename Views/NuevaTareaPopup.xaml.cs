using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using MauiTime.Models;
using MauiTime.Services;
using static MauiTime.Models.Evento;

namespace MauiTime.Views
{
    public partial class NuevaTareaPopup : ContentPage
    {
        private readonly DatabaseService _dbService;
        private DateTime _fechaSeleccionada;
        private DateTime _mesVisualizado;

        // Búferes de desplazamiento fluidos para las horas
        private double _scrollXHoras = 0;
        private double _scrollXMinutos = 0;

        private CancellationTokenSource? _ctsHoras;
        private CancellationTokenSource? _ctsMinutos;

        // ⚔️ SISTEMA DE DESPLEGABLE INTERNO: Opciones y control de estado
        private readonly List<string> _opcionesFrecuencia = new() { "NINGUNA", "DIARIO", "SEMANAL", "MENSUAL", "ANUAL" };
        private int _indiceFrecuenciaActual = 0;
        private bool _desplegableAbierto = false;

        public NuevaTareaPopup(DateTime fechaInicial, DatabaseService dbService)
        {
            InitializeComponent();
            _dbService = dbService;

            _fechaSeleccionada = DateTime.Now.Date;
            _mesVisualizado = new DateTime(_fechaSeleccionada.Year, _fechaSeleccionada.Month, 1);

            PickerFecha.Date = _fechaSeleccionada;
            LabelFecha.Text = $"NUEVO OBJETIVO: {fechaInicial:dd / MM / yyyy}";

            // Inicializamos el texto del campo select y ocultamos el menú de opciones
            LabelFrecuenciaCustom.Text = _opcionesFrecuencia[_indiceFrecuenciaActual];
            ContenedorOpcionesDrop.IsVisible = false;

            CargarRuedasPersonalizadas();
            ActualizarCabeceraMes();
            ConstruirCuadriculaCalendario();
        }

        private async void OnFrecuenciaHeaderTapped(object? sender, TappedEventArgs e)
        {
            _desplegableAbierto = !_desplegableAbierto;

            if (_desplegableAbierto)
            {
                // Mostramos el panel y hacemos que brote expandiendo su escala vertical
                ContenedorOpcionesDrop.IsVisible = true;
                ContenedorOpcionesDrop.Opacity = 0;
                ContenedorOpcionesDrop.ScaleY = 0;
                ContenedorOpcionesDrop.AnchorY = 0; // El eje de expansión es la parte superior

                await Task.WhenAll(
                    ContenedorOpcionesDrop.FadeToAsync(1, 120, Easing.CubicOut),
                    ContenedorOpcionesDrop.ScaleYToAsync(1, 180, Easing.SpringOut),
                    IndicadorAngulo.RotateToAsync(180, 120, Easing.CubicOut)
                );
            }
            else
            {
                // Contracción fluida de la persiana
                await Task.WhenAll(
                    ContenedorOpcionesDrop.FadeToAsync(0, 100, Easing.CubicIn),
                    ContenedorOpcionesDrop.ScaleYToAsync(0, 100, Easing.CubicIn),
                    IndicadorAngulo.RotateToAsync(0, 100, Easing.CubicIn)
                );
                ContenedorOpcionesDrop.IsVisible = false;
            }
        }

        private async void OnOpcionSeleccionadaTapped(object? sender, TappedEventArgs e)
        {
            if (sender is Label etiquetaOpcion)
            {
                string textoElegido = etiquetaOpcion.Text;
                _indiceFrecuenciaActual = _opcionesFrecuencia.IndexOf(textoElegido);
                LabelFrecuenciaCustom.Text = textoElegido;

                // Forzamos el cierre del desplegable tras la selección
                _desplegableAbierto = false;
                await Task.WhenAll(
                    ContenedorOpcionesDrop.FadeToAsync(0, 90, Easing.CubicIn),
                    ContenedorOpcionesDrop.ScaleYToAsync(0, 90, Easing.CubicIn),
                    IndicadorAngulo.RotateToAsync(0, 90, Easing.CubicIn)
                );
                ContenedorOpcionesDrop.IsVisible = false;
            }
        }

        private void CargarRuedasPersonalizadas()
        {
            var horas = new List<string>();
            for (int ciclo = 0; ciclo < 50; ciclo++)
            {
                for (int i = 0; i < 24; i++) horas.Add(i.ToString("D2"));
            }

            var minutos = new List<string>();
            for (int ciclo = 0; ciclo < 50; ciclo++)
            {
                for (int i = 0; i < 60; i++) minutos.Add(i.ToString("D2"));
            }

            ListaHoras.ItemsSource = horas;
            ListaMinutos.ItemsSource = minutos;

            var horaActual = DateTime.Now;

            Dispatcher.Dispatch(() =>
            {
                int puntoMedioHoras = (25 * 24) + horaActual.Hour;
                int puntoMedioMinutos = (25 * 60) + horaActual.Minute;

                ListaHoras.ScrollTo(puntoMedioHoras, position: ScrollToPosition.Center, animate: false);
                ListaMinutos.ScrollTo(puntoMedioMinutos, position: ScrollToPosition.Center, animate: false);
            });
        }

        public TimeSpan ObtenerHoraSeleccionada()
        {
            try
            {
                int indiceHora = (int)Math.Round(_scrollXHoras / 40.0);
                int indiceMinuto = (int)Math.Round(_scrollXMinutos / 40.0);

                int hora = (indiceHora % 24 + 24) % 24;
                int minuto = (indiceMinuto % 60 + 60) % 60;

                return new TimeSpan(hora, minuto, 0);
            }
            catch
            {
                return DateTime.Now.TimeOfDay;
            }
        }

        private void OnRuedaScrolled(object? sender, ItemsViewScrolledEventArgs e)
        {
            if (sender == ListaHoras)
            {
                _scrollXHoras = e.VerticalOffset;
                _ctsHoras?.Cancel();
                _ctsHoras = new CancellationTokenSource();
                var token = _ctsHoras.Token;

                Task.Delay(90, token).ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    Dispatcher.Dispatch(() =>
                    {
                        int indiceObjetivo = (int)Math.Round(_scrollXHoras / 40.0);
                        ListaHoras.ScrollTo(indiceObjetivo, position: ScrollToPosition.Center, animate: true);
                    });
                }, token);
            }
            else if (sender == ListaMinutos)
            {
                _scrollXMinutos = e.VerticalOffset;
                _ctsMinutos?.Cancel();
                _ctsMinutos = new CancellationTokenSource();
                var token = _ctsMinutos.Token;

                Task.Delay(90, token).ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    Dispatcher.Dispatch(() =>
                    {
                        int indiceObjetivo = (int)Math.Round(_scrollXMinutos / 40.0);
                        ListaMinutos.ScrollTo(indiceObjetivo, position: ScrollToPosition.Center, animate: true);
                    });
                }, token);
            }
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

        private void OnDiaCalendarioClicked(object? sender, EventArgs e)
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
            catch { /* Animaciones fail-safe */ }

            await Navigation.PopModalAsync(true);
        }

        private async void OnGuardarClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EntryTitulo.Text))
            {
                await DisplayAlertAsync("ALERTA", "El evento requiere un título obligatorio.", "ENTENDIDO");
                return;
            }

            TimeSpan horaSeleccionada = ObtenerHoraSeleccionada();
            DateTime fechaDatePicker = PickerFecha.Date.GetValueOrDefault(DateTime.Now.Date);

            DateTime fechaHoraFinal = new DateTime(
                fechaDatePicker.Year,
                fechaDatePicker.Month,
                fechaDatePicker.Day,
                horaSeleccionada.Hours,
                horaSeleccionada.Minutes,
                0
            );

            // EXTRAEMOS LA SELECCIÓN DIRECTA DEL ÍNDICE DEL DROPDOWN INTERNO
            string seleccionTexto = _opcionesFrecuencia[_indiceFrecuenciaActual];
            FrecuenciaEvento frecuenciaEnum = seleccionTexto switch
            {
                "DIARIO" => FrecuenciaEvento.Diario,
                "SEMANAL" => FrecuenciaEvento.Semanal,
                "MENSUAL" => FrecuenciaEvento.Mensual,
                "ANUAL" => FrecuenciaEvento.Anual,
                _ => FrecuenciaEvento.Ninguna
            };

            var nuevoEvento = new Evento
            {
                Titulo = EntryTitulo.Text.Trim().ToUpper(),
                Descripcion = EditorDescripcion.Text?.Trim() ?? "SIN DETALLES ADICIONALES.",
                FechaHora = fechaHoraFinal,
                Frecuencia = frecuenciaEnum,
                DuracionAvisoTicks = TimeSpan.Zero.Ticks,
                EsAlarmaAgresiva = SwitchAlarmaAgresiva.IsToggled
            };

            nuevoEvento.CalcularProximoAviso();
            await _dbService.GuardarEventoAsync(nuevoEvento);

            try
            {
                var notifier = new MauiTime.Services.NotificationService();
                await notifier.ProgramarRecordatorio(nuevoEvento);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NOTIFICACIÓN POPUP ERROR] {ex.Message}");
            }

            // --- 🎬 SECUENCIA DE ANIMACIÓN: IMPACTO DE TU NUEVO SELLO (selloEvento) ---
            if (selloEvento != null && ContenedorPrincipal != null)
            {
                // Forzamos al nuevo escudo a tomar la prioridad visual máxima de la pantalla
                selloEvento.ZIndex = 999;

                // 1. El escudo de MAUITIME cae gigante golpeando y rotando elásticamente
                await Task.WhenAll(
                    selloEvento.FadeToAsync(1, 150, Easing.CubicOut),
                    selloEvento.ScaleToAsync(1.0, 220, Easing.SpringOut),
                    selloEvento.RotateToAsync(-12, 220, Easing.SpringOut)
                );

                // 2. Pausa dramática: Mantenemos el documento quieto con el escudo plasmado
                await Task.Delay(450);

                // 3. Deslizamiento continuo hacia abajo del documento entero
                await Task.WhenAll(
                    ContenedorPrincipal.FadeToAsync(0, 250, Easing.CubicIn),
                    ContenedorPrincipal.TranslateToAsync(0, 500, 250, Easing.CubicIn)
                );
            }
            else
            {
                // Fallback de seguridad por si los componentes visuales no estuvieran listos
                if (ContenedorPrincipal != null)
                {
                    await ContenedorPrincipal.TranslateToAsync(0, 400, 150, Easing.CubicIn);
                }
            }

            // Cerramos el modal cancelando la transición por defecto del sistema operativo
            await Navigation.PopModalAsync(false);

            // --- RESTAURACIÓN FLUIDA DE LA OPACIDAD DE LA AGENDA PAGE ---
            if (this.Window?.Page is NavigationPage navPage && navPage.CurrentPage is Page agendaPage)
            {
                await agendaPage.FadeToAsync(1.0, 200, Easing.CubicOut);
            }
        }



        private async void OnCancelarClicked(object? sender, EventArgs e)
        {
            if (ContenedorPrincipal != null)
            {
                // Animación de salida hacia abajo y desvanecimiento
                await Task.WhenAll(
                    ContenedorPrincipal.FadeToAsync(0, 250, Easing.CubicIn),
                    ContenedorPrincipal.TranslateToAsync(0, 300, 250, Easing.CubicIn)
                );
            }

            await Navigation.PopModalAsync(false); // 'false' para deshabilitar la transición nativa y usar la nuestra
        }


        private void OnBtnCrearMouseIn(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
        {
            if (BtnCrearEventoGrid == null) return;
            try
            {
                BtnCrearEventoGrid.CancelAnimations();
                _ = Task.WhenAll(
                    BtnCrearEventoGrid.ScaleToAsync(1.12, 180, Easing.SpringOut),
                    BtnCrearEventoGrid.RotateToAsync(-7, 180, Easing.SpringOut)
                );
            }
            catch { }
        }

        private void OnBtnCrearMouseOut(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
        {
            if (BtnCrearEventoGrid == null) return;
            try
            {
                BtnCrearEventoGrid.CancelAnimations();
                _ = Task.WhenAll(
                    BtnCrearEventoGrid.ScaleToAsync(1.0, 140, Easing.CubicIn),
                    BtnCrearEventoGrid.RotateToAsync(-3.5, 140, Easing.CubicIn)
                );
            }
            catch { }
        }

        private void OnBtnAbortarMouseIn(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
        {
            if (BtnAbortarGrid == null) return;
            try
            {
                BtnAbortarGrid.CancelAnimations();
                _ = Task.WhenAll(
                    BtnAbortarGrid.ScaleToAsync(1.12, 180, Easing.SpringOut),
                    BtnAbortarGrid.RotateToAsync(6, 180, Easing.SpringOut)
                );
            }
            catch { }
        }

        private void OnBtnAbortarMouseOut(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
        {
            if (BtnAbortarGrid == null) return;
            try
            {
                BtnAbortarGrid.CancelAnimations();
                _ = Task.WhenAll(
                    BtnAbortarGrid.ScaleToAsync(1.0, 140, Easing.CubicIn),
                    BtnAbortarGrid.RotateToAsync(3, 140, Easing.CubicIn)
                );
            }
            catch { }
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Esperamos un instante mínimo para asegurar que el motor gráfico de MAUI está listo
            await Task.Delay(50);

            if (ContenedorPrincipal != null)
            {
                // Cancelamos cualquier animación previa por seguridad
                ContenedorPrincipal.CancelAnimations();

                // Subida fluida con efecto elástico y desvanecimiento simultáneo
                await Task.WhenAll(
                    ContenedorPrincipal.FadeToAsync(1, 400, Easing.CubicOut),
                    ContenedorPrincipal.TranslateToAsync(0, 0, 450, Easing.SpringOut)
                );
            }
        }

    }

}
