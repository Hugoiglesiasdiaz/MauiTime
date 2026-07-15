using System;
using Microsoft.Maui.Graphics;
using MauiTime.ViewModels; // Inyectamos el espacio de nombres de tu BaseViewModel

namespace MauiTime.Models
{
    // 💡 CAMBIO DE HERENCIA: Heredamos directamente de BaseViewModel para heredar SetProperty
    public class DiaCalendario : BaseViewModel
    {
        private string _numeroDia = string.Empty;
        private DateTime? _fechaCompleta;
        private bool _esMesActual;
        private bool _esHoy;
        private double _rotacionCelda;
        private Color _colorFondoCelda = Colors.White;
        private bool _tieneTareasPendientes = false;

        // Nuevos campos privados para el control de la banderita punk
        private bool _tieneEventos = false;
        private string _textoBanderita = "!";

        public string NumeroDia
        {
            get => _numeroDia;
            set => SetProperty(ref _numeroDia, value);
        }

        public DateTime? FechaCompleta
        {
            get => _fechaCompleta;
            set => SetProperty(ref _fechaCompleta, value);
        }

        public bool EsMesActual
        {
            get => _esMesActual;
            set => SetProperty(ref _esMesActual, value);
        }

        public bool EsHoy
        {
            get => _esHoy;
            set => SetProperty(ref _esHoy, value);
        }

        public double RotacionCelda
        {
            get => _rotacionCelda;
            set => SetProperty(ref _rotacionCelda, value);
        }

        public Color ColorFondoCelda
        {
            get => _colorFondoCelda;
            set => SetProperty(ref _colorFondoCelda, value);
        }

        public bool TieneTareasPendientes
        {
            get => _tieneTareasPendientes;
            set
            {
                if (SetProperty(ref _tieneTareasPendientes, value))
                {
                    // Sincronización automática: Si tiene tareas, activamos visualmente la banderita
                    TieneEventos = value;
                }
            }
        }

        // =======================================================================
        // 🚨 VINCULACIÓN DIRECTA CON TU NUEVO XAML (BANDERITA)
        // =======================================================================

        public bool TieneEventos
        {
            get => _tieneEventos;
            set => SetProperty(ref _tieneEventos, value);
        }

        public string TextoBanderita
        {
            get => _textoBanderita;
            set => SetProperty(ref _textoBanderita, value);
        }
    }
}
