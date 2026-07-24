using MauiTime.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using Plugin.LocalNotification;

namespace MauiTime;

public partial class App : Application
{
    public static IAudioPlayer? BackgroundPlayer { get; private set; }
    public static IAudioPlayer? AlarmaPlayer { get; private set; }

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
            var audioManager = AudioManager.Current;

            // =======================================================================
            // 🎵 CANCIÓN 1: BACKGROUND.MP3 (MÚSICA DE FONDO PERMANENTE)
            // =======================================================================
            bool existeBackground = await FileSystem.Current.AppPackageFileExistsAsync("background.mp3");

            if (existeBackground)
            {
                var streamBg = await FileSystem.OpenAppPackageFileAsync("background.mp3");
                BackgroundPlayer = audioManager.CreatePlayer(streamBg);

                BackgroundPlayer.Loop = true;
                BackgroundPlayer.Volume = 0.20; // Mantenemos tu volumen sutil del 20%

                BackgroundPlayer.Play(); // Arranca sonando en la Agenda/Calendario
                Console.WriteLine("\n[AUDIO] 🎵 ¡ÉXITO! Iniciada música de fondo (background.mp3) desde los assets.\n");
            }
            else
            {
                Console.WriteLine("\n[AUDIO INFO] 'Resources/Raw/background.mp3' no detectado. Fondo en silencio.\n");
            }

            // =======================================================================
            // 🎵 CANCIÓN 2: ALARMA.MP3 (MÚSICA DE ALERTA DE EMBOSCADA)
            // =======================================================================
            bool existeAlarma = await FileSystem.Current.AppPackageFileExistsAsync("alarma.mp3");

            if (existeAlarma)
            {
                var streamAlarm = await FileSystem.OpenAppPackageFileAsync("alarma.mp3");
                AlarmaPlayer = audioManager.CreatePlayer(streamAlarm);

                AlarmaPlayer.Loop = true;
                AlarmaPlayer.Volume = 0.45; // Volumen un poco más alto para generar la tensión del combate

                Console.WriteLine("[AUDIO] 🚨 ¡ÉXITO! Pre-cargada música de alerta (alarma.mp3) lista para emboscadas.\n");
            }
            else
            {
                Console.WriteLine("\n[AUDIO INFO] 'Resources/Raw/alarma.mp3' no detectado. Alerta en silencio.\n");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AUDIO ERROR] Fallo al procesar los recursos musicales empaquetado: {ex.Message}");
        }
    }

}
