using System;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public sealed class AzureBlobAppenderOptions
    {
        public AzureBlobAppenderOptions()
        {
            this.NamingPolicy = new DefaultAzureNamingPolicy();
            this.DisableMasterIndex = false;
            this.RetryPolicy = new TimeSpan[]
            {
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(100),
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMilliseconds(200),
                TimeSpan.FromMilliseconds(400),
                TimeSpan.FromMilliseconds(1000),
                TimeSpan.FromMilliseconds(1000)
            };
        }

        public TimeSpan[] RetryPolicy { get; set; }

        public IAppenderNamingPolicy NamingPolicy { get; set; }

        public bool DisableMasterIndex { get; set; }
    }
}