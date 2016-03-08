namespace TastyDomainDriven.Azure.AzureBlob
{
    public class PrefixedDirectory : IDirectoryNaming
    {
        private readonly string prefix;

        public PrefixedDirectory(string prefix)
        {
            this.prefix = prefix.Trim('/');
        }

        public string GetPath(string name)
        {
            return string.Concat(prefix, "/", name);
        }
    }
}