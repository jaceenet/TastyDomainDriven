namespace TastyDomainDriven
{
    using System;

    public interface IBaseProjection
    {
        void TryRecieve<TEvent>(TEvent e);
    }

    [Obsolete]
    public class BaseProjection : IBaseProjection
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(BaseProjection));

        public void TryRecieve<TEvent>(TEvent e)
        {
            Logger.Debug("Replaying " + e);
            // .NET magic to call one of the 'When' handlers with 
            // matching signature 
            ((dynamic)this).Consume((dynamic)e);
        }
    }
}