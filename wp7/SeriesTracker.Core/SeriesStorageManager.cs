using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SeriesTracker.Core
{
    public sealed class SeriesStorageManager
    {
        const string SubscriptionsFolderName = "subscriptions";

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

        public SeriesStorageManager()
        {
#if DEBUG
            if (GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
            {
                return;
            }
#endif

            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (new MutexLock(SubscriptionsFolderName))
                {
                    if (!storage.DirectoryExists(SubscriptionsFolderName))
                    {
                        storage.CreateDirectory(SubscriptionsFolderName);
                    }
                }
            }
        }

        public void Save(TvDbSeries series)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var directory = string.Format(@"{0}\{1}", SubscriptionsFolderName, series.Id);

                using (new MutexLock(directory))
                {
                    if (!storage.DirectoryExists(directory))
                        storage.CreateDirectory(directory);
                }

                var filename = string.Format(@"{0}\data.xml", directory);

                using (new MutexLock(filename))
                {
                    using (var file = new IsolatedStorageFileStream(filename, FileMode.Create, storage))
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
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var directory = string.Format(@"{0}\{1}", SubscriptionsFolderName, series.Id);

                using (new MutexLock(directory))
                {
                    if (!storage.DirectoryExists(directory))
                        storage.CreateDirectory(directory);
                }

                var filename = string.Format(@"{0}\seen.xml", directory);

                using (new MutexLock(filename))
                {
                    using (var file = new IsolatedStorageFileStream(filename, FileMode.Create, storage))
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
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var filename = string.Format(@"{0}\{1}\data.xml", SubscriptionsFolderName, series.Id);

                using (new MutexLock(filename))
                {
                    if (!storage.FileExists(filename))
                        return;

                    storage.DeleteFile(filename);
                }
            }
        }

        public IEnumerable<TvDbSeries> GetSavedSeries()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var dirs = storage.GetDirectoryNames(string.Format(@"{0}\*", SubscriptionsFolderName));
                if (dirs.Length > 0)
                {
                    foreach (var dir in dirs)
                    {
                        var filename = string.Format(@"{0}\{1}\data.xml", SubscriptionsFolderName, dir);

                        TvDbSeries series;
                        using (new MutexLock(filename))
                        {
                            if (!storage.FileExists(filename))
                                continue;

                            using (var stream = new IsolatedStorageFileStream(filename, FileMode.Open, storage))
                            {
                                if (stream.Length == 0)
                                    continue;

                                series = (TvDbSeries)Serializer.Deserialize(stream);
                            }
                        }

                        yield return series;
                    }
                }
            }
        }

        public void SetSeenEpisodes(TvDbSeries series)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var filename = string.Format(@"{0}\{1}\seen.xml", SubscriptionsFolderName, series.Id);

                using (new MutexLock(filename))
                {
                    if (!storage.FileExists(filename))
                        return;

                    using (var stream = new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, storage))
                    {
                        var seen = (List<string>)SeenSerializer.Deserialize(stream);

                        foreach (var episodeId in seen)
                        {
                            var episode = series.Episodes.FirstOrDefault(e => e.Id.Equals(episodeId));
                            if (episode != null)
                                episode.IsSeen = true;
                        }
                    }
                }
            }
        }
    }
}
