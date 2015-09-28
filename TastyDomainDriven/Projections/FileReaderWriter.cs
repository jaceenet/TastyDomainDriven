using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace TastyDomainDriven.Projections
{
	public class FileReaderWriter<T> : SerializableTableReaderWriter<T>
	{
		private readonly string filename;
		private static readonly BinaryFormatter Formatter = new BinaryFormatter();

		public FileReaderWriter(ITableReaderWriter<T> source, string filename) : base(source)
		{
			this.filename = filename;
		}

		protected override async Task Serialize(ITableReaderWriter<T> source)
		{
			using (Stream stream = System.IO.File.OpenWrite(filename))
			{
				var items = await source.GetAll();
				Formatter.Serialize(stream, items.ToArray());
				await stream.FlushAsync();
			}
		}

		protected override async Task Deserialize(ITableReaderWriter<T> source)
		{
			if (!System.IO.File.Exists(filename))
			{
				return;
			}

			using (var stream = new MemoryStream(System.IO.File.ReadAllBytes(filename)))
			{
				try
				{					
					T[] items = (T[])Formatter.Deserialize(stream);
					await this.InsertAll(items);
				}
				catch (Exception ex)
				{					
					throw new InvalidOperationException("Failed to deserialize data", ex);
				}
			}
		}		
	}
}
