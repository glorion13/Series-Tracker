using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SeriesTracker.Core
{
    public static class SharedSettings
    {
        public const string NotificationDeltaKey = "notificationDelta";

        private const string SharedFileName = "sharedSettings.xml"; 

        private static readonly Lazy<XmlSerializer> serializer = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Dictionary<string, object>)));
        private static XmlSerializer Serializer
        {
            get
            {
                return serializer.Value;
            }
        }

        public static object Get(string key)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (new MutexLock(SharedFileName))
                {
                    if (!storage.FileExists(SharedFileName))
                        return null;

                    using (var stream = new IsolatedStorageFileStream(SharedFileName, FileMode.Open, storage))
                    {
                        var dictionary = (Dictionary<string, object>)Serializer.Deserialize(stream);
                        if (!dictionary.ContainsKey(key))
                            return null;

                        return dictionary[key];
                    }
                }
            }
        }

        public static void Set(string key, object value)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (new MutexLock(SharedFileName))
                {
                    using (var stream = new IsolatedStorageFileStream(SharedFileName, FileMode.OpenOrCreate, storage))
                    {
                        var dictionary = (stream.Length != 0)
                            ? (Dictionary<string, object>) Serializer.Deserialize(stream)
                            : new Dictionary<string, object>();

                        dictionary[key] = value;

                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);

                        Serializer.Serialize(stream, dictionary);
                    }
                }
            }
        }
    }
}
