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
using System.Reactive.Linq;

namespace SeriesTracker.Api
{
    public class TvDb
    {
        public TvDb()
        {
            Initialize();
        }

        private readonly object key = new object();
        private bool initializing;
        private bool initialized;
        private void Initialize()
        {
            lock (key)
            {
                if (!initialized && !initializing)
                {
                    initializing = true;
                    DoInitialize();
                }
            }
        }

        private void DoInitialize()
        {
            WebClient client = new WebClient();
            Observable.FromEvent<DownloadStringCompletedEventHandler, DownloadStringCompletedEventArgs>(
                ev => new DownloadStringCompletedEventHandler((s, e) => ev(e)),
                ev => client.DownloadStringCompleted += ev,
                ev => client.DownloadStringCompleted -= ev)
                .Subscribe(o => ProcessMirrors(o.Result));
            client.DownloadStringAsync(new Uri("http://www.thetvdb.com/api/D8E7E19874B4F438/mirrors.xml"));
        }

        private void ProcessMirrors(string p)
        {
            Console.WriteLine(p);
            initializing = false;
        }

    }
}
