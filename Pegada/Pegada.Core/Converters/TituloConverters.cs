using System;
using System.Globalization;
using MobiliVendas.Core.Domain.Commands.Results;
using Xamarin.Forms;

namespace Pegada.Core.Converters
{
    // NSDecimalNumber toUIString (PegadaIOS): pt-BR com 2 casas e sem "R$"; zero aparece como "0,00".
    public class ValorPtBrConverter : IValueConverter
    {
        private static readonly CultureInfo PtBr = new CultureInfo("pt-BR");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case decimal d: return d.ToString("N2", PtBr);
                case double db: return db.ToString("N2", PtBr);
                case int i: return i.ToString("N2", PtBr);
                default: return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    // TituloViewController cellForRow (PegadaIOS): dias entre hoje e o vencimento, sem sinal; vencido (negativo) em
    // vermelho; título pago (CodSituacaoTitulo = 2) não mostra os dias (Defeito #20894).
    public class DiasTituloConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TituloCommandResult titulo))
                return targetType == typeof(Color) ? (object)Color.Black : string.Empty;

            int dias = (int)(titulo.DataVencimento.Date - DateTime.Today).TotalDays;

            if (targetType == typeof(Color))
                return dias < 0 ? Color.Red : Color.Black;

            if (titulo.CodSituacaoTitulo == "2")
                return string.Empty;

            return Math.Abs(dias).ToString(CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
