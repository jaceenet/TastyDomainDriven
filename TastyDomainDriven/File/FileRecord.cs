using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TastyDomainDriven.File
{
    public sealed class FileRecord
    {
        public byte[] Bytes { get; private set; }
        public string Name { get; private set; }
        public long Version { get; private set; }

        public byte[] Hash { get; private set; }

        public FileRecord()
        {
        }

        public FileRecord(byte[] bytes, string name, long version)
        {
            this.Bytes = bytes;
            this.Name = name;
            this.Version = version;
        }

        public void WriteContentToStream(Stream stream)
        {
            using (var sha1 = new SHA1Managed())
            {
                // version, ksz, vsz, key, value, sha1
                using (var memory = new MemoryStream())
                {
                    using (var crypto = new CryptoStream(memory, sha1, CryptoStreamMode.Write))
                    using (var binary = new BinaryWriter(crypto, Encoding.UTF8))
                    {
                        binary.Write(this.Version);
                        binary.Write(this.Name);
                        binary.Write(this.Bytes.Length);
                        binary.Write(this.Bytes);
                    }
                    var bytes = memory.ToArray();

                    stream.Write(bytes, 0, bytes.Length);
                }
                this.Hash = sha1.Hash;
                stream.Write(sha1.Hash, 0, sha1.Hash.Length);
            }
        }

        public bool ReadContentFromStream(BinaryReader binary)
        {
            try
            {
                var version = binary.ReadInt64();
                var name = binary.ReadString();
                var len = binary.ReadInt32();
                var bytes = binary.ReadBytes(len);
                var sha = binary.ReadBytes(20); // SHA1. TODO: verify data

                if (sha.All(s => s == 0))
                {
                    throw new InvalidOperationException("definitely failed");
                }

                this.Name = name;
                this.Bytes = bytes;
                this.Hash = sha;
                this.Version = version;
                return true;
            }
            catch (EndOfStreamException)
            {
                // we are done
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}