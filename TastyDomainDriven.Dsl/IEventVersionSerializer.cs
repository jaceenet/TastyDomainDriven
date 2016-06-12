using System.IO;

namespace TastyDomainDriven.Dsl
{
    using System;

    public interface IEventVersionSerializer
    {
        int GetEventId { get; }
        Type EventType { get; }

        void Write(object @event, BinaryWriter writer);
        object Read(BinaryReader writer);
    }
}
