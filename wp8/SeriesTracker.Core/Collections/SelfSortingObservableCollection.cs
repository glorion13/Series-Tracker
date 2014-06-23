using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Threading;
using ReactiveUI;

namespace SeriesTracker
{
    public enum SortOrder { Asc, Desc }

    public class SelfSortingObservableCollection<T, Tv> : ObservableCollection<T>
        where T : class, INotifyPropertyChanged
    {
        private Expression<Func<T, Tv>> sortingProperty;
        private Func<T, Tv> accessor;
        private IComparer<Tv> comparer;

        private Dictionary<T, IDisposable> subscriptions;

        private int sortModifier = 1;

        public SelfSortingObservableCollection(Expression<Func<T, Tv>> sortingProperty, IComparer<Tv> comparer = null, SortOrder order = SortOrder.Asc)
        {
            this.sortingProperty = sortingProperty;
            accessor = sortingProperty.Compile();
            this.comparer = comparer ?? Comparer<Tv>.Default;

            subscriptions = new Dictionary<T, IDisposable>();

            if (order == SortOrder.Desc)
                sortModifier = -1;
        }

        private object gate = new object();
        protected override void InsertItem(int index, T item)
        {
            lock (gate)
            {
                base.InsertItem(FindIndex(accessor(item)), item);
            }
        }

        private int FindIndex(Tv newValue)
        {
            int i = 0;
            for (; i < Count; i++)
            {
                var currentValue = accessor(this[i]);
                if (comparer.Compare(newValue, currentValue) * sortModifier < 0)
                    break;
            }
            return i;
        }

        private T beingReordered;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<T>())
                    {
                        if (beingReordered != item)
                        {
                            var subscription = item.ObservableForProperty(sortingProperty).Subscribe(change =>
                            {
                                DispatcherHelper.UIDispatcher.BeginInvoke(() => {
                                    beingReordered = item;

                                    // force reording
                                    Remove(item);
                                    Add(item);

                                    beingReordered = null;
                                });
                            });

                            subscriptions.Add(item, subscription);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.Cast<T>())
                    {
                        if (beingReordered != item)
                        {
                            var subscription = subscriptions[item];
                            if (subscription != null)
                            {
                                subscription.Dispose();
                                subscriptions.Remove(item);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var subscription in subscriptions)
                    {
                        subscription.Value.Dispose();
                    }
                    subscriptions.Clear();
                    break;

            }
            base.OnCollectionChanged(e);
        }

    }
}
