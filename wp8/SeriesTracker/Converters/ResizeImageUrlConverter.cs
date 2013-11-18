using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace SeriesTracker
{
    public class ResizeImageUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var url = value as string;
            if (string.IsNullOrEmpty(url))
                return null;

            int width;
            if (!int.TryParse(parameter as string, out width))
                throw new ArgumentException("You need to pass in an int as the paramater to specify the desired width");

            return string.Format("http://imageresizer-1.apphb.com/resize?url={0}&width={1}", url, width);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
