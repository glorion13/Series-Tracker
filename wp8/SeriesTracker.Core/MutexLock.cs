using System;
using System.Threading;

namespace SeriesTracker.Core
{
    public class MutexLock : IDisposable
    {
        private Mutex mutex;
        public MutexLock(string name)
        {
            name = name.Replace('\\', '_');
            mutex = new Mutex(false, name);
            mutex.WaitOne();
        }

        public void Dispose()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex = null;
            }
        }
    }
}