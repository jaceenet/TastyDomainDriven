using System.Threading.Tasks;
using TastyDomainDriven.Sample.Events;

namespace TastyDomainDriven.Sample.Projections
{
    public class SayingHistoryProjection
        : IConsumesAsync<PersonSaidEvent>
    {
        private readonly ITableReaderWriter<Saying> said;

        public SayingHistoryProjection(ITableReaderWriter<Saying> said)
        {
            this.said = said;
        }

        public async Task Consume(PersonSaidEvent e)
        {
            await this.said.AddOrUpdate(new Saying(e.PersonId), x => x.Said = e.Saying);
        }
    }
}