using System;

namespace MauiTime.Models
{
    public class DiaCalendario
    {
        public string NumeroDia { get; set; } = "";
        public DateTime? FechaCompleta { get; set; }
        public bool EsMesActual { get; set; } = true;
        public bool EsHoy { get; set; } = false;
        public double RotacionCelda { get; set; } = 0;
    }
}