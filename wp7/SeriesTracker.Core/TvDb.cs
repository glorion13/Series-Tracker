using System;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Xml.Linq;

namespace SeriesTracker
{
    public class TvDb
    {
        private const string ApiKey = "D8E7E19874B4F438";
        private string mirror = null;

        public TvDb()
        {
            new Thread(new ThreadStart(Initialize)).Start();
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
            var download = Observable.FromEvent<DownloadStringCompletedEventHandler, DownloadStringCompletedEventArgs>(
                ev => new DownloadStringCompletedEventHandler((s, e) => ev(e)),
                ev => client.DownloadStringCompleted += ev,
                ev => client.DownloadStringCompleted -= ev)
                .Subscribe(o => ProcessMirrors(o.Result));
            client.DownloadStringAsync(new Uri("http://www.thetvdb.com/api/" + ApiKey + "/mirrors.xml"));
        }

        private void ProcessMirrors(string p)
        {
            mirror = (from path in XDocument.Parse(p).Descendants("mirrorpath")
                      select path.Value).First();
            initialized = true;
            initializing = false;
        }

        public IObservable<TvDbSeries> FindSeries(string name)
        {
            var subject = new Subject<TvDbSeries>();

            Scheduler.NewThread.Schedule(() => {
                Initialize();
                WebClient client = new WebClient();
                var download = Observable.FromEvent<DownloadStringCompletedEventHandler, DownloadStringCompletedEventArgs>(
                    ev => new DownloadStringCompletedEventHandler((s, e) => ev(e)),
                    ev => client.DownloadStringCompleted += ev,
                    ev => client.DownloadStringCompleted -= ev)
                    .Subscribe(o => {
                        var list = from series in XDocument.Parse(o.Result).Descendants("Item")
                                   where string.Equals(series.Descendants("language").First().Value, "en")
                                   select new TvDbSeries() { Title = series.Descendants("translation").First().Value };
                        foreach (var s in list)
                        {
                            subject.OnNext(s);
                        }
                    });
                client.DownloadStringAsync(new Uri(mirror + "/api/GetSeries.php?seriesname="+name+"&language=en"));
            });

            return subject;
        }
    }
}
