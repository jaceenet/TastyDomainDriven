using System.Collections.Generic;
using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public interface IAsyncProjection
    {
        Task Consume(IEvent @event);
    }
}