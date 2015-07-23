namespace TastyDomainDriven
{
    public sealed class DataWithName
    {
        public readonly string Name;
        public readonly byte[] Data;

        public DataWithName(string name, byte[] data)
        {
            this.Name = name;
            this.Data = data;
        }
    }
}