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

    public bool EsAlarmaAgresiva { get; set; }


    /// <summary>
    /// Avanza de forma infalible la FechaHora original al futuro real y devuelve el instante del aviso.
    /// </summary>
    public DateTime CalcularProximoAviso()
    {
        var ahora = DateTime.Now;

        // Si no tiene frecuencia, la fecha se mantiene intacta.
        if (Frecuencia == FrecuenciaEvento.Ninguna)
        {
            return FechaHora - TiempoAviso;
        }

        // Avanzar de forma iterativa y matemática la FechaHora real del objeto
        // Usamos <= ahora para garantizar que pase obligatoriamente al futuro.
        while (FechaHora <= ahora)
        {
            FechaHora = Frecuencia switch
            {
                FrecuenciaEvento.Diario => FechaHora.AddDays(1),
                FrecuenciaEvento.Semanal => FechaHora.AddDays(7),
                FrecuenciaEvento.Mensual => FechaHora.AddMonths(1),
                FrecuenciaEvento.Anual => FechaHora.AddYears(1),
                _ => FechaHora
            };
        }

        // Retorna el instante en el que debe sonar la alarma respecto a la nueva fecha futura
        return FechaHora - TiempoAviso;
    }

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
