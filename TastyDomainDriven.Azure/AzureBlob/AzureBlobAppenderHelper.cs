using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using TastyDomainDriven.File;

namespace TastyDomainDriven.Azure.AzureBlob
{
    internal class AzureBlobAppenderHelper
    {
        private readonly CloudStorageAccount storage;
        private readonly string container;
        private CloudBlobContainer client;
        private List<string> leases;

        public FileIndexLock index;
        private AzureBlobAppenderOptions options;
        private static readonly AzureBlobAppenderOptions DefaultOptions = new AzureBlobAppenderOptions() { NamingPolicy = new DefaultAzureNamingPolicy() };
        private MasterIndex masterindex;

        public AzureBlobAppenderHelper(CloudStorageAccount storage, string container, AzureBlobAppenderOptions options)
        {
            this.storage = storage;
            this.container = container;
            this.options = options ?? DefaultOptions;
            this.client = storage.CreateCloudBlobClient().GetContainerReference(this.container);
            this.masterindex = new MasterIndex(new FileIndexLock(storage, container, options.NamingPolicy.GetIndexPath()));
        }

        public async Task WriteContent(string name, byte[] content, long expectedStreamVersion, int retry = 0)
        {
            this.index = new FileIndexLock(storage, container, options.NamingPolicy.GetIndexPath(name));
            
            try
            {
                //if (expectedStreamVersion == 0)
                //{
                //    await this.index.CreateIfNotExist();
                //    await this.masterindex.CreateIfNotExist();
                //}

                var canWrite = await index.GetLeaseAndRead(expectedStreamVersion != 0);

                if (!canWrite)
                {
                    if (retry < options.RetryPolicy.Length)
                    {
                        await Task.Delay(retry);
                        await this.WriteContent(name, content, expectedStreamVersion, retry++);
                        return;
                    }

                    await this.index.CreateIfNotExist();
                    await this.index.ReadIndex();
                    throw new AppendOnlyTimeoutException(expectedStreamVersion, index.OrderedIndex.Any() ? this.index.ReadLast().Version : 0, name);
                }

                await this.index.ReadIndex();
                var last = index.ReadLast();

                if ((last == null && expectedStreamVersion != 0) || (last != null && expectedStreamVersion != last.Version) || (last != null && last.Version != expectedStreamVersion ))
                {
                    throw new AppendOnlyStoreConcurrencyException(expectedStreamVersion, last?.Version ?? 0, name);
                }

                var blobcache = GetStreamCache(name);
                                
                if (last == null)
                {
                    //no file, create the stream to append to...
                    await blobcache.CreateOrReplaceAsync();
                }

                var master = this.GetMasterCache();

                using (var memstream = new MemoryStream())
                {
                    var record = new FileRecord(content, name, expectedStreamVersion + 1);
                    record.WriteContentToStream(memstream);

                    var mybytes = memstream.ToArray();

                    var appendmaster = Task.Run(async () =>
                    {
                        using (var masterstream = new MemoryStream(memstream.ToArray()))
                        {
                            await master.AppendBlockAsync(masterstream);
                        }
                    });

                    var up1 = blobcache.AppendFromByteArrayAsync(mybytes, 0, mybytes.Length);
                    var up3 = index.AppendWrite(record, blobcache.Name);
                    var up4 = masterindex.AppendWrite(record, blobcache.Name);

                    await index.EnsureLease();
                    if (!options.DisableMasterIndex)
                    {
                        await Task.WhenAll(up1, appendmaster, up3);
                    }
                    else
                    {
                        await Task.WhenAll(up4, up1, appendmaster, up3);
                    }
                }
            }
            finally
            {
                await index.Release();
            }
        }

        internal async Task Prerequisites()
        {
            if (!this.GetMasterCache().Exists())
            {
                await this.GetMasterCache().CreateOrReplaceAsync();
            }

            await this.masterindex.CreateIfNotExist();
        }

        public CloudAppendBlob GetMasterCache()
        {
            return this.client.GetAppendBlobReference(this.options.NamingPolicy.GetMasterPath());
        }

        public async Task<List<FileRecord>> ReadMasterCache()
        {
            var blob = this.GetMasterCache();
            return await ReadStreamToRecords(blob);
        }

        private static async Task<List<FileRecord>> ReadStreamToRecords(CloudAppendBlob blob)
        {
            List<FileRecord> records = new List<FileRecord>();

            using (var s = new MemoryStream())
            {
                if (!await blob.ExistsAsync())
                {
                    return records;
                }

                await blob.DownloadRangeToStreamAsync(s, 0, blob.Properties.Length);

                s.Position = 0;
                var reader = new BinaryReader(s);

                var record = new FileRecord();
                while (record.ReadContentFromStream(reader))
                {
                    records.Add(record);
                    record = new FileRecord();
                }
            }

            return records;
        }

        public async Task<List<FileRecord>> ReadStreamCache(string streamName)
        {
            var blob = this.GetStreamCache(streamName);

            if (await blob.ExistsAsync())
            {
                return await ReadStreamToRecords(blob);
            }

            return new List<FileRecord>();
        }

        //public CloudBlockBlob GetVersionStream(string name, long expectedStreamVersion)
        //{
        //    return this.client.GetBlockBlobReference(options.NamingPolicy.get);
        // $"{name}/{expectedStreamVersion + 1:00000000}_{DateTime.UtcNow:yyyy-MM-dd-hhmmss}_{name}.dat"
        //}

        internal CloudAppendBlob GetStreamCache(string name)
        {
            var blobName = options.NamingPolicy.GetStreamPath(name);
            return this.client.GetAppendBlobReference(blobName);
        }
    }
}