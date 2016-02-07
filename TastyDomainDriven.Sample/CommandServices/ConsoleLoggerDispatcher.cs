using System;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;

namespace TastyDomainDriven.Sample.CommandServices
{
    public sealed class ConsoleLoggerDispatcher : IAsyncCommandDispatcher
    {
        private readonly IAsyncCommandDispatcher dispatcher;

        public ConsoleLoggerDispatcher(IAsyncCommandDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public async Task Dispatch(ICommand command)
        {
            Console.WriteLine("Executing: {0}", command.GetType());
            await this.dispatcher.Dispatch(command);
            Console.WriteLine("Done Executing: {0}", command.GetType());
        }
    }
}