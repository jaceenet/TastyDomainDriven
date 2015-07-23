namespace TastyDomainDriven.Memory
{
    //public class MemoryStorage : IEventStorage
    //{
    //    private readonly Action<IEvent> forwardTo;

    //    public MemoryStorage()
    //    {
            
    //    }

    //    public MemoryStorage(Action<IEvent> forwardTo)
    //    {
    //        this.forwardTo = forwardTo;
    //    }

    //    public int CurrentVersion = 0;

    //    private Dictionary<Guid, EventStream> streams = new Dictionary<Guid, EventStream>();

    //    public EventStream LoadEventStream(Guid id)
    //    {
    //        try
    //        {
    //            return this.streams[id];
    //        }
    //        catch (KeyNotFoundException)
    //        {
    //            this.streams.Add(id, new EventStream());
    //            return this.streams[id];
    //        }
    //    }

    //    public void AppendToStream(Guid id, long version, IList<IEvent> changes)
    //    {
    //        if (!this.streams.ContainsKey(id))
    //        {
    //            this.streams[id] = new EventStream();
    //        }

    //        foreach (var e in changes)
    //        {
    //            this.CurrentVersion++;
    //            e.Version = CurrentVersion;
    //            this.streams[id].Append(e, version);
    //        }

    //        if (forwardTo != null)
    //        {
    //            foreach (var e in changes)
    //            {
    //                forwardTo(e);
    //            }
    //        }
    //    }
 //   }
}