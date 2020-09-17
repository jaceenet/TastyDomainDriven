using System;
using System.Collections.Generic;
using System.Text;

namespace TastyDomainDriven.DI
{
    public static class ClassActivatorService
    {
        /// <summary>
        /// Get class activator.
        /// </summary>
        public static IClassActivator Instance
        {
            get;
            private set;
        }

        static ClassActivatorService()
        {
            Instance = new DefaultClassActivator();
        }

        public static void InitializeActivator(IClassActivator activator)
        {
            if (Instance.GetType() != typeof(DefaultClassActivator))
            {
                throw new InvalidOperationException("Activator already been initialized");
            }

            Instance = activator;
        }
    }
}
