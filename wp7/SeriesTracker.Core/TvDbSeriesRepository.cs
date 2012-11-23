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
using GalaSoft.MvvmLight.Threading;

namespace SeriesTracker
{
    public class TvDbSeriesRepository
    {
        private readonly SeriesStorageManager storageManager;
        private readonly TvDb tvdb;
        private readonly Dictionary<TvDbSeries, Task> updates;
        private readonly object subscriptionLock = new object();
        private readonly object seenLock = new object();

        public TvDbSeriesRepository(SeriesStorageManager storageManager)
        {
            this.storageManager = storageManager;
            updates = new Dictionary<TvDbSeries, Task>();
            tvdb = new TvDb();
        }

        public async Task<IDictionary<TvDbSeries, Task>> FindAsync(string seriesName)
        {
            try
            {
                var results = await tvdb.FindSeries(seriesName);
                
                var dict = new Dictionary<TvDbSeries, Task>();
                foreach (var series in results) {
                    dict.Add(series, CheckUpdateSeriesAsync(series));
                }

                return dict;
            }
            catch (XmlException e)
            {
                Debug.WriteLine("Search failed for '{0}', message: '{1}'", seriesName, e.Message);
                return new Dictionary<TvDbSeries, Task>();
            }
        }

        public async Task<ObservableCollection<TvDbSeries>> GetSubscribedAsync()
        {
            var subscriptions = await storageManager.GetSavedSeries();
            foreach (var series in subscriptions)
            {
                CheckUpdateSeriesAsync(series);
            }

            return subscriptions;
        }

        private async Task CheckUpdateSeriesAsync(TvDbSeries series)
        {
            await Task.Factory.StartNew(() => {
                bool needsUpdating = (series.Updated == null) || (DateTime.Now - series.Updated > TimeSpan.FromHours(1));
                if (needsUpdating)
                {
                    Task update;
                    lock (subscriptionLock)
                    {
                        update = UpdateDataAsync(series);
                        updates.Add(series, update);
                    }
                    update.ContinueWith(_ =>
                    {
                        lock (subscriptionLock)
                        {
                            updates.Remove(series);
                            if (series.IsSubscribed)
                                storageManager.Save(series);
                        }
                    });
                }
            });
        }

        private async Task UpdateDataAsync(TvDbSeries series)
        {
            try
            {
                var update = tvdb.UpdateData(series);
                var subs = UpdateSubscirptionStatusAsync(series);

                await update;
                await subs;
                await Task.Factory.StartNew(() => storageManager.SetSeenEpisodes(series));;
                return;
            }
            catch (XmlException e)
            {
                Debug.WriteLine("Search failed for '{0}', message: '{1}'", series.Title, e.Message);
                return;
            }
        }

        private async Task UpdateSubscirptionStatusAsync(TvDbSeries series)
        {
            var subs = await storageManager.GetSavedSeries();
            var isSubscribed = subs.Any(s => series.Id == s.Id);
            series.IsSubscribed = isSubscribed;
        }

        public async Task MarkSeenAsync(TvDbSeries series, TvDbSeriesEpisode episode)
        {
            episode.IsSeen = true;
            await Task.Factory.StartNew(() =>
            {
                lock (seenLock)
                {
                    if (!updates.ContainsKey(series))
                    {
                        storageManager.SaveSeen(series);
                    }
                }
            });
        }
        public async Task UnmarkSeenAsync(TvDbSeries series, TvDbSeriesEpisode episode)
        {
            episode.IsSeen = false;
            await Task.Factory.StartNew(() =>
            {
                lock (seenLock)
                {
                    if (!updates.ContainsKey(series))
                    {
                        storageManager.SaveSeen(series);
                    }
                }
            });
        }

        public async Task SubscribeAsync(TvDbSeries series) {
            series.IsSubscribed = true;
            var subscriptions = await storageManager.GetSavedSeries();
            subscriptions.Add(series);
            await Task.Factory.StartNew(() =>
            {
                lock (subscriptionLock)
                {
                    if (!updates.ContainsKey(series)) {
                        storageManager.Save(series);
                    }
                }
            });
        }

        public async Task UnsubscribeAsync(TvDbSeries series)
        {
            series.IsSubscribed = false;
            var subscriptions = await storageManager.GetSavedSeries();
            subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);
            await Task.Factory.StartNew(() =>
            {
                lock (subscriptionLock)
                {
                    storageManager.Remove(series);
                }
            });
        }
    }
}
