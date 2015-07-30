using System.IO;

namespace TastyDomainDriven.Azure
{
    /// <summary>
    /// Delegate that writes pages to the underlying paged store.
    /// </summary>
    /// <param name="offset">The offset.</param>
    /// <param name="source">The source.</param>
    public delegate void AppendWriterDelegate(int offset, Stream source);
}