using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

namespace SeriesTracker
{
    public class SubscriptionManager
    {

        private object saveLock = new object();
        public async Task Subscribe(TvDbSeries series) {
            series.IsSubscribed = true;
            var subscriptions = await GetCachedSubscriptions();
            subscriptions.Add(series);

            await Task.Factory.StartNew(() =>
            {
                lock (saveLock)
                {
                    SaveSubscriptions().Wait();
                }
            });
        }

        public async Task Unsubscribe(TvDbSeries series)
        {
            series.IsSubscribed = false;
            var subscriptions = await GetCachedSubscriptions();
            subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);

            await Task.Factory.StartNew(() =>
            {
                lock (saveLock)
                {
                    SaveSubscriptions().Wait();
                }
            });
        }

        public async Task SaveSubscriptions()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("subscriptions.xml", FileMode.Truncate, storage))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<TvDbSeries>));
                    serializer.Serialize(file, (await GetCachedSubscriptions()).ToList());
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }


        public async Task<ObservableCollection<TvDbSeries>> GetSubscriptions()
        {
            return await GetCachedSubscriptions();
        }

        ObservableCollection<TvDbSeries> subscriptions = null;
        private object key = new object();

        private async Task<ObservableCollection<TvDbSeries>> GetCachedSubscriptions() {
            return subscriptions ?? await Task.Factory.StartNew(() =>
            {
                lock (key)
                {
                    if (subscriptions == null)
                    {
                        LoadSubscriptions().ContinueWith(t =>
                        {
                            subscriptions = t.Result;
                        }).Wait();
                    }

                    return subscriptions;
                }
            });
        }

        private async Task<ObservableCollection<TvDbSeries>> LoadSubscriptions()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("subscriptions.xml", FileMode.OpenOrCreate, storage))
                {
                    if (file.Length > 0)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<TvDbSeries>));
                        var collection = new SelfSortingObservableCollection<TvDbSeries, DateTime?>(s => s.NextEpisodeAirDateTime);
                        await collection.AddAll(serializer.Deserialize(file) as List<TvDbSeries>);
                        return collection;                        
                    }
                    else
                    {
                        return new ObservableCollection<TvDbSeries>();
                    }
                }
            }         
        }
    }
}
