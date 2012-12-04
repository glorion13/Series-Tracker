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

        private ObservableCollection<TvDbSeries> series;
        public ObservableCollection<TvDbSeries> Series
        {
            get
            {
                return series;
            }
            set
            {
                Set(() => Series, ref series, value);
            }
        }

        ObservableCollection<TvDbSeries> searchResults;
        public ObservableCollection<TvDbSeries> SearchResults
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

                searchResults = new SelfSortingObservableCollection<TvDbSeries, float>(s => s.Rating, order: SortOrder.Desc);
                //series = new SelfSortingObservableCollection<SeriesRecord, string>(s => s.Series.Title);
            }
            else if (IsInDesignMode)
            {
                searchResults = new ObservableCollection<TvDbSeries>();
                series = new ObservableCollection<TvDbSeries>();

                Search = "Simpsons";
                series.Add(new TvDbSeries()
                {
                    Title = "Futurama",
                    Image = "http://thetvdb.com/banners/posters/73871-2.jpg",
                    Rating = 5,
                    AirsTime = "11 PM",
                    Episodes = new List<TvDbSeriesEpisode>()
                    {
                        new TvDbSeriesEpisode() {
                            Name = "Episode 1",
                            FirstAired = DateTime.Now.AddDays(14).AddHours(5),
                            SeriesNumber = "1",
                            EpisodeNumber = "1",
                            Description = "bla bla bla bla bla lba",
                            Image = "http://thetvdb.com/banners/episodes/121361/3254641.jpg"
                        },
                        new TvDbSeriesEpisode() {
                            Name = "Episode 2",
                            FirstAired = DateTime.Now.AddDays(7),
                            SeriesNumber = "1",
                            EpisodeNumber = "2",
                            Description = "bla bla bla bla bla lba",
                            Image = "http://thetvdb.com/banners/episodes/121361/3254641.jpg"
                        },
                        new TvDbSeriesEpisode() {
                            Name = "Episode 1",
                            FirstAired = DateTime.Now,
                            SeriesNumber = "2",
                            EpisodeNumber = "1",
                            Description = "bla bla bla bla bla lba",
                            Image = "http://thetvdb.com/banners/episodes/121361/3254641.jpg"
                        }
                    }
                });

                series.Add(new TvDbSeries()
                {
                    Title = "Simpsons",
                    Image = "http://thetvdb.com/banners/posters/71663-10.jpg",
                    Rating = 10
                });

                searchResults = series;
            }
        }

        public async Task Initialize()
        {
            await LoadSubscriptions();
            SetupSearch();
        }

        private void SetupSearch()
        {
            var ui = DispatcherSynchronizationContext.Current;
            this.ObservableForProperty(m => m.Search).ObserveOn(ui).Subscribe(async change =>
            {
                searchResults.Clear();
                IsSearching = true;

                var results = await repository.FindAsync(change.Value);

                await searchResults.AddAll(results.Keys);

                await TaskEx.WhenAll(results.Values);

                IsSearching = false;
            });
        }

        private async Task LoadSubscriptions()
        {
            IsLoadingSubscriptions = true;

            try
            {
                Series = await repository.GetSubscribedAsync();
            }
            finally
            {
                IsLoadingSubscriptions = false;
            }
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

        private bool isLoadingSubscriptions = true;
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
                        //series.Add(new SeriesRecord(s));
                        repository.SubscribeAsync(s);
                    }
                    else
                    {
                        //series.Remove(series.FirstOrDefault(old => old.Series.Id == s.Id));
                        repository.UnsubscribeAsync(s); 
                    }
                }));
            }
        }

        private ICommand viewDetails;
        public ICommand ViewDetails
        {
            get
            {
                return viewDetails ?? (viewDetails = new RelayCommand<TvDbSeries>(s =>
                    {
                        MessengerInstance.Send(s);
                        MessengerInstance.Send(new Uri("/SeriesDetails.xaml", UriKind.Relative));
                    }));
            }
        }
    }
}
