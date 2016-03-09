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
    public sealed class MasterIndex
    {
        private readonly FileIndexLock index;
        
        public MasterIndex(FileIndexLock index)
        {
            this.Aggregates = new Dictionary<string, List<BlobIndex>>();
            this.index = index;
        }

        public async Task ReadIndex()
        {
            await index.ReadIndex();

            foreach (var info in index.OrderedIndex)
            {
                if (!this.Aggregates.ContainsKey(info.Name))
                {
                    this.Aggregates[info.Name] = new List<BlobIndex>();
                }

                this.Aggregates[info.Name].Add(info);
            }
        }

        public Dictionary<string, List<BlobIndex>> Aggregates { get; private set; }

        public async Task AppendWrite(FileRecord record, string filename)
        {
            await this.index.AppendWriteNoLease(record, filename);
        }

        public Task CreateIfNotExist()
        {
            return this.index.CreateIfNotExist();
        }
    }
}