using Microsoft.Phone.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeriesTracker
{
    public class ConnectivityService
    {
        public event EventHandler InternetDown;
        public event EventHandler InternetUp;

        public void ReportHealth(bool success)
        {
            if (success != IsUp)
            {
                IsUp = success;

                if (IsUp && InternetUp != null)
                    InternetUp.Invoke(this, EventArgs.Empty);
                else if (!IsUp && InternetDown != null)
                    InternetDown.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsUp
        {
            get;
            protected set;
        }

        public ConnectivityService()
        {
            IsUp = NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None;
        }
    }
}
