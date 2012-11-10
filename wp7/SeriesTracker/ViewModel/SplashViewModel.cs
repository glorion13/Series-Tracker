using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeriesTracker
{
    public class SplashViewModel : ViewModelBase
    {
        private ViewModelLocator locator;
        public SplashViewModel(ViewModelLocator locator)
        {
            this.locator = locator;

            TvContent = new Uri("tv_white_noise.gif", UriKind.Relative);
        }

        private Uri tvContent;
        public Uri TvContent
        {
            get
            {
                return tvContent;
            }
            set
            {
                Set(() => TvContent, ref tvContent, value);
            }
        }

        private bool isLoaded = false;
        public bool IsLoaded
        {
            get
            {
                return isLoaded;
            }
            set
            {
                Set(() => IsLoaded, ref isLoaded, value);
            }
        }

        private RelayCommand initialize;
        public RelayCommand Initialize
        {
            get
            {
                return initialize ?? (initialize = new RelayCommand(async () =>
                  {
                      await locator.MainViewModel.Initialize();
                      IsLoaded = true;
                  }));
            }
        }
    }
}
