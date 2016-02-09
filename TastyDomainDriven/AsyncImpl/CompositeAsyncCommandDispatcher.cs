using System.Threading.Tasks;

namespace TastyDomainDriven.AsyncImpl
{
    public class CompositeAsyncCommandDispatcher : IAsyncCommandDispatcher
    {
        private readonly IAsyncCommandDispatcher[] dispatchers;

        public CompositeAsyncCommandDispatcher(params IAsyncCommandDispatcher[] dispatchers)
        {
            this.dispatchers = dispatchers;
        }

        public async Task Dispatch(ICommand command)
        {
            foreach (var dispatcher in this.dispatchers)
            {
                await dispatcher.Dispatch(command);
            }
        }
    }
}