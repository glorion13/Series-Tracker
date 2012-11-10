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
using System.Collections.ObjectModel;

namespace SeriesTracker
{
    public class TvDbSeriesRepository
    {
        private readonly SubscriptionManager subscriptionManager;
        private readonly TvDb tvdb;

        public TvDbSeriesRepository(SubscriptionManager subscriptionManager)
        {
            this.subscriptionManager = subscriptionManager;
            tvdb = new TvDb();
        }

        public async Task<IEnumerable<TvDbSeries>> Find(string seriesName)
        {
            try
            {
                return await tvdb.FindSeries(seriesName);
            }
            catch (XmlException e)
            {
                Debug.WriteLine("Search failed for '{0}', message: '{1}'", seriesName, e.Message);
                return new List<TvDbSeries>();
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

        private async Task UpdateSubscirptionStatus(TvDbSeries series)
        {
            var isSubscribed = await Task.Factory.StartNew(async () =>
            {
                var subs = await subscriptionManager.GetSubscriptions();
                return subs.Any(s => series.Id == s.Id);
            }).Unwrap();
            series.IsSubscribed = isSubscribed;
        }

        public async Task<ObservableCollection<TvDbSeries>> GetSubscribed()
        {
            var subscriptions = await subscriptionManager.GetSubscriptions();
            foreach (var sub in subscriptions.ToList())
            {
                CheckUpdateSeries(sub);
            }

            return subscriptions;
        }

        private async Task CheckUpdateSeries(TvDbSeries series)
        {
            bool needsUpdating = DateTime.Now - series.Updated > TimeSpan.FromMinutes(30);
            if (needsUpdating)
            {
                await UpdateData(series);
            }
        }

        public async Task Subscribe(TvDbSeries series)
        {
            await subscriptionManager.Subscribe(series);
        }

        public async Task Unsubscribe(TvDbSeries series)
        {
            await subscriptionManager.Unsubscribe(series);
        }
    }
}
