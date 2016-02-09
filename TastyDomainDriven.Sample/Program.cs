using System;
using System.IO;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Memory;
using TastyDomainDriven.Projections;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.CommandServices;
using TastyDomainDriven.Sample.Projections;
using TastyDomainDriven.Sample.Properties;
using TastyDomainDriven.Serialization;

namespace TastyDomainDriven.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Run().Wait();
            }
            catch (AggregateException aex)
            {
                Console.WriteLine("===== ERRORS: =====");

                foreach (var ex in aex.InnerExceptions)
                {
                    Console.WriteLine(ex);
                }

                throw;
            }            
        }

        static async Task Run()
        {
            IAppendOnlyStore appender = new MemoryAppendStore();
            IEventStore es = new EventStore.EventStore(appender);

            IDtoConverter<SayingDto, Saying> converter = new MyDtoConverters();
            Stream mem = System.IO.File.Open("test.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            IDtoSerializer<SayingDto> dtoSerializer = new FastJsonSerializer<SayingDto>(mem);
            ITableReaderWriter<Saying> said = new TableWriterForSerializer<Saying, SayingDto>(new MemoryHashTableWriter<Saying>(), converter, dtoSerializer);

            IAsyncProjection projection = new ConsoleProjection(new AsyncProjectFromImplementation(new SayingHistoryProjection(said)));

            IEventStore es2 = new EventStoreForAsyncProjection(es, projection);
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
                Console.WriteLine("{0} said: {1}", saying.PersonId, saying.Said);
            }
        }

        static async Task Run2()
        {
            IAppendOnlyStore appender = new MemoryAppendStore();
            IEventStore es = new EventStore.EventStore(appender);

            IDtoConverter<SayingDto, Saying> converter = new MyDtoConverters();
            Stream mem = System.IO.File.Open("test.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);

            IDtoSerializer<SayingDto> dtoSerializer = new FastJsonSerializer<SayingDto>(mem);
            ITableReaderWriter<Saying> said = new TableWriterForSerializer<Saying, SayingDto>(new MemoryHashTableWriter<Saying>(), converter, dtoSerializer);

            IAsyncProjection projection = new ConsoleProjection(new AsyncProjectFromImplementation(new SayingHistoryProjection(said)));

            IEventStore es2 = new EventStoreForAsyncProjection(es, projection);
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
                Console.WriteLine("{0} said: {1}", saying.PersonId, saying.Said);
            }
        }
    }
}
