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

        private TvDbSeriesEpisode selectedEpisode = null;
        public TvDbSeriesEpisode SelectedEpisode
        {
            get
            {
                return selectedEpisode;
            }
            set
            {
                Set(() => SelectedEpisode, ref selectedEpisode, value);
            }
        }

        private ICommand episodeSelected;
        public ICommand EpisodeSelected
        {
            get
            {
                return episodeSelected ?? (episodeSelected = new RelayCommand<SelectionChangedEventArgs>(s =>
                    {
                        SelectedEpisode = s.AddedItems.OfType<LongListSelectorItem>().Select(i => i.Item as TvDbSeriesEpisode).FirstOrDefault();
                    }));
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

        public LongListCollection<TvDbSeriesEpisode, string> Episodes
        {
            get
            {
                return new LongListCollection<TvDbSeriesEpisode, string>(
                    series.Episodes.OrderByDescending(l => l.SeriesNumber).ThenByDescending(l => l.EpisodeNumber),
                    e => e.SeriesNumber,
                    series.Episodes.Select(e => e.SeriesNumber).ToList());
            }
        }
    }
}
