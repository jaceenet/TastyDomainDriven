namespace TastyDomainDriven.Projections
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public class EventRegisterAsync
	{
		private readonly Type genericType;

		public EventRegisterAsync(Type genericType)
		{
			this.genericType = genericType;
		}

		protected readonly Dictionary<Type, List<Func<IEvent, Task>>> EventSubscriptions = new Dictionary<Type, List<Func<IEvent, Task>>>();

		public void Subscribe<T>(T source)
		{
			foreach (var type in source.GetImplementedGenericType(this.genericType))
			{
				if (!this.EventSubscriptions.ContainsKey(type))
				{
					this.EventSubscriptions.Add(type, new List<Func<IEvent,Task>>());
				}

				this.EventSubscriptions[type].Add(x => ((dynamic)source).Consume((dynamic)x));
			}
		}

		public async Task Consume<T>(T e) where T : IEvent
		{
			var eventType = e.GetType();

			if (this.EventSubscriptions.ContainsKey(eventType))
			{
				foreach (var action in this.EventSubscriptions[eventType])
				{
					var t = action(e);
					await t;
				}
			}
		}
	}
}