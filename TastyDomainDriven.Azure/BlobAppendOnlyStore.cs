using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TastyDomainDriven.Azure
{
    /// <summary>
    /// <para>This is embedded append-only store implemented on top of cloud page blobs 
    /// (for persisting data with one HTTP call).</para>
    /// <para>This store ensures that only one writer exists and writes to a given event store</para>
    /// <para>This code is frozen to match IDDD book. For latest practices see Lokad.CQRS Project</para>
    /// </summary>
    public sealed class BlobAppendOnlyStore : IAppendOnlyStore
    {
        private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(BlobAppendOnlyStore));
        readonly CloudBlobContainer _container;

        // Caches
        readonly ConcurrentDictionary<string, DataWithVersion[]> _items = new ConcurrentDictionary<string, DataWithVersion[]>();
        DataWithName[] _all = new DataWithName[0];

        /// <summary>
        /// Used to synchronize access between multiple threads within one process
        /// </summary>
        readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();


        bool _closed;

        /// <summary>
        /// Currently open file
        /// </summary>
        AppendOnlyStream _currentWriter;

        /// <summary>
        /// Renewable Blob lease, used to prohibit multiple writers outside a given process
        /// </summary>
        BlobLease _lock;

        private readonly Guid leaseId = Guid.NewGuid();

        public BlobAppendOnlyStore(CloudBlobContainer container)
        {
            this._container = container;
        }

        public void Dispose()
        {
            if (!this._closed)
                this.Close();
        }

        public void InitializeWriter()
        {
            CreateIfNotExists(this._container, TimeSpan.FromSeconds(60));
            // grab the ownership
            
            this._lock = new BlobLease(_container);

            this.LoadCaches();
        }
        public void InitializeReader()
        {
            CreateIfNotExists(this._container, TimeSpan.FromSeconds(60));
            this.LoadCaches();
        }

        public void Append(string streamName, byte[] data, long expectedStreamVersion = -1)
        {

            // should be locked
            try
            {
                this._cacheLock.EnterWriteLock();

                var list = this._items.GetOrAdd(streamName, s => new DataWithVersion[0]);
                if (expectedStreamVersion >= 0)
                {
                    if (list.Length != expectedStreamVersion)
                    {
                        throw new AppendOnlyStoreConcurrencyException(expectedStreamVersion, list.Length, streamName);
                    }
                }

                this.EnsureWriterExists(this._all.Length);
                long commit = list.Length + 1;

                this.Persist(streamName, data, commit);
                this.AddToCaches(streamName, data, commit);
            }
            catch
            {
                this.Close();
                throw;
            }
            finally
            {
                this._cacheLock.ExitWriteLock();
            }
        }

        public IEnumerable<DataWithVersion> ReadRecords(string streamName, long afterVersion, int maxCount)
        {
            // no lock is needed, since we are polling immutable object.
            DataWithVersion[] list;
            return this._items.TryGetValue(streamName, out list) ? list : Enumerable.Empty<DataWithVersion>();
        }

        public IEnumerable<DataWithName> ReadRecords(long afterVersion, int maxCount)
        {
            // collection is immutable so we don't care about locks
            return this._all.Skip((int)afterVersion).Take(maxCount);
        }

        public void Close()
        {
            using (this._lock)
            {
                this._closed = true;

                if (this._currentWriter == null)
                {
                    return;
                }

                var tmp = this._currentWriter;
                this._currentWriter = null;
                tmp.Dispose();
            }
        }

        async IAsyncEnumerable<Record> EnumerateHistory()
        {
            BlobContinuationToken token = null;
            List<IListBlobItem> items = new List<IListBlobItem>();
            BlobResultSegment files;
            do
            {
                files = await _container.ListBlobsSegmentedAsync(token);
                items.AddRange(files.Results);
            } while (files.ContinuationToken != null && !string.IsNullOrWhiteSpace(files.ContinuationToken.NextMarker));

            // cleanup old pending files
            // load indexes
            // build and save missing indexes
            var datFiles = items
                .OrderBy(s => s.Uri.ToString())
                .OfType<CloudPageBlob>();

            foreach (var fileInfo in datFiles)
            {
                var memStream = new MemoryStream();
                await fileInfo.DownloadToStreamAsync(memStream);
                memStream.Position = 0;

                using (var stream = memStream)
                using (var reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    Record result;
                    while (TryReadRecord(reader, out result))
                    {
                        yield return result;
                    }
                }
            }
        }

        static bool TryReadRecord(BinaryReader binary, out Record result)
        {
            result = null;

            try
            {
                var version = binary.ReadInt64();
                var name = binary.ReadString();
                var len = binary.ReadInt32();
                var bytes = binary.ReadBytes(len);

                var sha1 = binary.ReadBytes(20);
                if (sha1.All(s => s == 0))
                    throw new InvalidOperationException("definitely failed (zero hash)");

                byte[] actualSha1;
                PersistRecord(name, bytes, version, out actualSha1);

                if (!sha1.SequenceEqual(actualSha1))
                {
                    throw new InvalidOperationException("hash mismatch");
                }

                result = new Record(bytes, name, version);
                return true;
            }
            catch (EndOfStreamException)
            {
                // we are done
                return false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                // Auto-clean?
                return false;
            }
        }

        async Task LoadCaches()
        {
            try
            {
                this._cacheLock.EnterWriteLock();

                await foreach (var record in this.EnumerateHistory())
                {
                    this.AddToCaches(record.Name, record.Bytes, record.Version);
                }
            }
            finally
            {
                this._cacheLock.ExitWriteLock();
            }
        }

        void AddToCaches(string key, byte[] buffer, long commit)
        {
            var record = new DataWithVersion(commit, buffer);
            this._all = AddToNewArray(this._all, new DataWithName(key, buffer));
            this._items.AddOrUpdate(key, s => new[] { record }, (s, records) => AddToNewArray(records, record));
        }

        static T[] AddToNewArray<T>(T[] source, T item)
        {
            var copy = new T[source.Length + 1];
            Array.Copy(source, copy, source.Length);
            copy[source.Length] = item;
            return copy;
        }

        void Persist(string key, byte[] buffer, long commit)
        {
            byte[] hash;
            var bytes = PersistRecord(key, buffer, commit, out hash);

            if (!this._currentWriter.Fits(bytes.Length + hash.Length))
            {
                this.CloseWriter();
                this.EnsureWriterExists(this._all.Length);
            }

            this._currentWriter.Write(bytes);
            this._currentWriter.Write(hash);
            this._currentWriter.Flush();
        }

        static byte[] PersistRecord(string key, byte[] buffer, long commit, out byte[] hash)
        {
            using (var sha1 = new SHA1Managed())
            using (var memory = new MemoryStream())
            {
                using (var crypto = new CryptoStream(memory, sha1, CryptoStreamMode.Write))
                using (var binary = new BinaryWriter(crypto, Encoding.UTF8))
                {
                    // version, ksz, vsz, key, value, sha1
                    binary.Write(commit);
                    binary.Write(key);
                    binary.Write(buffer.Length);
                    binary.Write(buffer);
                }

                hash = sha1.Hash;
                return memory.ToArray();
            }
        }

        void CloseWriter()
        {
            this._currentWriter.Dispose();
            this._currentWriter = null;
        }

        async Task EnsureWriterExists(long version)
        {
            if (this._lock.Exception != null)
                throw new InvalidOperationException("Can not renew lease", this._lock.Exception);

            if (this._currentWriter != null)
                return;

            var fileName = string.Format("{0:00000000}-{1:yyyy-MM-dd-HHmm}.dat", version, DateTime.UtcNow);
            var blob = this._container.GetPageBlobReference(fileName);
            await blob.CreateAsync(1024 * 512);

            async void Writer(int i, Stream bytes) => await blob.WritePagesAsync(bytes, i, null);

            this._currentWriter = new AppendOnlyStream(512, Writer, 1024 * 512);
        }

        static async Task CreateIfNotExists(CloudBlobContainer container, TimeSpan timeout)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                try
                {
                    await container.CreateIfNotExistsAsync();
                    return;
                }
                catch (StorageException e)
                {
                    Logger.Info("Got StorageException: ", e);

                    if (e.RequestInformation.ExtendedErrorInformation != null)
                    {
                        throw;
                    }


                    if (e.RequestInformation.HttpStatusCode != (int)HttpStatusCode.Conflict)
                    {
                        throw;
                    }
                    
                }
                Thread.Sleep(500);
            }

            throw new TimeoutException(string.Format("Can not create container within {0} seconds.", timeout.TotalSeconds));
        }

        sealed class Record
        {
            public readonly byte[] Bytes;
            public readonly string Name;
            public readonly long Version;

            public Record(byte[] bytes, string name, long version)
            {
                this.Bytes = bytes;
                this.Name = name;
                this.Version = version;
            }
        }
    }
}
