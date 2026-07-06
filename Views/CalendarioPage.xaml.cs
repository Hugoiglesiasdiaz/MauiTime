using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using MauiTime.Models;

namespace MauiTime.Views
{
    public partial class CalendarioPage : ContentPage, INotifyPropertyChanged
    {
        private DateTime _mesActualReferencia = DateTime.Today;
        private bool _isAnimating = false;
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

        public CalendarioPage()
        {
            InitializeComponent();
            BindingContext = this;

            this.Loaded += (s, e) =>
            {
                ConstruirLetrasRansomMes(_mesActualReferencia.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")).ToUpper());
                ConstruirLetrasRansomAnio(_mesActualReferencia.ToString("yyyy"));
            };

            RefrescarCalendario();
        }


        private void RefrescarCalendario()
        {
            // CAMBIO: Actualizamos las propiedades reactivas en lugar de llamar a los controles rotos
            TextoMes = _mesActualReferencia.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")).ToUpper();
            TextoAnio = _mesActualReferencia.ToString("yyyy");

            // NUEVO: Ejecuta la lógica de recortes de letras si el layout visual ya está listo en memoria
            ConstruirLetrasRansomMes(TextoMes);

            // Calculamos la matriz de días
            var listaDias = new List<DiaCalendario>();
            DateTime primerDiaMes = new DateTime(_mesActualReferencia.Year, _mesActualReferencia.Month, 1);
            int totalDiasMes = DateTime.DaysInMonth(_mesActualReferencia.Year, _mesActualReferencia.Month);

            // Convertimos DayOfWeek de MAUI (Domingo=0) a formato europeo (Lunes=0, ..., Domingo=6)
            int desfaseInicio = ((int)primerDiaMes.DayOfWeek + 6) % 7;

            // Generador de rotaciones asimétricas orgánicas para el estilo P5
            Random rand = new Random();

            // 1. Días del mes anterior (Fondo atenuado)
            DateTime mesAnterior = primerDiaMes.AddMonths(-1);
            int diasMesAnterior = DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
            for (int i = desfaseInicio - 1; i >= 0; i--)
            {
                listaDias.Add(new DiaCalendario
                {
                    NumeroDia = (diasMesAnterior - i).ToString(),
                    EsMesActual = false,
                    RotacionCelda = rand.Next(-6, 7)
                });
            }

            // 2. Días del mes actual (Reemplaza este bucle dentro de RefrescarCalendario)
            for (int dia = 1; dia <= totalDiasMes; dia++)
            {
                var fecha = new DateTime(_mesActualReferencia.Year, _mesActualReferencia.Month, dia);

                // Comparación estricta de año, mes y día para evitar desfases de zona horaria
                bool esElDiaDeHoy = fecha.Year == DateTime.Today.Year &&
                                    fecha.Month == DateTime.Today.Month &&
                                    fecha.Day == DateTime.Today.Day;

                listaDias.Add(new DiaCalendario
                {
                    NumeroDia = dia.ToString(),
                    FechaCompleta = fecha,
                    EsMesActual = true,
                    EsHoy = fecha == DateTime.Today,
                    RotacionCelda = rand.Next(-6, 7),
                    ColorFondoCelda = Colors.White // 🚨 Aseguramos que inicien limpios en blanco
                });
            }


            // 3. Días del mes siguiente para rellenar el Grid de 7x6 (42 celdas)
            int totalCeldasHastaFilaActual = listaDias.Count;

            // Si el mes no termina justo en domingo (módulo 7 != 0), calculamos cuántos días faltan para cerrar esa fila
            int diasParaCerrarSemana = (7 - (totalCeldasHastaFilaActual % 7)) % 7;
            int celdasObjetivoDinamico = totalCeldasHastaFilaActual + diasParaCerrarSemana;

            int diaSiguiente = 1;
            while (listaDias.Count < celdasObjetivoDinamico)
            {
                listaDias.Add(new DiaCalendario
                {
                    NumeroDia = diaSiguiente.ToString(),
                    EsMesActual = false,
                    RotacionCelda = rand.Next(-6, 7)
                });
                diaSiguiente++;
            }

            DiasDelMes = new ObservableCollection<DiaCalendario>(listaDias);
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

            // Forzamos el redibujado de los carteles dinámicos
            ConstruirLetrasRansomMes(TextoMes);
            ConstruirLetrasRansomAnio(TextoAnio);
        }

        private void OnNextMonthClicked(object? sender, EventArgs e)
        {
            _mesActualReferencia = _mesActualReferencia.AddMonths(1);
            RefrescarCalendario();

            // Forzamos el redibujado de los carteles dinámicos
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

            // Hacemos el calendario visible al 100% de forma inmediata.
            // Ya no ejecutamos el sismo aquí; se lo dejamos encargado al cuchillo en su evento 'Loaded'
            GridDiasCollectionView.Opacity = 1.0;
        }


        /// Se ejecuta de forma automática en cuanto el puñal de "Hoy" se dibuja físicamente en la pantalla.
        /// </summary>
        private void OnCuchilloLoaded(object? sender, EventArgs e)
        {
            if (sender is Image objetoCuchillo)
            {
                // 1. PREPARACIÓN OCULTA EN LAS ALTURAS (Viene volando en diagonal)
                objetoCuchillo.Opacity = 0;
                objetoCuchillo.TranslationY = -250;
                objetoCuchillo.TranslationX = 80;
                objetoCuchillo.Rotation = -45;

                Dispatcher.Dispatch(async () =>
                {
                    // Espera táctica inicial para que cargue la interfaz limpia
                    await Task.Delay(200);

                    // 2. TRAYECTORIA DE CAÍDA SUAVE Y VISIBLE
                    objetoCuchillo.Opacity = 1;
                    await Task.WhenAll(
                        objetoCuchillo.TranslateToAsync(0, 0, 350, Easing.CubicIn),
                        objetoCuchillo.RotateToAsync(0, 350, Easing.CubicIn)
                    );

                    // 3. ¡IMPACTO VISUAL! (Cambio de color al tocar el papel)
                    if (objetoCuchillo.BindingContext is MauiTime.Models.DiaCalendario diaActual && diaActual.EsHoy)
                    {
                        // Teñimos la tarjeta delantera de Rojo Fuego al tocar el papel
                        diaActual.ColorFondoCelda = Color.FromArgb("#E31D26");

                        // Refrescamos los disparadores del XAML para pasar el texto del número a Blanco
                        var contextoTemporal = objetoCuchillo.BindingContext;
                        objetoCuchillo.BindingContext = null;
                        objetoCuchillo.BindingContext = contextoTemporal;
                    }

                    // Pequeño rebote elástico individual del puñal al clavarse profundo
                    _ = objetoCuchillo.ScaleToAsync(1.4, 40, Easing.CubicOut)
                        .ContinueWith(t => MainThread.BeginInvokeOnMainThread(() => objetoCuchillo.ScaleToAsync(1.0, 100, Easing.SpringOut)));

                    // 4. EL ÚNICO TERREMOTO GENERAL DE ALTA CALIDAD
                    // Borrado el sismo duplicado. Ahora la energía del golpe sacude la cuadrícula una sola vez
                    if (!_isAnimating)
                    {
                        _isAnimating = true;
                        try
                        {
                            // Un único sismo directo sobre todo el conjunto de días a la vez
                            await EjecutarTemblorSismico(GridDiasCollectionView);
                        }
                        finally
                        {
                            _isAnimating = false;
                        }
                    }
                });
            }
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
