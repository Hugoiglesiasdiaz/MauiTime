using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using PathShape = Microsoft.Maui.Controls.Shapes.Path;

namespace MauiTime.Views;

public partial class TransicionCristalPage : ContentPage
{
    public TransicionCristalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var descendientes = this.GetVisualTreeDescendants();
        if (descendientes == null) return;

        var centroImpacto = descendientes.OfType<Border>().FirstOrDefault(p => p.StyleId == "CentroImpacto");

        // Recuperamos los 8 fragmentos masivos
        var fragmentos = new List<PathShape>();
        for (int i = 1; i <= 8; i++)
        {
            var f = descendientes.OfType<PathShape>().FirstOrDefault(p => p.StyleId == $"F{i}");
            if (f != null) fragmentos.Add(f);
        }

        // Recuperamos los dos pedazos mini ampliados
        var miniMetralla1 = descendientes.OfType<PathShape>().FirstOrDefault(p => p.StyleId == "M1");
        var miniMetralla2 = descendientes.OfType<PathShape>().FirstOrDefault(p => p.StyleId == "M2");

        if (centroImpacto == null || fragmentos.Count < 8 || miniMetralla1 == null || miniMetralla2 == null) return;

        // ESTADO INICIAL
        centroImpacto.Scale = 0;
        centroImpacto.Opacity = 1;
        if (ContenedorCristales != null) ContenedorCristales.Opacity = 1;

        miniMetralla1.TranslationX = 0; miniMetralla1.TranslationY = 0; miniMetralla1.Rotation = 0;
        miniMetralla2.TranslationX = 0; miniMetralla2.TranslationY = 0; miniMetralla2.Rotation = 0;

        foreach (var f in fragmentos)
        {
            f.TranslationX = 0; f.TranslationY = 0; f.Rotation = 0;
        }

        Task.Run(async () =>
{
    // 1. Pausa de asentamiento inicial para la GPU
    await Task.Delay(80);

    // 🎬 ACTO 1: EL DISPARO (El impacto muerde el centro)
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        centroImpacto.TranslationX = -45;
        centroImpacto.TranslationY = -45;
        await centroImpacto.ScaleToAsync(1.3, 70, Easing.CubicOut);
        _ = centroImpacto.ScaleToAsync(1.0, 30, Easing.Linear);
    });

    // 🎬 ACTO 2: EL ESTALLIDO PARALELO (Desplome por gravedad)
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        _ = centroImpacto.FadeToAsync(0, 120, Easing.Linear);

        // Lanzamos la caída libre de los fragmentos en segundo plano gráfico
        _ = AnimarHastaElSuelo(miniMetralla1, -550, -200, -720);
        _ = AnimarHastaElSuelo(miniMetralla2, 600, -150, 900);

        _ = AnimarHastaElSuelo(fragmentos[0], -250, -100, -180);
        _ = AnimarHastaElSuelo(fragmentos[1], 50, -150, 120);
        _ = AnimarHastaElSuelo(fragmentos[2], 280, -120, 200);
        _ = AnimarHastaElSuelo(fragmentos[3], 350, 50, 160);
        _ = AnimarHastaElSuelo(fragmentos[4], 200, 200, -90);
        _ = AnimarHastaElSuelo(fragmentos[5], -50, 150, 75);
        _ = AnimarHastaElSuelo(fragmentos[7], -350, 0, 110);

        // 🚨 EL TRUCO DEFINITIVO:
        // Hacemos un await real sobre la pieza inferior izquierda masiva (F7).
        // Obligamos al hilo principal a esperar a que este fragmento termine 
        // su caída física completa y smooth de principio a fin.
        await AnimarHastaElSuelo(fragmentos[6], -300, 180, -140);

        // 🎬 ACTO 3: NAVEGACIÓN EN EL MILISEGUNDO DE ORO
        // Como ya terminó la animación y el lienzo quedó limpio de forma natural,
        // MAUI abre el calendario al instante sin freezeos ni pantallas negras muertas.
        await Shell.Current.GoToAsync("//LinternaAnimacionPage");
    });
});

    }

    // Ecuación calibrada: 220ms de latigazo inicial + 900ms de caída progresiva natural
    // Ecuación calibrada: 220ms de latigazo inicial + 1050ms de caída progresiva pesada
    private async Task AnimarHastaElSuelo(PathShape path, double impulsoX, double impulsoY, double rotacionF)
    {
        // 1. ONDA EXPANSIVA (Sacudida elástica corta - 220ms con CubicOut)
        await Task.WhenAll(
            path.TranslateToAsync(impulsoX * 0.2, impulsoY * 0.3, 220, Easing.CubicOut),
            path.RotateToAsync(rotacionF * 0.1, 220, Easing.CubicOut)
        );

        // 2. CAÍDA LIBRE RALENTIZADA (Subido de 900ms a 1050ms para ganar presencia cinematográfica)
        await Task.WhenAll(
            path.TranslateToAsync(impulsoX * 1.5, 1700, 1050, Easing.SinIn),
            path.RotateToAsync(rotacionF, 1050, Easing.SinIn)
        );
    }

}
