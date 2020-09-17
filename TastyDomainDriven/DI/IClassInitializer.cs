using System.Collections.Generic;
using System.Text;

namespace TastyDomainDriven.DI
{
    public interface IClassActivator
    {
        /// <summary>
        /// Create new instance of T.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <returns></returns>
        public T CreateInstance<T>();
    }
}
