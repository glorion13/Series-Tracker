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
using Nito.AsyncEx;

namespace SeriesTracker
{
    public class TvDbSeriesRepository
    {
        private readonly SeriesStorageManager storageManager;
        private readonly TvDb tvDb;
        private readonly Dictionary<TvDbSeries, Task> updates;
        private readonly AsyncLock subscriptionLock = new AsyncLock();
        private readonly AsyncLock seenLock = new AsyncLock();

        public TvDbSeriesRepository(SeriesStorageManager storageManager, TvDb tvDb)
        {
            this.storageManager = storageManager;
            updates = new Dictionary<TvDbSeries, Task>();
            this.tvDb = tvDb;
        }

        public async Task<IDictionary<TvDbSeries, Task>> FindAsync(string seriesName)
        {
            try
            {
                var results = from result in await tvDb.FindSeries(seriesName)
                              join subscribed in await storageManager.GetSavedSeries() on result.Id equals subscribed.Id into matches
                              from match in matches.DefaultIfEmpty()
                              select match ?? result;
                
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
            Task update = null;
            using (await subscriptionLock.LockAsync())
            {
                var needsUpdating = !updates.ContainsKey(series) && (series.Updated == null) || (DateTime.Now - series.Updated > TimeSpan.FromHours(1));
                if (needsUpdating)
                {
                    update = UpdateDataAsync(series);
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
            updateSeriesUnseenEpisodeCount(series);
        }

        private async Task UpdateDataAsync(TvDbSeries series)
        {
            try
            {
                var update = tvDb.UpdateData(series);
                var subs = UpdateSubscirptionStatusAsync(series);

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

        private async Task UpdateSubscirptionStatusAsync(TvDbSeries series)
        {
            var subs = await storageManager.GetSavedSeries();
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

        private void updateSeriesUnseenEpisodeCount(TvDbSeries series)
        {
            int episodeCount = series.Episodes.Count<TvDbSeriesEpisode>(e => !e.IsSeen);
            series.UnseenEpisodeCount = (episodeCount > 0) ? episodeCount.ToString() : "None";
        }

        private async Task SaveSeenAsync(TvDbSeries series)
        {
            using (await seenLock.LockAsync())
            {
                updateSeriesUnseenEpisodeCount(series);

                await Task.Factory.StartNew(() => storageManager.SaveSeen(series));
            }
        }

        public async Task SubscribeAsync(TvDbSeries series) {
            try
            {
                series.IsSubscribed = true;
                var subscriptions = await storageManager.GetSavedSeries();
                subscriptions.Add(series);

                using (await subscriptionLock.LockAsync())
                {
                    if (!updates.ContainsKey(series))
                    {
                        updateSeriesUnseenEpisodeCount(series);
                        storageManager.Save(series);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Error unsubscribing");
            }
        }

        public async Task UnsubscribeAsync(TvDbSeries series)
        {
            try
            {
                series.IsSubscribed = false;
                var subscriptions = await storageManager.GetSavedSeries();
                subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);
                using (await subscriptionLock.LockAsync())
                {
                    storageManager.Remove(series);
                }
            }
            catch
            {
                Debug.WriteLine("Error unsubscribing");
            }
        }
    }
}
