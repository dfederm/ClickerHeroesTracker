namespace ClickerHeroesTrackerWebsite.Instrumentation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Owin;

    public sealed class MeasureLatencyMiddleware : OwinMiddleware
    {
        private readonly ICounterProvider counterProvider;

        public MeasureLatencyMiddleware(OwinMiddleware next, ICounterProvider counterProvider)
            : base(next)
        {
            this.counterProvider = counterProvider;
        }

        public async override Task Invoke(IOwinContext context)
        {
            using (this.counterProvider.Measure(Counter.Total))
            using (this.counterProvider.Measure(Counter.Internal))
            {
                await this.Next.Invoke(context);
            }
        }
    }
}