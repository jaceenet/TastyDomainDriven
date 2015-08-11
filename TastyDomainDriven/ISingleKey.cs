using System;

namespace TastyDomainDriven
{
	[Obsolete("Using GetHashCode instead")]
    public interface ISingleKey
    {
        string GetIndexKey();
    }
}