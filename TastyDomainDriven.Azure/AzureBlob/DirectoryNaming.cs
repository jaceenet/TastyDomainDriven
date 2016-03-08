using System.Linq;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public sealed class DirectoryNaming : IDirectoryNaming
    {
        private readonly IDirectoryNaming[] names;

        public DirectoryNaming(params IDirectoryNaming[] names)
        {
            this.names = names;
        }

        public string GetPath(string name)
        {
            return names.Aggregate(name, (current, naming) => naming.GetPath(current));
        }
    }
}