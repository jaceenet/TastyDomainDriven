using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using TastyDomainDriven.File;

namespace TastyDomainDriven.Azure.AzureBlob
{
    public class AzureBlobAppender : IAppendOnlyStore
    {
        private readonly CloudStorageAccount storage;
        private readonly string container;
        private readonly string pathprefix;
        private AzureBlobAppenderHelper helper;

        public AzureBlobAppender(CloudStorageAccount storage, string container, string pathprefix = "events")
        {
            this.storage = storage;
            this.container = container;
            this.pathprefix = pathprefix;

            this.helper = new AzureBlobAppenderHelper(storage, container, pathprefix);
        }

        public void Dispose()
        {
            this.helper.DropAllLeases();            
        }

        public void Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            this.helper.WriteContent(streamName, data, expectedStreamVersion);

            this.helper.WriteContent("global_stream", data, expectedStreamVersion);
        }

        public IEnumerable<DataWithVersion> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            //read file folder, fast and good

            throw new System.NotImplementedException();
        }

        public IEnumerable<DataWithName> ReadRecords(long afterVersion, int maxCount)
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {            
        }
    }

    public class AzureBlobAppenderHelper
    {
        private readonly CloudStorageAccount storage;
        private readonly string container;
        private readonly string pathprefix;
        private CloudBlobContainer client;
        private List<string> leases;

        public AzureBlobAppenderHelper(CloudStorageAccount storage, string container, string pathprefix = "events")
        {
            this.storage = storage;
            this.container = container;
            this.pathprefix = pathprefix;

            this.client = storage.CreateCloudBlobClient().GetContainerReference(this.container);
        }

        public void WriteContent(string name, byte[] content, long expectedStreamVersion)
        {
            //Read the version file and keep a lock on it until write is done...
            using (var index = new FolderIndexVersion(this.client.GetBlobReferenceFromServer(GetStreamFile(name))))
            {
                var blob = this.client.GetBlobReferenceFromServer(index.ReadLast(name).path);

                using (var memstream = new MemoryStream())
                {
                    if (blob.Exists())
                    {
                        blob.DownloadToStream(memstream);
                        var bak = this.client.GetBlobReferenceFromServer(GetStreamFile(blob.Name + ".bak"));

                        //make a backup
                        memstream.Position = 0;
                        bak.UploadFromStream(memstream);                        
                    }

                    var record = new FileRecord(content, name, expectedStreamVersion);
                    record.WriteContentToStream(memstream);

                    memstream.Position = 0;
                    blob.UploadFromStream(memstream);

                    index.WriteLine(name, blob.Name, (int)expectedStreamVersion);
                }                
            }
        }

        

        private string GetStreamFile(string name, string filename = "version.txt")
        {
            return string.Concat(pathprefix, "/", name, filename);
        }

        public void DropAllLeases()
        {
            foreach (var lease in this.leases)
            {
                this.client.ReleaseLease(new AccessCondition() {LeaseId = lease});
            }
        }
    }
}