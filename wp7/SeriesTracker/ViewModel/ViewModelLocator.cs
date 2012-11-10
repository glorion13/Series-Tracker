/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:SeriesTracker"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using ImageTools.IO.Gif;

namespace SeriesTracker
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator : ViewModelBase
    {
        public ViewModelLocator()
        {
            DispatcherHelper.Initialize();
            ImageTools.IO.Decoders.AddDecoder<GifDecoder>();
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (!IsInDesignMode)
            {
                SimpleIoc.Default.Register<ViewModelLocator>(() => this);
            }

            SimpleIoc.Default.Register<TvDbSeriesRepository>();
            SimpleIoc.Default.Register<SubscriptionManager>(true);
            
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SeriesDetailsViewModel>(true);

            
            SimpleIoc.Default.Register<SplashViewModel>(true);
        }

        public MainViewModel MainViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public SeriesDetailsViewModel SeriesDetails
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SeriesDetailsViewModel>();
            }
        }

        public SplashViewModel SplashScreen
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SplashViewModel>();
            }
        }

        
    }
}