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
            Scheduler.NewThread.Schedule(() => Initialize());
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

        private void EnsureInitialized() {
            while (!initialized || initializing)
            {
                Thread.Sleep(100);
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

        public IObservable<TvDbSeriesBase> FindSeries(string name)
        {
            var subject = new Subject<TvDbSeriesBase>();

            Scheduler.NewThread.Schedule(() => {
                EnsureInitialized();
                WebClient client = new WebClient();
                var download = Observable.FromEvent<DownloadStringCompletedEventHandler, DownloadStringCompletedEventArgs>(
                    ev => new DownloadStringCompletedEventHandler((s, e) => ev(e)),
                    ev => client.DownloadStringCompleted += ev,
                    ev => client.DownloadStringCompleted -= ev)
                    .Subscribe(o => {
                        var list = from series in XDocument.Parse(o.Result).Descendants("Series")
                                   where string.Equals(series.Descendants("language").First().Value, "en")
                                   select new TvDbSeries() { 
                                       Title = series.Descendants("SeriesName").First().Value,
                                       Id = series.Descendants("seriesid").First().Value
                                   };
                        foreach (var s in list)
                        {
                            subject.OnNext(s);              
                        }
                        subject.OnCompleted();
                    });
                client.DownloadStringAsync(new Uri(mirror + "/api/GetSeries.php?seriesname="+name+"&language=en"));
            });

            return subject;
        }

        public IObservable<TvDbSeries> UpdateData(TvDbSeriesBase baseRecord)
        {
            WebClient client = new WebClient();
            var download = Observable.FromEvent<DownloadStringCompletedEventHandler, DownloadStringCompletedEventArgs>(
                ev => new DownloadStringCompletedEventHandler((s, e) => ev(e)),
                ev => client.DownloadStringCompleted += ev,
                ev => client.DownloadStringCompleted -= ev)
            .Select(r =>
            {
                var series = new TvDbSeries(baseRecord);
                
                var doc = XDocument.Parse(r.Result);
                var poster = doc.Descendants("poster").FirstOrDefault();
                if (poster != null ) {
                    if (!string.IsNullOrEmpty(poster.Value))
                    {
                        var originalUrl = mirror + "/banners/" + poster.Value;
                        series.Image = "http://quickthumbnail.com/rspic.php?wm=&wm_size=16&wm_color=1filter=none&filename=" + originalUrl + "&width=147";
                    }
                }

                var rating = doc.Descendants("Rating").FirstOrDefault();
                if (rating != null)
                {
                    if (!string.IsNullOrEmpty(rating.Value))
                    {
                        series.Rating = float.Parse(rating.Value);
                    }
                }
                return series;
            });              
 
            client.DownloadStringAsync(new Uri(mirror + "/api/" + ApiKey + "/series/" + baseRecord.Id + "/all/en.xml"));
            return download;
        }
    }
}
