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

namespace SeriesTracker
{
    public class Main : ViewModelBase
    {
        ObservableCollection<SeriesRecord> series;
        public ObservableCollection<SeriesRecord> Series
        {
            get
            {
                return series;
            }
        }

        private TvDb tvdb;

        public Main()
        {
            series = new ObservableCollection<SeriesRecord>();
            
            if (!IsInDesignMode)
            {
                tvdb = new TvDb();

                this.ObservableForProperty(m => m.Search).ObserveOnDispatcher().Subscribe(change =>
                {
                    series.Clear();
                    IsSearching = true;
                    var list = new List<SeriesRecord>();

                    tvdb.FindSeries(change.Value).Select(seriesBase => tvdb.UpdateData(seriesBase).First()).ObserveOnDispatcher().Subscribe(s =>
                    {
                        list.Add(new SeriesRecord(s));
                    }, () =>
                    {
                        IsSearching = false;
                        series = new ObservableCollection<SeriesRecord>(list.OrderByDescending(s => s.Series.Rating).ToList());
                        RaisePropertyChanged(() => Series);
                    });
                });
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
    }
}
