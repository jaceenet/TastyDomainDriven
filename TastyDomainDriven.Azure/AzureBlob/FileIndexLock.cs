using System;
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
    public class FileIndexLock
    {
        private readonly CloudBlobContainer client;
        private readonly string file;
        private readonly TimeSpan leaseTime;
        private string lease = null;
        private bool gotlease = false;
        private CloudAppendBlob blob;
        private Stopwatch leaseTimer = new Stopwatch();

        public Dictionary<string, BlobIndex> Hashes = new Dictionary<string, BlobIndex>();
        public List<BlobIndex> OrderedIndex = new List<BlobIndex>();

        /// <summary>
        /// FileIndex for keeping order of events...
        /// </summary>
        /// <param name="storage">Connection</param>
        /// <param name="container">Azure blob container</param>
        /// <param name="filename"></param>
        /// <param name="leaseTime">You can acquire leases for 15s up to 60s or you can acquire a lease for an infinite time period.</param>
        public FileIndexLock(CloudStorageAccount storage, string container, string filename, TimeSpan? leaseTime = null)
        {
            if (leaseTime.HasValue && (leaseTime.Value.TotalSeconds < 15 || leaseTime.Value.TotalSeconds > 60))
            {
                throw new ArgumentException("Leasetime must be between 15-60seconds");
            }

            this.leaseTime = leaseTime ?? TimeSpan.FromSeconds(60); //You can acquire leases for 15s up to 60s or you can acquire a lease for an infinite time period.
            this.blob = storage.CreateCloudBlobClient().GetContainerReference(container).GetAppendBlobReference(filename);
        }

        public async Task<bool> GetLeaseAndRead(bool shouldexist = false)
        {
            if (gotlease)
            {
                return true;
            }

            return await GetLease(shouldexist);
        }

        private async Task<bool> GetLease(bool shouldexist = false)
        {
            try
            {
                if (!shouldexist)
                {
                    await this.CreateIfNotExist();
                }

                if (lease == null && !this.leaseTimer.IsRunning)
                {
                    this.lease = await this.blob.AcquireLeaseAsync(leaseTime, null);
                    this.leaseTimer.Restart();
                    return true;
                }
                else if (lease != null && leaseTime.Subtract(this.leaseTimer.Elapsed).TotalMilliseconds < 45000 )
                {
                    await this.blob.RenewLeaseAsync(new AccessCondition() { LeaseId = lease});
                    this.leaseTimer.Restart();
                    return true;
                }

                if (lease != null && this.leaseTimer.IsRunning && this.leaseTimer.Elapsed > this.leaseTime)
                {
                    return true;
                }                               
                                
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    if (shouldexist)
                    {
                        return await GetLease(false);
                    }

                    return false;
                }

                if (ex.RequestInformation.HttpStatusMessage == "There is already a lease present.")
                {
                    return false;
                }
            }
            gotlease = true;
            await this.ReadIndex();
            return true;
        }

        public async Task AppendWrite(FileRecord record, string filename)
        {
            await GetLeaseAndRead();
            await AppendWriteNoLease(record, filename);
        }

        public async Task AppendWriteNoLease(FileRecord record, string filename)
        {
            using (var mem = new MemoryStream())
            {               
                if (lease != null)
                {
                    mem.Write(existingdata, 0, existingdata.Length);

                    var writer = new StreamWriter(mem);
                    var hashhex = string.Join(String.Empty, record.Hash.Select(x => x.ToString("x2")));
                    writer.WriteLine(String.Join("\t", record.Name, record.Version, hashhex, filename));
                    writer.Flush();
                    mem.Position = 0;

                    await this.blob.UploadFromStreamAsync(mem, new AccessCondition() { LeaseId = this.lease }, new BlobRequestOptions(), null);
                }
                else
                {
                    var writer = new StreamWriter(mem);
                    var hashhex = string.Join(String.Empty, record.Hash.Select(x => x.ToString("x2")));
                    writer.WriteLine(String.Join("\t", record.Name, record.Version, hashhex, filename));
                    writer.Flush();
                    mem.Position = 0;

                    await this.blob.AppendBlockAsync(mem);
                }                
            }
        }

        private int LeaseTry = 0;

        private byte[] existingdata = new byte[0];

        public async Task ReadIndex()
        {
                        
            using (var mem = new MemoryStream())
            {
                await this.blob.DownloadToStreamAsync(mem);

                if (gotlease)
                {
                    this.existingdata = mem.ToArray();
                }

                this.Hashes.Clear();
                this.OrderedIndex.Clear();

                mem.Position = 0;
                var reader = new StreamReader(mem);

                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('\t');
                    var index = new BlobIndex() { Name = line[0], Version = int.Parse(line[1]), Hash = line[2], Path = line[2] };

                    Hashes[index.Hash] = index;
                    OrderedIndex.Add(index);
                }
            }
        }

        public BlobIndex ReadLast()
        {
            return OrderedIndex.LastOrDefault();
        }

        public async Task Release()
        {
            if (blob != null && lease != null)
            {
                await blob.ReleaseLeaseAsync(new AccessCondition() { LeaseId = lease });
                this.lease = null;
            }
        }

        public async Task CreateIfNotExist()
        {
            if (!await this.blob.ExistsAsync())
            {
                await blob.UploadFromByteArrayAsync(new byte[0], 0, 0);
            }
        }

        public Task EnsureLease()
        {
            return this.GetLease(true);
        }
    }
}