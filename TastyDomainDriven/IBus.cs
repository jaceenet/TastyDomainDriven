namespace TastyDomainDriven
{
    public interface IBus
    {
        void Dispatch(ICommand cmd);
    }
}