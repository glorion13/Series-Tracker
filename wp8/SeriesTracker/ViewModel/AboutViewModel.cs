using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Controls;
using SeriesTracker.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace SeriesTracker
{
    public class AboutViewModel : ViewModelBase
    {
        public AboutViewModel()
        {
        }

        private ICommand openEmailLink;
        public ICommand OpenEmailLink
        {
            get
            {
                return openEmailLink ?? (openEmailLink = new RelayCommand(() =>
                {
                    var task = new Microsoft.Phone.Tasks.EmailComposeTask();
                    task.To = "seriestracker@outlook.com";
                    task.Show();
                }));
            }
        }
        private ICommand openFacebookLink;
        public ICommand OpenFacebookLink
        {
            get
            {
                return openFacebookLink ?? (openFacebookLink = new RelayCommand(() =>
                {
                    var task = new Microsoft.Phone.Tasks.WebBrowserTask();
                    task.Uri = new Uri("http://www.facebook.com/SeriesTracker");
                    task.Show();
                }));
            }
        }
    }

}
