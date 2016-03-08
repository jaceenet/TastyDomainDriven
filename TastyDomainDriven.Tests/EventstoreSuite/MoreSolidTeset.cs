using System;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Memory;
using TastyDomainDriven.Projections;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.Projections;
using TastyDomainDriven.Sample.Properties;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class AppenderTests
    {
        [Fact]
        public async Task AppendToEmptyStream()
        {
            var appender = new MemoryAppendStoreAsync();
            var es = new EventStoreAsync(appender);
            
            var dispatcher = new TastyDomainDriven.Sample.CommandServices.SaySomething(es);

            await
                dispatcher.GetExecutor(new SayCommand()
                {
                    PersonId = new PersonId(1),
                    Say = "I have something to say",
                    Timestamp = DateTime.UtcNow
                }).Execute();

            Assert.Equal(1, (await es.ReplayAll()).Events.Count);
        }

        [Fact]
        public async Task AppendToToPublisher()
        {
            var appender = new MemoryAppendStoreAsync();
            var es = new EventStoreAsync(appender);
            ITableReaderWriter<Saying> table = new MemoryHashTableWriter<Saying>();

            var publisher = new EventStoreAsyncPublisher(es, new AsyncProjectFromImplementation(new SayingHistoryProjection(table)));
            var dispatcher = new Sample.CommandServices.SaySomething(publisher);

            await
                dispatcher.GetExecutor(new SayCommand()
                {
                    PersonId = new PersonId(1),
                    Say = "I have something to say",
                    Timestamp = DateTime.UtcNow
                }).Execute();

            Assert.Equal(1, (await es.ReplayAll()).Events.Count);
            Assert.Equal(1, (await table.GetAll()).Count);
        }


    }
}