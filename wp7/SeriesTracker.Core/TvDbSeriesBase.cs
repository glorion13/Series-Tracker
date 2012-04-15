using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight;

namespace SeriesTracker
{
    public class TvDbSeriesBase : ViewModelBase
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
    }
}
