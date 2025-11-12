using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace proyectoRefaccionaria.Converters
{
    public class StockToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // Si el valor es un número y es menor a 10...
            if (value is int stock && stock < 10)
            {
                // ... ¡ALERTA! Retorna Rojo
                return Application.Current.Resources["DangerColor"] as SolidColorBrush;
            }

            // Si no, retorna el color de texto normal (Claro)
            // OJO: Si usas 'SuccessColor' (verde) por defecto, cámbialo aquí
            return Application.Current.Resources["TextColorOnAccent"] as SolidColorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}