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
        private readonly AgentScheduler agentScheduler;

        public ReminderService(TvDbSeriesRepository repository, AgentScheduler agentScheduler)
        {
            this.repository = repository;
            this.agentScheduler = agentScheduler;
        }


        public bool NotificationsEnabled
        {
            get { return agentScheduler.IsAgentActive; }
        }

        public Task EnableNotifications()
        {
            var result = agentScheduler.ScheduleAgent();
            if (!result)
            {
                MessageBox.Show(
                    "There was a problem enabling notifications. Please ensure background agents are not disabled for Series Tracker in your phone settings, or that your device did not reach the maximum amount of agents available.");
            }
            return CreateOrUpdateRemindersAsync();
        }

        public void DisableNotifications()
        {
            agentScheduler.RemoveAgent();
            RemoveAllReminders();
        }

        public async Task CreateOrUpdateRemindersAsync()
        {
            RemoveAllReminders();
            if (!NotificationsEnabled)
                return;

            var subscribedSeries = await repository.GetSubscribedAsync(false);

            foreach (var series in subscribedSeries)
            {
                if (!series.RemindersEnabled)
                    continue;

                var nextEpisode = series.Episodes.Where(e => e.FirstAired > DateTime.Today).OrderBy(e => e.FirstAired).FirstOrDefault(e => e.FirstAired > DateTime.Today);
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
