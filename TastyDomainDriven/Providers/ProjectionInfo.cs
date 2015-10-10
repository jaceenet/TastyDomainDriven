using System;
using TastyDomainDriven.Projections;

namespace TastyDomainDriven.Providers
{
    public class ProjectionInfo
    {
        public ProjectionType type;
        public Func<IViewProvider, IBus, object> Create;

        public ProjectionInfo(ProjectionType type, Func<IViewProvider, IBus, object> create)
        {
            this.type = type;
            this.Create = create;
        }
    }
}