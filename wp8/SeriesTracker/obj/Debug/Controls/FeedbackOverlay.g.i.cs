﻿#pragma checksum "D:\Alexandros\Documents\GitHub\Series-Tracker\wp7\SeriesTracker\Controls\FeedbackOverlay.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "C3F9B381C51DF536DE4E896910474342"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace SeriesTracker.Controls {
    
    
    public partial class FeedbackOverlay : System.Windows.Controls.UserControl {
        
        internal System.Windows.Media.Animation.Storyboard showContent;
        
        internal System.Windows.Media.Animation.Storyboard hideContent;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Border content;
        
        internal System.Windows.Controls.TextBlock title;
        
        internal System.Windows.Controls.TextBlock message;
        
        internal System.Windows.Controls.Button yesButton;
        
        internal System.Windows.Controls.Button noButton;
        
        internal System.Windows.Media.PlaneProjection xProjection;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/SeriesTracker;component/Controls/FeedbackOverlay.xaml", System.UriKind.Relative));
            this.showContent = ((System.Windows.Media.Animation.Storyboard)(this.FindName("showContent")));
            this.hideContent = ((System.Windows.Media.Animation.Storyboard)(this.FindName("hideContent")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.content = ((System.Windows.Controls.Border)(this.FindName("content")));
            this.title = ((System.Windows.Controls.TextBlock)(this.FindName("title")));
            this.message = ((System.Windows.Controls.TextBlock)(this.FindName("message")));
            this.yesButton = ((System.Windows.Controls.Button)(this.FindName("yesButton")));
            this.noButton = ((System.Windows.Controls.Button)(this.FindName("noButton")));
            this.xProjection = ((System.Windows.Media.PlaneProjection)(this.FindName("xProjection")));
        }
    }
}

