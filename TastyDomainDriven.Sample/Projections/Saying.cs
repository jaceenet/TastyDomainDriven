using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample.Projections
{
    public class Saying
    {
        public Saying(PersonId personId)
        {
            this.PersonId = personId;
        }

        public PersonId PersonId { get; set; }

        protected bool Equals(Saying other)
        {
            return PersonId.Equals(other.PersonId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Saying) obj);
        }

        public override int GetHashCode()
        {
            return PersonId.GetHashCode();
        }

        public string Said { get; set; }
    }
}