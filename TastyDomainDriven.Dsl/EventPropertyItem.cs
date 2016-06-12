using System.Collections.Generic;
using System.Linq;

namespace TastyDomainDriven.Dsl
{
    public class EventPropertyItem
    {
        public EventPropertyItem(int propertyid, string name, EventPropertyType type, bool isPrivate = true)
        {
            this.Propertyid = propertyid;
            this.PropertyName = name;
            this.PropertyType = type;
            this.IsPrivate = isPrivate;
        }

        public bool IsPrivate { get; set; }

        public int Propertyid { get; set; }
        public string PropertyName { get; set; }

        public EventPropertyType PropertyType { get; set; }

        public string PrivateField
        {
            get
            {
                return string.Concat("_", this.PropertyName.Substring(0, 1).ToLower(), this.PropertyName.Substring(1));
            }
        }

        public string InstanceVariable
        {
            get
            {
                return string.Concat(this.PropertyName.Substring(0, 1).ToLower(), this.PropertyName.Substring(1));
            }
        }
    }

    public class EventClass
    {
        private List<EventPropertyItem> members;

        public string ClassName { get; set; }
        public int ClassId { get; set; }

        public EventClass(string className, int classId)
        {
            this.ClassName = className;
            this.ClassId = classId;
            this.members = new List<EventPropertyItem>();
        }

        public List<EventPropertyItem> PublicMembers => members;

        public List<EventPropertyItem> PrivateMembers
        {
            get { return this.members.Where(x => x.IsPrivate).ToList(); }
        }
    }
}