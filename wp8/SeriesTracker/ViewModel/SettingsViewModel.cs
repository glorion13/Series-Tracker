using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using SeriesTracker.Agent;
using Microsoft.Phone.Controls.Primitives;
using SeriesTracker.Core;

namespace SeriesTracker
{
    public class SettingsViewModel : ViewModelBase
    {
        //public class IntDataSource : ILoopingSelectorDataSource
        //{
        //    private readonly int min;
        //    private readonly int max;

        //    public IntDataSource(int min, int max)
        //    {
        //        this.min = min;
        //        this.max = max;
        //    }

        //    private int selectedInt;

        //    public object GetNext(object relativeTo)
        //    {
        //        var oldInt = (int)relativeTo;
        //        if (oldInt >= max)
        //            return null;
        //        return (int) relativeTo + 1;
        //    }

        //    public object GetPrevious(object relativeTo)
        //    {
        //        var oldInt = (int)relativeTo;
        //        if (oldInt <= min)
        //            return null;
        //        return (int)relativeTo - 1;
        //    }

        //    public object SelectedItem
        //    {
        //        get { return selectedInt; }
        //        set
        //        {
        //            int newValue = (int) value;
        //            if (newValue != selectedInt)
        //            {
        //                object previousSelectedItem = selectedInt;
        //                selectedInt = newValue;
        //                var handler = SelectionChanged;
        //                if (null != handler)
        //                {
        //                    handler(this, new SelectionChangedEventArgs(new object[] { previousSelectedItem }, new object[] { selectedInt }));
        //                }
        //            }
        //        }
        //    }

        //    public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        //}

        private readonly ReminderService reminderService;

        public SettingsViewModel(ReminderService reminderService)
        {
            this.reminderService = reminderService;
            /*var savedNotificationDelta = SharedSettings.Get(SharedSettings.NotificationDeltaKey);
            this.NotificationDelta = savedNotificationDelta == null ? TimeSpan.FromHours(-2) : (TimeSpan)savedNotificationDelta;*/

            //hoursDataSource = new IntDataSource(0, 24);
            //hoursDataSource.SelectionChanged += (sender, args) =>
            //{
            //    SelectedHour = (int)args.AddedItems[0];
            //};

            //minutesDataSource = new IntDataSource(0, 59);
            //minutesDataSource.SelectionChanged += (sender, args) =>
            //{
            //    SelectedMinute = (int)args.AddedItems[0];
            //};

            //orientationDataSource = new IntDataSource(0, 1);
            //orientationDataSource.SelectionChanged += (sender, args) =>
            //{
            //    SelectedOrientation = (int)args.AddedItems[0];
            //};
        }


        //private readonly IntDataSource hoursDataSource;
        //public IntDataSource Hours
        //{
        //    get { return hoursDataSource; }
        //}

        //public int SelectedHour
        //{
        //    get
        //    {
        //        return Math.Abs(NotificationDelta.Hours);
        //    }
        //    set
        //    {
        //        NotificationDelta = new TimeSpan(SelectedOrientation * value, SelectedOrientation * SelectedMinute, 0);
        //    }
        //}

        //private readonly IntDataSource minutesDataSource;
        //public IntDataSource Minutes
        //{
        //    get { return minutesDataSource; }
        //}

        //public int SelectedMinute
        //{
        //    get
        //    {
        //        return Math.Abs(NotificationDelta.Minutes);
        //    }
        //    set
        //    {
        //        NotificationDelta = new TimeSpan(SelectedOrientation * SelectedHour, SelectedOrientation * value, 0);
        //    }
        //}

        //private readonly IntDataSource orientationDataSource;
        //public IntDataSource Orientation
        //{
        //    get { return orientationDataSource; }
        //}

        //public int SelectedOrientation
        //{
        //    get
        //    {
        //        return NotificationDelta.Ticks <= 0 ? -1 : 1;
        //    }
        //    set
        //    {
        //        value = (value == 0 ? -1 : 1);
        //        NotificationDelta = new TimeSpan(value * SelectedHour, value * SelectedMinute, 0);
        //    }
        //}
        
        public bool NotificationsEnabled
        {
            get
            {
                return reminderService.NotificationsEnabled;
            }
            set
            {
                if (value)
                {
                    reminderService.EnableNotifications();
                }
                else
                {
                    reminderService.DisableNotifications();
                }
                RaisePropertyChanged(() => NotificationsEnabled);
            }
        }

    }
}
