using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using MauiTime.Models;
using MauiTime.Services;

namespace MauiTime.ViewModels
{
    public class AgendaViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly NotificationService _notificationService;
        private bool _isBusy;

        public ObservableCollection<Evento> Eventos { get; } = new();

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public ICommand CargarEventosCommand { get; }
        // Añade este comando en tus propiedades de AgendaViewModel.cs
        public ICommand EventoCompletadoCommand { get; }

        // Modifica o añade esto en tu constructor del ViewModel:
        public AgendaViewModel(DatabaseService databaseService, NotificationService notificationService)
        {
            _databaseService = databaseService;
            _notificationService = notificationService;
            CargarEventosCommand = new Command(async () => await LoadEventosAsync());

            // Comando preparado para cuando el usuario completa un evento en la lista
            EventoCompletadoCommand = new Command<Evento>(async (evento) => await ProcesarEventoVencidoAsync(evento));
        }

        /// <summary>
        /// Avanza la fecha de un evento frecuente y la impacta en la persistencia real.
        /// </summary>
        public async Task ProcesarEventoVencidoAsync(Evento evento)
        {
            if (evento == null) return;

            // Calcular la nueva fecha e impactarla en el modelo
            evento.CalcularProximoAviso();

            if (evento.Frecuencia == Evento.FrecuenciaEvento.Ninguna)
            {
                // Si no se repite, se extirpa del disco duro tras completarse y se borran notificaciones (Punto Ciego 2)
                _notificationService.CancelarRecordatorio(evento.Id);
                await _databaseService.BorrarEventoAsync(evento);
            }
            else
            {
                // Si es frecuente, se actualiza su nueva fecha recalculada en SQLite y se reprograman alertas
                await _databaseService.GuardarEventoAsync(evento);
                await _notificationService.ProgramarRecordatorio(evento);
            }

            // Refrescar la UI de forma limpia
            await LoadEventosAsync();
        }

        public async Task LoadEventosAsync()
{
    // Si ya está ocupado, salimos inmediatamente para evitar solapamientos
    if (IsBusy) return;

    try
    {
        // Activamos el indicador de carga
        IsBusy = true;

        // 1. Obtención de datos en hilo secundario de forma segura
        var lista = await Task.Run(async () =>
        {
            await _databaseService.SeedDataAsync().ConfigureAwait(false);
            var raw = await _databaseService.ObtenerEventosAsync().ConfigureAwait(false);
            return raw.OrderBy(e => e.FechaHora).ToList();
        }).ConfigureAwait(false);

        // 2. Volcado directo y limpio en el hilo de interfaz de usuario
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Eventos.Clear();
            foreach (var item in lista)
            {
                Eventos.Add(item);
            }
        });
    }
    catch (Exception ex)
    {
        // Registramos el error exacto en la consola de depuración para ver si algo falla por dentro
        System.Diagnostics.Debug.WriteLine($"[ERROR CRITICO EN RECARGA]: {ex.Message}");
    }
    finally
    {
        // 3. APAGADO BLINDADO: Garantizamos en el hilo principal que IsBusy pase a false pase lo que pase
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            IsBusy = false;
        });
    }
}

    }
}
