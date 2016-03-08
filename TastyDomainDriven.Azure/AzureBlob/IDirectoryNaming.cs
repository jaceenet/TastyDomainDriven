namespace TastyDomainDriven.Azure.AzureBlob
{
    public interface IDirectoryNaming
    {
        string GetPath(string name);
    }
}