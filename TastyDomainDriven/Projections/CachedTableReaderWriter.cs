namespace TastyDomainDriven.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// All data goes through to the backupTable, but all reads are done through the cachedTable. Use Initialize to prepopulate the cachedTable from the backupTable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachedTableReaderWriter<T> : ITableReaderWriter<T>
    {
        private readonly ITableReaderWriter<T> backupTable;

        private readonly ITableReaderWriter<T> cachedTable;

        public CachedTableReaderWriter(ITableReaderWriter<T> backupTable, ITableReaderWriter<T> cachedTable)
        {
            this.backupTable = backupTable;
            this.cachedTable = cachedTable;                        
        }

        public Task<T> Get(T match)
        {            
            return this.cachedTable.Get(match);
        }

        public Task<List<T>> GetAll()
        {
            return this.cachedTable.GetAll();
        }

        public async Task InsertOrUpdate(T add, Func<T, T> update)
        {
            await this.backupTable.InsertOrUpdate(add, update);
            await this.cachedTable.InsertOrUpdate(add, update);
        }

        public async Task<T> Remove(T key)
        {
            await this.backupTable.Remove(key);
            return await this.cachedTable.Remove(key);
        }

        /// <summary>
        /// Reads all data from the backupTable to the cachedTable.
        /// </summary>
        /// <returns></returns>
        public async Task Initialize()
        {			
            await this.cachedTable.Initialize(this.backupTable);
        }
    }
}