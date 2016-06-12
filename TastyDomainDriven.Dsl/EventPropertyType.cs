namespace TastyDomainDriven.Dsl
{
    public class EventPropertyType
    {
        public EventPropertyType(string typeName, string defaultValue, string ns, string binaryto = "", string binaryfrom = "")
        {
            this.TypeName = typeName;
            this.DefaultValue = defaultValue;
            this.Namespace = ns;
            Binaryfrom = binaryfrom;
            Binaryto = binaryto;
        }

        public string TypeName { get; }

        public string Namespace { get;  }
        public string Binaryfrom { get; set; }
        public string Binaryto { get; set; }

        public string DefaultValue { get; }

        public string WriteBinary(string binarywriter, string value)
        {
            if (this.Binaryto != null)
            {
                return string.Format(this.Binaryto, binarywriter, value);
            }

            return "";
        }

        public string ReadBinary(string binarywriter)
        {
            if (this.Binaryto != null)
            {
                return string.Format(this.Binaryfrom, binarywriter);
            }

            return "";
        }
    }
}