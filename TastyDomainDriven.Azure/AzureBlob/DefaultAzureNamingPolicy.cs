using System;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public sealed class DefaultAzureNamingPolicy : IAzureAppenderBlobNamingPolicy
    {
        private readonly string prefix;

        public DefaultAzureNamingPolicy() : this(String.Empty)
        {
        }

        public DefaultAzureNamingPolicy(string prefix)
        {
            this.prefix = prefix.Trim('/');

            if (prefix.Length > 0)
            {
                this.prefix = prefix + "/";
            }
        }
        public string GetMasterPath()
        {
            return $"{prefix}master.dat";
        }

        public string GetStreamPath(string streamid)
        {
            return $"{prefix}{streamid}/{streamid}_fullstream.dat";
        }

        public string GetIndexPath()
        {
            return $"{prefix}index.txt";
        }

        public string GetIndexPath(string streamid)
        {
            return $"{prefix}{streamid}/index.txt";
        }
    }
}