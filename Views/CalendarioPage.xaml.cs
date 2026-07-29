using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using MauiTime.Models;
using MauiTime.Services;

namespace MauiTime.Views
{
    public partial class CalendarioPage : ContentPage, INotifyPropertyChanged
    {
        private DateTime _mesActualReferencia = DateTime.Today;
        private bool _isAnimating = false;
        private readonly DatabaseService _dbService;
        private double _scrollOffsetInicial = -1;
        private double _posicionBaseX = 0;
        private double _posicionBaseY = 0;
        private int _indiceCeldaHoy = -1;


        private ObservableCollection<DiaCalendario> _diasDelMes = new();

        // Campos privados para el control de los textos mediante Binding
        private string _textoMes = string.Empty;
        private string _textoAnio = string.Empty;

        public ObservableCollection<DiaCalendario> DiasDelMes
        {
            get => _diasDelMes;
            set { _diasDelMes = value; OnPropertyChanged(); }
        }

        // Nueva propiedad expuesta para el XAML (Mes)
        public string TextoMes
        {
            get => _textoMes;
            set { _textoMes = value; OnPropertyChanged(); }
        }

        // Nueva propiedad expuesta para el XAML (Año)
        public string TextoAnio
        {
            get => _textoAnio;
            set { _textoAnio = value; OnPropertyChanged(); }
        }

