using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;
using GalaSoft.MvvmLight;

namespace SeriesTracker
{
    public class SeriesRecord : ViewModelBase
    {
        private TvDbSeries series;

        public SeriesRecord(TvDbSeries series)
        {
            this.series = series;
        }

        public TvDbSeries Series
        {
            get
            {
                return series;
            }
        }
    }
}
