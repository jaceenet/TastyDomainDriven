using System.Threading.Tasks;

namespace TastyDomainDriven.AggregateService
{
    /// <summary>
    /// Command executor dispatch the command. This replace the IBus interface
    /// </summary>
    public interface ICommandExecutor
    {
        Task Execute();
    }
}