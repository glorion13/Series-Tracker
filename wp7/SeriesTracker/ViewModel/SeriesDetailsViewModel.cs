using GalaSoft.MvvmLight;
using SeriesTracker.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SeriesTracker
{
    public class SeriesDetailsViewModel : ViewModelBase
    {
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

        public SeriesDetailsViewModel()
        {
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
