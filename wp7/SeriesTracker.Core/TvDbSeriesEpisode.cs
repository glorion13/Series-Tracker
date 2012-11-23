using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeriesTracker
{
    public class TvDbSeriesEpisode : ViewModelBase, IComparable<TvDbSeriesEpisode>
    {
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

            if (this.SeriesNumber == null)
                return -1;

            return -1 * this.SeriesNumber.CompareTo(other.SeriesNumber);
        }
    }
}
