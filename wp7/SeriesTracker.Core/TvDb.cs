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
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

namespace SeriesTracker
{
    public class TvDb
    {
        private const string ApiKey = "D8E7E19874B4F438";
        private string mirror = null;

        private readonly object key = new object();
        
        private bool initialized;
        private async Task EnsureInitialized() {
            await Task.Factory.StartNew(() => {
                lock (key)
                {
                    if (!initialized)
                    {
                        DoInitialize();
                    }
                    while (!initialized)
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        private void DoInitialize()
        {
            var url = "http://www.thetvdb.com/api/" + ApiKey + "/mirrors.xml";
            var wc = new WebClient();
            wc.DownloadStringCompleted += wc_DownloadStringCompleted;
            wc.DownloadStringAsync(new Uri(url));
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                throw e.Error;

            this.mirror = (from path in XDocument.Parse(e.Result).Descendants("mirrorpath")
                           select path.Value).First();
            initialized = true;
        }


        public async Task<IEnumerable<TvDbSeries>> FindSeries(string name)
        {
            await EnsureInitialized();

            var url = mirror + "/api/GetSeries.php?seriesname=" + name + "&language=en";
            var wc = new WebClient();
            string s = await wc.DownloadStringTaskAsync(url);

            var list = from series in XDocument.Parse(s).Descendants("Series")
                        where string.Equals(series.Descendants("language").Select(n => n.Value).FirstOrDefault(), "en")
                        select new TvDbSeries()
                        {
                            Title = series.Descendants("SeriesName").Select(n => n.Value).FirstOrDefault(),
                            Id = series.Descendants("seriesid").Select(n => n.Value).FirstOrDefault(),
                            Banner = series.Descendants("banner").Select(n => string.Format("{0}/banners/{1}", mirror, n.Value)).FirstOrDefault()
                        };

            return list;
        }

        public async Task UpdateData(TvDbSeries series)
        {
            await EnsureInitialized();
            
            var url = mirror + "/api/" + ApiKey + "/series/" + series.Id + "/all/en.xml";
            var wc = new WebClient();
            var updated = DateTime.Now;

            string s = await wc.DownloadStringTaskAsync(url);
          
            var doc = XDocument.Parse(s);
            var poster = doc.Descendants("poster").FirstOrDefault();
            if (poster != null ) {
                if (!string.IsNullOrEmpty(poster.Value))
                {
                    var originalUrl = mirror + "/banners/" + poster.Value;
                    var newUrl = "http://imageresizer-1.apphb.com/resize?url=" + originalUrl + "&width=147";

                    series.Image = originalUrl;
                    series.Thumbnail = newUrl;
                }
            }

            var rating = doc.Descendants("Rating").FirstOrDefault();
            if (rating != null)
            {
                if (!string.IsNullOrEmpty(rating.Value))
                {
                    var newRating = float.Parse(rating.Value, CultureInfo.InvariantCulture.NumberFormat);
                    series.Rating = newRating;
                }
            }

            series.Episodes = new ObservableCollection<TvDbSeriesEpisode>(
            from e in doc.Descendants("Episode")
            select new TvDbSeriesEpisode()
            {
                Name = e.Descendants("EpisodeName").Select(n => n.Value).FirstOrDefault(),
                SeriesNumber = e.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault(),
                EpisodeNumber = e.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault(),
                Description = e.Descendants("Overview").Select(n => n.Value).FirstOrDefault(),
                Image = e.Descendants("filename").Select(n => string.Format("http://imageresizer-1.apphb.com/resize?url={0}/banners/{1}&width=162", mirror, n.Value)).FirstOrDefault()
            });

            series.Updated = updated;
        }
    }
}
