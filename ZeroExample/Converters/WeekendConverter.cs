using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ZeroExample.Converters
{ 
    public class WeekendConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday)
                {
                    return Brushes.LightSeaGreen;
                }
                else
                {
                    return Brushes.White;
                }
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
