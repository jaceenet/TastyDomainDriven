namespace TastyDomainDriven
{
	public interface ITableReaderWriter<TEntity> : ITableReader<TEntity>, ITableWriter<TEntity>
	{
		
	}
}