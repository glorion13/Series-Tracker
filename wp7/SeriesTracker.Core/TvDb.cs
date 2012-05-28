using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Akavache;

namespace SeriesTracker
{
    public class TvDb
    {
        private const string ApiKey = "D8E7E19874B4F438";
        private string mirror = null;

        public TvDb()
        {
            Initialize();
        }

        private readonly object key = new object();
        private bool initialized;
        private void Initialize()
        {
            if (!initialized)
            {
                DoInitialize();
            }
        }

        private void EnsureInitialized() {
            while (!initialized)
            {
                Thread.Sleep(100);
            }
        }

        private void DoInitialize()
        {
            BlobCache.LocalMachine.DownloadUrl("http://www.thetvdb.com/api/" + ApiKey + "/mirrors.xml", TimeSpan.FromDays(1))
                .AsContentString().Subscribe(s => ProcessMirrors(s));
        }

        private void ProcessMirrors(string p)
        {
            mirror = (from path in XDocument.Parse(p).Descendants("mirrorpath")
                      select path.Value).First();
            initialized = true;
        }

        public IObservable<TvDbSeries> FindSeries(string name)
        {
            var subject = new Subject<TvDbSeries>();

            NewThreadScheduler.Default.Schedule(() =>
            {
                EnsureInitialized();

                BlobCache.LocalMachine.DownloadUrl(mirror + "/api/GetSeries.php?seriesname=" + name + "&language=en", TimeSpan.FromHours(1))
                    .AsContentString().Subscribe(result =>
                    {
                        var list = from series in XDocument.Parse(result).Descendants("Series")
                                   where string.Equals(series.Descendants("language").First().Value, "en")
                                   select new TvDbSeries()
                                   {
                                       Title = series.Descendants("SeriesName").First().Value,
                                       Id = series.Descendants("seriesid").First().Value
                                   };
                        foreach (var s in list)
                        {
                            subject.OnNext(s);
                        }
                        subject.OnCompleted();
                    });
            });

            return subject;
        }

        public IObservable<TvDbSeries> UpdateData(TvDbSeries series)
        {
            EnsureInitialized();

            var download = BlobCache.LocalMachine.DownloadUrl(mirror + "/api/" + ApiKey + "/series/" + series.Id + "/all/en.xml", TimeSpan.FromMinutes(15))
            .AsContentString().Select(r =>
            {                
                var doc = XDocument.Parse(r);
                var poster = doc.Descendants("poster").FirstOrDefault();
                if (poster != null ) {
                    if (!string.IsNullOrEmpty(poster.Value))
                    {
                        var originalUrl = mirror + "/banners/" + poster.Value;
                        var newUrl = "http://imageresizer-1.apphb.com/resize?url=" + originalUrl + "&width=147";
                        
                        DispatcherScheduler.Instance.Schedule(() => {
                            series.Image = newUrl;
                        });
                    }
                }

                var rating = doc.Descendants("Rating").FirstOrDefault();
                if (rating != null)
                {
                    if (!string.IsNullOrEmpty(rating.Value))
                    {
                        var newRating = float.Parse(rating.Value);
                        DispatcherScheduler.Instance.Schedule(() => {
                            series.Rating = newRating;
                        });  
                    }
                }
                return series;
            });              

            return download;
        }
    }
}
