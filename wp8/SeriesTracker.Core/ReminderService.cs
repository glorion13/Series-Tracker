using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Scheduler;
using SeriesTracker.Agent;

namespace SeriesTracker.Core
{
    public class ReminderService
    {
        private readonly TvDbSeriesRepository repository;

        public ReminderService(TvDbSeriesRepository repository)
        {
            this.repository = repository;
        }

        public async Task CreateOrUpdateRemindersAsync()
        {
            RemoveAllReminders();
            var subscribedSeries = await repository.GetSubscribedAsync(false);

            foreach (var series in subscribedSeries)
            {
                if (!series.RemindersEnabled)
                    continue;

                var nextEpisode = series.Episodes.Where(e => e.FirstAired >= DateTime.Today).OrderBy(e => e.FirstAired).FirstOrDefault();
                if (nextEpisode == null || nextEpisode.FirstAired == null)
                    continue;

                var notificationTime = series.NotificationTime ?? DateTime.Today.AddHours(18);

                var notificationDate = nextEpisode.FirstAired.Value.Date + notificationTime.TimeOfDay;

                var reminder = new Reminder(series.Id)
                {
                    BeginTime = notificationDate,
                    Title = series.Title,
                    Content = string.Format("New {0} episode is up!", series.Title)
                };

                ScheduledActionService.Add(reminder);
            }
        }

        public void RemoveAllReminders()
        {
            var reminders = ScheduledActionService.GetActions<Reminder>();
            foreach (var reminder in reminders)
            {
                ScheduledActionService.Remove(reminder.Name);
            }
        }
    }
}
