using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace SeriesTracker.ManualTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.DataContext = new TvDbSeries()
            {
                AirsDayOfWeek = 0,
                AirsTime = "9 PM",
                Episodes = new List<TvDbSeriesEpisode>()
                {
                     new TvDbSeriesEpisode() {
                        FirstAired = DateTime.Today.AddDays(1)
                     }
                }
            };
        }
    }
}