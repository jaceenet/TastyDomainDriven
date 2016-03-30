using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;

namespace TastyDomainDriven.File
{
    public class FileAppendOnlyStoreAsync : IAppendOnlyAsync
    {
        private Options options;

        public class Options
        {
            public Func<string, string> FileStream { get; set; }
        }

        public FileAppendOnlyStoreAsync(string path)
        {
            this.options = GetDefaultOptions(path);
        }

        private Options GetDefaultOptions(string path)
        {
            return new Options() { FileStream = s => Path.Combine(path, s + ".dat") };
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
            throw new System.NotImplementedException();
        }

        public Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount)
        {
            throw new System.NotImplementedException();
        }
    }

    
}