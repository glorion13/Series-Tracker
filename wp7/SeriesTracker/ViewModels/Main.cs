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

namespace SeriesTracker
{
    public class Main : ViewModelBase
    {
        SelfSortingObservableCollection<SeriesRecord, float> series;
        public SelfSortingObservableCollection<SeriesRecord, float> Series
        {
            get
            {
                return series;
            }
        }

        private TvDb tvdb;

        public Main()
        {
            series = new SelfSortingObservableCollection<SeriesRecord, float>(s => s.Series.Rating);
            
            if (!IsInDesignMode)
            {
                tvdb = new TvDb();

                this.ObservableForProperty(m => m.Search).ObserveOnDispatcher().Subscribe(change =>
                {
                    series.Clear();
                    IsSearching = true;
                    var list = new List<SeriesRecord>();

                    tvdb.FindSeries(change.Value).ObserveOnDispatcher().Do(s =>
                    {
                        series.Add(new SeriesRecord(s));
                    })
                    .ObserveOn(Scheduler.ThreadPool).Select(seriesBase => tvdb.UpdateData(seriesBase).First())
                    .ObserveOnDispatcher().Finally(() =>
                    {
                        IsSearching = false;
                        RaisePropertyChanged(() => Series);
                    }).Subscribe();
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
