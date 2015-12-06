namespace ClickerHeroesTrackerWebsite.Unity
{
    using Microsoft.Owin;
    using Microsoft.Practices.Unity;
    using System.Threading.Tasks;

    public sealed class UnityOwinMiddleware<T> : OwinMiddleware where T : OwinMiddleware
    {
        private readonly IUnityContainer container;

        public UnityOwinMiddleware(OwinMiddleware next, IUnityContainer container)
            : base(next)
        {
            this.container = container;
        }

        public override Task Invoke(IOwinContext context)
        {
            return this.container.Resolve<T>(new ParameterOverride("next", this.Next)).Invoke(context);
        }
    }
}