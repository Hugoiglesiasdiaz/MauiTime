using MauiTime.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using Plugin.LocalNotification;

namespace MauiTime;

public partial class App : Application
{
    // Propiedad global para controlar la música desde cualquier otra pantalla de la app
    public static IAudioPlayer? SoundtrackPlayer { get; private set; }

    public App()
    {
        InitializeComponent();

        // 🚨 EL CAPTURADOR SUPREMO DE CRASHES (Tu lógica nativa e intacta en Disco D)
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            var ex = (Exception)error.ExceptionObject;
            try
            {
                System.IO.File.WriteAllText(@"D:\Error_Crash_Maui.txt", ex.ToString());
            }
            catch { }
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Creamos la ventana de forma nativa e impecable
        var window = new Window(new AppShell());

        // 🖥️ INICIALIZACIÓN UNIFICADA EN EL SEGUNDO FRAME DE WINDOWS
        window.Dispatcher.Dispatch(async () =>
        {
            // 1. Inicialización segura de la Base de Datos SQLite (Tu lógica original)
            var dbService = IPlatformApplication.Current?.Services.GetService<DatabaseService>();
            if (dbService != null)
            {
                await dbService.SeedDataAsync();
            }

            // 2. 📱 SOLICITUD DE PERMISOS EN CALIENTE (ANDROID) - REUBICADO CON SEGURIDAD
#if ANDROID
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                await LocalNotificationCenter.Current.RequestNotificationPermission();
            }

            // Permiso de superposición para invadir la pantalla en Android si está en otra App
            if (!Android.Provider.Settings.CanDrawOverlays(Android.App.Application.Context))
            {
                var intent = new Android.Content.Intent(
                    Android.Provider.Settings.ActionManageOverlayPermission,
                    Android.Net.Uri.Parse($"package:{Android.App.Application.Context.PackageName}"));
                intent.Flags = Android.Content.ActivityFlags.NewTask;
                Android.App.Application.Context.StartActivity(intent);
            }
#endif

            // 3. 🎵 INICIALIZACIÓN DE LA BANDA SONORA INTEGRADA (ESTILO GITHUB)
            await InicializarAudioProyectoAsync();

            // =========================================================================
            // 🚨 EL DETONADOR DE EMBOSCADAS DE WINDOWS (INTEGRADO Y REPARADO)
            // =========================================================================
            LocalNotificationCenter.Current.NotificationReceived += async (e) =>
            {
                // Si el ID es del pre-aviso de 30 minutos, lo ignoramos
                if (e.Request.NotificationId >= 100000) return;

                // 🎯 REPARACIÓN SUPREMA WINDOWS: Si la descripción contiene la marca del switch apagado,
                // Windows mostrará su banner común pero frenamos de golpe la invasión a pantalla completa.
                if (e.Request.Description.StartsWith("[BANNER]"))
                {
                    return;
                }

                // Forzamos a que la ventana de la aplicación cobre vida mediante hilos gráficos
                window.Dispatcher.Dispatch(async () =>
                {
                    // ⚡ REPARACIÓN DE MINIMIZADO: Forzamos la restauración nativa por hardware en Windows
#if WINDOWS
                    var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                    if (nativeWindow != null)
                    {
                        // 1. Extraemos el puntero de control físico de la ventana de Microsoft
                        var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

                        // 2. SECUENCIA DE CONTROL SUPREMA: Rompe la suspensión de Windows
                        PInvoke.User32.ShowWindow(windowHandle, PInvoke.User32.WindowShowStyle.SW_RESTORE);
                        PInvoke.User32.ShowWindow(windowHandle, PInvoke.User32.WindowShowStyle.SW_SHOW);
                        PInvoke.User32.SetForegroundWindow(windowHandle);
                    }
#endif

                    // Lanzamos la app encima de cualquier juego o navegador abierto con el modal rojo gigante
                    if (window.Page?.Navigation != null)
                    {
                        await window.Page.Navigation.PushModalAsync(new Views.AlarmaCriticaPage(e.Request.Title, e.Request.Description));
                    }
                });
            };
        });

        return window;
    }

    private async Task InicializarAudioProyectoAsync()
    {
        try
        {
            // 🎯 BLINDAJE ABSOLUTO GITHUB: Verificamos de forma asíncrona si el asset existe en el paquete compilado
            bool existeCancion = await FileSystem.Current.AppPackageFileExistsAsync("soundtrack.mp3");

            if (!existeCancion)
            {
                Console.WriteLine("\n[AUDIO REPOSITORIO] 'Resources/Raw/soundtrack.mp3' no detectado. Modo silencioso activo.\n");
                return;
            }

            var streamMusica = await FileSystem.OpenAppPackageFileAsync("soundtrack.mp3");
            SoundtrackPlayer = AudioManager.Current.CreatePlayer(streamMusica);

            SoundtrackPlayer.Loop = true;
            SoundtrackPlayer.Volume = 0.20;

            SoundtrackPlayer.Play();
            Console.WriteLine("\n[AUDIO] 🎵 ¡ÉXITO! Iniciada banda sonora integrada desde los assets del proyecto.\n");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AUDIO ERROR] Fallo al procesar el recurso musical empaquetado: {ex.Message}");
        }
    }
}
