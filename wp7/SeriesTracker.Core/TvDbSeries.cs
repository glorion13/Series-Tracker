using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Globalization;
using GalaSoft.MvvmLight.Threading;
using ReactiveUI;

namespace SeriesTracker
{
    public class TvDbSeries : ViewModelBase, IComparable<TvDbSeries>
    {
        private string id;
        public string Id
        {
            get
            {
                return id;
            }

            set
            {
                Set(() => Id, ref id, value);
            }
        }

        private string imdbId;
        public string ImdbId
        {
            get
            {
                return imdbId;
            }

            set
            {
                Set(() => ImdbId, ref imdbId, value);
            }
        }

        private string title;
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                Set(() => Title, ref title, value);
            }
        }

        private string image;
        public string Image
        {
            get
            {
                return image;
            }

            set
            {
                Set(() => Image, ref image, value);
            }
        }

        private string banner;
        public string Banner
        {
            get
            {
                return banner;
            }

            set
            {
                Set(() => Banner, ref banner, value);
            }
        }

        private string overview;
        public string Overview
        {
            get
            {
                return overview;
            }

            set
            {
                Set(() => Overview, ref overview, value);
            }
        }

        private float rating;
        public float Rating
        {
            get
            {
                return rating;
            }

            set
            {
                Set(() => Rating, ref rating, value);
            }
        }

        private bool isSubscribed;
        public bool IsSubscribed
        {
            get
            {
                return isSubscribed;
            }
            set
            {
                Set(() => IsSubscribed, ref isSubscribed, value);
            }
        }

        public int CompareTo(TvDbSeries other)
        {
            if (other == null) return 1;

            if (Title == null)
            {
                if (other.Title == null)
                    return 0;
                return -1;
            }

            if (other.Title == null)
                return 1;

            var seriesOrder = -1 * String.Compare(Title, other.Title, StringComparison.Ordinal);

            if (seriesOrder != 0)
                return seriesOrder;

            if (Title == null)
            {
                if (other.Title == null)
                    return 0;
                return -1;
            }

            if (other.Title == null)
                return 1;

            return -1 * String.Compare(Title, other.Title, StringComparison.Ordinal);
        }

        private readonly Dictionary<TvDbSeriesEpisode, IDisposable> isSeenListeners = new Dictionary<TvDbSeriesEpisode, IDisposable>();
        private ObservableCollection<TvDbSeriesEpisode> episodes;
        public ObservableCollection<TvDbSeriesEpisode> Episodes
        {
            get
            {
                return episodes;
            }

            set
            {
                if (episodes != null)
                    episodes.CollectionChanged -= OnEpisodesCollectionChanged;

                foreach (var isSeenListener in isSeenListeners)
                {
                    isSeenListener.Value.Dispose();
                }
                isSeenListeners.Clear();

                Set(() => Episodes, ref episodes, value);
                
                episodes.CollectionChanged += OnEpisodesCollectionChanged;
                foreach (var episode in episodes)
                {
                    isSeenListeners.Add(episode, episode.ObservableForProperty(e => e.IsSeen).Subscribe(e => DispatcherHelper.CheckBeginInvokeOnUI(() => RaisePropertyChanged(() => UnseenEpisodeCount))));
                }
                CalculateMetrics();
            }
        }

        private void OnEpisodesCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                foreach (TvDbSeriesEpisode added in args.NewItems)
                {
                    isSeenListeners.Add(added, added.ObservableForProperty(e => e.IsSeen).Subscribe(e => DispatcherHelper.CheckBeginInvokeOnUI(() => RaisePropertyChanged(() => UnseenEpisodeCount))));
                }
            }

            if (args.OldItems != null)
            {
                foreach (TvDbSeriesEpisode removed in args.OldItems)
                {
                    IDisposable subscription;
                    if (isSeenListeners.TryGetValue(removed, out subscription))
                    {
                        subscription.Dispose();
                        isSeenListeners.Remove(removed);
                    }
                }
            }


            CalculateMetrics();
        }

        private void CalculateMetrics()
        {
            switch (airsDayOfWeek)
            {
                case null:
                    Airs = string.Empty;
                    break;
                case -1:
                    Airs = "irregularly";
                    break;
                default:
                    Airs = CultureInfo.InvariantCulture.DateTimeFormat.DayNames[airsDayOfWeek.Value] + " " + AirsTime;
                    break;
            }

            if (Episodes == null)
                return;

            var nextEpisodeAirDateTime = Episodes.Where(e => e.FirstAired != null && e.FirstAired >= DateTime.Today).Select(e => e.FirstAired).OrderBy(x => x).FirstOrDefault();
            if (nextEpisodeAirDateTime == null)
            {
                NextEpisodeAirDateTime = null;
                NextEpisodeETA = null;
                NextEpisodeAirs = "N/A";
            }
            else
            {
                var nextAirDateTime = nextEpisodeAirDateTime.Value.Date;

                DateTime time;
                if (!string.IsNullOrEmpty(AirsTime) && DateTime.TryParse(AirsTime, out time))
                {
                    ///TODO: take daylight saving time into account
                    nextAirDateTime = nextAirDateTime.AddHours(5); // EST to UTC
                    nextAirDateTime = nextAirDateTime.Add(time.TimeOfDay);
                }

                var localOffset = TimeZoneInfo.Local.BaseUtcOffset;
                nextAirDateTime = nextAirDateTime.Add(localOffset);

                NextEpisodeAirDateTime = nextAirDateTime;
                NextEpisodeAirs = nextAirDateTime.ToShortDateString();

                var durationMinutes = Runtime ?? 30;

                var delta = nextAirDateTime - DateTime.Now;
                if (delta.TotalDays > 30)
                {
                    NextEpisodeETA = NextEpisodeAirs;
                }
                else
                {
                    NextEpisodeETA = delta.Days > 0 ? string.Format("{0}d {1}h", delta.Days, delta.Hours) :
                        delta.Hours > 0 ? string.Format("{0}h {1}m", delta.Hours, delta.Minutes) :
                        delta.Minutes > 2 ? string.Format("{0}m {1}s", delta.Minutes, delta.Seconds) :
                        delta.Ticks > 0 ? "due" :
                        delta.TotalMinutes >= durationMinutes ? "LIVE" :
                            "ended";
                }
            }

            RaisePropertyChanged(() => UnseenEpisodeCount);
        }

        public TvDbSeries()
        {
            Episodes = new ObservableCollection<TvDbSeriesEpisode>();
        }

        private DateTime? updated;
        public DateTime? Updated
        {
            get
            {
                return updated;
            }
            set
            {
                Set(() => Updated, ref updated, value);
            }
        }

        private string airsTime = string.Empty;
        public string AirsTime
        {
            get
            {
                return airsTime;
            }
            set
            {
                Set(() => AirsTime, ref airsTime, value);
                CalculateMetrics();
            }
        }

        private int? airsDayOfWeek = null;
        public int? AirsDayOfWeek
        {
            get
            {
                return airsDayOfWeek;
            }
            set
            {
                Set(() => AirsDayOfWeek, ref airsDayOfWeek, value);
                CalculateMetrics();
            }
        }

        private string airs;
        [XmlIgnore]
        public string Airs
        {
            get { return airs; }
            protected set { Set(() => Airs, ref airs, value); }
        }

        private int? runtime;
        public int? Runtime
        {
            get
            {
                return runtime;
            }
            set
            {
                Set(() => Runtime, ref runtime, value);
            }
        }

        private DateTime? nextEpisodeAirDateTime;
        [XmlIgnore]
        public DateTime? NextEpisodeAirDateTime
        {
            get { return nextEpisodeAirDateTime; }
            protected set { Set(() => NextEpisodeAirDateTime, ref nextEpisodeAirDateTime, value); }
        }

        [XmlIgnore]
        public string UnseenEpisodeCount
        {
            get
            {
                var episodeCount = Episodes.Count(e => !e.IsSeen && (e.FirstAired < DateTime.Today));
                return (episodeCount > 0) ? episodeCount.ToString() : "None";
            }
        }

        private string nextEpisodeEta;
        [XmlIgnore]
        public string NextEpisodeETA
        {
            get { return nextEpisodeEta; }
            protected set { Set(() => NextEpisodeETA, ref nextEpisodeEta, value); }
        }

        private string nextEpisodeAirs;
        [XmlIgnore]
        public string NextEpisodeAirs
        {
            get { return nextEpisodeAirs; }
            protected set { Set(() => NextEpisodeAirs, ref nextEpisodeAirs, value); }
        }
    }
}
