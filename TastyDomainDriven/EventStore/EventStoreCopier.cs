namespace TastyDomainDriven.EventStore
{
	public static class EventStoreExtensions
    {
        //public static void CopyAppender(this IAppendOnlyStore appenderFrom, IAppendOnlyStore appenderTo)
        //{
        //    if (appenderFrom == null)
        //    {
        //        throw new ArgumentNullException("appenderFrom");
        //    }

        //    if (appenderTo == null)
        //    {
        //        throw new ArgumentNullException("appenderTo");
        //    }

        //    var es = new EventStore(appenderFrom);
        //    var events = es.ReplayAll().Events.ToArray();

        //    var versions = new Dictionary<IIdentity, int>();
        //    var stack = new List<IEvent>();
        //    var dest = new EventStore(appenderTo);

        //    Action savestack = () =>
        //    {

        //        if (!versions.ContainsKey(stack[0].AggregateId))
        //        {
        //            versions[stack[0].AggregateId] = 0;
        //        }

        //        dest.AppendToStream(stack[0].AggregateId, versions[stack[0].AggregateId], stack);
        //        versions[stack[0].AggregateId]++;
        //        stack.Clear();
        //    };

        //    if (events.Any())
        //    {
        //        IIdentity lastid = null;

        //        foreach (var e in events)
        //        {
        //            if (stack.Any() && !lastid.Equals(e.AggregateId))
        //            {
        //                savestack();
        //            }

        //            stack.Add(e);
        //            lastid = e.AggregateId;
        //        }
        //    }

        //    savestack();
        //}
    }
}