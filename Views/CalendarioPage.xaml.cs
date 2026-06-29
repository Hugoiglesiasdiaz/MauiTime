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

            // EVENTO SEGURO: Nos aseguramos de pintar los recortes cuando 
            // todo el árbol visual de Windows Desktop esté 100% montado en memoria.
            this.Loaded += (s, e) =>
            {
                string nombreMes = _mesActualReferencia.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")).ToUpper();
                string stringAnio = _mesActualReferencia.ToString("yyyy");

                ConstruirLetrasRansomMes(nombreMes);
                ConstruirLetrasRansomAnio(stringAnio);
            };

            // Generamos la matriz matemática de 42 celdas
            RefrescarCalendario();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Forzamos el refresco dinámico al conmutar entre pestañas del AppShell
            if (ContenedorLetrasAnio != null && ContenedorLetrasMes != null)
            {
                string nombreMes = _mesActualReferencia.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")).ToUpper();
                string stringAnio = _mesActualReferencia.ToString("yyyy");

                ConstruirLetrasRansomMes(nombreMes);
                ConstruirLetrasRansomAnio(stringAnio);
            }
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

            // 2. Días del mes actual
            for (int dia = 1; dia <= totalDiasMes; dia++)
            {
                var fecha = new DateTime(_mesActualReferencia.Year, _mesActualReferencia.Month, dia);
                listaDias.Add(new DiaCalendario
                {
                    NumeroDia = dia.ToString(),
                    FechaCompleta = fecha,
                    EsMesActual = true,
                    EsHoy = fecha == DateTime.Today,
                    RotacionCelda = rand.Next(-6, 7)
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
    }
}
