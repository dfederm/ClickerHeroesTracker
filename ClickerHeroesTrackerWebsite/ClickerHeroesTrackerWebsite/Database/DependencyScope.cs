namespace ClickerHeroesTrackerWebsite.Database
{
    using Models;
    using System;

    public sealed class DependencyScope : IDisposable
    {
        private bool isDisposed = false;

        public DependencyScope()
        {
            LatencyCounter.InternalLatency.Stop();
            LatencyCounter.DependencyLatency.Start();
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                LatencyCounter.DependencyLatency.Stop();
                LatencyCounter.InternalLatency.Start();

                this.isDisposed = true;
            }
        }
    }
}