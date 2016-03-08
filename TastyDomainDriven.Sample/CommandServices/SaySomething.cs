using System;
using System.Threading.Tasks;
using TastyDomainDriven.AsyncImpl;
using TastyDomainDriven.Sample.Commands;
using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample.CommandServices
{
    public class SaySomething : 
        AsyncCommandDispatcherBase<PersonAggregate>,
        IAcceptCommand<SayCommand>
    {
        public SaySomething(IEventStore eventStore) : base(eventStore)
        {
        }

        public override Task Dispatch(ICommand command)
        {
            if (command is SayCommand)
            {
                this.When(command as SayCommand);                
            }
            return Task.FromResult(0);
        }

        public void When(SayCommand cmd)
        {
            //validate cmd arguments here, before dispatch

            this.Update(cmd.PersonId, x => x.Say(cmd.PersonId, cmd.Say, cmd.Timestamp));
        }
    }

    public class SaySomethingAsync :
        AsyncCommandDispatcher<PersonAggregate>
    {
        public SaySomethingAsync(IEventStoreAsync eventStore) : base(eventStore)
        {
        }

        public override async Task Dispatch(ICommand command)
        {
            if (command is SayCommand)
            {
                await this.When(command as SayCommand);
            }            
        }

        public async Task When(SayCommand cmd)
        {
            //validate cmd arguments here, before dispatch
            await this.Update(cmd.PersonId, x => x.Say(cmd.PersonId, cmd.Say, cmd.Timestamp));            
        }
    }
}