using System.Threading.Tasks;

namespace TastyDomainDriven
{
    public interface IApplicationService
    {
        void Execute(ICommand cmd);
    }

    public interface IAcceptCommand<in T>
        where T : ICommand
    {
        void When(T cmd);
    }    
}