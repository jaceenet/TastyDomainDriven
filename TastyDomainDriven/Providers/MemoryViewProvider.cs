using TastyDomainDriven.Projections;

namespace TastyDomainDriven.Providers
{
    public class MemoryViewProvider : BaseViewProvider
    {
        protected override ITableReaderWriter<T> Create<T>(string viewname)
        {
            return new MemoryHashTableWriter<T>();
        }
    }
}