using System;
using System.Net.Configuration;
using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    [Obsolete("Use the ICommandHandler interface")]
    public interface IAsyncCommandDispatcher
    {
        Task Dispatch(ICommand command);
    }    
}