using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using GalaSoft.MvvmLight;

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
    }
}
