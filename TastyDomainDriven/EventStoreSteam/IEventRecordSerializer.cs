using System.IO;
using System.Threading.Tasks;

namespace TastyDomainDriven.EventStoreSteam
{
    public interface IEventRecordSerializer
    {
        Task<IEvent[]> Read(Stream stream);

        Task Write(IEvent[] events, Stream stream);
    }
}