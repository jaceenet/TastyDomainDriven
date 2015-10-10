namespace TastyDomainDriven.Bus
{
    public class NoBus : IBus
    {
        public void Dispatch(ICommand cmd)
        {
        }
    }
}