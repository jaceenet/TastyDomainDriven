using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public interface IAppendOnlyAsync
    {
        /// <summary>
        /// <para>
        /// Appends data to the stream with the specified name. If <paramref name="expectedStreamVersion"/> is supplied and
        /// it does not match server version, then <see cref="AppendOnlyStoreConcurrencyException"/> is thrown.
        /// </para> 
        /// </summary>
        /// <param name="streamName">The name of the stream, to which data is appended.</param>
        /// <param name="data">The data to append.</param>
        /// <param name="expectedStreamVersion">The server version (supply -1 to append without check).</param>
        /// <exception cref="AppendOnlyStoreConcurrencyException">thrown when expected server version is
        /// supplied and does not match to server version</exception>
        Task Append(string streamName, byte[] data, long expectedStreamVersion = -1);
        /// <summary>
        /// Reads the records by stream name.
        /// </summary>
        /// <param name="streamName">The key.</param>
        /// <param name="afterVersion">The after version.</param>
        /// <param name="maxCount">The max count.</param>
        /// <returns></returns>
        Task<DataWithVersion[]> ReadRecords(string streamName, long afterVersion, int maxCount);
        /// <summary>
        /// Reads the records across all streams.
        /// </summary>
        /// <param name="afterVersion">The after version.</param>
        /// <param name="maxCount">The max count.</param>
        /// <returns></returns>
        Task<DataWithName[]> ReadRecords(long afterVersion, int maxCount);        
    }


}