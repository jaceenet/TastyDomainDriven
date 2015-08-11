using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TastyDomainDriven.Tests
{
    public class MemoryTableReaderWriterTests
    {
	    public void WriteOnce()
	    {
			var writer = new TastyDomainDriven.Projections.MemoryTableWriter<MyItem>();
	    }
    }

	public class MyItem
	{
		private string _name;
		private readonly Guid _accountId;

		public string Name => _name;

		public Guid AccountId => _accountId;

		public string Titel { get; set; }

		
		public MyItem(string name, Guid accountId)
		{
			this._name= name;
			_accountId = accountId;
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

		public override string ToString()
		{
			return this._name.ToString();
		}
	}
}
