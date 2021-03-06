﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using GalaSoft.MvvmLight.Threading;

namespace SeriesTracker.Core
{
    public sealed class StorageFailureEventArgs : EventArgs
    {
        public StorageFailureEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }

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

        public event EventHandler<StorageFailureEventArgs> StorageFailure;


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
                            OnStorageFailure(
                                new StorageFailureEventArgs(
                                    "Failed to save data to local storage. Any modifications or updates will be lost, sorry! :("));
                        }
                    }
                }
            }
        }

        [Obsolete("Save the whole series")]
        public void SaveSeen(TvDbSeries series)
        {
            Save(series);
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

                            try
                            {
                                using (var stream = new IsolatedStorageFileStream(filename, FileMode.Open, storage))
                                {
                                    if (stream.Length == 0)
                                        continue;

                                    series = (TvDbSeries)Serializer.Deserialize(stream);

                                    var seenFilename = string.Format(@"{0}\{1}\seen.xml", SubscriptionsFolderName, series.Id);

                                    using (new MutexLock(seenFilename))
                                    {
                                        if (storage.FileExists(seenFilename))
                                        {
                                            using (var seenStream = new IsolatedStorageFileStream(seenFilename, FileMode.Open, storage))
                                            {
                                                var seen = (List<string>)SeenSerializer.Deserialize(seenStream);

                                                foreach (var episodeId in seen)
                                                {
                                                    var episode = series.Episodes.FirstOrDefault(e => e.Id.Equals(episodeId));
                                                    if (episode != null)
                                                        episode.IsSeen = true;
                                                }
                                            }

                                            storage.DeleteFile(seenFilename);
                                        }
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                try
                                {
                                    storage.DeleteFile(filename);
                                }
                                catch
                                {

                                }
                                OnStorageFailure(
                                    new StorageFailureEventArgs(
                                        "Error loading series from phone storage. Part of your data will be lost. Very sorry :("));
                                continue;
                            }
                        }

                        yield return series;
                    }
                }
            }
        }

        private void OnStorageFailure(StorageFailureEventArgs storageFailureEventArgs)
        {
            if (StorageFailure != null)
            {
                StorageFailure(this, storageFailureEventArgs);
            }
        }
    }
}
