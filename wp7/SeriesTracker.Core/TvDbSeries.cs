using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using GalaSoft.MvvmLight;

namespace SeriesTracker
{
    public class TvDbSeries : TvDbSeriesBase
    {
        public TvDbSeries () {}
        public TvDbSeries (TvDbSeriesBase baseRecorod) {
            this.Title = baseRecorod.Title;
            this.Id = baseRecorod.Id;
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
