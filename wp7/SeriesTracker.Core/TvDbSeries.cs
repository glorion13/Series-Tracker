using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace SeriesTracker
{
    public class TvDbSeries : ViewModelBase
    {
        [XmlElement]
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

        [XmlElement]
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

        [XmlElement]
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

        [XmlElement]
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

        [XmlElement]
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

        [XmlElement]
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
    }
}
