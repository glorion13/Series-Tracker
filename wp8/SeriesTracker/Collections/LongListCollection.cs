using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SeriesTracker.Collections
{
    public class LongListCollection<T, TKey> : ObservableCollection<LongListItem<T, TKey>>
            where T : IComparable<T>
    {
        #region Constructor
        public LongListCollection()
        {
        }

        public LongListCollection(IEnumerable<T> items, Func<T, TKey> keySelector, List<TKey> defaultHeaders)
        {
            if (items == null)
                throw new ArgumentException("items");

            var groups = new Dictionary<TKey, LongListItem<T, TKey>>();
            this.FillGroup(groups, defaultHeaders);

            foreach (var item in items.OrderBy(x => x))
            {
                var key = keySelector(item);

                if (groups.ContainsKey(key) == false)
                    groups.Add(key, new LongListItem<T, TKey>(key));

                groups[key].Add(item);
            }

            foreach (var value in groups.Values.OrderByDescending(i => i.Key.ToString().PadLeft(10, '0')))
                this.Add(value);
        }
        #endregion

        #region Private methods
        private void FillGroup(Dictionary<TKey, LongListItem<T, TKey>> groups, List<TKey> defaultHeaders)
        {
            foreach (TKey key in defaultHeaders)
            {
                if (!groups.ContainsKey(key))
                    groups.Add(key, new LongListItem<T, TKey>(key));
            }
        }
        #endregion
    }

    public class LongListItem<T, TKey> : ObservableCollection<T>
    {
        public LongListItem()
        {
        }

        public LongListItem(TKey key)
        {
            this.Key = key;
        }

        public TKey Key
        {
            get;
            set;
        }

        public bool HasItems
        {
            get
            {
                return Count > 0;
            }
        }

        public Brush GroupBackgroundBrush
        {
            get
            {
                if (HasItems)
                    return (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
                else
                    return (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"];
            }
        }
    }
}
