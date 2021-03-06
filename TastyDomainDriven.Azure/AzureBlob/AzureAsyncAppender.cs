﻿using System;
using System.Linq;
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
        private readonly AzureBlobAppenderOptions options;
        private readonly CloudBlobContainer client;
        
        public AzureAsyncAppender(string connection, string container, AzureBlobAppenderOptions options = null)
        {
            if (string.IsNullOrEmpty(connection))
            {
                throw new ArgumentNullException(nameof(connection));
            }

            this.storage = CloudStorageAccount.Parse(connection);
            this.container = container;
            this.options = options;
            this.client = this.storage.CreateCloudBlobClient().GetContainerReference(container);
        }

        public AzureAsyncAppender(CloudStorageAccount storage, string container, AzureBlobAppenderOptions options = null)
        {
            this.storage = storage;
            this.container = container;
            this.options = options;
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
            var index = new FolderIndexVersion(storage, container, this.options.NamingPolicy.GetIndexPath());
            await index.Create();
            await new AzureBlobAppenderHelper(storage, container, options).Prerequisites();
        }

        private int[] retrypolicy = new int[] {1000,2000,4000,8000};
        
        public async Task Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            int retry = 0;

            try
            {
                var azurehelper = new AzureBlobAppenderHelper(this.storage, this.container, this.options);
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
                var azurehelper = new AzureBlobAppenderHelper(this.storage, this.container, this.options);
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
                var azurehelper = new AzureBlobAppenderHelper(this.storage, this.container, this.options);
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