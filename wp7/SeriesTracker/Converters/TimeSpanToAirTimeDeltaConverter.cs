using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SeriesTracker.Converters
{
    public class TimeSpanToAirTimeDeltaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var delta = (TimeSpan) value;

            if (delta.Ticks == 0)
                return "as episode airs";

            var orientation = delta.Ticks < 0 ? "before" : "after";

            return string.Format("{0} hours {1} minutes\n{2} air time", Math.Abs(delta.Hours), Math.Abs(delta.Minutes), orientation);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
