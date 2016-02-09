using System;
using TastyDomainDriven.Serialization;

namespace TastyDomainDriven
{
    public interface IEventSerializer
    {
        byte[] SerializeEvent(IEvent[] e);

        IEvent[] DeserializeEvent(byte[] data);
    }

    //public class CustomEventSerializer : IEventSerializer
    //{
    //    private readonly Func<IEvent, object> serialize;
    //    private readonly Func<object, IEvent> deserialize;

    //    public CustomEventSerializer(Func<IEvent, object> serialize, Func<object, IEvent> deserialize)
    //    {
    //        this.serialize = serialize;
    //        this.deserialize = deserialize;
    //    }

    //    public byte[] SerializeEvent(IEvent[] e)
    //    {
    //        return serialize(e);
    //    }

    //    public IEvent[] DeserializeEvent(byte[] data)
    //    {
    //        return deserialize(data)
    //    }
    //}
}