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
    public class FolderIndexVersion
    {
        private readonly CloudBlobContainer client;
        private readonly string file;
        private readonly TimeSpan? leaseTime;
        private string lease = null;
        private bool gotlease = false;
        private ICloudBlob blob;

        /// <summary>
        /// FileIndex for keeping order of events...
        /// </summary>
        /// <param name="storage">Connection</param>
        /// <param name="container">Azure blob container</param>
        /// <param name="directoryNaming">directoryNaming if you wanna write to a sub path</param>
        /// <param name="leaseTime">You can acquire leases for 15s up to 60s or you can acquire a lease for an infinite time period.</param>
        public FolderIndexVersion(CloudStorageAccount storage, string container, IDirectoryNaming directoryNaming, TimeSpan? leaseTime = null)
        {
            this.leaseTime = leaseTime ?? TimeSpan.FromSeconds(30); //You can acquire leases for 15s up to 60s or you can acquire a lease for an infinite time period.
            this.blob = storage.CreateCloudBlobClient().GetContainerReference(container).GetAppendBlobReference(directoryNaming.GetPath("index.txt"));
        }

        public async Task GetLeaseAndRead()
        {
            if (gotlease)
            {
                return;
            }

            await GetLease();
        }

        private async Task GetLease()
        {
            this.lease = await this.blob.AcquireLeaseAsync(leaseTime, null);
            gotlease = true;
            await this.ReadIndex();
        }

        public async Task AppendWrite(FileRecord record, string filename)
        {
            await GetLeaseAndRead();

            using (var mem = new MemoryStream())
            {
                await this.blob.DownloadToStreamAsync(mem);
                var writer = new StreamWriter(mem);
                var hashhex = string.Join(String.Empty, record.Hash.Select(x => x.ToString("x2")));
                writer.WriteLine(String.Join("\t", record.Name, record.Version, hashhex, filename));
                writer.Flush();
                mem.Position = 0;
                await this.blob.UploadFromStreamAsync(mem, new AccessCondition() { LeaseId = this.lease }, new BlobRequestOptions(), null);
            }
        }

        public Dictionary<string, BlobIndex> Hashes = new Dictionary<string, BlobIndex>();        
        public Dictionary<string, List<BlobIndex>> Aggregates = new Dictionary<string, List<BlobIndex>>();
        public List<BlobIndex> OrderedIndex = new List<BlobIndex>();
        private int LeaseTry = 0;

        public async Task ReadIndex()
        {            
            using (var mem = new MemoryStream())
            {
                await this.blob.DownloadToStreamAsync(mem);
                this.Hashes.Clear();
                this.OrderedIndex.Clear();

                mem.Position = 0;
                var reader = new StreamReader(mem);

                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('\t');
                    var index = new BlobIndex() { Name = line[0], Version = int.Parse(line[1]), Hash = line[2], Path = line[2] };

                    if (!Aggregates.ContainsKey(index.Name))
                    {
                        Aggregates[index.Name] = new List<BlobIndex>();
                    }

                    Hashes[index.Hash] = index;
                    Aggregates[index.Name].Add(index);
                    OrderedIndex.Add(index);
                }
            }
        }

        public BlobIndex ReadLast(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (this.Aggregates.ContainsKey(name))
            {
                return Aggregates[name].Last();
            }

            return null;
        }

        public async Task Release()
        {
            if (blob != null && lease != null)
            {
                await blob.ReleaseLeaseAsync(new AccessCondition() { LeaseId = lease });
                this.lease = null;
            }
        }

        public async Task Create()
        {
            if (!await this.blob.ExistsAsync())
            {
                await blob.UploadFromByteArrayAsync(new byte[0], 0, 0);
            }
        }
    }
}