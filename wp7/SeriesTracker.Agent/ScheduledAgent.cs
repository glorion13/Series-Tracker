using System;
using System.Diagnostics;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using SeriesTracker.Core;

namespace SeriesTracker.Agent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected async override void OnInvoke(ScheduledTask task)
        {
            //nasty hack... let's pretend this thread is the Dispatcher, haha :-P
            DispatcherHelper.Initialize();

            var repository = new TvDbSeriesRepository(new SeriesStorageManager(), new TvDb(new ConnectivityService()));
            await repository.GetSubscribedAsync(false);


            // If debugging is enabled, launch the agent again in one minute.
            #if DEBUG_AGENT
              ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
            #endif

            // Call NotifyComplete to let the system know the agent is done working.
            NotifyComplete();
        }
    }
}