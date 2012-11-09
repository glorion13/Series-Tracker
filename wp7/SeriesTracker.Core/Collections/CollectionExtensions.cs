using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SeriesTracker
{
    public static class CollectionExtensions
    {
        public static void RemoveAllThatMatch<T>(this Collection<T> collection, Func<T, bool> predicate)
        {
            var matching = collection.Where(predicate).ToList();
            foreach (var match in matching)
            {
                collection.Remove(match);
            }
        }

        public static void AddAll<T>(this Collection<T> collection, IEnumerable<T> items)
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
