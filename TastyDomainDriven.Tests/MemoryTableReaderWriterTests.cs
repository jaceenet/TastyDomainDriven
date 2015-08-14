using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class MemoryTableReaderWriterTests
    {
		[Fact]
		public void AddAndUpdateTest()
	    {
			var writer = new TastyDomainDriven.Projections.MemoryTableWriter<MyItem>();

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

	public class MyItem : ISingleKey
	{
		private string _name;
		private readonly Guid _accountId;

		public string Name => _name;

		public Guid AccountId => _accountId;

		public string Titel { get; set; }

		
		public MyItem(string name, Guid accountId)
		{
			this._name= name;
			this._accountId = accountId;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MyItem) obj);
		}

		protected bool Equals(MyItem other)
		{
			return string.Equals(_name, other._name) && _accountId.Equals(other._accountId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_name != null ? _name.GetHashCode() : 0)*397) ^ _accountId.GetHashCode();
			}
		}

		public string GetIndexKey()
		{
			return string.Concat(this.Name, this.AccountId);
		}

		public override string ToString()
		{
			return this._accountId.ToString("N") + " - " + this._name.ToString();
		}
	}
}
