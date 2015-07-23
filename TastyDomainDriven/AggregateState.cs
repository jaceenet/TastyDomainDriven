namespace TastyDomainDriven
{
    using System;

    using Microsoft.CSharp.RuntimeBinder;

    public abstract class AggregateState
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(AggregateState));

        internal void Mutate(IEvent e)
        {
            Logger.DebugFormat("Apply state {0} (mutate)", e);
            // .NET magic to call one of the 'When' handlers with 
            // matching signature 
            try
            {
               ((dynamic)this).When((dynamic)e);
            }
            catch (RuntimeBinderException)
            {
                throw new Exception(string.Format("The State {0} should implement IStateEvent<{1}> even if no state change is required", this.GetType().FullName, e.GetType().Name));
            }            
        }
    }
}