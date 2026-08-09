using System.Globalization;
using System.Windows.Data;

namespace ZeroExample.Converters
{
    public class VatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double vat)
            {
                return $"{vat} %";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
