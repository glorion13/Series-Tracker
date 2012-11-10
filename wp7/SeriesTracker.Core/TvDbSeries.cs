using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Globalization;

namespace SeriesTracker
{
    public class TvDbSeries : ViewModelBase
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

        private string thumbnail;
        public string Thumbnail
        {
            get
            {
                return thumbnail;
            }

            set
            {
                Set(() => Thumbnail, ref thumbnail, value);
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

        private ObservableCollection<TvDbSeriesEpisode> episodes;
        public ObservableCollection<TvDbSeriesEpisode> Episodes
        {
            get
            {
                return episodes;
            }

            set
            {
                Set(() => Episodes, ref episodes, value);
            }
        }

        public TvDbSeries() {
            episodes = new ObservableCollection<TvDbSeriesEpisode>();
        }

        private DateTime? updated = null;
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
                RaisePropertyChanged(() => Airs);
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
                RaisePropertyChanged(() => Airs);
            }
        }


        public string Airs
        {
            get
            {
                switch (airsDayOfWeek)
                {
                    case null:
                        return string.Empty;
                    case -1:
                        return "irregularly";
                    default:
                        return CultureInfo.InvariantCulture.DateTimeFormat.DayNames[airsDayOfWeek.Value] + " " + AirsTime;
                }
            }
        }

        public DateTime? GetNextEpisodeAirTime()
        {
            return episodes.Where(e => e.FirstAired != null && e.FirstAired >= DateTime.Today).Select(e => e.FirstAired.Value).OrderBy(x => x).FirstOrDefault();
        }

        public string NextEpisodeAirs
        {
            get
            {
                var date = GetNextEpisodeAirTime();
                if (date == null) {
                    return "N/A";
                }
                return  date.Value.ToShortDateString() + " " + AirsTime;
            }
        }
    }
}
