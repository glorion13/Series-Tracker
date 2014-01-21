using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeriesTracker.Core
{
    public static class SemaphoreSlimExtensions
    {
        public static async Task<IDisposable> DisposableWaitAsync(this SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            return new Releaser(semaphore);
        }

        public class Releaser : IDisposable
        {
            private SemaphoreSlim semaphore;

            public Releaser(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            public void Dispose()
            {
                if (semaphore != null)
                {
                    semaphore.Release();
                    semaphore = null;
                }    
            }
        }
    }
}