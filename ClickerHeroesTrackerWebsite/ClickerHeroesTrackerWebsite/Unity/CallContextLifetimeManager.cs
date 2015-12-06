namespace ClickerHeroesTrackerWebsite.Unity
{
    using Microsoft.Practices.Unity;
    using System;
    using System.Runtime.Remoting.Messaging;

    public class CallContextLifetimeManager: LifetimeManager
    {
        private readonly string key = Guid.NewGuid().ToString();

        public override object GetValue()
        {
            return CallContext.GetData(this.key);
        }

        public override void SetValue(object newValue)
        {
            CallContext.SetData(this.key, newValue);
        }

        public override void RemoveValue()
        {
            CallContext.FreeNamedDataSlot(this.key);
        }
    }
}