using System.Collections.Generic;
using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public interface IAsyncProjection
    {
        Task Consume<T>(T @event) where T : IEvent;
    }
}