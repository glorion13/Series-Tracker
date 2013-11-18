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
using SeriesTracker.Core;

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

        private XmlSerializer seenSerializer;
        private XmlSerializer SeenSerializer
        {
            get
            {
                return seenSerializer ?? (seenSerializer = new XmlSerializer(typeof(List<string>)));
            }
        }

        private object ioLock = new object();
        public void Save(TvDbSeries series)
        {
            lock (ioLock)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!storage.DirectoryExists(string.Format(@"subscriptions\{0}", series.Id)))
                        storage.CreateDirectory(string.Format(@"subscriptions\{0}", series.Id));

                    using (IsolatedStorageFileStream file = new IsolatedStorageFileStream(string.Format(@"subscriptions\{0}\data.xml", series.Id), FileMode.Create, storage))
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
        }

        public void SaveSeen(TvDbSeries series)
        {
            lock (ioLock)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!storage.DirectoryExists(string.Format(@"subscriptions\{0}", series.Id)))
                        storage.CreateDirectory(string.Format(@"subscriptions\{0}", series.Id));

                    using (IsolatedStorageFileStream file = new IsolatedStorageFileStream(string.Format(@"subscriptions\{0}\seen.xml", series.Id), FileMode.Create, storage))
                    {
                        try
                        {
                            SeenSerializer.Serialize(file, series.Episodes.Where(e => e.IsSeen).Select(e => e.Id).ToList());
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
                        }
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
                    if (!storage.FileExists(string.Format(@"subscriptions\{0}\data.xml", series.Id)))
                        return;

                    storage.DeleteFile(string.Format(@"subscriptions\{0}\data.xml", series.Id));
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
            lock (ioLock)
            {
                var collection = new SelfSortingObservableCollection<TvDbSeries, DateTime?>(s => s.NextEpisodeAirDateTime, new SoonestFirstComparer());

                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!storage.DirectoryExists("subscriptions"))
                        storage.CreateDirectory("subscriptions");

                    var dirs = storage.GetDirectoryNames(@"subscriptions\*");
                    if (dirs.Length > 0)
                    {
                        foreach (var dir in dirs)
                        {
                            if (!storage.FileExists(string.Format(@"subscriptions\{0}\data.xml", dir)))
                                continue;

                            TvDbSeries series;
                            using (var stream = new IsolatedStorageFileStream(string.Format(@"subscriptions\{0}\data.xml", dir), FileMode.Open, storage))
                            {
                                if (stream.Length == 0)
                                    continue;

                                series = (TvDbSeries)Serializer.Deserialize(stream);
                            }

                            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                            {
                                collection.Add(series);
                            });
                        }
                    }
                }

                return collection;
            }
        }

        public void SetSeenEpisodes(TvDbSeries series)
        {
            lock (ioLock)
            {
                using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var filename = string.Format(@"subscriptions\{0}\seen.xml", series.Id);

                    if (!storage.FileExists(filename))
                        return;

                    using (var stream = new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, storage))
                    {
                        List<string> seen = (List<string>)SeenSerializer.Deserialize(stream);
                        DispatcherHelper.CheckBeginInvokeOnUI(() =>
                        {
                            foreach (var episodeId in seen)
                            {
                                var episode = series.Episodes.FirstOrDefault(e => e.Id.Equals(episodeId));
                                if (episode != null)
                                    episode.IsSeen = true;                                    
                            }
                        });
                    }
                }
            }
        }
    }
}
