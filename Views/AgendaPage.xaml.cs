namespace MauiTime.Views;

using MauiTime.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts; // <--- ASEGÚRATE DE QUE ESTA LÍNEA ESTÉ AQUÍ Arriba

public partial class AgendaPage : ContentPage
{
    private readonly AgendaViewModel _viewModel;

    public AgendaPage(AgendaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // 1. LLAMAMOS AL GENERADOR DEL TÍTULO ESTILO P5 AQUÍ
        GenerarTituloP5("AGENDA DE EVENTOS");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null && !_viewModel.Eventos.Any())
        {
            await _viewModel.LoadEventosAsync();
        }
    }

    private void OnFechaHudLoaded(object? sender, EventArgs e)
    {
        if (sender is not AbsoluteLayout contenedor || contenedor.BindingContext is not Models.Evento evento) return;

        contenedor.Children.Clear();
        var random = new Random();

        string dia = evento.FechaHora.ToString("dd");
        string mes = evento.FechaHora.ToString("MM");

        // =========================================================================
        // CAPA 1: Placa del Mes (Empujada más hacia arriba a la izquierda)
        // =========================================================================
        var bloqueMes = new Border
        {
            BackgroundColor = Colors.Black,
            Padding = new Thickness(7, 3),
            Rotation = random.Next(-12, -7),
            StrokeThickness = 2,
            Stroke = Colors.White,
            Content = new Label
            {
                Text = mes,
                TextColor = Colors.White,
                FontSize = 21,
                FontAttributes = FontAttributes.Bold,
                FontFamily = "Impact",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        // Desplazamos a la esquina superior extrema (X: 1, Y: 2)
        AbsoluteLayout.SetLayoutBounds(bloqueMes, new Rect(1, 2, 46, 36));
        contenedor.Children.Add(bloqueMes);

        // =========================================================================
        // CAPA 2: Números del Día (Empujados más hacia abajo a la derecha)
        // =========================================================================
        var stackDia = new HorizontalStackLayout
        {
            Spacing = -2,
            Rotation = random.Next(6, 12)
        };

        foreach (char digito in dia)
        {
            var stickerDigito = new Border
            {
                BackgroundColor = Colors.White,
                Padding = new Thickness(5, 3),
                Rotation = random.Next(-4, 5),
                StrokeThickness = 1.5,
                Stroke = Colors.Black,
                Margin = new Thickness(-2, random.Next(-3, 4), 0, 0),
                Content = new Label
                {
                    Text = digito.ToString(),
                    TextColor = Colors.Black,
                    FontSize = 30,
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "Impact"
                }
            };

            stickerDigito.Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(4, 4),
                Radius = 0,
                Opacity = 1
            };

            stackDia.Children.Add(stickerDigito);
        }
        // Desplazamos a la esquina inferior extrema (X: 42, Y: 36) para abrir la separación
        AbsoluteLayout.SetLayoutBounds(stackDia, new Rect(42, 36, 60, 50));
        contenedor.Children.Add(stackDia);

        // =========================================================================
        // CAPA 3: La Cuchillada de Alto Impacto (GRUESA Y CON SOMBRA P5)
        // =========================================================================
        var tajoRojo = new BoxView
        {
            BackgroundColor = Color.FromArgb("#E31D26"), // Rojo P5 puro encendido
            HeightRequest = 9.0, // ¡DUPLICADO EL GROSOR! Ahora tiene la fuerza de un brochazo
            WidthRequest = 48,   // Mantenemos el largo exacto del pasillo
            Rotation = -30,      // Mantenemos tu ángulo inclinado perfecto
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Añadimos una sombra negra dura (Drop Shadow) sin difuminar (Radius = 0)
        // Esto duplica el contraste y hace que la línea resalte con violencia visual
        tajoRojo.Shadow = new Shadow
        {
            Brush = Colors.Black,
            Offset = new Point(3, 3), // Desfase plano estilo manga/cómic
            Radius = 0,
            Opacity = 1
        };

        // Tu posicionamiento perfecto se mantiene intacto al 100%
        AbsoluteLayout.SetLayoutBounds(tajoRojo, new Rect(18, 26, 48, 20));
        contenedor.Children.Add(tajoRojo);
    }


    private void OnListaEventosScrolled(object? sender, ItemsViewScrolledEventArgs e)
    {
        if (PunteroScrollP5 == null || sender is not CollectionView lista) return;

        double scrollOffset = e.VerticalOffset;
        int totalElementos = _viewModel.Eventos.Count;

        // Cada celda en Windows tiene una altura física renderizada de 125.5 píxeles netos.
        double altoTotalEstimado = totalElementos * 125.5;
        double altoVisible = lista.Height;

        // Le restamos 15 píxeles de holgura para absorber el margen inferior del contenedor
        double maxScroll = altoTotalEstimado - altoVisible - 15;

        if (maxScroll > 0)
        {
            double porcentaje = scrollOffset / maxScroll;
            porcentaje = Math.Clamp(porcentaje, 0, 1);

            AbsoluteLayout.SetLayoutFlags(PunteroScrollP5, AbsoluteLayoutFlags.XProportional | AbsoluteLayoutFlags.YProportional);
            AbsoluteLayout.SetLayoutBounds(PunteroScrollP5, new Rect(0.5, porcentaje, 14, 24));
        }
    }



    /// <summary>
    /// Divide una cadena de texto en bloques de letras con estilos, colores 
    /// y rotaciones aleatorias emulando la interfaz "Ransom Note" de Persona 5.
    /// </summary>
    private void GenerarTituloP5(string textoCompleto)
    {
        if (ContenedorTitulo == null) return;

        var random = new Random();
        ContenedorTitulo.Children.Clear();

        string[] palabras = textoCompleto.Split(' ');

        foreach (var palabra in palabras)
        {
            var stackPalabra = new HorizontalStackLayout
            {
                Spacing = 2,
                Margin = new Thickness(0, 0, 15, 0)
            };

            for (int i = 0; i < palabra.Length; i++)
            {
                char letra = palabra[i];
                bool esPar = i % 2 == 0;

                var fondoLetra = esPar ? Colors.Black : Colors.White;
                var colorTexto = esPar ? Colors.White : Colors.Black;

                double rotacionAleatoria = random.Next(-6, 7);
                double paddingVertical = random.Next(4, 9);

                var borderLetra = new Border
                {
                    BackgroundColor = fondoLetra,
                    Padding = new Thickness(7, paddingVertical),
                    Rotation = rotacionAleatoria,
                    StrokeThickness = 0,
                    Content = new Label
                    {
                        Text = letra.ToString().ToUpper(),
                        TextColor = colorTexto,
                        FontSize = 26,
                        FontAttributes = FontAttributes.Bold,
                        FontFamily = "Impact",
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                };

                stackPalabra.Children.Add(borderLetra);
            }

            ContenedorTitulo.Children.Add(stackPalabra);
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

        TabAgenda.ZIndex = 2;
        TabCalendario.ZIndex = 1;

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

        TabAgenda.ZIndex = 2;
        TabCalendario.ZIndex = 1;

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

    private void OnAgendaTabTapped(object? sender, TappedEventArgs e)
    {
        // Lógica para refrescar o reaccionar al tocar la pestaña activa (opcional por ahora)
    }

    private void OnCalendarioTabTapped(object? sender, TappedEventArgs e)
    {
        // Aquí programaremos más adelante el salto fluido hacia CalendarioPage
    }

}
