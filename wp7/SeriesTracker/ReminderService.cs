using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Phone.Scheduler;
using SeriesTracker.Core;

namespace SeriesTracker
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

                var nextEpisodes = series.Episodes.Where(e => e.FirstAired >= DateTime.Today).OrderBy(e => e.FirstAired).Take(2).ToList();


                if (nextEpisodes.Count == 0)
                    continue;

                var notificationTime = series.NotificationTime ?? DateTime.Today.AddHours(18);

                foreach (var episode in nextEpisodes)
                {
                    if (episode.FirstAired == null)
                        continue;

                    var notificationDate = episode.FirstAired.Value.Date + notificationTime.TimeOfDay;

                    var reminder = new Reminder(series.Id + episode.Id)
                    {
                        BeginTime = notificationDate,
                        Title = series.Title,
                        Content = string.Format("New {0} episode is up!", series.Title)
                    };

                    ScheduledActionService.Add(reminder);
                }
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
