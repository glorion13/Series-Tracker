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
using Nito.AsyncEx;

namespace SeriesTracker
{
    public class TvDb
    {
        private const string ApiKey = "D8E7E19874B4F438";
   
        private readonly ConnectivityService connectivityService;
        private readonly AsyncLock key = new AsyncLock();

        private string mirror = null;

        private bool initialized;        

        public TvDb(ConnectivityService connectivityService)
        {
            this.connectivityService = connectivityService;
        }

        private async Task<bool> EnsureInitialized()
        {
            using(await key.LockAsync())
            {
                if (!initialized)
                {
                    try
                    {
                        await DoInitialize();
                        initialized = true;
                    }
                    catch (System.Net.WebException we)
                    {
                        connectivityService.ReportHealth(false);
                        Console.Out.WriteLine("Error initializing: " + we.Message);
                    }
                }

                return initialized;
            }
        }

        private async Task DoInitialize()
        {
            var url = "http://www.thetvdb.com/api/" + ApiKey + "/mirrors.xml";
            var wc = new WebClient();
            var result = await wc.DownloadStringTaskAsync(new Uri(url));
            connectivityService.ReportHealth(true);

            mirror = await Task.Factory.StartNew(() => (from path in XDocument.Parse(result).Descendants("mirrorpath") select path.Value).First());
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

                var list = await Task.Factory.StartNew(() => 
                    from series in XDocument.Parse(s).Descendants("Series")
                    where string.Equals(series.Descendants("language").Select(n => n.Value).FirstOrDefault(), "en")
                    select new TvDbSeries()
                    {
                        Title = series.Descendants("SeriesName").Select(n => n.Value).FirstOrDefault(),
                        Id = series.Descendants("seriesid").Select(n => n.Value).FirstOrDefault(),
                        Banner = series.Descendants("banner").Select(n => string.Format("{0}/banners/{1}", mirror, n.Value)).FirstOrDefault(),
                        Overview = series.Descendants("Overview").Select(n => n.Value).FirstOrDefault(),
                        ImdbId = series.Descendants("IMDB_ID").Select(n => n.Value).FirstOrDefault()
                    });

                return list;
            }
            catch (System.Net.WebException we)
            {
                connectivityService.ReportHealth(false);
                Console.Out.WriteLine("Error initializing: " + we.Message);
                return new List<TvDbSeries>();
            }   
        }

        private static List<string> daysOfWeek = CultureInfo.InvariantCulture.DateTimeFormat.DayNames.Select(d => d.ToLowerInvariant()).ToList();

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

                var doc = await Task.Factory.StartNew(() => XDocument.Parse(s));
                var poster = await doc.ParseAsync(d => d.Descendants("poster").FirstOrDefault());
                if (poster != null ) {
                    if (!string.IsNullOrEmpty(poster.Value))
                    {
                        series.Image = string.Format("{0}/banners/{1}", mirror, poster.Value);
                    }
                }

                await doc.ParseDescendantNodeAsync("Rating", value => series.Rating = float.Parse(value, CultureInfo.InvariantCulture.NumberFormat));
                await doc.ParseDescendantNodeAsync("Airs_Time", value => series.AirsTime = value);
                await doc.ParseDescendantNodeAsync("Airs_DayOfWeek", value => series.AirsDayOfWeek = daysOfWeek.IndexOf(value.Trim().ToLowerInvariant()));
                await doc.ParseDescendantNodeAsync("Runtime", value => series.Runtime = int.Parse(value));

                var episodeUpdates = await Task.Factory.StartNew(() =>
                                        from newData in doc.Descendants("Episode")
                                        join episode in series.Episodes on TvDbSeriesEpisode.GetEpisodeId(
                                            newData.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault(),
                                            newData.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault())
                                            equals episode.Id into matches
                                        from destination in matches.DefaultIfEmpty(new TvDbSeriesEpisode())
                                        select new
                                        {
                                            Episode = destination, 
                                            Data = newData
                                        });

                var list = new List<TvDbSeriesEpisode>();

                foreach (var update in episodeUpdates)
                {

                    update.Episode.Name = await update.Data.ParseAsync(d => d.Descendants("EpisodeName").Select(n => n.Value).FirstOrDefault());
                    update.Episode.SeriesNumber = await update.Data.ParseAsync(d => d.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault());
                    update.Episode.EpisodeNumber = await update.Data.ParseAsync(d => d.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault());
                    update.Episode.Description = await update.Data.ParseAsync(d => d.Descendants("Overview").Select(n => n.Value).FirstOrDefault());
                    update.Episode.FirstAired = await update.Data.ParseAsync(d => d.Descendants("FirstAired").Select(n =>
                    {
                        DateTime date;
                        if (DateTime.TryParse(n.Value, out date))
                            return (DateTime?)date;

                        return null;
                    }).FirstOrDefault());
                    update.Episode.Image = await update.Data.ParseAsync(d => d.Descendants("filename").Select(n => string.Format("{0}/banners/{1}", mirror, n.Value)).FirstOrDefault());

                    list.Add(update.Episode);
                }
                series.Episodes = list.OrderByDescending(e => e.SeriesNumber).ThenByDescending(e => e.EpisodeNumber).ToList();
             
                series.Updated = updated;
            }
            catch (WebException we)
            {
                connectivityService.ReportHealth(false); 
                Console.Out.WriteLine("Error initializing: " + we.Message);
            }  
        }
    }

    public static class XDocumentExtensions
    {
        public static Task<TResult> ParseAsync<TResult>(this XContainer document, Func<XContainer, TResult> parse)
        {
            return Task.Factory.StartNew(() => parse(document));
        }

        public static async Task ParseDescendantNodeAsync(this XDocument doc, string nodeName, Action<string> processValue)
        {
            var node = await Task.Factory.StartNew(() => doc.Descendants(nodeName).FirstOrDefault());
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    processValue(node.Value);
                }
            }
        }
    }
}
