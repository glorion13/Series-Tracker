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
using System.Reactive.Linq;

namespace SeriesTracker
{
    public static class ReactiveExtensions
    {
        public static IObservable<string> AsContentString(this IObservable<byte[]> observable) {
            return observable.Select(array => System.Text.Encoding.UTF8.GetString(array, 0, array.Length));
        }
    }
}
