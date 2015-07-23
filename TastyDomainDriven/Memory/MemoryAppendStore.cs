namespace TastyDomainDriven.Memory
{
	using System.Collections.Generic;
	using System.Linq;

	public class MemoryAppendStore : IAppendOnlyStore
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(MemoryAppendStore));
        private class DataWithVersionAndName
        {
            public string Name;
            public long Version;
            public byte[] Data;
        }

        private Dictionary<string, List<DataWithVersionAndName>> name_events = new Dictionary<string, List<DataWithVersionAndName>>();

        private List<DataWithVersionAndName> all_events = new List<DataWithVersionAndName>();

        public void Dispose()
        {
        }

        private object locker = new object();

        public void Append(string name, byte[] data, long expectedVersion = -1)
        {
            lock (this.locker)
            {
                var version = this.name_events.ContainsKey(name) ? this.name_events[name].Last().Version : 0;

                if (version != expectedVersion)
                {
                    throw new AppendOnlyStoreConcurrencyException(version, expectedVersion, name);
                }

                long next = 0;

                if (!this.name_events.ContainsKey(name))
                {
                    this.name_events[name] = new List<DataWithVersionAndName>();
                }
                else
                {
                    next = this.name_events[name].Last().Version;
                }

                next++;

                var stored = new DataWithVersionAndName() { Data = data, Name = name, Version = next };
                this.all_events.Add(stored);
                this.name_events[name].Add(stored);

                Logger.Debug("Saved name: " + name);
            }
        }

        public IEnumerable<DataWithVersion> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            if (this.name_events.ContainsKey(streamName))
            {
                foreach (var b in this.name_events[streamName])
                {
                    yield return new DataWithVersion(b.Version, b.Data);
                }
            }
        }

        public IEnumerable<DataWithName> ReadRecords(long afterVersion, int maxCount)
        {
            return this.all_events.Skip((int)afterVersion).Take(maxCount).Select(x => new DataWithName(x.Name, x.Data));
        }

        public void Close()
        {
        }
    }
}