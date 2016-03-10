using System;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Azure.AzureBlob;
using TastyDomainDriven.Memory;
using TastyDomainDriven.Projections;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.CommandServices;
using TastyDomainDriven.Sample.Projections;
using TastyDomainDriven.Sample.Properties;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class AsyncBusTeste
    {
        [Fact()]
        public async Task ExecuteCommand()
        {
            string container = "testing";
            string connection = "";
            //var appender = new AzureAsyncAppender(connection, container, new PrefixedDirectory("demo"));
            //await appender.Initialize();
            
            var appender = new MemoryAppendStoreAsync();
            
            var es = new EventStoreAsync(appender);

            var dispatcher = new SaySomething(es);

            await dispatcher.GetExecutor(
            new SayCommand
            {
                PersonId = new PersonId(1),
                Say = "I am Winter ",
                Timestamp = DateTime.UtcNow
            }).Execute();

            await dispatcher.GetExecutor(
            new SayCommand
            {
                PersonId = new PersonId(1),
                Say = "How are you?",
                Timestamp = DateTime.UtcNow
            }).Execute();

            await dispatcher.GetExecutor(
             new SayCommand
             {
                 PersonId = new PersonId(1),
                 Say = "Dunno...",
                 Timestamp = DateTime.UtcNow
             }).Execute();

            await dispatcher.GetExecutor(
            new SayCommand
            {
                PersonId = new PersonId(1),
                Say = "ok say something more",
                Timestamp = DateTime.UtcNow
            }).Execute();

            await dispatcher.GetExecutor(
            new SayCommand
            {
                PersonId = new PersonId(1),
                Say = "NO!",
                Timestamp = DateTime.UtcNow
            }).Execute();

            var events = (await es.ReplayAll()).Events;

            Assert.Equal(5, events.Count);
        }
    }
}