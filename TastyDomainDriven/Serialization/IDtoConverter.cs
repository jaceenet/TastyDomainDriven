
namespace TastyDomainDriven.Serialization
{
    public interface IDtoConverter<TDto, TClass>
    {
        TDto Serialize(TClass obj);
        TClass Deserialize(TDto obj);
    }
}