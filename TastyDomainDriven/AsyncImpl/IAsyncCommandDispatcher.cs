using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public interface IAsyncCommandDispatcher
    {
        Task Dispatch(ICommand command);
    }    
}