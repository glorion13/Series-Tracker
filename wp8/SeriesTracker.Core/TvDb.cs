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
using SeriesTracker.Core;

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
            using (await @lock.DisposableWaitAsync())
            {
                if (!initialized)
                {
                    try
                    {
                        await DoInitialize();
                        initialized = true;
                    }
                    catch (WebException)
                    {
                        connectivityService.ReportHealth(false);
                    }
                }
                
                return initialized;
            }
        }

        private async Task DoInitialize()
        {
            var result = await new WebClient().DownloadStringTaskAsync(new Uri(ApiUrl));
            connectivityService.ReportHealth(true);

            mirror = await Task.Factory.StartNew(() => (from path in XDocument.Parse(result).Descendants("mirrorpath")
                           select path.Value).First());
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
            catch (WebException)
            {
                connectivityService.ReportHealth(false);
                return new List<TvDbSeries>();
            }   
        }

        private static readonly List<string> DaysOfWeek = CultureInfo.InvariantCulture.DateTimeFormat.DayNames.Select(d => d.ToLowerInvariant()).ToList();

        public async Task UpdateData(TvDbSeries series)
        {
            if (!await EnsureInitialized())
                return;

            try
            {
                var url = string.Format("{0}/api/{1}/series/{2}/all/en.xml", mirror, ApiKey, series.Id);
                var wc = new WebClient();
                string response = await wc.DownloadStringTaskAsync(url);
                connectivityService.ReportHealth(true);
                try
                {
                    var updates = await Task.Factory.StartNew(() => GetSeriesUpdates(response).ToList());
                    foreach (var update in updates)
                    {
                        if (update != null)
                            update(series);
                    }
                }
                catch (XmlException)
                {

                }
            }
            catch (WebException we)
            {
                connectivityService.ReportHealth(false);
                Console.Out.WriteLine("Error initializing: " + we.Message);
            }
        }

        private IEnumerable<Action<TvDbSeries>> GetSeriesUpdates(string response)
        {
            var doc = XDocument.Parse(response);
            var poster = doc.Descendants("poster").FirstOrDefault();
            if (poster != null)
            {
                if (!string.IsNullOrEmpty(poster.Value))
                {
                    var image = string.Format("{0}/banners/{1}", mirror, poster.Value);
                    yield return s => s.Image = image;
                }
            }

            yield return
                doc.ParseAndGetUpdateAction("Rating",
                    value => float.Parse(value, CultureInfo.InvariantCulture.NumberFormat), (s, r) => s.Rating = r);

            yield return doc.ParseAndGetUpdateAction("Airs_Time", (s, v) => s.AirsTime = v);

            yield return
                doc.ParseAndGetUpdateAction("Airs_DayOfWeek",
                    value => DaysOfWeek.IndexOf(value.Trim().ToLowerInvariant()), (s, v) => s.AirsDayOfWeek = v);

            yield return
                doc.ParseAndGetUpdateAction("Runtime", int.Parse, (s, i) => s.Runtime = i);

            var episodeUpdates = from newData in doc.Descendants("Episode")
                                 select new
                                 {
                                     Id = TvDbSeriesEpisode.GetEpisodeId(
                                         newData.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault(),
                                         newData.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault()),
                                     Data = newData
                                 };

            var newEpisodes = (from update in episodeUpdates
                               let name = update.Data.Descendants("EpisodeName").Select(e => e.Value).FirstOrDefault()
                               let seasonNumber = update.Data.Descendants("SeasonNumber").Select(n => n.Value).FirstOrDefault()
                               let episodeNumber = update.Data.Descendants("EpisodeNumber").Select(n => n.Value).FirstOrDefault()
                               let overview = update.Data.Descendants("Overview").Select(n => n.Value).FirstOrDefault()
                               let firstAired = update.Data.Descendants("FirstAired").Select(n =>
                               {
                                   DateTime date;
                                   if (DateTime.TryParse(n.Value, out date))
                                       return (DateTime?)date;

                                   return null;
                               }).FirstOrDefault()
                               let image = update.Data.Descendants("filename").Select(n => string.Format("{0}/banners/{1}", mirror, n.Value)).FirstOrDefault()
                               select (Func<IList<TvDbSeriesEpisode>, TvDbSeriesEpisode>)(episodes =>
                               {
                                   var episode = episodes.FirstOrDefault(e => e.Id == update.Id);
                                   if (episode == null)
                                   {
                                       episode = new TvDbSeriesEpisode();
                                   }
                                   else
                                   {
                                       episodes.Remove(episode);
                                   }

                                   episode.Name = name;
                                   episode.SeriesNumber = seasonNumber;
                                   episode.EpisodeNumber = episodeNumber;
                                   episode.Description = overview;
                                   episode.FirstAired = firstAired;
                                   episode.Image = image;

                                   return episode;
                               })).ToList();

            yield return s =>
            {
                var episodes = new SelfSortingObservableCollection<TvDbSeriesEpisode, string>(e => e.Id);
                var current = s.Episodes.ToList();
                foreach (var episode in newEpisodes.Select(e => e(current)))
                {
                    episodes.Add(episode);
                }
                s.Episodes = episodes;
            };

            yield return s => s.Updated = DateTime.Now;
        }
    }

    public static class XDocumentExtensions
    {
        public static T ParseDescendantNode<T>(this XDocument doc, string nodeName, Func<string, T> processValue)
        {
            var node = doc.Descendants(nodeName).FirstOrDefault();
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    return processValue(node.Value);
                }
            }

            return default(T);
        }

        public static Action<TvDbSeries> ParseAndGetUpdateAction<T>(this XDocument doc, string nodeName, Func<string, T> processValue, Action<TvDbSeries, T> apply)
        {
            var node = doc.Descendants(nodeName).FirstOrDefault();
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    var value = processValue(node.Value);
                    return s => apply(s, value);
                }
            }

            return null;
        }

        public static Action<TvDbSeries> ParseAndGetUpdateAction(this XDocument doc, string nodeName, Action<TvDbSeries, string> apply)
        {
            var node = doc.Descendants(nodeName).FirstOrDefault();
            if (node != null)
            {
                if (!string.IsNullOrEmpty(node.Value))
                {
                    var value = node.Value;
                    return s => apply(s, value);
                }
            }

            return null;
        }
    }
}
