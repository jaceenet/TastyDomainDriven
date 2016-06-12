namespace TastyDomainDriven.Azure.AzureBlob
{
    public interface IAppenderNamingPolicy
    {
        /// <summary>
        /// Returns the append stream for all events combined. Used when replaying all events back.
        /// </summary>
        /// <returns></returns>
        string GetMasterPath();

        /// <summary>
        /// Returns the append stream for a given streamid
        /// </summary>
        /// <param name="streamid"></param>
        /// <returns></returns>
        string GetStreamPath(string streamid);

        /// <summary>
        /// Returns the location of the index for all aggregates
        /// </summary>
        /// <returns></returns>
        string GetIndexPath();
        /// <summary>
        /// Returns the location of the index for the aggregates. 
        /// Serve also as a transaction lock file when appending.
        /// </summary>
        /// <param name="streamid"></param>
        /// <returns></returns>
        string GetIndexPath(string streamid);        
    }
}