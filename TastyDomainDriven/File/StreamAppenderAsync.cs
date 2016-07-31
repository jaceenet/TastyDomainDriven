using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;

namespace TastyDomainDriven.File
{
    public class StreamAppenderAsync
        : IAppendOnlyAsync
    {
        private readonly StreamAppenderOptions options;

        public StreamAppenderAsync(StreamAppenderOptions options)
        {
            this.options = options;
        }

        public async Task Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {
            var stream = this.options.AppendStream(streamName);

            if (stream.Position != stream.Length)
            {
                throw new Exception("Stream must be end of the stream in order to append.");
            }

            var record = new FileRecord(data, streamName, expectedStreamVersion + 1);
            await record.WriteContentToStreamAsync(stream);

            if (this.options.AfterAppend != null)
            {
                await this.options.AfterAppend(stream);
            }
        }

        public async Task<DataWithVersion[]> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            var stream = this.options.ReadStream(streamName);

            if (!stream.CanRead || stream.Length == 0)
            {
                return new DataWithVersion[0];
            }

            int version = 1;
            var list = new List<FileRecord>();
            var reader = new BinaryReader(stream);

            while (stream.Position < stream.Length)
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

            if (this.options.AfterRead != null)
            {
                await this.options.AfterRead(stream);
            }

            return list.Where(x => x.Name.Equals(streamName) && x.Version > afterVersion).Take(maxCount).Select(x => new DataWithVersion(x.Version, x.Bytes)).ToArray();
        }

        public async Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount)
        {
            var stream = this.options.ReadStreamAll();

            if (!stream.CanRead || stream.Length == 0)
            {
                if (this.options.AfterRead != null)
                {
                    await this.options.AfterRead(stream);
                }

                return new DataWithName[0];
            }

            int version = 1;
            var list = new List<DataWithName>();
            var reader = new BinaryReader(stream);

            while (stream.Position < stream.Length)
            {
                try
                {
                    var record = new FileRecord(version);
                    record.ReadContentFromStream(reader);
                    list.Add(new DataWithName(record.Name, record.Bytes));
                    version++;
                }
                catch (IOException)
                {
                }
            }

            if (this.options.AfterRead != null)
            {
                await this.options.AfterRead(stream);
            }

            return list.Skip((int)afterVersion).Take(maxCount).ToArray();
        }

        public class StreamAppenderOptions
        {
            public StreamAppenderOptions(Func<Stream> masterstream, Func<string, Stream> appendstream, Func<string, Stream> readstream, 
                Func<Stream, Task> afterAppend = null, Func<Stream, Task> afterRead = null)
            {
                this.AppendStream = appendstream;
                this.ReadStream = readstream;
                this.ReadStreamAll = masterstream;
                this.AfterAppend = afterAppend;
                this.AfterRead = afterRead;
            }

            public Func<Stream> ReadStreamAll { get; private set; }
            public Func<string, Stream> ReadStream { get; private set; }

            public Func<string, Stream> AppendStream { get; private set; }

            public Func<Stream, Task> AfterAppend { get; }
            public Func<Stream, Task> AfterRead { get; }

            public static StreamAppenderOptions UseSingleStream(Stream appendStream)
            {
                Func<string, Stream> readstream = s =>
                {
                    appendStream.Position = 0;
                    return appendStream;
                };

                Func<Stream> masterstream = () =>
                {
                    appendStream.Position = 0;
                    return appendStream;
                };

                Func<string, Stream> appendstream = s =>
                {
                    appendStream.Position = appendStream.Length;
                    return appendStream;
                };

                return new StreamAppenderOptions(masterstream, appendstream, readstream);
            }
        }
    }

    
}