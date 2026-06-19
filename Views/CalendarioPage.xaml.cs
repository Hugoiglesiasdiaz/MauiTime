using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiTime.Models; // AÑADIDO: Importamos la carpeta de modelos

namespace MauiTime.Views
{
    public partial class CalendarioPage : ContentPage, INotifyPropertyChanged
    {
        private DateTime _mesActualReferencia = DateTime.Today;
        private ObservableCollection<DiaCalendario> _diasDelMes = new();

        public ObservableCollection<DiaCalendario> DiasDelMes
        {
            get => _diasDelMes;
            set { _diasDelMes = value; OnPropertyChanged(); }
        }

        public CalendarioPage()
        {
            InitializeComponent();
            BindingContext = this;
            
            // Generamos el mes inicial
            RefrescarCalendario();
        }

        private void RefrescarCalendario()
        {
            // Actualizamos los textos de nuestro HUD superior nativo
            TxtMes.Text = _mesActualReferencia.ToString("MMMM", new System.Globalization.CultureInfo("es-ES")).ToUpper();
            TxtAnio.Text = _mesActualReferencia.ToString("yyyy");

            // Calculamos la matriz de días
            var listaDias = new List<DiaCalendario>();
            DateTime primerDiaMes = new DateTime(_mesActualReferencia.Year, _mesActualReferencia.Month, 1);
            int totalDiasMes = DateTime.DaysInMonth(_mesActualReferencia.Year, _mesActualReferencia.Month);
            
            // Convertimos DayOfWeek de MAUI (Domingo=0) a formato europeo (Lunes=0, ..., Domingo=6)
            int desfaseInicio = ((int)primerDiaMes.DayOfWeek + 6) % 7;

            // Generador de rotaciones asimétricas orgánicas para el estilo P5
            Random rand = new Random();

            // 1. Días del mes anterior (Fondo atenuado)
            DateTime mesAnterior = primerDiaMes.AddMonths(-1);
            int diasMesAnterior = DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
            for (int i = desfaseInicio - 1; i >= 0; i--)
            {
                listaDias.Add(new DiaCalendario { 
                    NumeroDia = (diasMesAnterior - i).ToString(), 
                    EsMesActual = false,
                    RotacionCelda = rand.Next(-6, 7)
                });
            }

            // 2. Días del mes actual
            for (int dia = 1; dia <= totalDiasMes; dia++)
            {
                var fecha = new DateTime(_mesActualReferencia.Year, _mesActualReferencia.Month, dia);
                listaDias.Add(new DiaCalendario { 
                    NumeroDia = dia.ToString(), 
                    FechaCompleta = fecha,
                    EsMesActual = true,
                    EsHoy = fecha == DateTime.Today,
                    RotacionCelda = rand.Next(-6, 7) // Cada número rota a un ángulo diferente
                });
            }

            // 3. Días del mes siguiente para rellenar el Grid de 7x6 (42 celdas)
            int diaSiguiente = 1;
            while (listaDias.Count < 42)
            {
                listaDias.Add(new DiaCalendario { 
                    NumeroDia = diaSiguiente.ToString(), 
                    EsMesActual = false,
                    RotacionCelda = rand.Next(-6, 7)
                });
                diaSiguiente++;
            }

            DiasDelMes = new ObservableCollection<DiaCalendario>(listaDias);
        }

        private void OnPrevMonthClicked(object? sender, EventArgs e)
        {
            _mesActualReferencia = _mesActualReferencia.AddMonths(-1);
            RefrescarCalendario();
        }

        private void OnNextMonthClicked(object? sender, EventArgs e)
        {
            _mesActualReferencia = _mesActualReferencia.AddMonths(1);
            RefrescarCalendario();
        }

        private async void OnAgendaTabTapped(object? sender, TappedEventArgs e)
        {
            try { await Shell.Current.GoToAsync("//AgendaPage"); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}"); }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
