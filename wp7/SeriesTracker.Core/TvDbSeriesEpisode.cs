using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeriesTracker
{
    public class TvDbSeriesEpisode : ViewModelBase
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
    }
}
