using SQLite;

namespace MauiTime.Models;

public class Evento
{
    public enum FrecuenciaEvento
    {
        Ninguna,
        Diario,
        Semanal,
        Mensual,
        Anual
    }

    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }

    public FrecuenciaEvento Frecuencia { get; set; } = FrecuenciaEvento.Ninguna;

    public long DuracionAvisoTicks { get; set; }

    [Ignore]
    public TimeSpan TiempoAviso
    {
        get => TimeSpan.FromTicks(DuracionAvisoTicks);
        set => DuracionAvisoTicks = value.Ticks;
    }

    /// <summary>
    /// Calcula de forma iterativa y segura el próximo aviso en el futuro real.
    /// </summary>
    public DateTime CalcularProximoAviso()
    {
        var fecha = FechaHora;
        var ahora = DateTime.Now;

        // Si la frecuencia es "Ninguna", no tiene sentido calcular bucles hacia el futuro
        if (Frecuencia == FrecuenciaEvento.Ninguna)
        {
            return fecha - TiempoAviso;
        }

        // Cambiamos el 'if' por un 'while' para asegurar que la fecha avance 
        // tantas veces como sea necesario hasta superar el momento presente actual.
        while (fecha < ahora)
        {
            fecha = Frecuencia switch
            {
                FrecuenciaEvento.Anual => fecha.AddYears(1),
                FrecuenciaEvento.Mensual => fecha.AddMonths(1),
                FrecuenciaEvento.Semanal => fecha.AddDays(7),
                FrecuenciaEvento.Diario => fecha.AddDays(1),
                _ => fecha
            };
        }

        return fecha - TiempoAviso;
    }
    // =========================================================================
    // CONTROL DE VISIBILIDAD DE STICKERS (AGREGA ESTO EN TU EVENTO.CS)
    // =========================================================================

    [Ignore]
    public bool EsAnual => Frecuencia == FrecuenciaEvento.Anual;

    [Ignore]
    public bool EsMensual => Frecuencia == FrecuenciaEvento.Mensual;

    [Ignore]
    public bool EsSemanal => Frecuencia == FrecuenciaEvento.Semanal;

    [Ignore]
    public bool EsDiario => Frecuencia == FrecuenciaEvento.Diario;

    [Ignore]
    public string Hora => FechaHora.ToString("HH:mm");
}
