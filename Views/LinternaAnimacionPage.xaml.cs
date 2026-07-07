using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Threading.Tasks;

namespace MauiTime.Views
{
    public partial class LinternaAnimacionPage : ContentPage
    {
        private bool _isAnimating = false;

        public LinternaAnimacionPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Blindaje contra ejecuciones duplicadas de MAUI
            if (_isAnimating) return;
            _isAnimating = true;

            // FASE 0: RESET VISUAL DE SEGURIDAD ABSOLUTO
            this.Opacity = 1.0;
            this.BackgroundColor = Colors.Black;
            ContenedorLineasAccion.Children.Clear();
            ContenedorLineasAccion.Opacity = 0;
            FocoLinterna.Scale = 1.0;
            FocoLinterna.TranslationX = 0;
            FocoLinterna.TranslationY = 0;
            FocoLinterna.BackgroundColor = Colors.White;
            TextoLinterna.Text = "SEARCHING...";

            // CONTROL DE DIMENSIONES SEGURO PARA WINDOWS
            int intentos = 0;
            while ((this.Width <= 0 || this.Height <= 0) && intentos < 10)
            {
                await Task.Delay(40);
                intentos++;
            }

            double anchoReal = this.Width > 0 ? this.Width : 400;
            double altoReal = this.Height > 0 ? this.Height : 700;

            try
            {
                // Pre-generamos la ráfaga masiva de líneas anime en memoria
                GenerarRafagaAnimeDinamica(anchoReal, altoReal);

                await Task.Delay(150); // Breve suspenso inicial en la oscuridad

                // FASE 1: PATRULLAJE DINÁMICO ACELERADO (Estilo búsqueda nerviosa)
                double limX = anchoReal * 0.25;
                double limY = altoReal * 0.25;
                await FocoLinterna.TranslateToAsync(limX, -limY, 320, Easing.SinOut);
                await FocoLinterna.TranslateToAsync(-limX, limY, 320, Easing.Linear);
                await FocoLinterna.TranslateToAsync(-limX, -limY, 280, Easing.SinIn);
                await FocoLinterna.TranslateToAsync(0, 0, 250, Easing.CubicOut);

                await Task.Delay(80); // Pausa mínima milimétrica antes del golpe de efecto

                // FASE 2: DETONACIÓN TARGET LOCK + INCURSIÓN LÍNEAS ANIME
                FocoLinterna.BackgroundColor = Color.FromArgb("#E31D26");
                TextoLinterna.Text = "TARGET LOCK";
                ContenedorLineasAccion.Opacity = 1.0;

                // 🚨 EL TRUCO DEFINITIVO: Creamos un token para detener el bucle cuando saltemos de página
                bool seguirVibrando = true;

                // FASE 3: DISPARAR BUCLE DE SHAKE INFINITO EN PARALELO (Sin "await")
                // Al usar un método "async void" sin await, el bucle se queda corriendo de fondo de forma perpetua
                _ = Task.Run(async () =>
                {
                    Random r = new Random();
                    while (seguirVibrando)
                    {
                        // Generamos coordenadas caóticas en cada iteración ultrarrápida
                        double fx = r.Next(-25, 26);
                        double fy = r.Next(-20, 21);
                        double cx = -fx * 0.6; // Desfase óptico inverso para las líneas
                        double cy = -fy * 0.6;

                        // Modificamos las posiciones en el hilo de la UI de forma inmediata
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            FocoLinterna.TranslationX = fx;
                            FocoLinterna.TranslationY = fy;
                            ContenedorLineasAccion.TranslationX = cx;
                            ContenedorLineasAccion.TranslationY = cy;
                        });

                        // Pausa de 12 milisegundos entre sacudidas para una vibración violenta
                        await Task.Delay(12);
                    }
                });

                // Dejamos que el usuario experimente la vibración a pantalla completa un instante
                await Task.Delay(140);

                // FASE 4: EXPULSIÓN AL CALENDARIO MIENTRAS SIGUE VIBRANDO
                // Disparamos la carga de la cuadrícula
                _ = Shell.Current.GoToAsync("//CalendarioPage");

                // Hacemos el desvanecimiento de la pantalla. El bucle de arriba sigue agitando los polígonos
                // porque "seguirVibrando" sigue siendo true mientras cae la opacidad.
                await this.FadeToAsync(0, 100, Easing.CubicOut);

                // Apagamos el bucle justo al terminar el desvanecimiento para liberar memoria
                seguirVibrando = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en secuencia linterna: {ex.Message}");
                // Salida de emergencia blindada si algo falla para no colgar la interfaz
                await Shell.Current.GoToAsync("//CalendarioPage");
            }
            finally
            {
                _isAnimating = false;
            }
        }

        private void GenerarRafagaAnimeDinamica(double width, double height)
        {
            Random rand = new Random();
            int totalLineas = 28; // Cantidad masiva de puñales gráficos
            double centroX = width / 2;
            double centroY = height / 2;
            double radioMaximo = Math.Max(width, height) * 0.7;

            for (int i = 0; i < totalLineas; i++)
            {
                // Distribución radial de los ángulos con pequeñas variaciones orgánicas
                double anguloBase = (360.0 / totalLineas) * i;
                double anguloRad = (anguloBase + rand.Next(-5, 6)) * (Math.PI / 180.0);

                // Base ancha anclada en el borde exterior del lienzo
                double baseDistancia = radioMaximo;
                double bx = centroX + Math.Cos(anguloRad) * baseDistancia;
                double by = centroY + Math.Sin(anguloRad) * baseDistancia;

                double anchoBase = rand.Next(40, 95);
                double perpRad = anguloRad + (Math.PI / 2);

                double p1x = bx - Math.Cos(perpRad) * (anchoBase / 2);
                double p1y = by - Math.Sin(perpRad) * (anchoBase / 2);

                double p2x = bx + Math.Cos(perpRad) * (anchoBase / 2);
                double p2y = by + Math.Sin(perpRad) * (anchoBase / 2);

                // El pico afilado que pincha agresivamente cerca del círculo rojo
                double cercaniaCentro = rand.Next(135, 190);
                double p3x = centroX + Math.Cos(anguloRad) * cercaniaCentro;
                double p3y = centroY + Math.Sin(anguloRad) * cercaniaCentro;

                // Construcción de la pieza geométrica flat
                var trianguloAccion = new Polygon
                {
                    Points = new PointCollection { new Point(p1x, p1y), new Point(p2x, p2y), new Point(p3x, p3y) },
                    // Paleta Persona 5: Mayoría blancos puros, algunos rojos del mismo tono que el target
                    Fill = (rand.Next(0, 4) == 0) ? Color.FromArgb("#E31D26") : Colors.White,
                    Opacity = rand.NextDouble() * (1.0 - 0.7) + 0.7
                };

                AbsoluteLayout.SetLayoutBounds(trianguloAccion, new Rect(0, 0, width, height));
                ContenedorLineasAccion.Children.Add(trianguloAccion);
            }
        }
    }
}
