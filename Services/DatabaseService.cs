using SQLite;
using MauiTime.Models;
using static MauiTime.Models.Evento;

namespace MauiTime.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _dbConnection;

    private async Task Init()
    {
        if (_dbConnection is not null) return;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "MauiTime.db3");
        _dbConnection = new SQLiteAsyncConnection(dbPath);

        await _dbConnection.CreateTableAsync<Evento>();
    }

    public async Task<List<Evento>> ObtenerEventosAsync()
    {
        await Init();
        // Agregamos el ! después de _dbConnection
        return await _dbConnection!.Table<Evento>().ToListAsync();
    }

    public async Task SeedDataAsync()
    {
        await Init();

        // Verificar si ya hay datos para no duplicar
        var eventos = await _dbConnection!.Table<Evento>().ToListAsync();
        if (eventos.Count == 0)
        {
            var mockEventos = new List<Evento>
{
    new Evento { Titulo = "Meditación", Descripcion = "10 min de paz", FechaHora = DateTime.Now, Frecuencia = FrecuenciaEvento.Diario },
    new Evento { Titulo = "Reunión de Equipo", Descripcion = "Avances", FechaHora = DateTime.Now.AddDays(1), Frecuencia = FrecuenciaEvento.Semanal },
    new Evento { Titulo = "Pago Alquiler", Descripcion = "Transferencia", FechaHora = DateTime.Now.AddDays(2), Frecuencia = FrecuenciaEvento.Mensual },
    new Evento { Titulo = "Cumpleaños Mamá", Descripcion = "Comprar regalo", FechaHora = DateTime.Now.AddDays(3), Frecuencia = FrecuenciaEvento.Anual },
    new Evento { Titulo = "Cita Médico", Descripcion = "Chequeo", FechaHora = DateTime.Now.AddDays(5), Frecuencia = FrecuenciaEvento.Ninguna },
    new Evento { Titulo = "Suscripción Revista", Descripcion = "Renovación", FechaHora = DateTime.Now.AddDays(7), Frecuencia = FrecuenciaEvento.Mensual },
    new Evento { Titulo = "Clase de Idiomas", Descripcion = "Intermedio", FechaHora = DateTime.Now.AddDays(8), Frecuencia = FrecuenciaEvento.Semanal },
    new Evento { Titulo = "Aniversario", Descripcion = "Cena", FechaHora = DateTime.Now.AddDays(10), Frecuencia = FrecuenciaEvento.Anual },
    new Evento { Titulo = "Gym", Descripcion = "Fuerza", FechaHora = DateTime.Now.AddDays(11), Frecuencia = FrecuenciaEvento.Diario },
    new Evento { Titulo = "Revisión Coche", Descripcion = "Taller", FechaHora = DateTime.Now.AddDays(14), Frecuencia = FrecuenciaEvento.Mensual }
};

            await _dbConnection.InsertAllAsync(mockEventos);
        }
    }

    public async Task<int> GuardarEventoAsync(Evento evento)
    {
        await Init();
        // Agregamos el ! después de _dbConnection en ambos casos
        if (evento.Id != 0)
            return await _dbConnection!.UpdateAsync(evento);
        else
            return await _dbConnection!.InsertAsync(evento);
    }

    public async Task ResetDatabaseAsync()
    {
        await Init();
        await _dbConnection!.DeleteAllAsync<Evento>(); // Borra todos los registros actuales
    }

    public async Task<List<Evento>> ObtenerEventosPorMes(int mes, int anio)
    {
        await Init();
        // Agregamos el ! después de _dbConnection
        var eventos = await _dbConnection!.Table<Evento>().ToListAsync();
        return eventos.Where(e => e.FechaHora.Month == mes && e.FechaHora.Year == anio).ToList();
    }
}