namespace TastyDomainDriven
{
    public sealed class DataWithVersion
    {
        public readonly long Version;
        public readonly byte[] Data;

        public DataWithVersion(long version, byte[] data)
        {
            this.Version = version;
            this.Data = data;
        }
    }
}