using System;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Scheduler;
using System.ComponentModel;

namespace SeriesTracker
{
    public class AgentScheduler
    {
        public const string NotificationsEnabledKey = "notificationsEnabled";
        private const string PeriodicTaskName = "PeriodicAgent";

        public AgentScheduler()
        {
#if DEBUG
            if (!DesignerProperties.IsInDesignTool)
            {
#endif
                bool agentEnabled;
                if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(NotificationsEnabledKey, out agentEnabled))
                {
                    if (agentEnabled)
                    {
                        ScheduleAgent(false);
                    }
                }
#if DEBUG
            }
#endif
        }

        public bool IsAgentActive
        {
            get { return ScheduledActionService.Find(PeriodicTaskName) != null; }
        }

        public bool ScheduleAgent(bool permanently = true)
        {
            // If the task already exists and background agents are enabled for the
            // application, you must remove the task and then add it again to update 
            // the schedule
            if (IsAgentActive)
            {
                RemoveAgent(false);
            }

            var periodicTask = new PeriodicTask(PeriodicTaskName)
            {
                Description = "Updates subscribed series data and enables live tile updates and notifications."
            };

            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);

                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG_AGENT)
                ScheduledActionService.LaunchForTest(periodicTask.Name, TimeSpan.FromSeconds(60));
#endif
                if (permanently)
                {
                    IsolatedStorageSettings.ApplicationSettings[NotificationsEnabledKey] = true;
                }
                
                return true;
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {

                }

                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
                }
            }
            catch (SchedulerServiceException)
            {

            }

            return false;
        }
        public void RemoveAgent(bool permanently = true)
        {
            try
            {
                if (IsAgentActive)
                {
                    ScheduledActionService.Remove(PeriodicTaskName);
                    if (permanently)
                    {
                        IsolatedStorageSettings.ApplicationSettings[NotificationsEnabledKey] = false;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
