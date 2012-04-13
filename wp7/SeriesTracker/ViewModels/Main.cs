using System;
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

namespace SeriesTracker
{
    public class Main : ReactiveObject
    {
        ReactiveCollection<Series> series;
        public ReactiveCollection<Series> Series
        {
            get
            {
                return series;
            }
        }

        private TvDb tvdb;

        public Main()
        {
            series = new ReactiveCollection<Series>();
            tvdb = new TvDb();
            
            this.ObservableForProperty(m => m.Search).Throttle(TimeSpan.FromMilliseconds(250), Scheduler.CurrentThread).Subscribe(change =>
            {
                series.Clear();
                tvdb.FindSeries(change.Value).ObserveOnDispatcher().Subscribe(s => series.Add(new Series(s)));
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
                this.RaiseAndSetIfChanged(s => s.Search, ref search, value);
            }
        }
    }
}
