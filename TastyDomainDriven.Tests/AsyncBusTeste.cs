using System;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Azure.AzureBlob;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.CommandServices;
using TastyDomainDriven.Sample.Properties;
using Xunit;

namespace TastyDomainDriven.Tests
{
    public class AsyncBusTeste
    {
        [Fact(Skip = "needs connectionstring")]
        public async Task ExecuteCommand()
        {
            string container = "testing";
            string connection = "";
            var appender = new AzureAsyncAppender(connection, container, new PrefixedDirectory("demo"));
            await appender.Initialize();

            var es = new EventStoreAsync(appender);

            IAsyncCommandDispatcher dispatcher = new CompositeAsyncCommandDispatcher(new SaySomethingAsync(es));

            await
                dispatcher.Dispatch(new SayCommand()
                {
                    PersonId = new PersonId(1),
                    Say = "I say hello 1dsf ",
                    Timestamp = DateTime.UtcNow
                });

            await
                dispatcher.Dispatch(new SayCommand()
                {
                    PersonId = new PersonId(1),
                    Say = "I say ha a fsdsfdello",
                    Timestamp = DateTime.UtcNow
                });

            await
                dispatcher.Dispatch(new SayCommand()
                {
                    PersonId = new PersonId(1),
                    Say = "I say helloa fsf ",
                    Timestamp = DateTime.UtcNow
                });

            await
                dispatcher.Dispatch(new SayCommand()
                {
                    PersonId = new PersonId(1),
                    Say = "I say hello 123",
                    Timestamp = DateTime.UtcNow
                });
        }
    }
}