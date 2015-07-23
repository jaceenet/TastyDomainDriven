namespace TastyDomainDriven.Bus
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
    /// This is a simplified representation of real application server. 
    /// In production it is wired to messaging and/or services infrastructure.</summary>
    public sealed class InProcessBus : IBus
    {
        private static readonly object onecommand = new object();
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(InProcessBus));

        internal readonly IDictionary<Type, List<IBaseProjection>> projections = new Dictionary<Type, List<IBaseProjection>>();
        internal readonly IDictionary<Type, List<IApplicationService>> services = new Dictionary<Type, List<IApplicationService>>();

        public void Dispatch(ICommand cmd)
        {
            if (cmd.Timestamp == DateTime.MinValue)
            {
                throw new ArgumentException("Command must have a timestamp: " + cmd.GetType());
            }

            if (this.services.ContainsKey(cmd.GetType()))
            {
                foreach (var handler in this.services[cmd.GetType()])
                {
                    lock (onecommand)
                    {
                        Logger.DebugFormat("Dispatching Command: {0}", cmd.ToString());
                        handler.Execute(cmd);                        
                    }
                }
            }
            else
            {
                throw new InvalidOperationException(string.Format("No service to execute command {0}", cmd));
            }
        }

        public void TryDispatch(ICommand cmd)
        {
            try
            {
                Logger.DebugFormat("Dispatching command {0}", cmd);
                this.Dispatch(cmd);
            }
            catch (Exception)
            {
                Logger.WarnFormat("Failed Command {0}", cmd);
            }
        }

        public void Replay(IEvent e)
        {
            if (this.projections.ContainsKey(e.GetType()))
            {                
                foreach (var projection in this.projections.First(x => x.Key == e.GetType()).Value)
                {
                    Logger.DebugFormat("Play: {0} on {1}" , e, projection);
                    projection.TryRecieve(e);
                }    
            }
            else
            {
                Logger.DebugFormat("No player for: {0}", e);
            }

            
        }
    }
}