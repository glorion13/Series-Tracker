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
using System.Threading.Tasks;

namespace SeriesTracker
{
    public class MainViewModel : ViewModelBase
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

        public MainViewModel(TvDbSeriesRepository repository)
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
                    Thumbnail = "http://thetvdb.com/banners/posters/73871-2.jpg",
                    Rating = 5
                }));

                series.Add(new SeriesRecord(new TvDbSeries()
                {
                    Title = "Simpsons",
                    Thumbnail = "http://thetvdb.com/banners/posters/71663-10.jpg",
                    Rating = 10
                }));
            }
        }

        private void SetupSearch()
        {
            var ui = DispatcherSynchronizationContext.Current;
            this.ObservableForProperty(m => m.Search).ObserveOn(ui).Subscribe(async change =>
            {
                searchResults.Clear();
                IsSearching = true;

                var results = await repository.Find(change.Value);

                results.ToObservable()
                    .Do(s => searchResults.Add(new SeriesRecord(s)))
                    .Select(sb => repository.UpdateData(sb))
                    .ObserveOn(new NewThreadScheduler())
                    .ToArray()
                    .Do(l => Task.WaitAll(l))
                    .Finally(() =>
                        {
                            DispatcherHelper.UIDispatcher.BeginInvoke(() =>
                            {
                                IsSearching = false;
                                RaisePropertyChanged(() => Series);
                            });
                        })
                    .Subscribe();
            });
        }

        private async Task LoadSubscriptions()
        {
            IsLoadingSubscriptions = true;

            var subs = await repository.GetSubscribed();
            foreach (var s in subs)
            {
                series.Add(new SeriesRecord(s));
            }

            IsLoadingSubscriptions = false;
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
                        series.Add(new SeriesRecord(s));
                        repository.Subscribe(s);
                    }
                    else
                    {
                        series.Remove(series.FirstOrDefault(old => old.Series.Id == s.Id));
                        repository.Unsubscribe(s); 
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
                    }));
            }
        }
    }
}
