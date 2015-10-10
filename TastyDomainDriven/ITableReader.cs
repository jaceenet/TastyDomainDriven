using System.Linq.Expressions;

namespace TastyDomainDriven
{
	using System.Collections.Generic;
	using System.Threading.Tasks;

	public interface ITableReader<TEntity>
	{
		Task<TEntity> Get(TEntity match);

		Task<List<TEntity>> GetAll();
	}
}