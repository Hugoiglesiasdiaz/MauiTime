using Android.App;
using Android.Content.PM;
using Android.OS;

using Android.Content;

namespace MauiTime;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        PurgeActiveNotifications(Intent);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        Intent = intent;
        PurgeActiveNotifications(intent);
    }

    protected override void OnResume()
    {
        base.OnResume();
        PurgeActiveNotifications(Intent);
    }

    private void PurgeActiveNotifications(Intent? intent)
    {
        try
        {
            var notificationManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.NotificationService) as Android.App.NotificationManager;
            if (notificationManager == null) return;

            // 🎯 CORRECCIÓN CRÍTICA: Reemplazamos la llamada rota al plugin por la clave de texto directa que usa Android
            string elementIdKey = "LocalNotificationId";

            if (intent != null && intent.HasExtra(elementIdKey))
            {
                int notificationId = intent.GetIntExtra(elementIdKey, -1);
                if (notificationId != -1)
                {
                    notificationManager.Cancel(notificationId);

                    // Limpiamos en cascada el carril doble (Pre-aviso)
                    if (notificationId < 100000)
                    {
                        notificationManager.Cancel(notificationId + 100000);
                    }
                    else
                    {
                        notificationManager.Cancel(notificationId - 100000);
                    }
                }
            }
        }
        catch
        {
            // Absorción de seguridad para evitar crashes en el ciclo de vida nativo
        }
    }

}
