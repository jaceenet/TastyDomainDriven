using System;
using System.Collections.Generic;
using System.Linq;

namespace TastyDomainDriven.Dsl
{
    public sealed class EventDslReader
    {
        private List<EventClass> events = new List<EventClass>();
        private Dictionary<string, EventPropertyType> myTypes = new Dictionary<string, EventPropertyType>();

        public EventDslReader()
        {            
        }

        public void AddCsharpDefaults()
        {
            this.myTypes.Add("string", new EventPropertyType("string", "null", "System", "{0}.Write({1})", "{0}.ReadString()"));
            this.myTypes.Add("double", new EventPropertyType("double", "0", "System", ""));
            this.myTypes.Add("bool", new EventPropertyType("bool", "false", "System"));
            this.myTypes.Add("int", new EventPropertyType("int", "0", "System", "{0}.Write({1})", "{0}.ReadInt32()"));
            this.myTypes.Add("Guid", new EventPropertyType("Guid", "Guid.Empty", "System", "{0}.Write({1}.ToByteArray())", "new Guid({0}.ReadBytes(16))"));
            this.myTypes.Add("DateTime", new EventPropertyType("DateTime", "DateTime.MinValue", "System", "{0}.Write({1}.ToBinary())", "DateTime.FromBinary({0}.ReadInt64())"));
        }

        public void ParseCsvFile(string filename)
        {
            var lines = System.IO.File.ReadAllLines(filename);

            this.ParseLines(lines);
        }

        public string[] GetNamespaces()
        {
            var found = this.events.SelectMany(x => x.PublicMembers).Select(x => x.PropertyType.Namespace).Distinct().ToList();
            if (!found.Contains("TastyDomainDriven"))
            {
                found.Add("TastyDomainDriven");
            }

            if (!found.Contains(typeof(IEventVersionSerializer).Namespace))
            {
                found.Add(typeof(IEventVersionSerializer).Namespace);
            }
            return found.OrderBy(x => x).ToArray();
        }

        public void ParseLines(string[] lines)
        {
            int pos = 0;

            foreach (var line in lines)
            {
                ++pos;

                if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim()) || line.StartsWith("//"))
                {
                    continue;
                }

                if (line.StartsWith("#"))
                {
                    var cols = line.Substring(1).Split(';');

                    if (cols.Length == 5)
                    {
                        myTypes.Add(cols[0], new EventPropertyType(cols[0], cols[1], cols[2], cols[3], cols[4]));
                        continue;
                    }
                    else
                    {
                        throw new Exception("Missing type definition for line: {line} should be like: \"TypeName;DefaultValue;Namespace;{0}.Write({1});{0}.ReadString()\"");
                    }
                }

                var props = line.Split(';');

                if (props.Length < 3)
                {
                    throw new Exception($"Invalid Class: {line} line number {pos} expected {3} instead of {props.Length} items when splitting string on ;");
                }


                var item = new EventClass(props[0], int.Parse(props[1]));

                foreach (var prop in props.Skip(2))
                {
                    var p = prop.Split(':', '=');

                    if (p.Length != 3)
                    {
                        throw new Exception($"Invalid Property on line: {pos}, looking in '{prop}' expected 'Field:Type'");
                    }

                    if (!myTypes.ContainsKey(p[2]))
                    {
                        throw new Exception("Could not find type: " + p[2]);
                    }

                    int propid;

                    if (!int.TryParse(p[0], out propid))
                    {
                        throw new Exception($"Could not parse {p[0]} as int");
                    }

                    item.PublicMembers.Add(new EventPropertyItem(propid, p[1], myTypes[p[2]], true));

                    if (!item.PublicMembers.Any(x => x.PropertyName.Equals("AggregateId")))
                    {
                        item.PublicMembers.Add(new EventPropertyItem(-1, "AggregateId", new EventPropertyType("IIdentity", "", "TastyDomainDriven"), false));
                    }
                }

                this.events.Add(item);
            }
        }

        public List<EventClass> GetEventObjects()
        {
            return events;
        }
    }
}