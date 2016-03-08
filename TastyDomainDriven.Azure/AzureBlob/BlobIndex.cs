using System;
using System.Linq;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public class BlobIndex
    {
        public string Path;
        public string Name;
        public int Version;
        public string Hash;

        public static string ToHashHex(byte[] hash)
        {
            return string.Join(String.Empty, hash.Select(x => x.ToString("x2")));
        }        
    }
}