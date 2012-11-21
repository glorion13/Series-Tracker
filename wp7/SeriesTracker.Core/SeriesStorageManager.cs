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
using System.Xml.Schema;

namespace SeriesTracker
{
    public sealed class SeriesStorageManager
    {
        private XmlSerializer serializer;
        private XmlSerializer Serializer
        {
            get
            {
                return serializer ?? (serializer = new XmlSerializer(typeof(TvDbSeries)));
            }
        }

        private object ioLock = new object();
        public void Save(TvDbSeries series)
        {
            lock (ioLock)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream(string.Format(@"subscriptions\{0}.xml", series.Id), FileMode.Create, storage))
                {
                    try
                    {
                        Serializer.Serialize(file, series);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                }
            }
        }

        public void Remove(TvDbSeries series)
        {
            lock (ioLock)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    storage.DeleteFile(string.Format(@"subscriptions\{0}.xml", series.Id));
                }
            }
        }

        public async Task<ObservableCollection<TvDbSeries>> GetSavedSeries()
        {
            return await GetCachedSubscriptions();
        }

        
        private object key = new object();
        ObservableCollection<TvDbSeries> subscriptions = null;
        private async Task<ObservableCollection<TvDbSeries>> GetCachedSubscriptions() {
            return subscriptions ?? await Task.Factory.StartNew(() =>
            {
                lock (key)
                {
                    return subscriptions ?? (subscriptions = DoGetSavedSeries());
                }
            });
        }

        private ObservableCollection<TvDbSeries> DoGetSavedSeries()
        {
            var collection = new SelfSortingObservableCollection<TvDbSeries, DateTime?>(s => s.NextEpisodeAirDateTime);

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storage.DirectoryExists("subscriptions"))
                    storage.CreateDirectory("subscriptions");

                var files = storage.GetFileNames(@"subscriptions\*.xml");
                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        using (var stream = new IsolatedStorageFileStream(string.Format(@"subscriptions\{0}", file), FileMode.Open, storage))
                        {
                            var series = (TvDbSeries)Serializer.Deserialize(stream);
                            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {
                                collection.Add(series);
                            });
                        }
                    }
                }
            }

            return collection;
        }
    }
}
