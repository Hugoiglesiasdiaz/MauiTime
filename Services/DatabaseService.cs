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

    public async Task<int> GuardarEventoAsync(Evento evento)
    {
        await Init();
        // Agregamos el ! después de _dbConnection en ambos casos
        if (evento.Id != 0)
            return await _dbConnection!.UpdateAsync(evento);
        else
            return await _dbConnection!.InsertAsync(evento);
    }

    public async Task<List<Evento>> ObtenerEventosPorMes(int mes, int anio)
    {
        await Init();
        // Agregamos el ! después de _dbConnection
        var eventos = await _dbConnection!.Table<Evento>().ToListAsync();
        return eventos.Where(e => e.FechaHora.Month == mes && e.FechaHora.Year == anio).ToList();
    }
}