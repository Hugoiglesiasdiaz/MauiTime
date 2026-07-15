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
        public AgendaViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
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
                // Si no se repite, se extirpa del disco duro tras completarse
                await _databaseService.BorrarEventoAsync(evento);
            }
            else
            {
                // Si es frecuente, se actualiza su nueva fecha recalculada en SQLite
                await _databaseService.GuardarEventoAsync(evento);
            }

            // Refrescar la UI de forma limpia
            await LoadEventosAsync();
        }

        public async Task LoadEventosAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // 1. Verificar datos iniciales en SQLite
                await _databaseService.SeedDataAsync();

                // 2. Extraer todos los eventos de la base de datos
                var lista = await _databaseService.ObtenerEventosAsync();

                // 3. 🎯 ORDEN CRÍTICO ASCENDENTE: Los más cercanos/vencidos van PRIMERO
                // .OrderBy(e => e.FechaHora) coloca el tiempo menor (más antiguo/cercano) arriba,
                // y las fechas futuras más lejanas se van acumulando al final de la lista.
                var listaOrdenada = lista.OrderBy(e => e.FechaHora).ToList();

                // 4. Volcado atómico en el hilo principal de Windows Desktop
                MainThread.BeginInvokeOnMainThread(async () =>
            {
                // 1. Vaciamos la lista por completo
                Eventos.Clear();

                // 2. ⚡ TRUCO MAUI WINDOWS: Forzamos un micro-retraso asíncrono.
                // Esto obliga al motor gráfico a procesar que la lista está vacía y destruir las celdas viejas.
                await Task.Delay(10);

                // 3. Inyectamos los elementos limpios y ordenados cronológicamente
                foreach (var item in listaOrdenada)
                {
                    Eventos.Add(item);
                }
            });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ViewModel Error] {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
