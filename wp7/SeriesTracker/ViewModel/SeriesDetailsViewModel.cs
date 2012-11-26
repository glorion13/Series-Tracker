using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Controls;
using SeriesTracker.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace SeriesTracker
{
    public class SeriesDetailsViewModel : ViewModelBase
    {
        private TvDbSeriesRepository repository;

        private TvDbSeries series;
        public TvDbSeries Series
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

        private ICommand toggleExpanded;
        public ICommand ToggleExpanded
        {
            get
            {
                return toggleExpanded ?? (toggleExpanded = new RelayCommand<EpisodeViewModel>(e => e.IsExpanded = !e.IsExpanded));
            }
        }

        private ICommand subscribe;
        public ICommand Subscribe
        {
            get
            {
                return subscribe ?? (subscribe = new RelayCommand(async () =>
                {
                    await repository.SubscribeAsync(series);
                }));
            }
        }

        private ICommand unsubscribe;
        public ICommand Unsunscribe
        {
            get
            {
                return unsubscribe ?? (unsubscribe = new RelayCommand(async () =>
                {
                    await repository.UnsubscribeAsync(series);
                }));
            }
        }

        private RelayCommand<EpisodeViewModel> toggleEpisodeSeen;
        public RelayCommand<EpisodeViewModel> ToggleEpisodeSeen
        {
            get
            {
                return toggleEpisodeSeen ?? (toggleEpisodeSeen = new RelayCommand<EpisodeViewModel>(s =>
                {
                    if (!s.Episode.IsSeen)
                    {
                        repository.MarkSeenAsync(Series, s.Episode);
                        //series.Add(new SeriesRecord(s));
                    }
                    else
                    {
                        repository.UnmarkSeenAsync(Series, s.Episode);
                        //series.Remove(series.FirstOrDefault(old => old.Series.Id == s.Id));
                        //repository.UnsubscribeAsync(s);
                    }
                }));
            }
        }

        private RelayCommand<string> markSeenSeason;
        public RelayCommand<string> MarkSeenSeason
        {
            get
            {
                return markSeenSeason ?? (markSeenSeason = new RelayCommand<string>(s =>
                {
                    foreach (var episode in series.Episodes)
                        if (episode.SeriesNumber == s)
                            repository.MarkSeenAsync(Series, episode);
                }));
            }
        }

        private RelayCommand<string> unmarkSeenSeason;
        public RelayCommand<string> UnmarkSeenSeason
        {
            get
            {
                return unmarkSeenSeason ?? (unmarkSeenSeason = new RelayCommand<string>(s =>
                {
                    foreach (var episode in series.Episodes)
                        if (episode.SeriesNumber == s)
                            repository.UnmarkSeenAsync(Series, episode);
                }));
            }
        }

        private ICommand markSeenAll;
        public ICommand MarkSeenAll
        {
            get
            {
                return markSeenAll ?? (markSeenAll = new RelayCommand(() =>
                {
                    foreach (var episode in series.Episodes)
                        repository.MarkSeenAsync(Series, episode);
                }));
            }
        }

        private ICommand unmarkSeenAll;
        public ICommand UnmarkSeenAll
        {
            get
            {
                return unmarkSeenAll ?? (unmarkSeenAll = new RelayCommand(() =>
                {
                    foreach (var episode in series.Episodes)
                        repository.UnmarkSeenAsync(Series, episode);
                }));
            }
        }

        private ICommand openImdbLink;
        public ICommand OpenImdbLink
        {
            get
            {
                return openImdbLink ?? (openImdbLink = new RelayCommand(() =>
                {
                    var task = new Microsoft.Phone.Tasks.WebBrowserTask();
                    task.Uri = new Uri("http://www.imdb.com/title/" + Series.ImdbId + "/");
                    task.Show();
                }));
            }
        }

        public SeriesDetailsViewModel(TvDbSeriesRepository repository)
        {
            this.repository = repository;

            if (!IsInDesignMode)
            {
                MessengerInstance.Register<TvDbSeries>(this, s => Series = s);
            }
            else
            {
                Series = new TvDbSeries()
                {
                    Title = "Futurama",
                    Image = "http://thetvdb.com/banners/posters/73871-2.jpg",
                    Banner = "http://thetvdb.com/banners/graphical/121361-g19.jpg",
                    Rating = 5,
                    AirsTime = "9 PM",
                    AirsDayOfWeek = 6,
                    Episodes = new ObservableCollection<TvDbSeriesEpisode>() {
                        new TvDbSeriesEpisode() {
                            Name = "Episode 1",
                            SeriesNumber = "1",
                            EpisodeNumber = "1",
                            Description = "bla bla bla bla bla lba",
                            Image = "http://thetvdb.com/banners/episodes/121361/3254641.jpg"
                        },
                        new TvDbSeriesEpisode() {
                            Name = "Episode 2",
                            SeriesNumber = "1",
                            EpisodeNumber = "2",
                            FirstAired = DateTime.Today.AddDays(2),
                            Description = "bla bla bla bla bla lba",
                            Image = "http://thetvdb.com/banners/episodes/121361/3254641.jpg"
                        },
                        new TvDbSeriesEpisode() {
                            Name = "Episode 1",
                            SeriesNumber = "2",
                            EpisodeNumber = "1",
                            Description = "bla bla bla bla bla lba",
                            Image = "http://thetvdb.com/banners/episodes/121361/3254641.jpg"
                        }
                    } 
                };
            }
        }

        public class EpisodeViewModel : ViewModelBase, IComparable<EpisodeViewModel>
        {
            private TvDbSeriesEpisode episode = null;
            public TvDbSeriesEpisode Episode
            {
                get
                {
                    return episode;
                }
                set
                {
                    Set(() => Episode, ref episode, value);
                }
            }

            private bool isExpanded = false;
            public bool IsExpanded
            {
                get
                {
                    return isExpanded;
                }
                set
                {
                    Set(() => IsExpanded, ref isExpanded, value);
                }
            }

            public EpisodeViewModel(TvDbSeriesEpisode episode) {
                this.episode = episode;
            }

            public int CompareTo(EpisodeViewModel other)
            {
                if (other == null) return 1;

                return this.episode.CompareTo(other.episode);
            }
        }

        public LongListCollection<EpisodeViewModel, string> Episodes
        {
            get
            {
                return new LongListCollection<EpisodeViewModel, string>(
                    series.Episodes.OrderByDescending(l => l.SeriesNumber).ThenByDescending(l => l.EpisodeNumber).Select(x => new EpisodeViewModel(x)),
                    e => e.Episode.SeriesNumber,
                    series.Episodes.Select(e => e.SeriesNumber).ToList());
            }
        }

    }
}
