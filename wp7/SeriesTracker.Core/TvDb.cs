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
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

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

        private static List<string> daysOfWeek = CultureInfo.InvariantCulture.DateTimeFormat.DayNames.Select(d => d.ToLowerInvariant()).ToList();

        public async Task UpdateData(TvDbSeries series)
        {
            await EnsureInitialized();
            
            var url = string.Format("{0}/api/{1}/series/{2}/all/en.xml", mirror, ApiKey, series.Id);
            var wc = new WebClient();
            var updated = DateTime.Now;

            string s = await wc.DownloadStringTaskAsync(url);
          
            var doc = XDocument.Parse(s);
            var poster = doc.Descendants("poster").FirstOrDefault();
            if (poster != null ) {
                if (!string.IsNullOrEmpty(poster.Value))
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        series.Image = string.Format("{0}/banners/{1}", mirror, poster.Value);
                    });
                }
            }

            ParseFromDetailsDoc(doc, "Rating", value => series.Rating = float.Parse(value, CultureInfo.InvariantCulture.NumberFormat));
            ParseFromDetailsDoc(doc, "Airs_Time", value => series.AirsTime = value);
            ParseFromDetailsDoc(doc, "Airs_DayOfWeek", value => series.AirsDayOfWeek = daysOfWeek.IndexOf(value.Trim().ToLowerInvariant()));
            ParseFromDetailsDoc(doc, "Runtime", value => series.Runtime = int.Parse(value));

            var episodes = new ObservableCollection<TvDbSeriesEpisode>(
            from e in doc.Descendants("Episode")
            select new TvDbSeriesEpisode()
            {
                Name = e.Descendants("EpisodeName").Select(n => n.Value).FirstOrDefault(),
                SeriesNumber = e.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault(),
                EpisodeNumber = e.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault(),
                Description = e.Descendants("Overview").Select(n => n.Value).FirstOrDefault(),
                FirstAired = e.Descendants("FirstAired").Select(n =>
                {
                    DateTime date;
                    if (DateTime.TryParse(n.Value, out date))
                        return (DateTime?)date;

                    return null;
                }).FirstOrDefault(),
                Image = e.Descendants("filename").Select(n => string.Format("{0}/banners/{1}", mirror, n.Value)).FirstOrDefault()
            });

            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                series.Episodes = episodes;

                series.Updated = updated;
            });
        }

        private static void ParseFromDetailsDoc(XDocument doc, string nodeName, Action<string> processValue)
        {
            var node = doc.Descendants(nodeName).FirstOrDefault();
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        processValue(node.Value);
                    });
                }
            }
        }
    }
}
