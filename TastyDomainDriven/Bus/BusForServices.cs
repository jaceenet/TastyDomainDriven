using System;
using System.Collections.Generic;
using System.Linq;

namespace TastyDomainDriven.Bus
{
    /// <summary>
    /// Bus register for a cached implementation of IAcceptCommand.
    /// Use the Register in the constructor to add services
    /// </summary>
    public  abstract class BusForServices : IBus
    {
        public void Dispatch(ICommand cmd)
        {
            DispatchCommand((dynamic)cmd);
        }

        public void DispatchCommand<T>(T command) where T : ICommand
        {
            if (!this.registrations.ContainsKey(typeof(T)))
            {
                throw new Exception("Command not supported " + typeof(T) + ", please register a service!");
            }

            IAcceptCommand<T> service = (IAcceptCommand<T>)this.registrations[typeof(T)];
            service.When(command);
        }

        private readonly Dictionary<Type, object> registrations = new Dictionary<Type, object>();

        protected void Register<T>(T service)
        {
            if (CachedImplementations<T>.implementation.Length == 0)
            {
                throw new Exception("No IAcceptCommand<> on class " + typeof(T));
            }

            foreach (var commands in CachedImplementations<T>.implementation)
            {
                this.registrations[commands] = service;
            }
        }

        private static class CachedImplementations<T>
        {            
            public static Type[] implementation = (from i in typeof(T).GetInterfaces()
                                                   where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAcceptCommand<>)
                                                   select i.GetGenericArguments()[0]).ToArray();
        }
    }
}