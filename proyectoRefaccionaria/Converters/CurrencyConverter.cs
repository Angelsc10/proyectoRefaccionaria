using Microsoft.UI.Xaml.Data;
using System;

namespace proyectoRefaccionaria.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        // Convierte el número a formato moneda ($)
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double d) return d.ToString("C2");
            if (value is decimal dec) return dec.ToString("C2");
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}