using System.Collections;
using System.Threading.Tasks;

namespace TastyDomainDriven.Providers
{
    public interface IViewProvider
    {
        ITableReaderWriter<T> GetReaderWriter<T>(string name = null) where T : class;
        bool Exist<T>(string name = null);
        
        ITableReaderWriter<T> GetReaderWriterOrThrow<T>(string name = null) where T : class;

        Task<IEnumerable> GetAll(string name);
        string[] Keys { get; }
    }
}