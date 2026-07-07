using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace MauiTime.Models
{
    public class DiaCalendario : INotifyPropertyChanged
    {
        public string NumeroDia { get; set; } = "";
        public DateTime? FechaCompleta { get; set; }
        public bool EsMesActual { get; set; } = true;
        public bool EsHoy { get; set; } = false;
        public double RotacionCelda { get; set; } = 0;

        // 🚨 PROPIEDAD DINÁMICA: Inicia en blanco y se notificará cuando pase a Rojo Fuego
        private Color _colorFondoCelda = Colors.White;
        public Color ColorFondoCelda
        {
            get => _colorFondoCelda;
            set 
            { 
                _colorFondoCelda = value; 
                OnPropertyChanged(); 
            }
        }

        // SOPORTE DE NOTIFICACIÓN NATIVA DE MAUI
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
