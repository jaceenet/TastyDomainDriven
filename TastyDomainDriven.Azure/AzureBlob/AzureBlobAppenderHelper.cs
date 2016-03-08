using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using TastyDomainDriven.File;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public class AzureBlobAppenderHelper
    {
        private readonly string container;
        private readonly string pathprefix;
        private CloudBlobContainer client;
        private List<string> leases;

        public FolderIndexVersion index;

        public AzureBlobAppenderHelper(CloudStorageAccount storage, string container, string pathprefix = "")
        {
            this.container = container;
            this.pathprefix = pathprefix;

            this.client = storage.CreateCloudBlobClient().GetContainerReference(this.container);
            this.index = new FolderIndexVersion(storage, container, this.pathprefix);
        }

        public async Task WriteContent(string name, byte[] content, long expectedStreamVersion)
        {
            try
            {
                //Read the version file and keep a lock on it until write is done...          
                try
                {
                    await index.GetLeaseAndRead();
                }
                catch (StorageException ex)
                {
                    //"There is already a lease present."
                    if (ex.RequestInformation.HttpStatusCode == 409)
                    {
                        await index.ReadIndex();

                        if (index.Aggregates.ContainsKey(name) && index.Aggregates[name].Any() &&
                            expectedStreamVersion != index.Aggregates[name].Last().Version)
                        {
                            throw new AppendOnlyStoreConcurrencyException(expectedStreamVersion, index.Aggregates[name].Last().Version, name);
                        }
                    }
                }

                var last = index.ReadLast(name);

                if (last != null && last.Version != expectedStreamVersion)
                {
                    throw new AppendOnlyStoreConcurrencyException(expectedStreamVersion, last.Version, last.Name);
                }

                var blob = GetVersionStream(name, expectedStreamVersion);
                var blobcache = GetStreamCache(name);

                var master = this.GetMasterCache();

                using (var memstream = new MemoryStream())
                {
                    var record = new FileRecord(content, name, expectedStreamVersion + 1);
                    record.WriteContentToStream(memstream);

                    memstream.Position = 0;
                    await blob.UploadFromStreamAsync(memstream);
                    await index.AppendWrite(record, blob.Name);

                    //add to cachedstream
                    if (expectedStreamVersion == 0)
                    {
                        await blobcache.CreateOrReplaceAsync();
                    }

                    memstream.Position = 0;
                    await blobcache.AppendFromStreamAsync(memstream);

                    //add to cachedmaster
                    if (!index.OrderedIndex.Any())
                    {
                        await master.CreateOrReplaceAsync();
                    }

                    memstream.Position = 0;
                    await master.AppendFromStreamAsync(memstream);
                    await index.Release();
                }

            }
            finally
            {
                await index.Release();
            }
        }

        public CloudAppendBlob GetMasterCache()
        {
            return this.client.GetAppendBlobReference($"{pathprefix}master.dat");
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

                await blob.DownloadToStreamAsync(s);

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

        public CloudBlockBlob GetVersionStream(string name, long expectedStreamVersion)
        {
            return this.client.GetBlockBlobReference($"{this.pathprefix}{name}/{expectedStreamVersion+1:00000000}_{DateTime.UtcNow:yyyy-MM-dd-hhmmss}_{name}.dat");
        }

        private CloudAppendBlob GetStreamCache(string name)
        {
            var blobName = $"{this.pathprefix}{name}/{name}_fullstream.dat";
            return this.client.GetAppendBlobReference(blobName);
        }
    }
}