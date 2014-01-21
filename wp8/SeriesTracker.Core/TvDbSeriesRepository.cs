using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Windows.Foundation.Metadata;

namespace SeriesTracker.Core
{
    public class TvDbSeriesRepository
    {
        private readonly SeriesStorageManager storageManager;
        private readonly TvDb tvDb;
        private readonly Dictionary<TvDbSeries, Task> updates;
        private readonly SemaphoreSlim subscriptionLock = new SemaphoreSlim(1);
        private readonly SemaphoreSlim seenLock = new SemaphoreSlim(1);
        private readonly AsyncLazy<ObservableCollection<TvDbSeries>> subscribed;
        public TvDbSeriesRepository(SeriesStorageManager storageManager, TvDb tvDb)
        {
            this.storageManager = storageManager;
            this.tvDb = tvDb;

            updates = new Dictionary<TvDbSeries, Task>();

            subscribed = new AsyncLazy<ObservableCollection<TvDbSeries>>(() =>
            {
                var subscriptions = storageManager.GetSavedSeries();
                var collection = new SelfSortingObservableCollection<TvDbSeries, DateTime?>(s => s.NextEpisodeAirDateTime, new SoonestFirstComparer());
                foreach (var series in subscriptions)
                {
                    collection.Add(series);
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

            List<Task> updates = new List<Task>(subs.Count);
            foreach (var series in subs)
            {
                updates.Add(UpdateSeriesIfNecessaryAsync(series));
            }

            if (updateInBackground)
                return subs;

            await Task.WhenAll(updates);
            return subs;
        }

        private async Task UpdateSeriesIfNecessaryAsync(TvDbSeries series)
        {
            Task update = null;
            using (await subscriptionLock.DisposableWaitAsync())
            {
                var needsUpdating = !updates.ContainsKey(series) && (series.Updated == null) || (DateTime.Now - series.Updated > TimeSpan.FromHours(1));
                if (needsUpdating)
                {
                    update = UpdateSeriesAsync(series);
                    updates.Add(series, update);
                }
            }
            if (update != null)
            {
                await update;
                using (await subscriptionLock.DisposableWaitAsync())
                {
                    updates.Remove(series);
                    if (series.IsSubscribed)
                        storageManager.Save(series);
                }
            }
                    
            await Task.Factory.StartNew(() => storageManager.SetSeenEpisodes(series));
            UpdateSeriesUnseenEpisodeCount(series);
        }

        private async Task UpdateSeriesAsync(TvDbSeries series)
        {
            try
            {
                var update = tvDb.UpdateData(series);
                var subs = UpdateSubscirptionStatusAsync(series);

                await update;
                await subs;
            }
            catch (XmlException e)
            {
                Debug.WriteLine("Search failed for '{0}', message: '{1}'", series.Title, e.Message);
            }
        }

        private async Task UpdateSubscirptionStatusAsync(TvDbSeries series)
        {
            var subs = await subscribed.Value;
            var isSubscribed = subs.Any(s => series.Id == s.Id);
            series.IsSubscribed = isSubscribed;
        }

        public async Task MarkSeenAsync(TvDbSeries series, TvDbSeriesEpisode episode)
        {
            episode.IsSeen = true;

            await SaveSeenAsync(series);
        }

        public async Task UnmarkSeenAsync(TvDbSeries series, TvDbSeriesEpisode episode)
        {
            episode.IsSeen = false;

            await SaveSeenAsync(series);
        }

        private static void UpdateSeriesUnseenEpisodeCount(TvDbSeries series)
        {
            int episodeCount = series.Episodes.Count(e => !e.IsSeen);
            series.UnseenEpisodeCount = (episodeCount > 0) ? episodeCount.ToString() : "None";
        }

        private async Task SaveSeenAsync(TvDbSeries series)
        {
            using (await seenLock.DisposableWaitAsync())
            {
                UpdateSeriesUnseenEpisodeCount(series);

                await Task.Factory.StartNew(() => storageManager.SaveSeen(series));
            }
        }

        public async Task SubscribeAsync(TvDbSeries series) {
            using (await subscriptionLock.DisposableWaitAsync())
            {
                series.IsSubscribed = true;
                var subscriptions = await subscribed.Value;
                subscriptions.Add(series);
                UpdateSeriesUnseenEpisodeCount(series);
                storageManager.Save(series);
            }
        }

        public async Task UnsubscribeAsync(TvDbSeries series)
        {
            using (await subscriptionLock.DisposableWaitAsync())
            {
                series.IsSubscribed = false;
                var subscriptions = await subscribed.Value;
                subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);
                storageManager.Remove(series);
            }
        }
    }
}
