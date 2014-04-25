using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation.Metadata;
using GalaSoft.MvvmLight.Threading;

namespace SeriesTracker.Core
{
    public class TvDbSeriesRepository
    {
        private readonly SeriesStorageManager storageManager;
        private readonly TvDb tvDb;
        private readonly Dictionary<TvDbSeries, Task> updates;
        private readonly SemaphoreSlim ioLock = new SemaphoreSlim(1);
        private readonly AsyncLazy<ObservableCollection<TvDbSeries>> subscribed;
        public TvDbSeriesRepository(SeriesStorageManager storageManager, TvDb tvDb)
        {
            this.storageManager = storageManager;
            this.tvDb = tvDb;

            updates = new Dictionary<TvDbSeries, Task>();

            subscribed = new AsyncLazy<ObservableCollection<TvDbSeries>>(async () =>
            {
                var subscriptions = storageManager.GetSavedSeries();
                var collection = new SelfSortingObservableCollection<TvDbSeries, DateTime?>(s => s.NextEpisodeAirDateTime, new SoonestFirstComparer());
                foreach (var series in subscriptions)
                {
                    await DispatcherHelper.UIDispatcher.InvokeAsync(() => collection.Add(series));
                }

                return collection;
            });
        }

        public async Task<IDictionary<TvDbSeries, Task>> FindAsync(string seriesName)
        {
            try
            {
                var results = from online in await tvDb.FindSeries(seriesName)
                              join local in await subscribed.Value on online.Id equals local.Id into matches
                              from match in matches.DefaultIfEmpty()
                              select match ?? online;

                return results.ToDictionary(series => series, UpdateSeriesIfNecessaryAsync);
            }
            catch (XmlException e)
            {
                Debug.WriteLine("Search failed for '{0}', message: '{1}'", seriesName, e.Message);
                return new Dictionary<TvDbSeries, Task>();
            }
        }

        public async Task<ObservableCollection<TvDbSeries>> GetSubscribedAsync(bool updateInBackground = true)
        {
            var subs = await subscribed.Value;

            var loading = (from series in subs select UpdateSeriesIfNecessaryAsync(series)).ToArray();

            if (updateInBackground)
                return subs;

            await Task.WhenAll(loading);
            return subs;
        }

        private async Task UpdateSeriesIfNecessaryAsync(TvDbSeries series)
        {
            Task update = null;
            using (await ioLock.DisposableWaitAsync())
            {
                var needsUpdating = !updates.ContainsKey(series) && ((series.Updated == null) || (DateTime.Now - series.Updated > TimeSpan.FromHours(1)));
                if (needsUpdating)
                {
                    update = UpdateSeriesAsync(series);
                    updates.Add(series, update);
                }
            }
            if (update != null)
            {
                await update;
                using (await ioLock.DisposableWaitAsync())
                {
                    updates.Remove(series);
                    if (series.IsSubscribed)
                        storageManager.Save(series);
                }
            }
        }

        private Task UpdateSeriesAsync(TvDbSeries series)
        {
            return Task.WhenAll(new[] { tvDb.UpdateData(series), UpdateSubscriptionStatusAsync(series) });
        }

        private async Task UpdateSubscriptionStatusAsync(TvDbSeries series)
        {
            var subs = await subscribed.Value;
            var isSubscribed = subs.Any(s => series.Id == s.Id);
            series.IsSubscribed = isSubscribed;
        }

        public async Task MarkSeenAsync(TvDbSeries series, TvDbSeriesEpisode episode)
        {
            episode.IsSeen = true;

            await SaveAsync(series);
        }

        public async Task UnmarkSeenAsync(TvDbSeries series, TvDbSeriesEpisode episode)
        {
            episode.IsSeen = false;

            await SaveAsync(series);
        }

        public async Task SaveAsync(TvDbSeries series)
        {
            using (await ioLock.DisposableWaitAsync())
            {
                await Task.Factory.StartNew(() => storageManager.Save(series));
            }
        }

        public async Task SubscribeAsync(TvDbSeries series) {
            using (await ioLock.DisposableWaitAsync())
            {
                var subscriptions = await subscribed.Value;
                subscriptions.Add(series);
                series.IsSubscribed = true;
                await Task.Factory.StartNew(() => storageManager.Save(series));
            }
        }

        public async Task UnsubscribeAsync(TvDbSeries series)
        {
            using (await ioLock.DisposableWaitAsync())
            {
                var subscriptions = await subscribed.Value;
                subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);
                await Task.Factory.StartNew(() => storageManager.Remove(series));
                series.IsSubscribed = false;
            }
        }
    }
}
