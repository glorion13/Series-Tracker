using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Windows.Threading;
using System.Reactive.Concurrency;
using System.Xml;
using System.Diagnostics;
using System.Threading.Tasks;

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
            try
            {
                return tvdb.FindSeries(seriesName);
            }
            catch (XmlException e)
            {
                Debug.WriteLine("Search failed for '{0}', message: '{1}'", seriesName, e.Message);
                return Observable.Empty<TvDbSeries>();
            }
        }

        public async Task UpdateData(TvDbSeries series)
        {
            try
            {
                var update = tvdb.UpdateData(series);
                var subs = UpdateSubscirptionStatus(series);
                await update;
                await subs;
                return;
            }
            catch (XmlException e)
            {
                Debug.WriteLine("Search failed for '{0}', message: '{1}'", series.Title, e.Message);
                return;
            }
        }

        private async Task UpdateSubscirptionStatus(TvDbSeries s)
        {
            var isSubscribed = await Task.Factory.StartNew(() => subscriptionManager.Subscriptions.Any(series => series.Id == s.Id));
            s.IsSubscribed = isSubscribed;
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
