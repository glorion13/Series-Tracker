using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace SeriesTracker
{
    public class Series : ReactiveObject
    {
        private string title;
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                this.RaiseAndSetIfChanged(s => s.Title, ref title, value);
            }
        }

        private string image;
        public string Image
        {
            get
            {
                return image;
            }

            set
            {
                this.RaiseAndSetIfChanged(s => s.Image, ref image, value);
            }
        }
    }
}
