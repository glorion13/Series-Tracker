﻿using System;
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

        public DateTime? NextEpisodeAirDateTime
        {
            get
            {
                var date = episodes.Where(e => e.FirstAired != null && e.FirstAired >= DateTime.Today).Select(e => e.FirstAired).OrderBy(x => x).FirstOrDefault();
                if (date == null)
                    return null;
                
                var offset = TimeZoneInfo.Local.BaseUtcOffset;
                decimal airsTime;
                var parsed = decimal.TryParse(new string(AirsTime.Where(ch => char.IsDigit(ch)).ToArray()), NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out airsTime);
                if (!parsed)
                {
                    airsTime = 0;
                }

                date = date.Value.Date.Add(offset).AddMinutes((int)(airsTime * 60));

                return date;
            }
        }

        public string NextEpisodeETA
        {
            get
            {
                DateTime? date = NextEpisodeAirDateTime as DateTime?;
                if (date == null)
                    return null;

                var durationMinutes = Runtime ?? 30;

                var delta = date.Value - DateTime.Now;
                if (delta.TotalDays > 30)
                    return date.Value.ToShortDateString();

                return delta.Days > 0 ? string.Format("{0}d {1}h", delta.Days, delta.Hours) :
                    delta.Hours > 0 ? string.Format("{0}h {1}m", delta.Hours, delta.Minutes) :
                    delta.Minutes > 2 ? string.Format("{0}m {1}s", delta.Minutes, delta.Seconds) :
                    delta.Ticks > 0 ? "due" :
                    delta.TotalMinutes >= durationMinutes ? "LIVE" :
                        "ended";
            }
        }

        public string NextEpisodeAirs
        {
            get
            {
                var date = NextEpisodeAirDateTime;
                if (date == null) {
                    return "N/A";
                }

                string airs = AirsTime.Trim();
                if (!string.IsNullOrEmpty(airs))
                {
                    airs += " EST";
                }

                return date.Value.ToShortDateString() + " " + airs;
            }
        }
    }
}
