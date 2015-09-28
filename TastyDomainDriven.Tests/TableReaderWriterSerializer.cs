using System;
using TastyDomainDriven.Projections;
using Xunit;

namespace TastyDomainDriven.Tests
{
	public class TableReaderWriterSerializerTest
	{

		[Fact]
		private void WriteFileAndRead()
		{        
			var writer = new FileReaderWriter<MyItem>(new MemoryTableWriter<MyItem>(), @"c:\temp\somefile.dat");

			Guid myid = Guid.NewGuid();

			writer.AddOrUpdate(new MyItem("john", myid), x =>
			{
				x.Titel = "Mr";
			});

			writer.AddOrUpdate(new MyItem("john2", myid), x =>
			{
				x.Titel = "Mr 2";
			});

			writer.AddOrUpdate(new MyItem("john2", myid), x =>
			{
				x.Titel = "Mr renamed";
			});

			Assert.Equal(2, writer.GetAll().Result.Count);
			Assert.Equal("Mr", writer.Get(new MyItem("john", myid)).Result.Titel);
			Assert.Equal("Mr renamed", writer.Get(new MyItem("john2", myid)).Result.Titel);
			Assert.Null(writer.Get(new MyItem("joHn2", myid)).Result);
		
	}
	}
}