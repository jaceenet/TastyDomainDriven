namespace TastyDomainDriven.Projections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class EventRegister
	{
		private readonly Type genericType;

		public EventRegister(Type genericType)
		{
			this.genericType = genericType;
		}

		protected readonly Dictionary<Type, List<Action<IEvent>>> EventSubscriptions = new Dictionary<Type, List<Action<IEvent>>>();
		
		public void Subscribe<T>(T source)
		{
			foreach (var type in source.GetImplementedGenericType(this.genericType))
			{
				if (!this.EventSubscriptions.ContainsKey(type))
				{
					this.EventSubscriptions.Add(type, new List<Action<IEvent>>());
				}

				this.EventSubscriptions[type].Add(x => ((dynamic)source).Consume((dynamic)x));
			}
		}

		public void Consume<T>(T e) where T : IEvent
		{
			var eventType = e.GetType();

			if (this.EventSubscriptions.ContainsKey(eventType))
			{
				foreach (var action in this.EventSubscriptions[eventType])
				{
					action(e);
				}
			}
		}

		public void ConsumeAsync<T>(T e) where T : IEvent
		{
			var eventType = e.GetType();

			if (this.EventSubscriptions.ContainsKey(eventType))
			{
				foreach (var action in this.EventSubscriptions[eventType])
				{
					action(e);
				}
			}
		}

		public IEnumerable<IEvent> GetEventsForProjection(object source, IEnumerable<IEvent> events)
		{
			var types = source.GetImplementedGenericType(this.genericType);
			return events.Where(x => types.Contains(events.GetType()));
		}

		/// <summary>
		/// Populate a single writer with all events of relevans (implementing interface defined in contructor)
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="events"></param>
		public void ConsumeOfTypes(object writer, IEnumerable<IEvent> events)
		{
			var register = new EventRegister(this.genericType);
			register.Subscribe(writer);
			foreach (var e in events)
			{
				register.Consume(e);
			}
		}


		public void ReplayAll(IEnumerable<IEvent> events)
		{
			foreach (var e in events)
			{
				this.Consume(e);
			}
		}
	}
}