namespace MauiTime.Views;

using Plugin.LocalNotification;

using MauiTime.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts; // <--- ASEGÚRATE DE QUE ESTA LÍNEA ESTÉ AQUÍ Arriba

public partial class AgendaPage : ContentPage
{
    private readonly AgendaViewModel _viewModel;
    // Campo de control para pausar el hilo de ejecución hasta que el usuario decida
    private TaskCompletionSource<bool>? _borradoTaskSource;
    private Models.Evento? _eventoSeleccionadoParaDestruir;

    public AgendaPage(AgendaViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // 1. LLAMAMOS AL GENERADOR DEL TÍTULO ESTILO P5 AQUÍ
        GenerarTituloP5("AGENDA DE EVENTOS");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null)
        {
            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    await Task.Delay(50); // Micro-pausa de estabilización visual

                    // 🚀 UNIFICACIÓN: Tu ViewModel se encargará de refrescar todo en un solo carril seguro
                    await _viewModel.LoadEventosAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en inicio Agenda: {ex.Message}");
                }
            });
        }
    }



    // 1. CAMBIA EL NOMBRE Y LA LÓGICA DEL MÉTODO A BindingContextChanged
    private void OnFechaHudBindingContextChanged(object? sender, EventArgs e)
    {
        if (sender is not AbsoluteLayout contenedor) return;

        // 🛡️ CONTROL CRÍTICO: Desvincular si el contexto es nulo (cuando MAUI limpia la celda)
        if (contenedor.BindingContext is not Models.Evento evento)
        {
            contenedor.Children.Clear();
            return;
        }

        // Limpieza total antes de volver a dibujar para evitar acumular stickers fantasmas
        contenedor.Children.Clear();
        var random = new Random();

        string dia = evento.FechaHora.ToString("dd");
        string mes = evento.FechaHora.ToString("MM");

        // =========================================================================
        // CAPA 1: Placa del Mes
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
        AbsoluteLayout.SetLayoutBounds(bloqueMes, new Rect(1, 2, 46, 36));
        contenedor.Children.Add(bloqueMes);

        // =========================================================================
        // CAPA 2: Números del Día
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
        AbsoluteLayout.SetLayoutBounds(stackDia, new Rect(42, 36, 60, 50));
        contenedor.Children.Add(stackDia);

        // =========================================================================
        // CAPA 3: La Cuchillada de Alto Impacto
        // =========================================================================
        var tajoRojo = new BoxView
        {
            BackgroundColor = Color.FromArgb("#E31D26"),
            HeightRequest = 9.0,
            WidthRequest = 48,
            Rotation = -30,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        tajoRojo.Shadow = new Shadow
        {
            Brush = Colors.Black,
            Offset = new Point(3, 3),
            Radius = 0,
            Opacity = 1
        };

        AbsoluteLayout.SetLayoutBounds(tajoRojo, new Rect(18, 26, 48, 20));
        contenedor.Children.Add(tajoRojo);
        // =========================================================================
        // CAPA 4: STICKER DEL AÑO (SÚPER MUESTREO GRÁFICO CONTRA BORROSIDAD)
        // =========================================================================
        if (evento.EsAnual)
        {
            string anio = evento.FechaHora.ToString("yyyy");
            double rotacionAnio = random.Next(8, 13);

            var bloqueAnio = new Border
            {
                BackgroundColor = Color.FromArgb("#E31D26"),
                Padding = new Thickness(12, 3), // Más padding para albergar el lienzo gigante
                Rotation = rotacionAnio,
                StrokeThickness = 2,
                Stroke = Colors.White,

                // ⚡ TRUCO MAUI WINDOWS: Encogemos el contenedor entero por hardware.
                // Al procesar una fuente gigante reducida, la GPU mantiene los bordes vectoriales perfectos.
                Scale = 0.38,

                Content = new Label
                {
                    Text = anio,
                    TextColor = Colors.White,
                    FontSize = 36, // 🔥 FUENTE GIGANTE: Obliga a generar una textura de alta resolución
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = "Arial Black", // Arial Black gestiona mejor el subpíxel que Impact en tamaños micro
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            bloqueAnio.Shadow = new Shadow
            {
                Brush = Colors.Black,
                Offset = new Point(6, 6), // Sombra proporcional a la escala nativa antes de encoger
                Radius = 0,
                Opacity = 1
            };

            // 🎯 AJUSTE DE RECUADRO: Al usar Scale, el tamaño lógico base debe ser mayor
            // para que al encogerse quede exactamente del tamaño del sticker original.
            AbsoluteLayout.SetLayoutBounds(bloqueAnio, new Rect(34, -30, 140, 65));
            contenedor.Children.Add(bloqueAnio);
        }

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

    private async void OnCalendarioTabTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            // 🚀 CAMBIO CLAVE: Viajamos primero a la página del balazo de cristal
            await Shell.Current.GoToAsync("//TransicionCristalPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al lanzar la transición: {ex.Message}");
        }
    }

    // =======================================================================
    // ➕ ACCIÓN 1: LANZAR LA PANTALLA POPUP PARA CREAR PROYECTOS / EVENTOS
    // =======================================================================
    private async void OnCrearProyectoClicked(object? sender, EventArgs e)
    {
        // 💡 TRUCO ARQUITECTÓNICO: Si tu ViewModel ya tiene inyectada la conexión real,
        // la extraemos directamente a través del motor de dependencias global de .NET 10.
        var dbServiceGlobal = App.Current?.Handler?.MauiContext?.Services.GetService<Services.DatabaseService>();

        if (dbServiceGlobal != null)
        {
            // Pasamos exactamente el mismo canal de conexión física
            var popupFormulario = new NuevaTareaPopup(DateTime.Today, dbServiceGlobal);
            await Navigation.PushModalAsync(popupFormulario);

            Console.WriteLine("[AGENDA] Regresando del formulario modal. Solicitando refresco al ViewModel...");

            // Le ordenamos a tu ViewModel que vuelva a leer el disco duro físico
            await _viewModel.LoadEventosAsync();
        }
        else
        {
            Console.WriteLine("[AGENDA ERROR] No se pudo recuperar el DatabaseService global del contenedor.");
        }
    }

    private async void OnMitigarAlarmaTargetClicked(object? sender, EventArgs e)
    {
        // 1. Validar el remitente (Grid Maestro) y extraer el Evento de forma segura
        if (sender is not Grid gridMaestro || gridMaestro.BindingContext is not Models.Evento eventoModificado)
            return;

        // 2. Conmutación booleana bidireccional (Toggle)
        eventoModificado.EsAlarmaAgresiva = !eventoModificado.EsAlarmaAgresiva;

        // 3. Recuperar servicios nativos desde el contenedor de dependencias
        var dbService = App.Current?.Handler?.MauiContext?.Services.GetService<Services.DatabaseService>();
        var notificationService = App.Current?.Handler?.MauiContext?.Services.GetService<Services.NotificationService>();

        if (dbService != null)
        {
            try
            {
                // 4. Persistencia asíncrona aislada en tu SQLite (HILO DE FONDO)
                _ = Task.Run(async () =>
                {
                    await dbService.GuardarEventoAsync(eventoModificado);
                });

                // 5. Reprogramación segura de hardware usando tu servicio del doble carril
                if (notificationService != null)
                {
                    await notificationService.ProgramarRecordatorio(eventoModificado);
                }

                // ============================================================
                // 💡 REPARACIÓN ULTRARÁPIDA: CONMUTACIÓN DIRECTA EN ELEMENTO HIJO
                // ============================================================
                // Buscamos al Grid secundario que tiene el nombre "CapaRojoFuego" entre los hijos del contenedor pulsado
                var capaRojo = gridMaestro.Children.FirstOrDefault(c => c is Grid g && g.StyleId == "CapaRojoFuego" || (c is Grid visualGrid && visualGrid.IsVisible != eventoModificado.EsAlarmaAgresiva)) as Grid;

                // Si el motor de renderizado no lo encuentra por tipado indirecto, forzamos la actualización directa del árbol visual de la celda
                foreach (var hijo in gridMaestro.Children)
                {
                    if (hijo is Grid capaVisual)
                    {
                        // Forzamos a la Capa 2 a igualar la visibilidad booleana real del objeto de forma instantánea
                        capaVisual.IsVisible = eventoModificado.EsAlarmaAgresiva;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MAUI-INTERACTIVO] Excepción mitigada: {ex.Message}");
            }
        }

        // 6. Feedback háptico opcional para dispositivos móviles
#if ANDROID || IOS
    try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
#endif
    }

    private async void OnEliminarProyectoClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject control && control.BindingContext is Models.Evento eventoDestruir)
        {
            _eventoSeleccionadoParaDestruir = eventoDestruir;

            string textoFormateado = $"¿DESEAS ELIMINAR EL OBJETIVO:\n'{eventoDestruir.Titulo.ToUpper()}'?";
            TxtPreguntaBorrar.Text = textoFormateado;
            TxtPreguntaBorrarSombra.Text = textoFormateado;

            // ============================================================
            // 💡 BLINDAJE DE MEMORIA GRÁFICA EN CALIENTE (CERO CONFLICTOS)
            // ============================================================
            // Detiene cualquier hilo o inercia anterior directamente sobre el Grid de fondo
            ContenedorPopupBorrar.CancelAnimations();

            // Reset estricto de seguridad para forzar a la GPU a arrancar de cero
            ContenedorPopupBorrar.Scale = 0.01;
            ContenedorPopupBorrar.Opacity = 0;

            // Encendemos la capa translúcida
            ContenedorPopupBorrar.IsVisible = true;

            // 🚀 INFLADO CINEMÁTICO REGULAR POR HARDWARE (MÉTODO ASÍNCRONO PURO)
            // Escalamos rápidamente el cartel rojo hasta un 108% en 280ms
            await Task.WhenAll(
                ContenedorPopupBorrar.ScaleToAsync(1.08, 280, Easing.CubicOut),
                ContenedorPopupBorrar.FadeToAsync(1, 220, Easing.CubicOut)
            );

            // 🎯 ASENTAMIENTO DE ALTO RENDIMIENTO CON FIRMA ASÍNCRONA ACTUALIZADA
            // Regresa del 108% al 100% real en 80ms de manera regular sin advertencias de obsolescencia
            await ContenedorPopupBorrar.ScaleToAsync(1.00, 80, Easing.Linear);

            _borradoTaskSource = new TaskCompletionSource<bool>();
            bool confirmar = await _borradoTaskSource.Task;

            if (confirmar && _eventoSeleccionadoParaDestruir != null)
            {
                var dbService = App.Current?.Handler?.MauiContext?.Services.GetService<Services.DatabaseService>();
                if (dbService != null)
                {
                    await dbService.BorrarEventoAsync(_eventoSeleccionadoParaDestruir);
                    if (_viewModel != null)
                    {
                        await _viewModel.LoadEventosAsync();
                    }
                }
            }

            _eventoSeleccionadoParaDestruir = null;
        }
    }

    private async void OnConfirmarBorradoPunkClicked(object? sender, EventArgs e)
    {
        // Limpieza de inercia antes del colapso inverso hacia el centro
        ContenedorPopupBorrar.CancelAnimations();

        await Task.WhenAll(
            ContenedorPopupBorrar.ScaleToAsync(0.01, 160, Easing.CubicIn),
            ContenedorPopupBorrar.FadeToAsync(0, 140, Easing.CubicIn)
        );

        ContenedorPopupBorrar.IsVisible = false;
        _borradoTaskSource?.SetResult(true);
    }

    private async void OnCancelarBorradoPunkClicked(object? sender, EventArgs e)
    {
        // Limpieza de inercia antes del colapso inverso hacia el centro
        ContenedorPopupBorrar.CancelAnimations();

        await Task.WhenAll(
            ContenedorPopupBorrar.ScaleToAsync(0.01, 160, Easing.CubicIn),
            ContenedorPopupBorrar.FadeToAsync(0, 140, Easing.CubicIn)
        );

        ContenedorPopupBorrar.IsVisible = false;
        _borradoTaskSource?.SetResult(false);
    }

    private async void OnBtnInfiltracionMouseIn(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnInfiltracion == null) return;

        // Cancelamos cualquier inercia gráfica previa en la GPU
        BtnInfiltracion.CancelAnimations();

        // 🚀 EFECTO INFLADO EMBOSCADA: Se estira al 112% y se tuerce más en diagonal (4 a 7 grados)
        // Usamos SpringOut para que dé ese pequeño brinco o latigazo elástico al llegar al tope
        await Task.WhenAll(
            BtnInfiltracion.ScaleToAsync(1.12, 180, Easing.SpringOut),
            BtnInfiltracion.RotateToAsync(7, 180, Easing.SpringOut)
        );
    }

    private async void OnBtnInfiltracionMouseOut(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnInfiltracion == null) return;

        // Cancelamos hilos de animación latentes
        BtnInfiltracion.CancelAnimations();

        // Regresa de forma fluida y limpia a sus valores exactos definidos en el XAML original (Scale 1.0, Rotation 4)
        await Task.WhenAll(
            BtnInfiltracion.ScaleToAsync(1.0, 140, Easing.CubicIn),
            BtnInfiltracion.RotateToAsync(4, 140, Easing.CubicIn)
        );
    }

    // =======================================================================
    // 🎸 ANIMACIONES HOVER ASÍNCRONAS PARA EL BOTÓN "BORRAR"
    // =======================================================================
    private async void OnBtnBorrarMouseIn(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnBorrarGrid == null) return;

        BtnBorrarGrid.CancelAnimations();

        // Latigazo elástico: se infla un 12% y se inclina a -6 grados (hacia la izquierda)
        await Task.WhenAll(
            BtnBorrarGrid.ScaleToAsync(1.12, 180, Easing.SpringOut),
            BtnBorrarGrid.RotateToAsync(-6, 180, Easing.SpringOut)
        );
    }

    private async void OnBtnBorrarMouseOut(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnBorrarGrid == null) return;

        BtnBorrarGrid.CancelAnimations();

        // Retorno limpio al tamaño y rotación originales (Escala 1, Rotación 0)
        await Task.WhenAll(
            BtnBorrarGrid.ScaleToAsync(1.0, 140, Easing.CubicIn),
            BtnBorrarGrid.RotateToAsync(0, 140, Easing.CubicIn)
        );
    }

    // =======================================================================
    // 🎸 ANIMACIONES HOVER ASÍNCRONAS PARA EL BOTÓN "ABORTAR"
    // =======================================================================
    private async void OnBtnAbortarMouseIn(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnAbortarGrid == null) return;

        BtnAbortarGrid.CancelAnimations();

        // Latigazo elástico: se infla un 12% y se inclina a 6 grados (hacia la derecha)
        await Task.WhenAll(
            BtnAbortarGrid.ScaleToAsync(1.12, 180, Easing.SpringOut),
            BtnAbortarGrid.RotateToAsync(6, 180, Easing.SpringOut)
        );
    }

    private async void OnBtnAbortarMouseOut(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnAbortarGrid == null) return;

        BtnAbortarGrid.CancelAnimations();

        // Retorno limpio al tamaño y rotación originales
        await Task.WhenAll(
            BtnAbortarGrid.ScaleToAsync(1.0, 140, Easing.CubicIn),
            BtnAbortarGrid.RotateToAsync(0, 140, Easing.CubicIn)
        );
    }

}