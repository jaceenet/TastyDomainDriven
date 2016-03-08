using System;

namespace TastyDomainDriven.Tests
{
    [Serializable]
    public struct GuidId : IIdentity
    {
        private readonly Guid id;

        public GuidId(Guid guid)
        {
            this.id = guid;
        }

        public static GuidId NewGuidId()
        {
            return new GuidId(Guid.NewGuid());
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return id.ToString();
        }
    }
}