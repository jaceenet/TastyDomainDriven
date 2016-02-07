using TastyDomainDriven.Sample.Properties;
using TastyDomainDriven.Serialization;

namespace TastyDomainDriven.Sample.Projections
{
    internal class MyDtoConverters : IDtoConverter<SayingDto, Saying>
    {
        public SayingDto Serialize(Saying obj)
        {
            return new SayingDto() { Id = obj.PersonId.id, Said = obj.Said };
        }

        public Saying Deserialize(SayingDto obj)
        {
            return new Saying(new PersonId(obj.Id)) { Said = obj.Said };
        }
    }
}