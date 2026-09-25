using System;
using System.Globalization;
using Xamarin.Forms;

namespace Pegada.Core.Converters
{
    // Botões de data do painel de filtros (PegadaIOS): mostram o título ("Início"/"Fim") até haver data selecionada.
    public class DataOuTextoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime data && data.Date != DateTime.MinValue.Date)
                return data.ToString("dd/MM/yyyy");

            return parameter?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
