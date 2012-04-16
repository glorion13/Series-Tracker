using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ReactiveUI;
using SeriesTracker;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using System.Reactive;
using GalaSoft.MvvmLight.Threading;

namespace SeriesTracker
{
    public class Main : ViewModelBase
    {
        private SubscriptionManager subscriptionManager;

        SelfSortingObservableCollection<SeriesRecord, string> series;
        public SelfSortingObservableCollection<SeriesRecord, string> Series
        {
            get
            {
                return series;
            }
        }

        SelfSortingObservableCollection<SeriesRecord, float> searchResults;
        public SelfSortingObservableCollection<SeriesRecord, float> SearchResults
        {
            get
            {
                return searchResults;
            }
        }

        private TvDb tvdb;

        public Main()
        {
            subscriptionManager = new SubscriptionManager();

            searchResults = new SelfSortingObservableCollection<SeriesRecord, float>(s => s.Series.Rating, order:SortOrder.Desc);
            series = new SelfSortingObservableCollection<SeriesRecord, string>(s => s.Series.Title);
            

            if (!IsInDesignMode)
            {
                LoadSubscriptions();
                SetupSearch();

            } else if (IsInDesignMode)
            {
                Search = "Simpsons";
                series.Add(new SeriesRecord(new TvDbSeries()
                {
                    Title = "Futurama",
                    Image = "http://thetvdb.com/banners/posters/73871-2.jpg",
                    Rating = 5
                }));

                series.Add(new SeriesRecord(new TvDbSeries()
                {
                    Title = "Simpsons",
                    Image = "http://thetvdb.com/banners/posters/71663-10.jpg",
                    Rating = 10
                }));
            }
        }

        private void SetupSearch()
        {
            tvdb = new TvDb();

            this.ObservableForProperty(m => m.Search).ObserveOnDispatcher().Subscribe(change =>
            {
                searchResults.Clear();
                IsSearching = true;
                var list = new List<SeriesRecord>();

                tvdb.FindSeries(change.Value).ObserveOnDispatcher().Do(s =>
                {
                    searchResults.Add(new SeriesRecord(s));
                })
                .ObserveOn(Scheduler.ThreadPool).Select(seriesBase => tvdb.UpdateData(seriesBase).First())
                .ObserveOnDispatcher().Finally(() =>
                {
                    IsSearching = false;
                    RaisePropertyChanged(() => Series);
                }).Subscribe();
            });
        }

        private void LoadSubscriptions()
        {
            IsLoadingSubscriptions = true;
            Scheduler.NewThread.Schedule(() =>
            {
                foreach (var s in subscriptionManager.Subscriptions)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(() => series.Add(new SeriesRecord(s)));
                }
                DispatcherHelper.CheckBeginInvokeOnUI(() => IsLoadingSubscriptions = false);
            });
        }

        private string search;
        public string Search
        {
            get
            {
                return search;
            }

            set
            {
                Set(() => Search, ref search, value);
            }
        }

        private bool isLoadingSubscriptions;
        public bool IsLoadingSubscriptions
        {
            get
            {
                return isLoadingSubscriptions;
            }
            set
            {
                Set(() => IsLoadingSubscriptions, ref isLoadingSubscriptions, value);
            }
        }

        private bool isSearching;
        public bool IsSearching
        {
            get
            {
                return isSearching;
            }
            set
            {
                Set(() => IsSearching, ref isSearching, value);
            }
        }

        private RelayCommand<TvDbSeries> subscribe;
        public RelayCommand<TvDbSeries> Subscribe
        {
            get {
                return subscribe ?? (subscribe = new RelayCommand<TvDbSeries>(s => {
                    subscriptionManager.Subscribe(s);
                    series.Add(new SeriesRecord(s));
                }));
            }
        }
    }
}
