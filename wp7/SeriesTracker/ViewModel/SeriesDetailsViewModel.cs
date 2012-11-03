using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeriesTracker.ViewModel
{
    public class SeriesDetailsViewModel : ViewModelBase
    {
        private TvDbSeries series = null;
        public TvDbSeries Series
        {
            get
            {
                return series;
            }
            set
            {
                Set(() => Series, ref series, value);
            }
        }

        public SeriesDetailsViewModel()
        {
            MessengerInstance.Register<TvDbSeries>(this, s => Series = s);
        }
    }
}
