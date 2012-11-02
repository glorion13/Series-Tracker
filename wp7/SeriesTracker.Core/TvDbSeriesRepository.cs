using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace SeriesTracker
{
    public class TvDbSeriesRepository
    {
        private readonly SubscriptionManager subscriptionManager;
        private readonly TvDb tvdb;

        public TvDbSeriesRepository()
        {
            subscriptionManager = new SubscriptionManager();
            tvdb = new TvDb();
        }

        public IObservable<TvDbSeries> Find(string seriesName)
        {
            return tvdb.FindSeries(seriesName);
        }

        public IObservable<TvDbSeries> UpdateData(TvDbSeries series)
        {
            return tvdb.UpdateData(series).Select(s => UpdateSubscirptionStatus(s));
        }

        private TvDbSeries UpdateSubscirptionStatus(TvDbSeries s)
        {
            s.IsSubscribed = subscriptionManager.Subscriptions.Any(series => series.Id == s.Id);
            return s;
        }

        public IEnumerable<TvDbSeries> GetSubscribed()
        {
            return subscriptionManager.Subscriptions;
        }

        public void Subscribe(TvDbSeries series)
        {
            subscriptionManager.Subscribe(series);
        }

        public void Unsubscribe(TvDbSeries series)
        {
            subscriptionManager.Unsubscribe(series);
        }
    }
}
