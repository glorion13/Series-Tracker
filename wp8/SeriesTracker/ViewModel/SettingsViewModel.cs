using System.Windows;
using GalaSoft.MvvmLight;
using SeriesTracker.Agent;
using SeriesTracker.Core;
using GalaSoft.MvvmLight.Messaging;

namespace SeriesTracker
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AgentScheduler agentScheduler;

        public SettingsViewModel(AgentScheduler agentScheduler)
        {
            this.agentScheduler = agentScheduler;
        }

        public bool BackgroundAgentEnabled
        {
            get
            {
                return AppSettings.Instance.BackgroundAgentEnabled && agentScheduler.IsAgentActive;
            }
            set
            {
                AppSettings.Instance.BackgroundAgentEnabled = value;
                if (value)
                {
                    if (!agentScheduler.ScheduleAgent())
                    {
                        MessageBox.Show(
                            "There was a problem enabling notifications. Please ensure background agents are not disabled for Series Tracker in your phone settings, or that your device did not reach the maximum amount of agents available.");
                    }
                }
                else
                {
                    agentScheduler.RemoveAgent();
                }
                RaisePropertyChanged(() => BackgroundAgentEnabled);
            }
        }

        public bool AlphabeticalSortingEnabled
        {
            get
            {
                return AppSettings.Instance.AlphabeticalSortingEnabled;
            }
            set
            {
                AppSettings.Instance.AlphabeticalSortingEnabled = value;
                RaisePropertyChanged(() => AlphabeticalSortingEnabled);
            }
        }

    }
}
