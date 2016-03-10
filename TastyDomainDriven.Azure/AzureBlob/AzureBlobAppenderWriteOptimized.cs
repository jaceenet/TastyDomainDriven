using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.File;

namespace TastyDomainDriven.Azure.AzureBlob
{
    /// <summary>
    /// AzureBlobAppender optimized for converting existing event streams. Append is done when calling WriteAll() not before;
    /// </summary>
    public class AzureBlobAppenderWriteOnce : IAppendOnlyAsync
    {
        private readonly CloudStorageAccount storage;
        private readonly string container;
        private readonly AzureBlobAppenderOptions options;
        private List<FileRecord> records = new List<FileRecord>();
        private List<string> names = new List<string>();

        public AzureBlobAppenderWriteOnce(string connection, string container, AzureBlobAppenderOptions options = null) 
            : this(CloudStorageAccount.Parse(connection), container, options)
        {            
        }
        public AzureBlobAppenderWriteOnce(CloudStorageAccount storage, string container, AzureBlobAppenderOptions options = null)
        {
            this.storage = storage;
            this.container = container;
            this.options = options ?? new AzureBlobAppenderOptions();
        }

        public Task Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            if (!names.Contains(streamName))
            {
                names.Add(streamName);
            }

            records.Add(new FileRecord(data, streamName, expectedStreamVersion+1));
            return Task.FromResult(0);
        }

        public Task<DataWithVersion[]> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            return Task.FromResult(records.Where(x => x.Name.Equals(streamName) && x.Version > afterVersion && x.Version < afterVersion + maxCount).Select(x => new DataWithVersion(x.Version, x.Bytes)).ToArray());
        }

        public Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount)
        {
            return Task.FromResult(records.Skip((int)afterVersion).Take(maxCount).Select(x => new DataWithName(x.Name, x.Bytes)).ToArray());
        }

        public async Task WriteAll()
        {
            var helper = new AzureBlobAppenderHelper(storage, container, options);

            var master = helper.GetMasterCache();

            using (var masterindex = new MemoryStream())
            {
                var writer = new StreamWriter(masterindex);
                using (var stream = new MemoryStream())
                {
                    foreach (var record in records)
                    {
                        record.WriteContentToStream(stream);
                        var hashhex = string.Join(String.Empty, record.Hash.Select(x => x.ToString("x2")));
                        writer.WriteLine(String.Join("\t", record.Name, record.Version, hashhex, this.options.NamingPolicy.GetStreamPath(record.Name)));
                    }

                    await writer.FlushAsync();

                    stream.Position = 0;
                    masterindex.Position = 0;

                    await master.CreateOrReplaceAsync();
                    await master.AppendBlockAsync(stream);

                    var indexblob = storage.CreateCloudBlobClient().GetContainerReference(container).GetAppendBlobReference(this.options.NamingPolicy.GetIndexPath());
                    await indexblob.CreateOrReplaceAsync();
                    await indexblob.AppendFromStreamAsync(masterindex);

                    var streams = GetNamedStreams(helper);
                    await Task.WhenAll(streams);
                }
            }
        }

        private IEnumerable<Task> GetNamedStreams(AzureBlobAppenderHelper helper)
        {
            foreach (var name in names)
            {
                yield return Task.Run(async () =>
                {
                    using (var indexstream = new MemoryStream())
                    {
                        var writer = new StreamWriter(indexstream);
                        
                        using (var s = new MemoryStream())
                        {
                            var blobstream = helper.GetStreamCache(name);

                            foreach (var record in this.records.Where(x => x.Name.Equals(name)).OrderBy(x => x.Version))
                            {
                                record.WriteContentToStream(s);
                                var hashhex = string.Join(String.Empty, record.Hash.Select(x => x.ToString("x2")));
                                writer.WriteLine(String.Join("\t", record.Name, record.Version, hashhex, blobstream.Name));                                
                            }

                            await writer.FlushAsync();

                            s.Position = 0;
                            indexstream.Position = 0;

                            var indexblob = storage.CreateCloudBlobClient().GetContainerReference(container).GetAppendBlobReference(this.options.NamingPolicy.GetIndexPath(name));
                            await indexblob.CreateOrReplaceAsync();
                            await indexblob.AppendFromStreamAsync(indexstream);
                            await blobstream.CreateOrReplaceAsync();
                            await blobstream.AppendBlockAsync(s);
                        }
                    }
                });
            }
        }

        public Task Initialize()
        {
            var appender = new AzureAsyncAppender(this.storage, this.container, this.options);
            return appender.Initialize();
        }
    }
}