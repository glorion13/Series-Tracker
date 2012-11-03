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
using System.Windows.Navigation;

namespace SeriesTracker
{
    public class Main : ViewModelBase
    {
        private TvDbSeriesRepository repository;

        ObservableCollection<SeriesRecord> series;
        public ObservableCollection<SeriesRecord> Series
        {
            get
            {
                return series;
            }
        }

        ObservableCollection<SeriesRecord> searchResults;
        public ObservableCollection<SeriesRecord> SearchResults
        {
            get
            {
                return searchResults;
            }
        }

        public Main(TvDbSeriesRepository repository)
        {
            if (!IsInDesignMode)
            {
                this.repository = repository;

                searchResults = new SelfSortingObservableCollection<SeriesRecord, float>(s => s.Series.Rating, order: SortOrder.Desc);
                series = new SelfSortingObservableCollection<SeriesRecord, string>(s => s.Series.Title);

                LoadSubscriptions();
                SetupSearch();

            }
            else if (IsInDesignMode)
            {
                searchResults = series = new ObservableCollection<SeriesRecord>();

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
            this.ObservableForProperty(m => m.Search).ObserveOnDispatcher().Subscribe(change =>
            {
                searchResults.Clear();
                IsSearching = true;
                var list = new List<SeriesRecord>();

                repository.Find(change.Value).ObserveOnDispatcher().Do(s =>
                {
                    searchResults.Add(new SeriesRecord(s));
                })
                .ObserveOn(ThreadPoolScheduler.Instance).Select(seriesBase => repository.UpdateData(seriesBase).First())
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
            new NewThreadScheduler().Schedule(() =>
            {
                foreach (var s in repository.GetSubscribed())
                {
                    var sr = new SeriesRecord(s);
                    DispatcherHelper.CheckBeginInvokeOnUI(() => series.Add(sr));
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

        private RelayCommand<TvDbSeries> toggleSubscribed;
        public RelayCommand<TvDbSeries> ToggleSubscribed
        {
            get
            {
                return toggleSubscribed ?? (toggleSubscribed = new RelayCommand<TvDbSeries>(s =>
                {
                    if (!s.IsSubscribed)
                    {
                        repository.Subscribe(s);
                        series.Add(new SeriesRecord(s));
                    }
                    else
                    {
                        repository.Unsubscribe(s);
                        series.Remove(series.FirstOrDefault(old => old.Series.Id == s.Id));
                    }
                }));
            }
        }

        private ICommand viewDetails;
        public ICommand ViewDetails
        {
            get
            {
                return viewDetails ?? (viewDetails = new RelayCommand<SeriesRecord>(s =>
                    {
                        MessengerInstance.Send(s.Series);
                        MessengerInstance.Send(new Uri("/SeriesDetails.xaml", UriKind.Relative));

                        /*var navigation = new NavigationService();
                        navigation.Navigate(new Uri("SeriesDetails.xaml"));*/
                    }));
            }
        }
    }
}
