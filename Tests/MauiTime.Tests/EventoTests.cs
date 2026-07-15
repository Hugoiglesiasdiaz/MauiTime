using Microsoft.VisualStudio.TestTools.UnitTesting;
using MauiTime.Models;

namespace MauiTime.Tests;

[TestClass]
public class EventoTests
{
    [TestMethod]
    public void CalcularProximoAviso_AvanzaHastaUnaFechaFuturaCuandoElEventoYaPasoVariasVeces()
    {
        var ahora = DateTime.Now;
        var evento = new Evento
        {
            FechaHora = ahora.AddDays(-10),
            Frecuencia = Evento.FrecuenciaEvento.Diario,
            TiempoAviso = TimeSpan.Zero
        };

        var proximoAviso = evento.CalcularProximoAviso();

        Assert.IsTrue(proximoAviso > ahora, $"Se esperaba una fecha futura, pero se obtuvo {proximoAviso:O}");
    }

    [TestMethod]
    public void Verificar_EventoDiario_PasadoDeDiezMinutos_AvanzaAlDiaSiguiente()
    {
        // Arrange: Evento programado hoy a las 09:00
        var hoy = DateTime.Now;
        var fechaPasada = new DateTime(hoy.Year, hoy.Month, hoy.Day, 9, 0, 0);

        var evento = new Evento
        {
            Titulo = "TEST BUG DIARIO",
            FechaHora = fechaPasada,
            Frecuencia = Evento.FrecuenciaEvento.Diario,
            TiempoAviso = TimeSpan.Zero
        };

        // Act: Ejecutamos el cálculo simulando que ya son las 09:10 (Pasado)
        evento.CalcularProximoAviso();

        // Assert: Debe marcar exactamente el día de mañana a las 09:00 sin saltarse 2 días
        var mañanaEsperado = fechaPasada.AddDays(1);
        Assert.Equals(mañanaEsperado, evento.FechaHora);
    }

}
