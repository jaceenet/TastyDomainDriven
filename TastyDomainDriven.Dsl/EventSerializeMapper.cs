using System;
using System.Collections.Generic;

namespace TastyDomainDriven.Dsl
{
    public sealed class EventSerializeMapper
    {
        private readonly Dictionary<int, IEventVersionSerializer> items_id = new Dictionary<int, IEventVersionSerializer>();
        private readonly Dictionary<Type, IEventVersionSerializer> items_type = new Dictionary<Type, IEventVersionSerializer>();

        public EventSerializeMapper(params IEventVersionSerializer[] serializers)
        {
            foreach (var serializer in serializers)
            {
                try
                {
                    items_id.Add(serializer.GetEventId, serializer);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(string.Format("Serializer with EventId {0} is already added. The duplicate {1} could not be added. Check {2}", serializer.GetEventId, serializer.GetType().FullName, items_id[serializer.GetEventId].GetType()));
                }

                try
                {
                    items_type.Add(serializer.EventType, serializer);
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException(string.Format("Serializer with EventType {0} is already added. The duplicate {1} could not be added. Check {2}", serializer.GetEventId, serializer.GetType().FullName, items_type[serializer.EventType].GetType()));
                }
            }

        }

        public IEventVersionSerializer GetSerializer(object e)
        {
            Type key = e.GetType();
            return items_type[key];
        }

        public IEventVersionSerializer GetSerializer(int id)
        {
            return items_id[id];
        }
    }
}