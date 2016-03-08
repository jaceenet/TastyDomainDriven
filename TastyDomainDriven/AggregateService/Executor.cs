using System;
using System.Threading.Tasks;

namespace TastyDomainDriven.AggregateService
{
    public class Executor<T> : ICommandExecutor where T : class, ICommand
    {
        private readonly T cmd;
        private readonly Func<T, Task> action;

        public Executor(T cmd, Func<T, Task> action)
        {
            this.cmd = cmd;
            this.action = action;
        }

        public Executor(ICommand cmd, Func<T, Task> action) : this(cmd as T, action)
        {
        }

        public async Task Execute()
        {
            await this.action(this.cmd);
        }
    }
}