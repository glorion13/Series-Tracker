using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SeriesTracker
{
    public class TvDbSeriesEpisode : ViewModelBase, IComparable<TvDbSeriesEpisode>
    {
        public string Id
        {
            get
            {
                return GetEpisodeId(seriesNumber, episodeNumber);
            }
        }

        public static string GetEpisodeId(string seriesNumber, string episodeNumber)
        {
            return string.Format("{0}-{1}", seriesNumber, episodeNumber);
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                Set(() => Name, ref name, value);
            }
        }

        private string seriesNumber;
        public string SeriesNumber
        {
            get
            {
                return seriesNumber;
            }
            set
            {
                Set(() => SeriesNumber, ref seriesNumber, value);
                RaisePropertyChanged(() => Id);
            }
        }

        private string episodeNumber;
        public string EpisodeNumber
        {
            get
            {
                return episodeNumber;
            }
            set
            {
                Set(() => EpisodeNumber, ref episodeNumber, value);
                RaisePropertyChanged(() => Id);
            }
        }

        private string description;
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                Set(() => Description, ref description, value);
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

        private bool isSeen;
        public bool IsSeen
        {
            get
            {
                return isSeen;
            }
            set
            {
                Set(() => IsSeen, ref isSeen, value);
            }
        }

        private DateTime? firstAired = null;
        public DateTime? FirstAired
        {
            get
            {
                return firstAired;
            }
            set
            {
                Set(() => FirstAired, ref firstAired, value);
            }
        }

        public int CompareTo(TvDbSeriesEpisode other)
        {
            if (other == null) return 1;

            if (SeriesNumber == null)
            {
                if (other.SeriesNumber == null)
                    return 0;
                return -1;
            }

            if (other.SeriesNumber == null)
                return 1;
                

            var seriesOrder = -1*String.Compare(SeriesNumber, other.SeriesNumber, StringComparison.Ordinal);

            if (seriesOrder != 0)
                return seriesOrder;

            if (EpisodeNumber == null)
            {
                if (other.EpisodeNumber == null)
                    return 0;
                return -1;
            }

            if (other.EpisodeNumber == null)
                return 1;

            return -1*String.Compare(EpisodeNumber, other.EpisodeNumber, StringComparison.Ordinal);
        }
    }
}
