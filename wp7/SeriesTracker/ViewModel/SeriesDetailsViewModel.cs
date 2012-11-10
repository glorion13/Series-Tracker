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
        private SubscriptionManager subscriptionManager;

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
                return subscribe ?? (subscribe = new RelayCommand(() =>
                {
                    subscriptionManager.Subscribe(series);
                }));
            }
        }

        private ICommand unsubscribe;
        public ICommand Unsunscribe
        {
            get
            {
                return unsubscribe ?? (unsubscribe = new RelayCommand(() =>
                {
                    subscriptionManager.Unsubscribe(series);
                }));
            }
        }

        public SeriesDetailsViewModel(SubscriptionManager subscriptionManager)
        {
            this.subscriptionManager = subscriptionManager;

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
                    Thumbnail = "http://thetvdb.com/banners/posters/73871-2.jpg",
                    Banner = "http://thetvdb.com/banners/graphical/121361-g19.jpg",
                    Rating = 5,
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
