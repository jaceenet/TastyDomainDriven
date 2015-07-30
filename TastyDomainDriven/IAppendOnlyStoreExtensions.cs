using System.Linq;

namespace TastyDomainDriven
{
    using System;
    using System.Collections.Generic;

    public static class AppendOnlyStoreExtensions
    {
        public static void CopyAppender(this IAppendOnlyStore source, IAppendOnlyStore to)
        {
            CopyAppender(source, to, int.MaxValue);
        }

		/// <summary>
		/// Copy the content of one appender to another. The bytestreams will be identical.
		/// Use AddEvents(appender, events) to compress the bytestreams.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="to"></param>
		/// <param name="version"></param>
        public static void CopyAppender(this IAppendOnlyStore source, IAppendOnlyStore to, int version)
        {
            var serializer = new BinaryFormatterSerializer();

            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (to == null)
            {
                throw new ArgumentNullException("to");
            }

            var versions = new Dictionary<string, int>();

            foreach (var record in source.ReadRecords(0, version))
            {
                if (!versions.ContainsKey(record.Name))
                {
                    versions[record.Name] = 0;
                }

                var events = serializer.DeserializeEvent(record.Data);
                to.Append(record.Name, record.Data, versions[record.Name]);
                versions[record.Name]++;
            }
        }

		/// <summary>
		/// Add events to an existing appender, events in order will be serialized as a single bytestream.
		/// </summary>
		/// <param name="dest">appender</param>
		/// <param name="source">events to add</param>
		/// <paramref name="serializer">serializer, default binary</paramref>
		public static void AddEvents(this IAppendOnlyStore dest, IEnumerable<IEvent> source, IEventSerializer serializer = null)
		{
			if (dest == null)
			{
				throw new ArgumentNullException("dest");
			}
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			var es = new EventStore.EventStore(dest, serializer ?? new BinaryFormatterSerializer());

			var events = source as IEvent[] ?? source.ToArray();

			var versions = new Dictionary<IIdentity, int>();

			List<IEvent> stack = new List<IEvent>();


			Action savestack = () =>
			{
				if (!versions.ContainsKey(stack[0].AggregateId))
				{
					versions[stack[0].AggregateId] = 0;
				}

				es.AppendToStream(stack[0].AggregateId, versions[stack[0].AggregateId], stack);
				versions[stack[0].AggregateId]++;
				stack.Clear();
			};

			if (events.Any())
			{
				IIdentity lastid = null;

				foreach (var e in events)
				{
					if (stack.Any() && !lastid.Equals(e.AggregateId))
					{
						savestack();
					}
					//else
					{
						stack.Add(e);
						lastid = e.AggregateId;
					}
				}
			}

			savestack();
		}
	}
}