using SQLite;
using MauiTime.Models;

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
            new Evento { Titulo = "Reunión de Equipo", Descripcion = "Revisión de avances", FechaHora = DateTime.Now, EsAnual = false },
            new Evento { Titulo = "Cumpleaños Mamá", Descripcion = "Comprar regalo", FechaHora = DateTime.Now.AddDays(2), EsAnual = true },
            new Evento { Titulo = "Cita Médico", Descripcion = "Chequeo anual", FechaHora = DateTime.Now.AddDays(5), EsAnual = false },
            new Evento { Titulo = "Entrega Proyecto", Descripcion = "Fecha límite de entrega", FechaHora = DateTime.Now.AddDays(7), EsAnual = false },
            new Evento { Titulo = "Clase de Idiomas", Descripcion = "Nivel intermedio", FechaHora = DateTime.Now.AddDays(8), EsAnual = false },
            new Evento { Titulo = "Aniversario", Descripcion = "Cena especial", FechaHora = DateTime.Now.AddDays(10), EsAnual = true },
            new Evento { Titulo = "Gym", Descripcion = "Rutina de fuerza", FechaHora = DateTime.Now.AddDays(11), EsAnual = false },
            new Evento { Titulo = "Revisión Coche", Descripcion = "Taller mecánico", FechaHora = DateTime.Now.AddDays(14), EsAnual = false },
            new Evento { Titulo = "Pago Alquiler", Descripcion = "Transferencia bancaria", FechaHora = DateTime.Now.AddDays(15), EsAnual = true }
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