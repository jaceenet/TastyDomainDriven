using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public class FolderIndexVersion : IDisposable
    {
        private readonly ICloudBlob blob;
        private string lease;

        public FolderIndexVersion(ICloudBlob blob, TimeSpan? leaseTime = null)
        {
            this.blob = blob;
            lease = blob.AcquireLease(leaseTime ?? TimeSpan.FromSeconds(30), null);
        }

        public void WriteLine(string name, string filename, int version)
        {
            using (var mem = new MemoryStream())
            {
                this.blob.DownloadToStream(mem);
                var writer = new StreamWriter(mem);
                writer.WriteLine(string.Concat(version, "\t", name, "\t", filename));
            }
        }

        public IEnumerable<BlobIndex> ReadIndex()
        {
            using (var mem = new MemoryStream())
            {
                this.blob.DownloadToStream(mem);
                mem.Position = 0;
                var reader = new StreamReader(mem);

                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split('\t');
                    yield return new BlobIndex() { name = line[0], path = line[2] , version = int.Parse(line[1]) };
                }
            }
        }

        public void Dispose()
        {
            blob.ReleaseLease(new AccessCondition() { LeaseId = lease });
        }

        public BlobIndex ReadLast(string name)
        {
            return this.ReadIndex().LastOrDefault();
        }
    }
}