using System.Threading.Tasks;
using TastyDomainDriven.AggregateService;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.PerformanceMeasurements;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample.CommandServices
{
    public class SaySomething : 
        AggregateHandler<PersonAggregate>        
    {
        public SaySomething(IEventStoreAsync eventStore, IPerformanceLogger logger = null) : base(eventStore, logger)
        {
            this.Register<SayCommand>(Say);
        }

        private async Task Say(SayCommand cmd)
        {
            //validate cmd arguments here, before dispatch
            await this.Update(cmd.PersonId, x => x.Say(cmd.PersonId, cmd.Say, cmd.Timestamp));
        }
    }
}