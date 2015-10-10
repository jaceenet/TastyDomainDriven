using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyDomainDriven
{
    public static class TableReaderExtensions
    {
        public static async Task<ReaderMatch<TEntity>> GetSingle<TEntity>(this ITableReader<TEntity> reader, TEntity match)
        {
            var r = await reader.Get(match);
            return await Task.FromResult(new ReaderMatch<TEntity>(r));
        }

        public static async Task<ReaderMatch<IEnumerable<TEntity>>> QueryAll<TEntity>(this ITableReader<TEntity> reader, System.Linq.Expressions.Expression<Func<TEntity, bool>> query)
        {
            var items = await reader.GetAll();

            if (items != null)
            {
                return await Task.FromResult(new ReaderMatch<IEnumerable<TEntity>>(items.Where(query.Compile())));
            }

            return await Task.FromResult(new ReaderMatch<IEnumerable<TEntity>>(new TEntity[0]) { IsMatch = false });
        } 
    }
}