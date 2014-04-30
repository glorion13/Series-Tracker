using System;
using System.Linq;
using System.Net;
using System.Threading;
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
using Microsoft.Phone.Shell;
using SeriesTracker.Agent;
using SeriesTracker.Core;
using SeriesTracker.Collections;

namespace SeriesTracker
{
    public class MainViewModel : ViewModelBase
    {
        // Live Tile stuff
        public class LiveTileUpdater
        {
            private MainViewModel mvm;
            public LiveTileUpdater(MainViewModel mvm)
            {
                this.mvm = mvm;
            }
            public void updateLiveTile()
            {

            }
        }
        private void initialiseLiveTile()
        {
            ShellTile PrimaryTile = ShellTile.ActiveTiles.First();

            if (PrimaryTile != null)
            {
                StandardTileData tile = new StandardTileData();

                tile.Count = 4;
                tile.Title = "Series Tracker";
                PrimaryTile.Update(tile);
            }
        }

        private readonly TvDbSeriesRepository repository;
        private readonly ConnectivityService connectivityService;
        private readonly ReminderService reminderService;
        private readonly AgentScheduler agentScheduler;
        private LiveTileUpdater ltUpdater;

        private bool connectionDown;
        public bool ConnectionDown
        {
            get
            {
                return connectionDown;
            }
            set
            {
                Set(() => ConnectionDown, ref connectionDown, value);
            }
        }

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
                Series.CollectionChanged += (e, v) => RaisePropertyChanged(() => SeriesSortedList);
            }
        }

        private bool alphabeticalSortingEnabled;
        public bool AlphabeticalSortingEnabled
        {
            get
            {
                return alphabeticalSortingEnabled;
            }
            set
            {
                Set(() => AlphabeticalSortingEnabled, ref alphabeticalSortingEnabled, value);
                RaisePropertyChanged(() => AlphabeticalSortingVisibility);
                RaisePropertyChanged(() => RegularSortingVisibility);
            }
        }

        private string alphabeticalSortingVisibility;
        public string AlphabeticalSortingVisibility
        {
            get
            {
                return AlphabeticalSortingEnabled ? "Visible" : "Collapsed";
            }
            set
            {
                Set(() => AlphabeticalSortingVisibility, ref alphabeticalSortingVisibility, value);
            }
        }
        private string regularSortingVisibility;
        public string RegularSortingVisibility
        {
            get
            {
                return AlphabeticalSortingEnabled ? "Collapsed" : "Visible";
            }
            set
            {
                Set(() => RegularSortingVisibility, ref regularSortingVisibility, value);
            }
        }

        public LongListCollection<TvDbSeries, char> SeriesSortedList
        {
            get
            {
                // We group the series by first letter. If it's a symbol or number, add it under '#'.
                // If the first word is 'the', then use the first letter of the second word.
                return new LongListCollection<TvDbSeries, char>(
                    series.OrderBy(l => l.Title[0]).Select(x => x),
                    s => GroupByNumberOrLetter(s.Title),
                    "#abcdefghijklmnopqrstuvwxyz".ToCharArray().OrderBy(l => l).ToList());
            }
        }

        private bool IsTheFirstWordRedundant(string seriesTitle)
        {
            string firstWord = seriesTitle.Split(' ').First();
            if (firstWord.Equals("the", StringComparison.OrdinalIgnoreCase))
                return true;
            if (firstWord.Equals("a", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private char GroupByFirstWordOrSecondWord(string seriesTitle)
        {
            string[] words = seriesTitle.Split(' ');
            return IsTheFirstWordRedundant(seriesTitle) ? words[1].ToLower()[0] : words[0].ToLower()[0];
        }

        private char GroupByNumberOrLetter(string seriesTitle)
        {
            char characterUsedForGrouping = GroupByFirstWordOrSecondWord(seriesTitle);
            return Char.IsLetter(characterUsedForGrouping) ? characterUsedForGrouping : '#';
        }

        ObservableCollection<TvDbSeries> searchResults;
        public ObservableCollection<TvDbSeries> SearchResults
        {
            get
            {
                return searchResults;
            }
        }

        public MainViewModel(TvDbSeriesRepository repository, ConnectivityService connectivityService, ReminderService reminderService, AgentScheduler agentScheduler)
        {
            this.connectivityService = connectivityService;
            this.reminderService = reminderService;
            this.agentScheduler = agentScheduler;
            connectivityService.InternetDown += connectivityService_InternetDown;
            connectivityService.InternetUp += connectivityService_InternetUp;

            Series = new SelfSortingObservableCollection<TvDbSeries, DateTime?>(s => s.NextEpisodeAirDateTime, new SoonestFirstComparer());
            repository.Subscribed += (sender, args) => DispatcherHelper.UIDispatcher.BeginInvoke(() => Series.Add(args.Series));
            repository.Unsubscribed += (sender, args) => DispatcherHelper.UIDispatcher.BeginInvoke(() => Series.Remove(args.Series));

            AlphabeticalSortingEnabled = AppSettings.Instance.AlphabeticalSortingEnabled;

            if (!IsInDesignMode)
            {
                this.repository = repository;
                IsSearchBoxEnabled = true;
                searchResults = new SelfSortingObservableCollection<TvDbSeries, float>(s => s.Rating, order: SortOrder.Desc);
                ltUpdater = new LiveTileUpdater(this);
                initialiseLiveTile();
                MessengerInstance.Register<AppSettings>(this, s => AlphabeticalSortingEnabled = s.AlphabeticalSortingEnabled);
                //series = new SelfSortingObservableCollection<TvDbSeries, string>(s => s.Title);
            }
            else if (IsInDesignMode)
            {
                searchResults = new ObservableCollection<TvDbSeries>();

                Search = "Simpsons";
                Series.Add(new TvDbSeries()
                {
                    Title = "Futurama",
                    Image = "http://thetvdb.com/banners/posters/73871-2.jpg",
                    Rating = 5,
                    AirsTime = "11 PM",
                    Episodes = new ObservableCollection<TvDbSeriesEpisode>()
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

                Series.Add(new TvDbSeries()
                {
                    Title = "Simpsons",
                    Image = "http://thetvdb.com/banners/posters/71663-10.jpg",
                    Rating = 10
                });

                searchResults = series;

                AlphabeticalSortingEnabled = true;
            }
        }

        void connectivityService_InternetUp(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => ConnectionDown = false);
        }

        void connectivityService_InternetDown(object sender, EventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() => ConnectionDown = true);
        }

        public async Task Initialize()
        {
            if (initialized || IsInDesignMode)
                return;

            initialized = true;

            SetupSearch();

            await LoadSubscriptions();

            reminderService.CreateOrUpdateRemindersAsync();
        }

        private readonly SemaphoreSlim searchLock = new SemaphoreSlim(1);

        private void SetupSearch()
        {
            var ui = SynchronizationContext.Current;

            CancellationTokenSource cancellationTokenSource = null;

            this.ObservableForProperty(m => m.Search).ObserveOn(ui).Subscribe(async change =>
            {
                var myCancelation = new CancellationTokenSource();
                var theirCancellation = Interlocked.Exchange(ref cancellationTokenSource, myCancelation);

                if (theirCancellation != null)
                {
                    theirCancellation.Cancel();
                }

                try
                {
                    await searchLock.WaitAsync(myCancelation.Token);
                    try
                    {
                        await DoSearch(change.Value, myCancelation.Token);
                    }
                    finally
                    {
                        searchLock.Release();
                    }
                }
                catch (OperationCanceledException)
                {

                }
            });
        }

        private async Task DoSearch(string searchTerm, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            IsSearching = true;

            token.ThrowIfCancellationRequested();

            searchResults.Clear();

            token.ThrowIfCancellationRequested();

            var results = await repository.FindAsync(searchTerm);

            token.ThrowIfCancellationRequested();

            await searchResults.AddAllAsync(results.Keys);

            token.ThrowIfCancellationRequested();

            await TaskEx.WhenAll(results.Values);

            token.ThrowIfCancellationRequested();

            IsSearching = false;
        }

        private async Task LoadSubscriptions()
        {
            IsLoadingSubscriptions = true;

            try
            {
                var subscribed = await repository.GetSubscribedAsync();
                await Series.AddAllAsync(subscribed);
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
        private bool isSearchBoxEnabled;
        public bool IsSearchBoxEnabled
        {
            get
            {
                return isSearchBoxEnabled;
            }

            set
            {
                Set(() => IsSearchBoxEnabled, ref isSearchBoxEnabled, value);
            }
        }
        private RelayCommand<KeyEventArgs> closeSoftKeyboard;
        public RelayCommand<KeyEventArgs> CloseSoftKeyboard
        {
            get
            {
                return closeSoftKeyboard ?? (closeSoftKeyboard = new RelayCommand<KeyEventArgs>(a =>
                {
                    if (a.Key == Key.Enter)
                    {
                        IsSearchBoxEnabled = false;
                        IsSearchBoxEnabled = true;
                    }
                }));
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

        private ICommand viewAboutPage;
        public ICommand ViewAboutPage
        {
            get
            {
                return viewAboutPage ?? (viewAboutPage = new RelayCommand<TvDbSeries>(s => MessengerInstance.Send(new Uri("/About.xaml", UriKind.Relative))));
            }
        }

        private ICommand viewSettingsPage;
        private bool initialized;

        public ICommand ViewSettingsPage
        {
            get
            {
                return viewSettingsPage ?? (viewSettingsPage = new RelayCommand<TvDbSeries>(s => MessengerInstance.Send(new Uri("/SettingsPage.xaml", UriKind.Relative))));
            }
        }
    }
}
