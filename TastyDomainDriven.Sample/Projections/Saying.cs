using TastyDomainDriven.Sample.Properties;

namespace TastyDomainDriven.Sample.Projections
{
    public class Saying
    {
        private readonly PersonId personId;

        public Saying(PersonId personId)
        {
            this.personId = personId;
        }

        protected bool Equals(Saying other)
        {
            return personId.Equals(other.personId);
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
            return personId.GetHashCode();
        }

        public string Said { get; set; }
    }
}