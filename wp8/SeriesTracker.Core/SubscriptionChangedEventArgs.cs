using System;

namespace SeriesTracker.Core
{
    public sealed class SubscriptionChangedEventArgs : EventArgs
    {
        public SubscriptionChangedEventArgs(TvDbSeries series)
        {
            Series = series;
        }

        public TvDbSeries Series { get; private set; }
    }
}