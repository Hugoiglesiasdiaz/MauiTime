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

        try
        {
            // Traemos la lista de la base de datos en un hilo secundario primero
            var lista = await _databaseService.ObtenerEventosAsync();

            // Una vez que tenemos los datos listos en la memoria, encendemos el hilo de la UI
            MainThread.BeginInvokeOnMainThread(() =>
            {
                IsBusy = true; // Se enciende solo durante el vaciado y rellenado rápido

                Eventos.Clear();
                foreach (var item in lista)
                {
                    Eventos.Add(item);
                }

                IsBusy = false; // Se apaga de inmediato
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            IsBusy = false;
        }
    }


}