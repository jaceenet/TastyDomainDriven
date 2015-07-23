namespace TastyDomainDriven
{
    using System;

    public abstract class ServiceFactory
    {
        public abstract IAcceptCommand<T> GetService<T>() where T : ICommand;
    }
}