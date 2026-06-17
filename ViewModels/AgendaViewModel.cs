using System.Collections.ObjectModel;
using System.Windows.Input; // Necesario para ICommand
using MauiTime.Models;
using MauiTime.Services;

namespace MauiTime.ViewModels;

public class AgendaViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private bool _isBusy;

    public ObservableCollection<Evento> Eventos { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    // Comando para actualizar la lista manualmente (ej: al hacer pull-to-refresh)
    public ICommand CargarEventosCommand { get; }

    public AgendaViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        
        // Inicializamos el comando
        CargarEventosCommand = new Command(async () => await LoadEventosAsync());
        
        // Carga inicial
        _ = LoadEventosAsync();
    }

    public async Task LoadEventosAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var lista = await _databaseService.ObtenerEventosAsync();
            
            // Si la lista no ha cambiado, evitamos borrar y rellenar todo
            if (Eventos.Count != lista.Count)
            {
                Eventos.Clear();
                foreach (var item in lista)
                {
                    Eventos.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            // Aquí podrías añadir un log o alerta al usuario
            System.Diagnostics.Debug.WriteLine($"Error cargando eventos: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}