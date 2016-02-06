using System;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Memory;
using TastyDomainDriven.Projections;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.CommandServices;
using TastyDomainDriven.Sample.Projections;
using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            Run().Wait();
        }

        static async Task Run()
        {
            IAppendOnlyStore appender = new MemoryAppendStore();
            IEventStore es = new EventStore.EventStore(appender);
            ITableReaderWriter<Saying> said = new MemoryHashTableWriter<Saying>();

            IAsyncProjection projection = new ConsoleProjection(new AsyncProjectFromImplementation(new SayingHistoryProjection(said)));

            IEventStore es2 = new EventStorePublisher2(es, projection);
            IAsyncCommandDispatcher dispatcher = new ConsoleLoggerDispatcher(new CompositeAsyncCommandDispatcher(new SaySomething(es2)));

            await dispatcher.Dispatch(new SayCommand()
            {
                PersonId = new PersonId(1),
                Say = "Say hello",
                Timestamp = DateTime.UtcNow
            });

            foreach (var saying in said.GetAll().Result)
            {
                Console.Write("Entry in projection:\t\t");
                Console.WriteLine(saying);
            }
        }
    }
}
