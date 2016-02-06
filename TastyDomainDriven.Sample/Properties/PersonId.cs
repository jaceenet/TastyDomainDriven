using System;

namespace TastyDomainDriven.Sample.Properties
{
    [Serializable]
    public struct PersonId : IIdentity
    {
        private readonly int id;

        public PersonId(int id)
        {
            this.id = id;
        }

        public bool Equals(PersonId other)
        {
            return id == other.id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is PersonId && Equals((PersonId) obj);
        }

        public override int GetHashCode()
        {
            return id;
        }


        public override string ToString()
        {
            //Importent, this is what gets serialized...
            return string.Concat("person-", id);
        }
    }
}