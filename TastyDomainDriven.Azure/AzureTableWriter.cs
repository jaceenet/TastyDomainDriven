using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace TastyDomainDriven.Azure
{
    public class AzureTableWriter<TEntity> : TableEntity, ITableReaderWriter<TEntity>
        where TEntity : TableEntity, new()
    {

		private static readonly TheLogger Logger = TheLogManager.GetLogger(typeof(AzureTableWriter<TEntity>));
        
		private readonly string tablename;

        private readonly CloudTable table;
        
        private List<TEntity> items = null;

        private readonly CloudTable projections;

        public string TableName { get; set; }

        public int EventCount { get; set; }

        public Guid EventId { get; set; }

        public DateTime EventTimestamp { get; set; }

        public AzureTableWriter(string connectionString, string tablename) : this(CloudStorageAccount.Parse(connectionString), tablename)
        {            
        }

        public AzureTableWriter(CloudStorageAccount storageAccount, string tablename)
        {
			this.tablename = tablename.ToLowerInvariant();
            
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            this.table = tableClient.GetTableReference(this.tablename);
            this.projections = tableClient.GetTableReference("projections");
        }

        public async Task<bool> Initialize()
        {
	        try
	        {
				await this.projections.CreateIfNotExistsAsync();
				await this.table.CreateIfNotExistsAsync();
	        }
			catch (StorageException ex)
	        {
				Logger.DebugFormat("Failed to initialize, best guess is bad naming of table. Check azure naming guidelines.");
				Logger.InfoFormat("Error: {0} with status code: {1}. Extended info: {2} ({3})", ex.RequestInformation.HttpStatusMessage, ex.RequestInformation.HttpStatusCode, ex.RequestInformation.ExtendedErrorInformation.ErrorMessage, ex.RequestInformation.ExtendedErrorInformation.ErrorCode);
				Logger.Debug(ex.ToString());
		        return false;
		        
	        }

	        return true;
        }

        protected async Task UpdateProjectIndex(IEvent e)
        {
            TableOperation operation = TableOperation.InsertOrReplace(this);
            await this.projections.ExecuteAsync(operation);
        }

		public async Task Initialize(ITableReaderWriter<TEntity> writer)
        {
            await this.table.CreateIfNotExistsAsync();
            var allitems = await writer.GetAll();

            foreach (var key in allitems.GroupBy(x => x.PartitionKey))
            {
                var existingItems = key.ToList();
                var index = 0;
                var count = existingItems.Count();

                do
                {
                    var pack = existingItems.Skip(index).Take(100);
                    index += (count - index) > 100 ? 100 : count - index - 1;

					var batch = new TableBatchOperation();
                    
					foreach (var entity in pack)
                    {                        
                        batch.InsertOrReplace(entity);
                    }

					await this.table.ExecuteBatchAsync(batch);
                }
                while (index < existingItems.Count()-1);                
            }            
        }

        public async Task<TEntity> Get(TEntity entity)
        {
            GuardKeys(entity);

	        try
	        {
				TableOperation retriveOperation = TableOperation.Retrieve<TEntity>(entity.PartitionKey, entity.RowKey);
				TableResult result = await this.table.ExecuteAsync(retriveOperation);

				if (result.Result != null)
				{
					return await Task.FromResult((TEntity)result.Result);
				}

				return default(TEntity);
	        }
	        catch (StorageException ex)
	        {
				Logger.WarnFormat("Failed to Get Entity: PartitionKey '{0}' Rowkey '{1}'", entity.PartitionKey, entity.RowKey);
				Logger.WarnFormat("Error: {0} with status code: {1}. Extended info: {2} ({3})", ex.RequestInformation.HttpStatusMessage, ex.RequestInformation.HttpStatusCode, ex.RequestInformation.ExtendedErrorInformation.ErrorMessage, ex.RequestInformation.ExtendedErrorInformation.ErrorCode);
		        return entity;
	        }
        }

	    public async Task<List<TEntity>> GetAll()
        {
            if (this.items == null)
            {
                TableContinuationToken token = null;

	            this.items = new List<TEntity>();

                do
                {
                    var queryResult = await this.table.ExecuteQuerySegmentedAsync(new TableQuery<TEntity>(), token);

	                var results = queryResult.Results;

	                if (results != null && results.Any())
	                {
		                this.items.AddRange(results);
	                }

                    token = queryResult.ContinuationToken;
                }
                
                while (token != null);
            }

            return await Task.FromResult(this.items);
        }

        public async Task InsertOrUpdate(TEntity entity, Func<TEntity,TEntity> update)
        {
            GuardKeys(entity);

	        try
	        {
				// Create a retrive operation
				TableOperation retrieveOperation = TableOperation.Retrieve<TEntity>(entity.PartitionKey, entity.RowKey);

				// Execute the retrieve operation.
				TableResult retrievedResult = await this.table.ExecuteAsync(retrieveOperation);

				if (retrievedResult.Result != null)
				{
					var updatedentity = update((TEntity)retrievedResult.Result);
					updatedentity.ETag = "*";
					TableOperation updateOperation = TableOperation.Replace(updatedentity);
					await this.table.ExecuteAsync(updateOperation);
					this.items = null;
				}
				else
				{
					TableOperation insertOrReplaceOperation = TableOperation.Insert(entity);
					await this.table.ExecuteAsync(insertOrReplaceOperation);
					this.items = null;
				}
	        }
	        catch (StorageException ex)
	        {
				Logger.WarnFormat("Failed to InsertOrUpdate Entity: PartitionKey '{0}' Rowkey '{1}'", entity.PartitionKey, entity.RowKey);
				Logger.WarnFormat("Error: {0} with status code: {1}. Extended info: {2} ({3})", ex.RequestInformation.HttpStatusMessage, ex.RequestInformation.HttpStatusCode, ex.RequestInformation.ExtendedErrorInformation.ErrorMessage, ex.RequestInformation.ExtendedErrorInformation.ErrorCode);		        
	        }	        
        }

	    public async Task<TEntity> Remove(TEntity key)
	    {
		    var item = await this.Get(key);

		    if (item != null)
		    {
				TableOperation deleteOperation = TableOperation.Delete(item);
				await this.table.ExecuteAsync(deleteOperation);
				return item;
		    }

		    return null;
	    }

	    private static void GuardKeys(TEntity entity)
        {
            if (string.IsNullOrEmpty(entity.PartitionKey))
            {
                throw new ArgumentException("entity must have partitionkey defined");
            }

            if (entity.RowKey == null)
            {
                throw new ArgumentException("entity must have rowkey defined or empty");
            }
        }
    }
}