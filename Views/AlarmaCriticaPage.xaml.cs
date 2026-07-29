using Microsoft.Maui.Controls;
using System;
using Plugin.LocalNotification;

namespace MauiTime.Views;

public partial class AlarmaCriticaPage : ContentPage
{
    public AlarmaCriticaPage(string titulo, string descripcion)
    {
        InitializeComponent();
        LblTituloMision.Text = titulo.ToUpper();
        LblDescripcionMision.Text = descripcion.ToUpper();

        // =========================================================================
        // 🔥 INVASIÓN HARDWARE WINDOWS: CONGELA EL ENTORNO A PANTALLA COMPLETA
        // =========================================================================
#if WINDOWS
        Dispatcher.Dispatch(() =>
        {
            // Extraemos de forma segura la ventana principal usando el indexador de la lista [0]
            var nativeWindow = App.Current?.Windows?[0]?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow != null)
            {
                // 1. Extraemos el puntero físico (HWND) de la ventana de Microsoft
                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    // 2. Forzamos el modo "FullScreen" nativo (Oculta barras de tareas y marcos de Windows)
                    appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.FullScreen);

                    // 3. 🎯 OBLIGATORIO: Usamos PInvoke para clavar la ventana encima de VS Code, Navegador, etc.
                    // IntPtr(-1) equivale a HWND_TOPMOST en la API nativa de Windows
                    PInvoke.User32.SetWindowPos(
                        windowHandle,
                        new IntPtr(-1),
                        0, 0, 0, 0,
                        PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE | PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE);
                }
            }
        });
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // 🔄 INTERCAMBIO DE AUDIO: Pausamos el fondo y encendemos el combate
        if (App.BackgroundPlayer != null && App.BackgroundPlayer.IsPlaying)
        {
            App.BackgroundPlayer.Stop(); // Se pausa para retomar en el mismo segundo al volver
        }

        if (App.AlarmaPlayer != null && !App.AlarmaPlayer.IsPlaying)
        {
            App.AlarmaPlayer.Play(); // Hace estallar la música de la alarma
        }
    }


    private async void OnDesactivarAlarmaClicked(object? sender, EventArgs? e)
    {
        // 🎯 PURGA DE BARRA DE ESTADO EN ANDROID: Cancela las notificaciones activas al apagar la alarma
        LocalNotificationCenter.Current.CancelAll();

        // 🔄 REVERSO DE AUDIO: Silenciamos el combate y despertamos la rutina diaria
        if (App.AlarmaPlayer != null && App.AlarmaPlayer.IsPlaying)
        {
            App.AlarmaPlayer.Stop(); // Esta se apaga por completo hasta la siguiente alerta
        }

        if (App.BackgroundPlayer != null)
        {
            App.BackgroundPlayer.Play(); // Reanuda la banda sonora permanente de tu app
        }
        // 🎯 AL APAGAR LA ALARMA: Devolvemos las propiedades normales del escritorio
#if WINDOWS
        var nativeWindow = App.Current?.Windows?[0]?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (nativeWindow != null)
        {
            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                // 1. Quitamos la superposición obligatoria regresando a HWND_NOTOPMOST (IntPtr(-2))
                PInvoke.User32.SetWindowPos(
                    windowHandle,
                    new IntPtr(-2),
                    0, 0, 0, 0,
                    PInvoke.User32.SetWindowPosFlags.SWP_NOMOVE | PInvoke.User32.SetWindowPosFlags.SWP_NOSIZE);

                // 2. Restauramos la ventana común de escritorio
                appWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.Default);
            }
        }
#endif

        await Navigation.PopModalAsync();
    }
    // =======================================================================
    // 🎸 COREOGRAFÍA HOVER ELÁSTICA PARA EL BOTÓN DE LA ALARMA CRÍTICA
    // =======================================================================
    private async void OnBtnReclamarMouseIn(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnReclamarGrid == null) return;

        BtnReclamarGrid.CancelAnimations();

        // Latigazo elástico: se infla un 10% y se inclina a -5 grados contracorriente
        await Task.WhenAll(
            BtnReclamarGrid.ScaleToAsync(1.10, 160, Easing.SpringOut),
            BtnReclamarGrid.RotateToAsync(-5, 160, Easing.SpringOut)
        );
    }

    private async void OnBtnReclamarMouseOut(object? sender, Microsoft.Maui.Controls.PointerEventArgs e)
    {
        if (BtnReclamarGrid == null) return;

        BtnReclamarGrid.CancelAnimations();

        // Retorno seco, limpio y fluido a sus valores exactos de inclinación (-2 grados)
        await Task.WhenAll(
            BtnReclamarGrid.ScaleToAsync(1.0, 130, Easing.CubicIn),
            BtnReclamarGrid.RotateToAsync(-2, 130, Easing.CubicIn)
        );
    }

}
