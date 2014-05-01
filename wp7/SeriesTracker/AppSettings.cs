using System.IO.IsolatedStorage;
using GalaSoft.MvvmLight.Messaging;

namespace SeriesTracker
{
    public class AppSettings
    {
        private const string BackgroundAgentEnabledKey = "BackgroundAgentEnabled";
        private const string AlphabeticalSortingEnabledKey = "AlphabeticalSortingEnabled";

        private static AppSettings instance;
        private readonly IsolatedStorageSettings settings;

        public static AppSettings Instance
        {
            get { return instance ?? (instance = new AppSettings()); }
        }

        public AppSettings()
        {
            settings = IsolatedStorageSettings.ApplicationSettings;
        }

        public bool BackgroundAgentEnabled
        {
            get
            {
                bool enabled;
                return settings.TryGetValue(BackgroundAgentEnabledKey, out enabled) && enabled;
            }
            set
            {
                settings[BackgroundAgentEnabledKey] = value;
                Messenger.Default.Send(this);
            }
        }

        public bool AlphabeticalSortingEnabled
        {
            get
            {
                bool enabled;
                return settings.TryGetValue(AlphabeticalSortingEnabledKey, out enabled) && enabled;
            }
            set
            {
                settings[AlphabeticalSortingEnabledKey] = value;
                Messenger.Default.Send(this);
            }
        }
    }
}