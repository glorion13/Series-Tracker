using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Phone.Scheduler;

namespace SeriesTracker.Core
{
    public static class ReminderService
    {
        public static void CreateOrUpdateReminders(IEnumerable<TvDbSeries> series)
        {
            var notificationDeltaValue = SharedSettings.Get(SharedSettings.NotificationDeltaKey);
            var notificationDelta = notificationDeltaValue == null
                ? TimeSpan.FromHours(-2)
                : (TimeSpan) notificationDeltaValue;

            foreach (var s in series)
            {
                /*
                var reminderExists = ScheduledActionService.Find(s.Id) != null;
 
                var nextEpisode = s.Episodes.OrderBy(e => e.FirstAired).FirstOrDefault(e => e.FirstAired > DateTime.Now);

                var nextAirDateTime = s.NextEpisodeAirDateTime;

                if (nextAirDateTime == null)
                {
                    if (reminderExists)
                        ScheduledActionService.Remove(s.Id);

                    continue;
                }

                

                var reminder = new Reminder(s.Id)
                {
                    BeginTime = nextAirDateTime.Value + notificationDelta,
                    Title = s.Title,
                    Content = string.Format("A new episode {0} will be aired at {1}.", );
                }*/
            }
        }

        public static void RemoveAllReminders()
        {
            var reminders = ScheduledActionService.GetActions<Reminder>();
            foreach (var reminder in reminders)
            {
                ScheduledActionService.Remove(reminder.Name);
            }
        }
    }
}
