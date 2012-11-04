using System;
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

namespace SeriesTracker
{
    public class SubscriptionManager
    {

        private object saveLock = new object();
        public async Task Subscribe(TvDbSeries series) {
            series.IsSubscribed = true;

            await Task.Factory.StartNew(() =>
            {
                lock (saveLock)
                {
                    var subs = GetCachedSubscriptions();
                    subs.ContinueWith(s => s.Result.Add(series)).Wait();
                    SaveSubscriptions();
                }
            });
        }

        public async Task Unsubscribe(TvDbSeries series)
        {
            series.IsSubscribed = false;

            await Task.Factory.StartNew(() =>
            {
                lock (saveLock)
                {
                    var subs = GetCachedSubscriptions();
                    subs.ContinueWith(s => s.Result.Remove(series)).Wait();
                    SaveSubscriptions();
                }
            });
        }

        private void SaveSubscriptions()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("subscriptions.xml", FileMode.Truncate, storage))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<TvDbSeries>));
                serializer.Serialize(file, GetCachedSubscriptions());
            }
        }


        public async Task<IEnumerable<TvDbSeries>> GetSubscriptions() {
            return await GetCachedSubscriptions();
        }

        List<TvDbSeries> subscriptions = null;
        private object key = new object();
        private async Task<List<TvDbSeries>> GetCachedSubscriptions() {
            return subscriptions ?? await Task.Factory.StartNew(() =>
            {
                lock (key)
                {
                    return subscriptions ?? (subscriptions = LoadSubscriptios());
                }
            });
        }

        private List<TvDbSeries> LoadSubscriptios()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("subscriptions.xml", FileMode.OpenOrCreate, storage))
                {
                    if (file.Length > 0)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<TvDbSeries>));
                        return serializer.Deserialize(file) as List<TvDbSeries>;
                    }
                    else
                    {
                        return new List<TvDbSeries>();
                    }
                }
            }         
        }
    }
}
