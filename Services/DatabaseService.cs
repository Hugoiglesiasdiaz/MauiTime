using SQLite;
using MauiTime.Models;
using static MauiTime.Models.Evento;

namespace MauiTime.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _dbConnection;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private async Task InitAsync()
        {
            if (_dbConnection is not null) return;

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_dbConnection is null)
                {
                    string dbPath;

                    if (DeviceInfo.Current.Platform == DevicePlatform.Android ||
                        DeviceInfo.Current.Platform == DevicePlatform.iOS)
                    {
                        dbPath = Path.Combine(FileSystem.AppDataDirectory, "MauiTimeApp.db3");
                    }
                    else
                    {
                        string carpetaWindows = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MauiTime");
                        if (!Directory.Exists(carpetaWindows)) Directory.CreateDirectory(carpetaWindows);
                        dbPath = Path.Combine(carpetaWindows, "MauiTimeApp.db3");
                    }

                    Console.WriteLine("\n=========================================");
                    Console.WriteLine($"[SQLITE] CONEXIÓN CONFIGURADA EN: {dbPath}");
                    Console.WriteLine("=========================================\n");

                    _dbConnection = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
                    await _dbConnection.CreateTableAsync<Evento>().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SQLITE ERROR INIT] {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Evento>> ObtenerEventosAsync()
        {
            await InitAsync().ConfigureAwait(false);
            return await _dbConnection!.Table<Evento>().ToListAsync().ConfigureAwait(false);
        }

        public async Task SeedDataAsync()
        {
            await InitAsync().ConfigureAwait(false);

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                int conteoRegistros = await _dbConnection!.Table<Evento>().CountAsync().ConfigureAwait(false);
                
                if (conteoRegistros > 0)
                {
                    Console.WriteLine($"[SQLITE] La base de datos ya tiene {conteoRegistros} elementos. Pasando de largo...");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SQLITE ERROR SEED] {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<int> GuardarEventoAsync(Evento evento)
        {
            await InitAsync().ConfigureAwait(false);

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                int resultado;
                if (evento.Id != 0)
                {
                    resultado = await _dbConnection!.UpdateAsync(evento).ConfigureAwait(false);
                    Console.WriteLine($"\n[SQLITE] 📝 EVENTO ACTUALIZADO EN DISCO. ID: {evento.Id}\n");
                }
                else
                {
                    resultado = await _dbConnection!.InsertAsync(evento).ConfigureAwait(false);
                    Console.WriteLine($"\n[SQLITE] 🚀 ¡NUEVO EVENTO GRABADO CON ÉXITO! ASIGNADO ID: {evento.Id}\n");
                }
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[SQLITE ERROR AL GUARDAR] {ex.Message}\n");
                return -1;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task ResetDatabaseAsync()
        {
            await InitAsync().ConfigureAwait(false);
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await _dbConnection!.DeleteAllAsync<Evento>().ConfigureAwait(false);
                Console.WriteLine("[SQLITE] Toda la tabla de eventos ha sido vaciada.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<Evento>> ObtenerEventosPorMes(int mes, int anio)
        {
            await InitAsync().ConfigureAwait(false);
            var todos = await _dbConnection!.Table<Evento>().ToListAsync().ConfigureAwait(false);
            return todos.Where(e => e.FechaHora.Month == mes && e.FechaHora.Year == anio).ToList();
        }

        public async Task<int> BorrarEventoAsync(Evento evento)
        {
            await InitAsync().ConfigureAwait(false);
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                int res = await _dbConnection!.DeleteAsync(evento).ConfigureAwait(false);
                Console.WriteLine($"\n[SQLITE] 💣 REGISTRO ELIMINADO FÍSICAMENTE. ID: {evento.Id}\n");
                return res;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
