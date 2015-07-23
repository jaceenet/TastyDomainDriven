namespace TastyDomainDriven.AzureAppender
{
    using System;
    using System.IO;

    /// <summary>
    /// Helps to write data to the underlying store, which accepts only
    /// pages with specific size
    /// </summary>
    public sealed class AppendOnlyStream : IDisposable
    {
        readonly int _pageSizeInBytes;
        readonly AppendWriterDelegate _writer;
        readonly int _maxByteCount;
        MemoryStream _pending;

        int _bytesWritten;
        int _bytesPending;
        int _fullPagesFlushed;

        public AppendOnlyStream(int pageSizeInBytes, AppendWriterDelegate writer, int maxByteCount)
        {
            this._writer = writer;
            this._maxByteCount = maxByteCount;
            this._pageSizeInBytes = pageSizeInBytes;
            this._pending = new MemoryStream();
        }

        public bool Fits(int byteCount)
        {
            return (this._bytesWritten + byteCount <= this._maxByteCount);
        }

        public void Write(byte[] buffer)
        {
            this._pending.Write(buffer, 0, buffer.Length);
            this._bytesWritten += buffer.Length;
            this._bytesPending += buffer.Length;
        }

        public void Flush()
        {
            if (this._bytesPending == 0)
                return;

            var size = (int)this._pending.Length;
            var padSize = (this._pageSizeInBytes - size % this._pageSizeInBytes) % this._pageSizeInBytes;

            using (var stream = new MemoryStream(size + padSize))
            {
                stream.Write(this._pending.ToArray(), 0, (int)this._pending.Length);
                
                if (padSize > 0)
                {
                    stream.Write(new byte[padSize], 0, padSize);
                }

                stream.Position = 0;
                this._writer(this._fullPagesFlushed * this._pageSizeInBytes, stream);
            }

            var fullPagesFlushed = size / this._pageSizeInBytes;

            if (fullPagesFlushed <= 0)
            {
                return;
            }

            // Copy remainder to the new stream and dispose the old stream
            var newStream = new MemoryStream();
            this._pending.Position = fullPagesFlushed * this._pageSizeInBytes;
            this._pending.CopyTo(newStream);
            this._pending.Dispose();
            this._pending = newStream;

            this._fullPagesFlushed += fullPagesFlushed;
            this._bytesPending = 0;
        }

        public void Dispose()
        {
            this.Flush();
            this._pending.Dispose();
        }
    }
}