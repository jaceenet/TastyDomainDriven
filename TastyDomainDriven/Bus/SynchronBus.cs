namespace TastyDomainDriven.Bus
{
	using System;

	public class SynchronBus : IBus
    {
        private readonly ServiceFactory serviceFactory;

        public SynchronBus(ServiceFactory serviceFactory)
        {            
            this.serviceFactory = serviceFactory;
        }

        public virtual void Dispatch<T>(T cmd) where T : ICommand
        {
            if (cmd.Timestamp == DateTime.MinValue)
            {
                throw new ArgumentException("Command must have a timestamp: " + cmd.GetType());
            }

            this.serviceFactory.GetService<T>().When(cmd);
        }

        public virtual void Dispatch(ICommand cmd)
        {
            ((dynamic)this).Dispatch((dynamic)cmd);
        }
    }
}