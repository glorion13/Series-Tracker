﻿using System;
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
    public class TvDbSeriesRepository
    {
        private readonly SeriesStorageManager storageManager;
        private readonly TvDb tvDb;
        private readonly Dictionary<TvDbSeries, Task> updates;
        private readonly Nito.AsyncEx.AsyncLock ioLock = new Nito.AsyncEx.AsyncLock();
        private readonly Nito.AsyncEx.AsyncLazy<List<TvDbSeries>> subscribed;

        public event EventHandler<SubscriptionChangedEventArgs> Subscribed;
        public event EventHandler<SubscriptionChangedEventArgs> Unsubscribed;

        public TvDbSeriesRepository(SeriesStorageManager storageManager, TvDb tvDb)
        {
            this.storageManager = storageManager;
            this.tvDb = tvDb;

            updates = new Dictionary<TvDbSeries, Task>();

            subscribed = new Nito.AsyncEx.AsyncLazy<List<TvDbSeries>>(async () =>
            {
                var subscriptions = await Task.Factory.StartNew<IEnumerable<TvDbSeries>>(() => storageManager.GetSavedSeries());
                var collection = new List<TvDbSeries>(subscriptions);
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

        public async Task<IEnumerable<TvDbSeries>> GetSubscribedAsync(bool updateInBackground = true)
        {
            var subs = await subscribed;

            var loading = (from series in subs select UpdateSeriesIfNecessaryAsync(series)).ToArray();

            if (updateInBackground)
                return subs;

            await TaskEx.WhenAll(loading);

            return subs;
        }

        private async Task UpdateSeriesIfNecessaryAsync(TvDbSeries series)
        {
            bool ownsTask = false;
            Task update = null;
            using (await ioLock.LockAsync())
            {
                if (updates.ContainsKey(series))
                    update = updates[series];

                var needsUpdating = (series.Updated == null) || (DateTime.Now - series.Updated > TimeSpan.FromHours(3));
                if (needsUpdating)
                {
                    update = UpdateSeriesAsync(series);
                    updates.Add(series, update);
                    ownsTask = true;
                }
            }
            if (update != null)
            {
                await update;
                if (ownsTask)
                {
                    using (await ioLock.LockAsync())
                    {
                        updates.Remove(series);
                        if (series.IsSubscribed)
                            storageManager.Save(series);
                    }
                }
            }
        }

        private Task UpdateSeriesAsync(TvDbSeries series)
        {
            return TaskEx.WhenAll(new[] { tvDb.UpdateData(series), UpdateSubscriptionStatusAsync(series) });
        }

        private async Task UpdateSubscriptionStatusAsync(TvDbSeries series)
        {
            var subs = await subscribed;
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
            using (await ioLock.LockAsync())
            {
                await Task.Factory.StartNew(() => storageManager.Save(series));
            }
        }

        public async Task SubscribeAsync(TvDbSeries series)
        {
            using (await ioLock.LockAsync())
            {
                var subscriptions = await subscribed;
                subscriptions.Add(series);
                series.IsSubscribed = true;
                OnSubscribed(new SubscriptionChangedEventArgs(series));
                await Task.Factory.StartNew(() => storageManager.Save(series));
            }
        }

        public async Task UnsubscribeAsync(TvDbSeries series)
        {
            using (await ioLock.LockAsync())
            {
                var subscriptions = await subscribed;
                subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);
                series.IsSubscribed = false;
                OnUnsubscribed(new SubscriptionChangedEventArgs(series));
                await Task.Factory.StartNew(() => storageManager.Remove(series));
            }
        }

        public void OnSubscribed(SubscriptionChangedEventArgs eventArgs)
        {
            var handler = Subscribed;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }

        public void OnUnsubscribed(SubscriptionChangedEventArgs eventArgs)
        {
            var handler = Unsubscribed;
            if (handler != null)
            {
                handler(this, eventArgs);
            }
        }
    }
}
