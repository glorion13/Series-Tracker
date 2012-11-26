using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeriesTracker
{
    public class SubscriptionToFollowConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if (str != null)
            {
                if (str.Equals("True"))
                    return "Unfollow";
                else
                    return "Follow";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
