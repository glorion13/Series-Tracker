using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Phone.Reactive;

namespace SeriesTracker
{
    public class TvDb
    {
        private const string ApiKey = "D8E7E19874B4F438";
        private const string ApiUrl = "http://www.thetvdb.com/api/" + ApiKey + "/mirrors.xml";
   
        private readonly ConnectivityService connectivityService;
        private readonly SemaphoreSlim @lock = new SemaphoreSlim(1);

        private string mirror;

        private bool initialized;        

        public TvDb(ConnectivityService connectivityService)
        {
            this.connectivityService = connectivityService;
        }

        private async Task<bool> EnsureInitialized()
        {
            if (initialized)
                return true;

            await @lock.WaitAsync();
            try
            {
                if (initialized) 
                    return true;

                try
                {
                    await DoInitialize();
                    initialized = true;
                }
                catch (WebException we)
                {
                    connectivityService.ReportHealth(false);
                    Console.Out.WriteLine("Error initializing: " + we.Message);
                }

                return initialized;
            }
            finally
            {
                @lock.Release();
            }
        }

        private async Task DoInitialize()
        {
            var result = await new WebClient().DownloadStringTaskAsync(new Uri(ApiUrl));
            connectivityService.ReportHealth(true);

            mirror = (from path in XDocument.Parse(result).Descendants("mirrorpath")
                           select path.Value).First();
        }

        public async Task<IEnumerable<TvDbSeries>> FindSeries(string name)
        {
            if (!await EnsureInitialized())
                return new List<TvDbSeries>();

            try
            {
                var url = mirror + "/api/GetSeries.php?seriesname=" + name + "&language=en";
                var wc = new WebClient();
                string s = await wc.DownloadStringTaskAsync(url);
                connectivityService.ReportHealth(true);

                var list = from series in XDocument.Parse(s).Descendants("Series")
                           where string.Equals(series.Descendants("language").Select(n => n.Value).FirstOrDefault(), "en")
                           select new TvDbSeries()
                           {
                               Title = series.Descendants("SeriesName").Select(n => n.Value).FirstOrDefault(),
                               Id = series.Descendants("seriesid").Select(n => n.Value).FirstOrDefault(),
                               Banner = series.Descendants("banner").Select(n => string.Format("{0}/banners/{1}", mirror, n.Value)).FirstOrDefault(),
                               Overview = series.Descendants("Overview").Select(n => n.Value).FirstOrDefault(),
                               ImdbId = series.Descendants("IMDB_ID").Select(n => n.Value).FirstOrDefault()
                           };

                return list;
            }
            catch (WebException we)
            {
                connectivityService.ReportHealth(false);
                Console.Out.WriteLine("Error initializing: " + we.Message);
                return new List<TvDbSeries>();
            }   
        }

        private static readonly List<string> DaysOfWeek = CultureInfo.InvariantCulture.DateTimeFormat.DayNames.Select(d => d.ToLowerInvariant()).ToList();

        public async Task UpdateData(TvDbSeries series)
        {
            if (!await EnsureInitialized())
                return;
            
            try {
                var url = string.Format("{0}/api/{1}/series/{2}/all/en.xml", mirror, ApiKey, series.Id);
                var wc = new WebClient();
                var updated = DateTime.Now;

                string s = await wc.DownloadStringTaskAsync(url);
                connectivityService.ReportHealth(true);

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
                ParseFromDetailsDoc(doc, "Airs_DayOfWeek", value => series.AirsDayOfWeek = DaysOfWeek.IndexOf(value.Trim().ToLowerInvariant()));
                ParseFromDetailsDoc(doc, "Runtime", value => series.Runtime = int.Parse(value));

                var episodeUpdates = from newData in doc.Descendants("Episode")
                                     join episode in series.Episodes on TvDbSeriesEpisode.GetEpisodeId(
                                         newData.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault(),
                                         newData.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault())
                                         equals episode.Id into matches
                                     from update in matches.DefaultIfEmpty(new TvDbSeriesEpisode())
                                     select new { Episode = update, Data = newData };


                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        var list = new List<TvDbSeriesEpisode>();

                        foreach (var update in episodeUpdates)
                        {

                            update.Episode.Name = update.Data.Descendants("EpisodeName").Select(n => n.Value).FirstOrDefault();
                            update.Episode.SeriesNumber = update.Data.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault();
                            update.Episode.EpisodeNumber = update.Data.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault();
                            update.Episode.Description = update.Data.Descendants("Overview").Select(n => n.Value).FirstOrDefault();
                            update.Episode.FirstAired = update.Data.Descendants("FirstAired").Select(n =>
                            {
                                DateTime date;
                                if (DateTime.TryParse(n.Value, out date))
                                    return (DateTime?)date;

                                return null;
                            }).FirstOrDefault();
                            update.Episode.Image = update.Data.Descendants("filename").Select(n => string.Format("{0}/banners/{1}", mirror, n.Value)).FirstOrDefault();

                            list.Add(update.Episode);
                        }
                        series.Episodes = list.OrderByDescending(e => e.SeriesNumber).ThenByDescending(e => e.EpisodeNumber).ToList();
                    });               


                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    series.Updated = updated;
                });
            }
            catch (WebException we)
            {
                connectivityService.ReportHealth(false);
                Console.Out.WriteLine("Error initializing: " + we.Message);
            }  
        }

        private static void ParseFromDetailsDoc(XDocument doc, string nodeName, Action<string> processValue)
        {
            var node = doc.Descendants(nodeName).FirstOrDefault();
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => processValue(node.Value));
                }
            }
        }
    }
}
