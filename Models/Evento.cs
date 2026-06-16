using SQLite;

namespace MauiTime.Models;

public class Evento
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }

    // Indica si el evento se repite cada año (ej. cumpleaños)
    public bool EsAnual { get; set; }

    // Almacenaremos esto como ticks (long) en SQLite, ya que SQLite no guarda TimeSpan nativamente.
    public long DuracionAvisoTicks { get; set; }

    [Ignore] // Esto le dice a SQLite que no intente guardar esta propiedad directamente
    public TimeSpan TiempoAviso
    {
        get => TimeSpan.FromTicks(DuracionAvisoTicks);
        set => DuracionAvisoTicks = value.Ticks;
    }

    public DateTime CalcularProximoAviso()
    {
        var fechaObjetivo = FechaHora;

        // Si es anual y ya pasó, calculamos para el próximo año
        if (EsAnual && fechaObjetivo < DateTime.Now)
        {
            fechaObjetivo = fechaObjetivo.AddYears(1);
        }

        return fechaObjetivo - TiempoAviso;
    }
}