using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Threading;
using System.Reactive.Concurrency;
using System.Xml;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Threading;
using Nito.AsyncEx;
using SeriesTracker.Core;

namespace SeriesTracker
{
    public class TvDbSeriesRepository
    {
        private readonly SeriesStorageManager storageManager;
        private readonly TvDb tvDb;
        private readonly Dictionary<TvDbSeries, Task> updates;
        private readonly AsyncLock subscriptionLock = new AsyncLock();
        private readonly AsyncLock seenLock = new AsyncLock();

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
                              join local in await subscribed on online.Id equals local.Id into matches
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
            var subs = await subscribed;

            var loading = (from series in subs select UpdateSeriesIfNecessaryAsync(series)).ToArray();

            if (updateInBackground)
                return subs;

            await Task.Factory.ContinueWhenAll(loading, _ => { });

            return subs;
        }

        private async Task UpdateSeriesIfNecessaryAsync(TvDbSeries series)
        {
            Task update = null;
            using (await subscriptionLock.LockAsync())
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
                using (await subscriptionLock.LockAsync())
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
            var subs = await subscribed;
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
            using (await seenLock.LockAsync())
            {
                UpdateSeriesUnseenEpisodeCount(series);

                await Task.Factory.StartNew(() => storageManager.SaveSeen(series));
            }
        }

        public async Task SubscribeAsync(TvDbSeries series)
        {
            using (await subscriptionLock.LockAsync())
            {
                series.IsSubscribed = true;
                var subscriptions = await subscribed;
                subscriptions.Add(series);
                UpdateSeriesUnseenEpisodeCount(series);
                storageManager.Save(series);
            }
        }

        public async Task UnsubscribeAsync(TvDbSeries series)
        {
            using (await subscriptionLock.LockAsync())
            {
                series.IsSubscribed = false;
                var subscriptions = await subscribed;
                subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);
                storageManager.Remove(series);
            }
        }
    }
}
