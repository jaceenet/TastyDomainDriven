using System;

namespace TastyDomainDriven.Tests
{
    [Serializable]
    public struct StringId : IIdentity
    {
        private readonly string fullid;

        public StringId(string fullid)
        {
            this.fullid = fullid;
        }

        public override int GetHashCode()
        {
            return fullid.GetHashCode();
        }

        public override string ToString()
        {
            return fullid.ToString();
        }
    }
}