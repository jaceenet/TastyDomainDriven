using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Azure.AzureBlob;

namespace TastyDomainDriven.File
{
    public class FileAppendOnlyStoreAsync : IAppendOnlyAsync
    {
        private readonly Options options;

        public class Options
        {
            public Options(string path)
            {                
                this.RootPath = path;
                this.MasterStreamName = "master";
                this.FileStream = s => Path.Combine(path, this.MasterStreamName+".dat");
            }

            public Options(string path, string masterStream)
            {
                this.RootPath = path;
                this.MasterStreamName = masterStream;
                this.FileStream = s => Path.Combine(path, this.MasterStreamName + ".dat");
            }

            public Func<string, string> FileStream { get; set; }
            
            public string RootPath { get; set; }
            public string MasterStreamName { get; set; }
        }

        public FileAppendOnlyStoreAsync(Options options)
        {
            this.options = options;
        }

        public FileAppendOnlyStoreAsync(string path)
        {
            this.options = GetDefaultOptions(path);
        }

        private Options GetDefaultOptions(string path)
        {
            return new Options(path);
        }

        /// <summary>
        /// Extract the masterstream to mimic the folder structure the AzureAsyncAppender
        /// </summary>
        /// <returns></returns>
        public async Task ExtractMasterStream(IAppenderNamingPolicy namingPolicy, long afterVersion, int maxCount, bool writeIndexFile = true)
        {
            var events = await this.ReadRecords(afterVersion, maxCount);
            var versions = new Dictionary<string, int>();

            var masterfile = Path.Combine(namingPolicy.GetIndexPath("master"));

            foreach (var @event in events)
            {
                var filename = new FileInfo(namingPolicy.GetStreamPath(@event.Name));
                var record = new FileRecord(@event.Data, @event.Name, versions.ContainsKey(@event.Name) ? versions[@event.Name]++:versions[@event.Name]=1);

                if (writeIndexFile)
                {
                    using (var fs = System.IO.File.OpenWrite(filename.FullName))
                    {
                        record.WriteContentToStream(fs);
                    }

                    var indexfile = namingPolicy.GetIndexPath(@event.Name);

                    System.IO.File.AppendAllText(masterfile, String.Join("\t", record.Name, record.Version, record.Hash, filename));
                    System.IO.File.AppendAllText(indexfile, String.Join("\t", record.Name, record.Version, record.Hash, filename));                    
                }                
            }
        }

        public async Task Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            var path = this.options.FileStream(options.MasterStreamName);
            var streampath = this.options.FileStream(streamName);
            
            using (Stream s = System.IO.File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Write))
            {
                var record = new FileRecord(data, streamName, expectedStreamVersion + 1);
                await record.WriteContentToStreamAsync(s);
            }

            if (!path.Equals(streampath))
            {
                using (Stream s = System.IO.File.Open(streampath, FileMode.Append, FileAccess.Write, FileShare.Write))
                {
                    var record = new FileRecord(data, streamName, expectedStreamVersion + 1);
                    await record.WriteContentToStreamAsync(s);
                }
            }
        }

        public Task<DataWithVersion[]> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            var filename = this.options.FileStream(streamName);

            var records = this.Read(filename);
            if (System.IO.File.Exists(filename))
            {
                records = this.Read(filename);
            }
            else
            {
                records = new List<FileRecord>();
            }

            var list = new List<DataWithVersion>();
            foreach (var record in records)
            {
                if (record.Name.Equals(streamName))
                {
                    list.Add(new DataWithVersion(record.Version, record.Bytes));
                }
            }

            return Task.FromResult(list.Where(x => x.Version > afterVersion).Take(maxCount).ToArray());
        }

        public Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount)
        {
            var filename = this.options.FileStream(options.MasterStreamName);

            List<FileRecord> records;
            if (System.IO.File.Exists(filename))
            {
                records = this.Read(filename);
            }
            else
            {
                records= new List<FileRecord>();
            }
           

            var list = new List<DataWithName>();
            foreach (var record in records)
            {
                list.Add(new DataWithName(record.Name, record.Bytes));
            }

            return Task.FromResult(list.Skip((int)afterVersion).Take(maxCount).ToArray());
        }

        public List<FileRecord> Read(string filename)
        {
            var list = new List<FileRecord>();
            int version = 1;

            if (!System.IO.File.Exists(filename))
            {
                return list;
            }

            using (var fs = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = new BinaryReader(fs))
                {
                    while (fs.Position < fs.Length)
                    {
                        try
                        {
                            var record = new FileRecord(version);
                            record.ReadContentFromStream(reader);
                            list.Add(record);
                            version++;
                        }
                        catch (IOException)
                        {
                        }
                    }
                }
            }

            return list;
        }

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~FileAppendOnlyStoreAsync()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // free other managed objects that implement
                // IDisposable only

            }

            // release any unmanaged objects
            // set the object references to null

            _disposed = true;
        }
    }


}