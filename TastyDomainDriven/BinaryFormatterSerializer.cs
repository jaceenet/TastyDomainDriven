namespace TastyDomainDriven
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    internal class BinaryFormatterSerializer : IEventSerializer
    {
        readonly BinaryFormatter formatter = new BinaryFormatter();

        public byte[] SerializeEvent(IEvent[] e)
        {
            using (var mem = new MemoryStream())
            {
                this.formatter.Serialize(mem, e);
                return mem.ToArray();
            }
        }

        public IEvent[] DeserializeEvent(byte[] data)
        {
            using (var mem = new MemoryStream(data))
            {
                return (IEvent[])this.formatter.Deserialize(mem);
            }
        }
    }
}