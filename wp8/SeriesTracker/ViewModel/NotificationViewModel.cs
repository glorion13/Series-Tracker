using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SeriesTracker.Core;

namespace SeriesTracker
{
    public class NotificationViewModel : ViewModelBase
    {
        private TvDbSeriesRepository repository;
        private readonly ReminderService reminderService;

        public NotificationViewModel(TvDbSeriesRepository repository, ReminderService reminderService)
        {
            this.repository = repository;
            this.reminderService = reminderService;

            if (!IsInDesignMode)
            {
                MessengerInstance.Register<TvDbSeries>(this, s => Series = s);
            }

            applyCommand = new RelayCommand(async () =>
            {
                series.NotificationTime = NotificationTime;
                series.RemindersEnabled = remindersEnabled;
                await repository.SaveAsync(series);
                await reminderService.CreateOrUpdateRemindersAsync();
                //MessengerInstance.Send(Series);
                //MessengerInstance.Send(new Uri("/SeriesDetails.xaml", UriKind.Relative));
            });
        }

        private TvDbSeries series;

        public TvDbSeries Series
        {
            get { return series; }
            set
            {
                Set(() => Series, ref series, value);
                RemindersEnabled = series.RemindersEnabled;
                NotificationTime = series.NotificationTime ?? series.NextEpisodeAirDateTime.GetValueOrDefault(DateTime.Today.AddHours(18));
            }
        }

        private bool remindersEnabled;
        public bool RemindersEnabled
        {
            get { return remindersEnabled; }
            set { Set(() => RemindersEnabled, ref remindersEnabled, value); }
        }

        private DateTime notificationTime;
        public DateTime NotificationTime
        {
            get { return notificationTime; }
            set { Set(() => NotificationTime, ref notificationTime, value); }
        }

        private readonly ICommand applyCommand;
        public ICommand Apply
        {
            get { return applyCommand; }
        }
    }
}
