using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using TastyDomainDriven.AsyncImpl;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public class AzureAsyncAppender : IAppendOnlyAsync
    {
        private readonly CloudStorageAccount storage;
        private readonly string container;
        private readonly string prefix;
        private readonly CloudBlobContainer client;
        
        public AzureAsyncAppender(string connection, string container, string prefix) : this(CloudStorageAccount.Parse(connection), container, prefix)
        {            
        }

        public AzureAsyncAppender(CloudStorageAccount storage, string container, string prefix)
        {
            this.storage = storage;
            this.container = container;
            this.prefix = !string.IsNullOrEmpty(prefix)&&!prefix.EndsWith("/") ? prefix + "/" : prefix;
            this.client = this.storage.CreateCloudBlobClient().GetContainerReference(container);
        }

        /// <summary>
        /// Initialize the appender on the Azure Container. This is a once only. When this code is run, you don't need to do it anymore.
        /// Create the container and the index file.
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {
            await client.CreateIfNotExistsAsync();
            var index = new FolderIndexVersion(storage, container, prefix);
            await index.Create();
        }

        private int[] retrypolicy = new int[] {1000,2000,4000,8000};
        
        public async Task Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            int retry = 0;

            try
            {
                var azurehelper = new AzureBlobAppenderHelper(this.storage, this.container, this.prefix);
                await azurehelper.WriteContent(streamName, data, expectedStreamVersion);
            }
            catch (ConcurrentIndexException)
            {
                while (retry < retrypolicy.Length)
                {
                    await Task.Delay(retrypolicy[retry]);
                    await this.Append(streamName, data, expectedStreamVersion);
                    retry++;
                }

                throw;
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                var extended = requestInformation.ExtendedErrorInformation;
                throw;
            }
        }

        public async Task<DataWithVersion[]> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            try
            {
                var azurehelper = new AzureBlobAppenderHelper(this.storage, this.container, this.prefix);
                var items = await azurehelper.ReadStreamCache(streamName);
                return items.Where(x => x.Version > afterVersion && x.Version <= maxCount).Select(x => new DataWithVersion(x.Version, x.Bytes)).ToArray();
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                var extended = requestInformation.ExtendedErrorInformation;
                throw;
            }            
        }

        public async Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount)
        {
            try
            {
                var azurehelper = new AzureBlobAppenderHelper(this.storage, this.container, this.prefix);
                var items = await azurehelper.ReadMasterCache();
                return items.Where(x => x.Version > afterVersion && x.Version <= maxCount).Select(x => new DataWithName(x.Name, x.Bytes)).ToArray();
            }
            catch (StorageException ex)
            {
                if (ex.HResult == 404)
                {
                    return new DataWithName[0];
                }

                var requestInformation = ex.RequestInformation;
                var extended = requestInformation.ExtendedErrorInformation;
                throw;
            }
            
        }
    }
}