        public CalendarioPage(DatabaseService dbService)
        {
            InitializeComponent();

            _dbService = dbService;
            BindingContext = this;

            this.Loaded += (s, e) =>
            {
                ConstruirLetrasRansomMes(_mesActualReferencia.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")).ToUpper());
                ConstruirLetrasRansomAnio(_mesActualReferencia.ToString("yyyy"));
            };

            RefrescarCalendario();
        }


        private async void RefrescarCalendario()
        {
            TextoMes = _mesActualReferencia.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")).ToUpper();
            TextoAnio = _mesActualReferencia.ToString("yyyy");

            ConstruirLetrasRansomMes(TextoMes);

            int mes = _mesActualReferencia.Month;
            int anio = _mesActualReferencia.Year;

            // Procesar cálculo de cuadrícula y consulta SQLite en hilo secundario
            var listaDias = await Task.Run(async () =>
            {
                var result = new List<DiaCalendario>();
                DateTime primerDiaMes = new DateTime(anio, mes, 1);
                int totalDiasMes = DateTime.DaysInMonth(anio, mes);
                int desfaseInicio = ((int)primerDiaMes.DayOfWeek + 6) % 7;
                Random rand = new Random();

                // 1. Días del mes anterior
                DateTime mesAnterior = primerDiaMes.AddMonths(-1);
                int diasMesAnterior = DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
                for (int i = desfaseInicio - 1; i >= 0; i--)
                {
                    result.Add(new DiaCalendario { NumeroDia = (diasMesAnterior - i).ToString(), EsMesActual = false, RotacionCelda = rand.Next(-6, 7) });
                }

                // 2. Días del mes actual
                for (int dia = 1; dia <= totalDiasMes; dia++)
                {
                    var fecha = new DateTime(anio, mes, dia);
                    result.Add(new DiaCalendario
                    {
                        NumeroDia = dia.ToString(),
                        FechaCompleta = fecha,
                        EsMesActual = true,
                        EsHoy = fecha.Year == DateTime.Today.Year && fecha.Month == DateTime.Today.Month && fecha.Day == DateTime.Today.Day,
                        RotacionCelda = rand.Next(-6, 7),
                        ColorFondoCelda = Colors.White
                    });
                }

                // 3. Días del mes siguiente
                int totalCeldasHastaFilaActual = result.Count;
                int diasParaCerrarSemana = (7 - (totalCeldasHastaFilaActual % 7)) % 7;
                int celdasObjetivoDinamico = totalCeldasHastaFilaActual + diasParaCerrarSemana;
                int diaSiguiente = 1;
                while (result.Count < celdasObjetivoDinamico)
                {
                    result.Add(new DiaCalendario { NumeroDia = diaSiguiente.ToString(), EsMesActual = false, RotacionCelda = rand.Next(-6, 7) });
                    diaSiguiente++;
                }

                try
                {
                    var eventosDelMes = await _dbService.ObtenerEventosPorMes(mes, anio).ConfigureAwait(false);

                    foreach (var dia in result)
                    {
                        if (dia.EsMesActual && dia.FechaCompleta.HasValue)
                        {
                            int totalMisionesEseDia = eventosDelMes.Count(e => e.FechaHora.Date == dia.FechaCompleta.Value.Date);

                            if (totalMisionesEseDia > 0)
                            {
                                dia.TieneTareasPendientes = true;
                                dia.TextoBanderita = totalMisionesEseDia == 1 ? "!" : $"+{totalMisionesEseDia}";
                            }
                            else
                            {
                                dia.TieneTareasPendientes = false;
                                dia.TextoBanderita = "!";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error SQLite al contar misiones: {ex.Message}");
                }

                return result;
            }).ConfigureAwait(false);

            // Volcado final en el hilo principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DiasDelMes = new ObservableCollection<DiaCalendario>(listaDias);
                _scrollOffsetInicial = -1;

                bool esElMesActualReal = _mesActualReferencia.Month == DateTime.Today.Month &&
                                         _mesActualReferencia.Year == DateTime.Today.Year;

                // Encontrar el índice de "Hoy"
                _indiceCeldaHoy = -1;
                for (int i = 0; i < listaDias.Count; i++)
                {
                    if (listaDias[i].EsHoy)
                    {
                        _indiceCeldaHoy = i;
                        break;
                    }
                }

                if (objetoCuchillo != null)
                {
                    objetoCuchillo.IsVisible = esElMesActualReal && _indiceCeldaHoy >= 0;
                }
            });
        }


        // CONTROLADOR DE CLIC: Abre la ventana emergente al tocar una celda válida
        private async void OnDiaSeleccionado(object sender, TappedEventArgs e)
        {
            if (sender is BindableObject border && border.BindingContext is DiaCalendario dia && dia.EsMesActual && dia.FechaCompleta.HasValue)
            {
                // 1. Resolvemos el NotificationService mediante el contenedor de dependencias inyectado o el servicio global ya optimizado
                var notificationService = App.Current?.Handler?.MauiContext?.Services.GetService<Services.NotificationService>();

                // 2. Instanciamos el popup pasándole los tres parámetros requeridos por su constructor refactorizado
                var popupFormulario = new NuevaTareaPopup(dia.FechaCompleta.Value, _dbService, notificationService!);

                await Navigation.PushModalAsync(popupFormulario);

                // 3. Refresco no bloqueante en segundo plano para mantener la fluidez en Android
                _ = Task.Run(async () =>
                {
                    RefrescarCalendario();
                });
            }
        }


        // NUEVO: Método encargado de recortar e inyectar las letras de la nota de rescate
        private void ConstruirLetrasRansomMes(string mes)
        {
            if (ContenedorLetrasMes == null) return;

            ContenedorLetrasMes.Children.Clear();
            var rand = new Random();

            foreach (char letra in mes)
            {
                if (char.IsWhiteSpace(letra)) continue;

                var bloqueLetra = new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle(),
                    // OPTIMIZACIÓN: Más acolchado para que el recorte resalte más por sí solo
                    Padding = new Thickness(9, 5),
                    Margin = new Thickness(0.5, 0)
                };

                var labelLetra = new Label
                {
                    Text = letra.ToString(),
                    FontFamily = "Impact",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                // CAOS CONTROLADO: Tiramos un dado de 3 caras para el estilo base de la tarjeta
                int tipoPapelAleatorio = rand.Next(0, 3);

                switch (tipoPapelAleatorio)
                {
                    case 0: // Recorte de diario: Papel blanco, letra negra
                        bloqueLetra.BackgroundColor = Colors.White;
                        bloqueLetra.StrokeThickness = 0;
                        labelLetra.TextColor = Colors.Black;
                        break;

                    case 1: // Recorte de revista punk: Papel rojo, letra blanca
                        bloqueLetra.BackgroundColor = Color.FromArgb("#E31D26");
                        bloqueLetra.StrokeThickness = 0;
                        labelLetra.TextColor = Colors.White;
                        break;

                    case 2: // Recorte de cartel nocturno: Papel negro, borde blanco, letra blanca
                        bloqueLetra.BackgroundColor = Colors.Black;
                        bloqueLetra.Stroke = Colors.White;
                        bloqueLetra.StrokeThickness = 1.5;
                        labelLetra.TextColor = Colors.White;
                        break;
                }

                // Variaciones dinámicas críticas de posición (A voleo puro)
                bloqueLetra.Rotation = rand.Next(-10, 11);      // Rotación orgánica extrema entre -10° y 10°
                bloqueLetra.TranslationY = rand.Next(-7, 8);   // Desfase vertical caótico entre -7px y 7px

                // OPTIMIZACIÓN: Letras del mes masivas (rango 32 a 38) para contrastar fuertemente con el año
                labelLetra.FontSize = rand.Next(32, 38);

                bloqueLetra.Content = labelLetra;
                ContenedorLetrasMes.Children.Add(bloqueLetra);
            }
        }


        private void OnPrevMonthClicked(object? sender, EventArgs e)
        {
            _mesActualReferencia = _mesActualReferencia.AddMonths(-1);
            RefrescarCalendario();

            ConstruirLetrasRansomMes(TextoMes);
            ConstruirLetrasRansomAnio(TextoAnio);
        }

        private void OnNextMonthClicked(object? sender, EventArgs e)
        {
            _mesActualReferencia = _mesActualReferencia.AddMonths(1);
            RefrescarCalendario();

            ConstruirLetrasRansomMes(TextoMes);
            ConstruirLetrasRansomAnio(TextoAnio);
        }


        private async void OnAgendaTabTapped(object? sender, TappedEventArgs e)
        {
            try { await Shell.Current.GoToAsync("//AgendaPage"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}"); }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // FUNCIÓN DEL AÑO: Recorta e inyecta los dígitos de la nota de rescate "a voleo"
        private void ConstruirLetrasRansomAnio(string anio)
        {
            if (ContenedorLetrasAnio == null) return;

            ContenedorLetrasAnio.Children.Clear();
            var rand = new Random();

            foreach (char digito in anio)
            {
                if (char.IsWhiteSpace(digito)) continue;

                var bloqueDigito = new Border
                {
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle(),
                    Padding = new Thickness(7, 4), // Ligeramente más compacto que el mes
                    Margin = new Thickness(2, 0)
                };

                var labelDigito = new Label
                {
                    Text = digito.ToString(),
                    FontFamily = "Impact",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

                // CAOS CONTROLADO: Selección de papel 100% aleatoria para romper patrones repetitivos
                int tipoPapelAleatorio = rand.Next(0, 3);

                switch (tipoPapelAleatorio)
                {
                    case 0: // Recorte blanco, letra negra
                        bloqueDigito.BackgroundColor = Colors.White;
                        bloqueDigito.StrokeThickness = 0;
                        labelDigito.TextColor = Colors.Black;
                        break;

                    case 1: // Recorte rojo, letra blanca
                        bloqueDigito.BackgroundColor = Color.FromArgb("#E31D26");
                        bloqueDigito.StrokeThickness = 0;
                        labelDigito.TextColor = Colors.White;
                        break;

                    case 2: // Recorte negro, borde blanco, letra blanca
                        bloqueDigito.BackgroundColor = Colors.Black;
                        bloqueDigito.Stroke = Colors.White;
                        bloqueDigito.StrokeThickness = 1.5;
                        labelDigito.TextColor = Colors.White;
                        break;
                }

                // Variaciones de posición orgánicas independientes
                bloqueDigito.Rotation = rand.Next(-9, 10);
                bloqueDigito.TranslationY = rand.Next(-4, 5);

                // OPTIMIZACIÓN: Rango de tamaño controlado para actuar como prefijo estilizado del mes
                labelDigito.FontSize = rand.Next(24, 29);

                bloqueDigito.Content = labelDigito;
                ContenedorLetrasAnio.Children.Add(bloqueDigito);
            }
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Forzamos el color de fondo oscuro de inmediato para conectar con la linterna
            this.BackgroundColor = Color.FromArgb("#1A1A1A");

            // Hacemos el calendario visible al 100% de forma inmediata
            GridDiasCollectionView.Opacity = 1.0;
        }




        private void OnGridDiasCollectionViewSizeChanged(object? sender, EventArgs e)
        {
            // Desenganchamos el evento de inmediato para que solo se ejecute una vez al cargar la página
            GridDiasCollectionView.SizeChanged -= OnGridDiasCollectionViewSizeChanged;

            // Buscamos el objeto de la celda de hoy dentro de tu colección real de datos
            var diaHoyModelo = DiasDelMes.FirstOrDefault(d => d.EsHoy);
            if (diaHoyModelo != null)
            {
                // Forzamos el salto instantáneo (animate: false) al centro visual exacto
                // Como el control ya tiene tamaño real, Windows centrará la fila de hoy (ej. día 22) inmediatamente
                GridDiasCollectionView.ScrollTo(diaHoyModelo, position: ScrollToPosition.Center, animate: false);
            }
        }



        private bool _isScrollingHandled = false;

        private void OnGridDiasScrolled(object? sender, ItemsViewScrolledEventArgs e)
        {
            if (objetoCuchillo == null || !objetoCuchillo.IsVisible) return;

            // Capturamos el desplazamiento real del scroll
            double scrollActual = e.VerticalOffset;

            // Capturamos la línea base del scroll (posición donde el cuchillo "aterrizó")
            if (_scrollOffsetInicial < 0)
            {
                _scrollOffsetInicial = scrollActual;
            }

            // 🎯 ACOPLAMIENTO MAGNÉTICO: El cuchillo se mueve solidariamente con la grilla.
            // La diferencia (scrollOffsetInicial - scrollActual) da exactamente los píxeles
            // que la grilla se ha desplazado desde que el cuchillo se posicionó.
            // Ese delta se RESTA de la posición base Y para que el cuchillo "viaje" con el día.
            double deltaScroll = scrollActual - _scrollOffsetInicial;
            objetoCuchillo.TranslationX = _posicionBaseX;
            objetoCuchillo.TranslationY = _posicionBaseY - deltaScroll;
        }

        /// Se ejecuta de forma automática en cuanto el puñal de "Hoy" se dibuja físicamente en la pantalla.
        
        private void OnCuchilloLoaded(object? sender, EventArgs e)
        {
            if (objetoCuchillo == null) return;

            // 🚨 EL CANDADO DEFINITIVO DE FECHA REAL:
            bool esElMesActualReal = _mesActualReferencia.Month == DateTime.Today.Month &&
                                     _mesActualReferencia.Year == DateTime.Today.Year;

            if (!esElMesActualReal || _indiceCeldaHoy < 0)
            {
                objetoCuchillo.Opacity = 0;
                objetoCuchillo.IsVisible = false;
                return;
            }

            // =======================================================================
            // ⚔️ POSICIONAMIENTO GEOMÉTRICO: Calculamos las coordenadas exactas 
            //    de la celda de hoy dentro de la cuadrícula de 7 columnas.
            //    El cuchillo vive en Grid.Row="3" (misma fila que el CollectionView),
            //    así que su (0,0) coincide con la esquina superior izquierda de la grilla.
            // =======================================================================
            int col = _indiceCeldaHoy % 7;
            int row = _indiceCeldaHoy / 7;

            if (DeviceInfo.Current.Platform == DevicePlatform.Android)
            {
                // Métricas Android: celda 48w + 2 spacing = 50 step, 68h + 4 spacing = 72 step
                // Margin del CollectionView: 4,0,4,5
                double anchoCollectionView = GridDiasCollectionView.Width > 0 ? GridDiasCollectionView.Width : 360;
                double anchoCelda = (anchoCollectionView - (6 * 2)) / 7.0; // 7 cols, 6 gaps de 2px
                double pasoX = anchoCelda + 2;
                double pasoY = 68 + 4; // HeightRequest + VerticalItemSpacing

                double inicioX = col * pasoX;
                double inicioY = row * pasoY;

                double anchoCuchillo = objetoCuchillo.WidthRequest > 0 ? objetoCuchillo.WidthRequest : 80;
                double altoCuchillo = objetoCuchillo.HeightRequest > 0 ? objetoCuchillo.HeightRequest : 80;

                // 🎯 ALINEACIÓN MÁGICA DE LA PUNTA: El vértice inferior izquierdo del PNG (donde está la punta) coincide con la esquina superior derecha de la celda
                _posicionBaseX = inicioX + anchoCelda - (anchoCuchillo * 0.15);
                _posicionBaseY = inicioY - (altoCuchillo * 0.68);
            }
            else // WinUI / Windows
            {
                double anchoCelda = 140;
                double pasoX = 140 + 14; // WidthRequest + HorizontalItemSpacing
                double pasoY = 140 + 16; // HeightRequest + VerticalItemSpacing

                double inicioX = col * pasoX;
                double inicioY = row * pasoY;

                double anchoCuchillo = objetoCuchillo.WidthRequest > 0 ? objetoCuchillo.WidthRequest : 150;
                double altoCuchillo = objetoCuchillo.HeightRequest > 0 ? objetoCuchillo.HeightRequest : 150;

                // 🎯 ALINEACIÓN MÁGICA DE LA PUNTA: El vértice inferior izquierdo del PNG (donde está la punta) coincide con la esquina superior derecha de la celda
                _posicionBaseX = inicioX + anchoCelda - (anchoCuchillo * 0.15);
                _posicionBaseY = inicioY - (altoCuchillo * 0.85);
            }

            // Scroll a la celda de hoy para centrarla en pantalla
            var diaHoyModelo = DiasDelMes.FirstOrDefault(d => d.EsHoy);
            if (diaHoyModelo != null)
            {
                GridDiasCollectionView.ScrollTo(diaHoyModelo, position: ScrollToPosition.Center, animate: false);
            }

            // Reseteamos la línea base del scroll (se capturará en el primer evento Scrolled)
            _scrollOffsetInicial = -1;

            // ---------------------------------------------------------------------
            // COREOGRAFÍA CINEMÁTICA DE BIENVENIDA (Solo corre en el mes real actual)
            // El cuchillo cae desde arriba y aterriza CLAVADO en la celda de hoy.
            // ---------------------------------------------------------------------
            objetoCuchillo.Opacity = 0;
            objetoCuchillo.TranslationX = _posicionBaseX + 80;
            objetoCuchillo.TranslationY = _posicionBaseY - 250;
            objetoCuchillo.Rotation = -45;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(200);

                // Trayectoria de caída suave con GPU: aterriza en las coordenadas exactas del día
                objetoCuchillo.Opacity = 1;
                await Task.WhenAll(
                    objetoCuchillo.TranslateToAsync(_posicionBaseX, _posicionBaseY, 350, Easing.CubicIn),
                    objetoCuchillo.RotateToAsync(0, 350, Easing.CubicIn)
                );

                // Impacto visual: Coloreamos la celda de hoy en rojo
                var diaHoy = DiasDelMes.FirstOrDefault(d => d.EsHoy);
                if (diaHoy != null)
                {
                    diaHoy.ColorFondoCelda = Color.FromArgb("#E31D26");
                }

                _ = objetoCuchillo.ScaleToAsync(1.4, 40, Easing.CubicOut)
                    .ContinueWith(t => MainThread.BeginInvokeOnMainThread(() => objetoCuchillo.ScaleToAsync(1.0, 100, Easing.SpringOut)));

                // Terremoto general unificado con candado anti-reentrada
                if (!_isAnimating)
                {
                    _isAnimating = true;
                    try
                    {
                        await EjecutarTemblorSismico(GridDiasCollectionView);
                    }
                    finally
                    {
                        _isAnimating = false;
                    }
                }
            });
        }

        // =========================================================================
        // ANIMACIONES COMPLEJAS DE ALTA FLUIDEZ (ESTILO SPRING / BOUNCE P5)
        // =========================================================================

        private async void OnTabAgendaMouseIn(object? sender, PointerEventArgs e)
        {
            if (TabAgenda == null || TabCalendario == null) return;

            TabAgenda.ZIndex = 3;
            TabCalendario.ZIndex = 1;

            // Agenda: Se estira un 12% con efecto muelle y se inclina un poco más de su eje original (-4 a -6)
            var animAgenda = Task.WhenAll(
                TabAgenda.ScaleToAsync(1.12, 180, Easing.SpringOut),
                TabAgenda.RotateToAsync(-6, 180, Easing.SpringOut)
            );

            // Calendario: Se encoge, baja en vertical (10px) y se endereza sutilmente (3 a 1 grado)
            var animCalendario = Task.WhenAll(
                TabCalendario.ScaleToAsync(0.92, 150, Easing.SinIn),
                TabCalendario.TranslateToAsync(TabCalendario.TranslationX, 10, 150, Easing.SinIn),
                TabCalendario.RotateToAsync(1, 150, Easing.SinIn)
            );

            await Task.WhenAll(animAgenda, animCalendario);
        }

        private async void OnTabAgendaMouseOut(object? sender, PointerEventArgs e)
        {
            if (TabAgenda == null || TabCalendario == null) return;

            // 🚨 CORRECCIÓN DE JERARQUÍA: Al salir el mouse de la Agenda, 
            // el Calendario debe volver a tener el ZIndex dominante (2) por diseño por defecto de la página
            TabAgenda.ZIndex = 1;
            TabCalendario.ZIndex = 2;

            // Regresan de forma fluida a sus valores exactos definidos en el XAML
            var animAgenda = Task.WhenAll(
                TabAgenda.ScaleToAsync(1.0, 140, Easing.CubicIn),
                TabAgenda.RotateToAsync(-4, 140, Easing.CubicIn)
            );
            var animCalendario = Task.WhenAll(
                TabCalendario.ScaleToAsync(1.0, 140, Easing.CubicIn),
                TabCalendario.TranslateToAsync(TabCalendario.TranslationX, 0, 140, Easing.CubicIn),
                TabCalendario.RotateToAsync(3, 140, Easing.CubicIn)
            );

            await Task.WhenAll(animAgenda, animCalendario);
        }

        private async void OnTabCalendarioMouseIn(object? sender, PointerEventArgs e)
        {
            if (TabAgenda == null || TabCalendario == null) return;

            TabCalendario.ZIndex = 3;
            TabAgenda.ZIndex = 1;

            // Calendario: Pasa al frente, rebota con muelle y se acuesta más hacia la derecha (3 a 6 grados)
            var animCalendario = Task.WhenAll(
                TabCalendario.ScaleToAsync(1.12, 180, Easing.SpringOut),
                TabCalendario.RotateToAsync(6, 180, Easing.SpringOut)
            );

            // Agenda: Se encoge, se sumerge 10px en vertical y reduce su ángulo (-4 a -2 grados)
            var animAgenda = Task.WhenAll(
                TabAgenda.ScaleToAsync(0.92, 150, Easing.SinIn),
                TabAgenda.TranslateToAsync(TabAgenda.TranslationX, 10, 150, Easing.SinIn),
                TabAgenda.RotateToAsync(-2, 150, Easing.SinIn)
            );

            await Task.WhenAll(animCalendario, animAgenda);
        }

        private async void OnTabCalendarioMouseOut(object? sender, PointerEventArgs e)
        {
            if (TabAgenda == null || TabCalendario == null) return;

            TabAgenda.ZIndex = 1;
            TabCalendario.ZIndex = 2; // Recupera el dominio de inicio

            var animCalendario = Task.WhenAll(
                TabCalendario.ScaleToAsync(1.0, 140, Easing.CubicIn),
                TabCalendario.RotateToAsync(3, 140, Easing.CubicIn)
            );
            var animAgenda = Task.WhenAll(
                TabAgenda.ScaleToAsync(1.0, 140, Easing.CubicIn),
                TabAgenda.TranslateToAsync(TabAgenda.TranslationX, 0, 140, Easing.CubicIn),
                TabAgenda.RotateToAsync(-4, 140, Easing.CubicIn)
            );

            await Task.WhenAll(animCalendario, animAgenda);
        }

        private async void OnCeldaCalendarioTapped(object? sender, TappedEventArgs? e)
        {
            var celdaVisual = sender as BindableObject;
            var diaData = celdaVisual?.BindingContext as MauiTime.Models.DiaCalendario;

            // Si la celda no tiene fecha o no tiene misiones, ignoramos el clic
            if (diaData == null || !diaData.FechaCompleta.HasValue || !diaData.TieneTareasPendientes)
                return;

            DateTime fechaSeleccionada = diaData.FechaCompleta.Value;

            try
            {
                // 1. Extraemos todos los eventos guardados en SQLite para este mes
                var todosLosEventosDelMes = await _dbService.ObtenerEventosPorMes(fechaSeleccionada.Month, fechaSeleccionada.Year);

                // 2. Filtramos quirúrgicamente solo los que coinciden con el día pulsado
                var eventosDelDia = todosLosEventosDelMes
                    .Where(ev => ev.FechaHora.Date == fechaSeleccionada.Date)
                    .OrderBy(ev => ev.FechaHora)
                    .ToList();

                if (!eventosDelDia.Any()) return;

                // 3. Seteamos el encabezado del bocadillo con la fecha de la celda
                LblBocadilloFecha.Text = fechaSeleccionada.ToString("MM / dd");

                // 4. Limpiamos las misiones anteriores para no acumular fantasmas
                ListaMisionesBocadillo.Children.Clear();
                var rand = new Random();

                // 5. Inyectamos las micro-tarjetas negras con la hora militar visible y blindada
                foreach (var mision in eventosDelDia)
                {
                    // Creamos la cuadrícula interna de dos columnas
                    var gridTarjeta = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection
                        {
                            new ColumnDefinition { Width = new GridLength(65) }, // 65px estrictos para que la hora no se tape
                            new ColumnDefinition { Width = GridLength.Star }
                        }
                    };

                    // ETIEQUETA 1: HORA MILITAR (COLUMNA 0)
                    var lblHora = new Label
                    {
                        Text = mision.FechaHora.ToString("HH:mm"),
                        TextColor = Color.FromArgb("#E31D26"), // Rojo fuego encendido
                        FontFamily = "Arial Black",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Start
                    };

                    // ETIQUETA 2: TÍTULO DE LA MISIÓN (COLUMNA 1)
                    var lblTitulo = new Label
                    {
                        Text = mision.Titulo.ToUpper(),
                        TextColor = Colors.White,
                        FontFamily = "Arial Black",
                        FontSize = 13,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.TailTruncation
                    };

                    // Enlazamos de forma canónica las posiciones de las columnas en C#
                    Grid.SetColumn(lblHora, 0);
                    Grid.SetColumn(lblTitulo, 1);

                    // Añadimos los hijos al Grid
                    gridTarjeta.Children.Add(lblHora);
                    gridTarjeta.Children.Add(lblTitulo);

                    // Envolvemos todo en el Border de alto contraste
                    var tarjetaMision = new Border
                    {
                        BackgroundColor = Colors.Black,
                        Padding = new Thickness(14, 8),
                        Rotation = rand.Next(-2, 3),
                        StrokeThickness = 1.5,
                        Stroke = Colors.White,
                        Content = gridTarjeta
                    };

                    ListaMisionesBocadillo.Children.Add(tarjetaMision);
                }


                // 6. ANIMACIÓN DE IMPACTO: Escalamos por hardware con efecto muelle elástico
                PopupBocadilloEventos.Scale = 0.4;
                PopupBocadilloEventos.IsVisible = true;

                // ⚡ CORRECCIÓN: Usamos ScaleTo nativo de MAUI
                await PopupBocadilloEventos.ScaleToAsync(1.0, 160, Easing.SpringOut);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al desplegar bocadillo: {ex.Message}");
            }
        }

        // ⚡ CORRECCIÓN: Tipado de parámetros alineado con EventHandler nativo
        private async void OnCerrarBocadilloClicked(object? sender, EventArgs? e)
        {
            // Animación de huida rápida antes de ocultar el contenedor
            // ⚡ CORRECCIÓN: Usamos ScaleTo nativo de MAUI
            await PopupBocadilloEventos.ScaleToAsync(0.7, 90, Easing.Linear);
            PopupBocadilloEventos.IsVisible = false;
        }



        #region ESCANEO DE ÁRBOL Y GEOMETRÍA NATIVA (.NET 10)

        private Task EjecutarTemblorSismico(VisualElement target)
        {
            var tcs = new TaskCompletionSource<bool>();

            // 1. Creamos el contenedor maestro de la animación
            var sismoCompleto = new Animation();

            // 2. Ida del latigazo (Ocupa del 0% al 50% del tiempo total de la animación)
            var ida = new Animation(v => target.TranslationX = v, 0, -14, Easing.Linear);
            var idaY = new Animation(v => target.TranslationY = v, 0, 10, Easing.Linear);

            // 3. Vuelta al origen (Ocupa del 51% al 100% del tiempo restante)
            var vuelta = new Animation(v => target.TranslationX = v, -14, 0, Easing.Linear);
            var vueltaY = new Animation(v => target.TranslationY = v, 10, 0, Easing.Linear);

            // 4. Ensamblamos las piezas en la línea de tiempo (Rango de 0.0 a 1.0)
            sismoCompleto.Add(0.0, 0.5, ida);
            sismoCompleto.Add(0.0, 0.5, idaY);
            sismoCompleto.Add(0.51, 1.0, vuelta);
            sismoCompleto.Add(0.51, 1.0, vueltaY);

            // 5. ¡FUEGO! Se ejecuta todo el bloque de golpe en la GPU en solo 60 milisegundos totales
            sismoCompleto.Commit(
                owner: this,
                name: "SismoPremium",
                rate: 16,
                length: 20, // Duración total del impacto en milisegundos (Ida y vuelta)
                finished: (v, c) =>
                {
                    // Aseguramos el punto cero absoluto al terminar
                    target.TranslationX = 0;
                    target.TranslationY = 0;
                    tcs.SetResult(true);
                });

            return tcs.Task;
        }


    }
        #endregion
}
