namespace TastyDomainDriven.AggregateService
{
    /// <summary>
    /// Provide the handler for executing the command
    /// </summary>
    public interface ICommandHandler
    {
        ICommandExecutor GetExecutor(ICommand command);
    }
}