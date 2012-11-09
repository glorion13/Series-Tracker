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

            await Task.Factory.StartNew(() =>
            {
                lock (saveLock)
                {
                    var subs = GetCachedSubscriptions();
                    subs.ContinueWith(async s => {
                        await DispatcherHelper.UIDispatcher.InvokeAsync(async () => (await s).Add(series));
                    }).Unwrap().Wait();
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
                    GetCachedSubscriptions().ContinueWith(s =>
                    {
                        DispatcherHelper.UIDispatcher.InvokeAsync(() => {
                            ObservableCollection<TvDbSeries> subscriptions = s.Result;
                            subscriptions.RemoveAllThatMatch(m => series.Id == m.Id);
                        }).Wait();
                    }).Wait();
                    SaveSubscriptions();
                }
            });
        }

        private async void SaveSubscriptions()
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
                    return subscriptions ?? (subscriptions = LoadSubscriptios());
                }
            });
        }

        private ObservableCollection<TvDbSeries> LoadSubscriptios()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("subscriptions.xml", FileMode.OpenOrCreate, storage))
                {
                    if (file.Length > 0)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<TvDbSeries>));
                        var collection = new SelfSortingObservableCollection<TvDbSeries, string>(s => s.Title);
                        collection.AddAll(serializer.Deserialize(file) as List<TvDbSeries>);
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
