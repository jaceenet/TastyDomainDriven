using System.Collections.Generic;
using TastyDomainDriven.Bus;

namespace TastyDomainDriven.Providers
{
    /// <summary>
    /// Dictionary for keeping info about projections
    /// </summary>
    public class ProjectionsProvider : Dictionary<string, ProjectionInfo>
    {
        /// <summary>
        /// Create all views, by calling the constructor on all projection registered. 
        /// </summary>
        /// <param name="viewProvider"></param>
        public void CreateAllViews(IViewProvider viewProvider)
        {
            //Ensure all views are loaded
            foreach (KeyValuePair<string, ProjectionInfo> item in this)
            {
                item.Value.Create(viewProvider, new NoBus());
            }
        }
    }
}