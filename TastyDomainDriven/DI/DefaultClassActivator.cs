using System;

namespace TastyDomainDriven.DI
{
    public sealed class DefaultClassActivator : IClassActivator
    {
        public T CreateInstance<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}