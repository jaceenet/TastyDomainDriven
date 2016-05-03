using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;

namespace TastyDomainDriven.File
{
    public class FileAppendOnlyStoreAsync : IAppendOnlyAsync
    {
        private readonly Options options;

        public class Options
        {
            public Options(string path)
            {
                this.ReadFromMasterStreamOnly = true;
                this.RootPath = path;
                this.MasterStreamFileName = "master";
                this.FileStream = s => Path.Combine(path, s + ".dat");
            }

            public Func<string, string> FileStream { get; set; }

            public bool ReadFromMasterStreamOnly { get; set; }

            public string RootPath { get; set; }
            public string MasterStreamFileName { get; set; }
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

        public async Task Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            using (Stream s = System.IO.File.Open(this.options.FileStream(streamName), FileMode.Append, FileAccess.ReadWrite))
            {
                var record = new FileRecord(data, streamName, expectedStreamVersion + 1);
                await record.WriteContentToStreamAsync(s);
            }
        }

        public Task<DataWithVersion[]> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            var filename = this.options.FileStream(streamName);

            var records = this.Read(filename);

            var list = new List<DataWithVersion>();
            foreach (var record in records)
            {
                list.Add(new DataWithVersion(record.Version, record.Bytes));
            }

            return Task.FromResult(list.Where(x => x.Version > afterVersion).Take(maxCount).ToArray());
        }

        public Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount)
        {
            var filename = this.options.FileStream(options.MasterStreamFileName);

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

            using (var fs = System.IO.File.Open(filename, FileMode.Open, FileAccess.Read))
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
    }


}