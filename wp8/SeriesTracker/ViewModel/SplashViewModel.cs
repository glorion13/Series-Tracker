using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeriesTracker
{
    public class SplashViewModel : ViewModelBase
    {
        private readonly MainViewModel main;
        public SplashViewModel(MainViewModel main)
        {
            this.main = main;

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
                    if (!IsInDesignMode)
                    {
                        //kicks of initialization and waits for max 4s, then hides the spalsh screen
                        await Task.WhenAny(main.Initialize(), Task.Delay(4000));
                        IsLoaded = true;
                    }
                }));
            }
        }
    }
}
