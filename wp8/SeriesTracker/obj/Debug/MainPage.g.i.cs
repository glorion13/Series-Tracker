﻿#pragma checksum "D:\Alexandros\Documents\GitHub\st8\semiupgraded\Series-Tracker\wp7\SeriesTracker\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "E2DC688CBC61A8313D695A90C4967BED"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace SeriesTracker {
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Media.Animation.Storyboard AnimatePopup;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.VisualStateGroup LoadingSubs;
        
        internal System.Windows.VisualState Loading;
        
        internal System.Windows.VisualState NotLoading;
        
        internal System.Windows.VisualStateGroup Search;
        
        internal System.Windows.VisualState Searching;
        
        internal System.Windows.VisualState NotSearching;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar subsProgressBar;
        
        internal Microsoft.Phone.Controls.PerformanceProgressBar searchProgressBar;
        
        internal System.Windows.Controls.Primitives.Popup popup;
        
        internal System.Windows.Controls.Grid grid;
        
        internal System.Windows.Controls.Canvas Vrstva_1;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/SeriesTracker;component/MainPage.xaml", System.UriKind.Relative));
            this.AnimatePopup = ((System.Windows.Media.Animation.Storyboard)(this.FindName("AnimatePopup")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.LoadingSubs = ((System.Windows.VisualStateGroup)(this.FindName("LoadingSubs")));
            this.Loading = ((System.Windows.VisualState)(this.FindName("Loading")));
            this.NotLoading = ((System.Windows.VisualState)(this.FindName("NotLoading")));
            this.Search = ((System.Windows.VisualStateGroup)(this.FindName("Search")));
            this.Searching = ((System.Windows.VisualState)(this.FindName("Searching")));
            this.NotSearching = ((System.Windows.VisualState)(this.FindName("NotSearching")));
            this.subsProgressBar = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("subsProgressBar")));
            this.searchProgressBar = ((Microsoft.Phone.Controls.PerformanceProgressBar)(this.FindName("searchProgressBar")));
            this.popup = ((System.Windows.Controls.Primitives.Popup)(this.FindName("popup")));
            this.grid = ((System.Windows.Controls.Grid)(this.FindName("grid")));
            this.Vrstva_1 = ((System.Windows.Controls.Canvas)(this.FindName("Vrstva_1")));
        }
    }
}

