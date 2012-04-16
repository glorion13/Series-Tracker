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

namespace SeriesTracker
{
    public class SubscriptionManager
    {
        public SubscriptionManager()
        {
        }

        public void Subscribe(TvDbSeries series) {
            series.IsSubscribed = true;
            DoGetSubscriptions().Add(series);
            SaveSubscriptions();
        }

        private void SaveSubscriptions()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("subscriptions.xml", FileMode.Truncate, storage))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<TvDbSeries>));
                    serializer.Serialize(file, DoGetSubscriptions());
                }
            } 
        }


        public IEnumerable<TvDbSeries> Subscriptions {
            get
            {
                return DoGetSubscriptions();
            }
        }

        List<TvDbSeries> subscriptions = null;
        private List<TvDbSeries> DoGetSubscriptions() {
            return subscriptions ?? (subscriptions = LoadSubscriptios());
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
