using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace SeriesTracker
{
    public partial class SplashPage : PhoneApplicationPage
    {
        public SplashPage()
        {
            InitializeComponent();
        }

        private void Storyboard_Completed_1(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            this.NavigationService.Navigated += NavigationService_Navigated;
        }

        void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            this.NavigationService.Navigated -= NavigationService_Navigated;
            this.NavigationService.RemoveBackEntry();
        }
    }
}