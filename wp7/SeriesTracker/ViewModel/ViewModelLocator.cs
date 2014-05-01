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
using SeriesTracker.Agent;
using SeriesTracker.Core;

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
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
  
            SimpleIoc.Default.Register<ConnectivityService>();
            SimpleIoc.Default.Register<TvDb>();
            SimpleIoc.Default.Register<AgentScheduler>(true);
            SimpleIoc.Default.Register<ReminderService>();

            SimpleIoc.Default.Register<TvDbSeriesRepository>();
            SimpleIoc.Default.Register<SeriesStorageManager>(true);
            
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SeriesDetailsViewModel>(true);
            SimpleIoc.Default.Register<NotificationViewModel>(true);
            SimpleIoc.Default.Register<AboutViewModel>(true);

            SimpleIoc.Default.Register<SplashViewModel>(true);

            SimpleIoc.Default.Register<SettingsViewModel>(true);
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

        public AboutViewModel About
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AboutViewModel>();
            }
        }

        public SplashViewModel SplashScreen
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SplashViewModel>();
            }
        }

        public SettingsViewModel Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingsViewModel>();
            }
        }

        public NotificationViewModel Notification
        {
            get
            {
                return ServiceLocator.Current.GetInstance<NotificationViewModel>();
            }
        }
        
    }
}