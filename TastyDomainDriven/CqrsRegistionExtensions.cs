namespace TastyDomainDriven
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TastyDomainDriven.Bus;

	public static class CqrsRegistionExtensions
    {
        public static InProcessBus AddView<T>(this InProcessBus app, Func<T> resolveView) where T : IBaseProjection
        {
            var view = resolveView();
            foreach (Type consume in view.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IConsumes<>)))
            {
                var key = consume.GenericTypeArguments[0];

                if (!app.projections.ContainsKey(key))
                {
                    app.projections.Add(key, new List<IBaseProjection>());
                }

                app.projections[key].Add(view);
            }            
            return app;
        }        

        public static InProcessBus AddService<TService>(this InProcessBus bus, Func<IApplicationService> resolveView) where TService : IApplicationService
        {
            var view = resolveView();
            foreach (Type cmd in view.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAcceptCommand<>)))
            {
                var key = cmd.GenericTypeArguments[0];
                if (!bus.services.ContainsKey(key))
                {
                    bus.services.Add(key, new List<IApplicationService>());
                }

                bus.services[key].Add(view);
            }
            
            return bus;
        }

        //public static InProcessBus Inject(this InProcessBus bus, Func<Autofac.ILifetimeScope> scope)
        //{
        //    foreach (var VARIABLE in scope.)
        //    {
                
        //    }
        //}
    }
}