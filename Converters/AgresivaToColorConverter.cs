using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiTime.Converters;

public class AgresivaToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Si el valor es null o no es booleano, devolvemos gris por seguridad para que no falle la UI
        if (value is not bool esAgresiva)
            return Color.FromArgb("#333333");

        return esAgresiva ? Color.FromArgb("#E31D26") : Color.FromArgb("#333333");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
