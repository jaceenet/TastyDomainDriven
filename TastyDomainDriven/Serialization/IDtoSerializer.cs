using System.Threading.Tasks;

namespace TastyDomainDriven.Serialization
{
    public interface IDtoSerializer<T>
    {
        Task Save(T[] items);
        Task<T[]> Load();
    }
}