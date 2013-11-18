using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeriesTracker.Core
{
    class SoonestFirstComparer : IComparer<DateTime?>
    {
        public int Compare(DateTime? x, DateTime? y)
        {
            if (x == null)
                return int.MaxValue;

            if (y == null)
                return int.MaxValue;

            else
                return x == y ? 0 : x < y ? -1 : 1;
        }
    }
}
