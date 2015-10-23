using System.Collections;
using System.Threading.Tasks;

namespace TastyDomainDriven.Providers
{
    public interface IViewProvider
    {
        ITableReaderWriter<T> GetReaderWriter<T>(string name = null) where T : class;

		/// <summary>
		/// Get instance of readerwriter without knowing the typename
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		object GetReaderWriter(string name);

		bool Exist<T>(string name = null);
        
        ITableReaderWriter<T> GetReaderWriterOrThrow<T>(string name = null) where T : class;

        Task<IEnumerable> GetAll(string name);
        string[] Keys { get; }
    }
}