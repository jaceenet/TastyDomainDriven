using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyDomainDriven.Providers
{
    public abstract class BaseViewProvider : IDisposable, IViewProvider
    {
        private readonly Dictionary<string, object> GetReaderWriterDef = new Dictionary<string, object>();
        private readonly Dictionary<string, Func<Task<IEnumerable>>> GetAllDelegates = new Dictionary<string, Func<Task<IEnumerable>>>();
		protected object _threadlock = new object();
        
		/// <summary>
		/// Gets the ITableReaderWriter threadsafe
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
        public ITableReaderWriter<T> GetReaderWriter<T>(string name = null) where T : class
        {
            var key = name ?? typeof(T).Name;

	        if (this.GetReaderWriterDef.ContainsKey(key))
	        {
		        return (ITableReaderWriter<T>) this.GetReaderWriterDef[key];
	        }

            if (!this.GetReaderWriterDef.ContainsKey(key))
            {
	            lock (_threadlock)
	            {
					if (this.GetReaderWriterDef.ContainsKey(key))
					{
						return (ITableReaderWriter<T>)this.GetReaderWriterDef[key];
					}

					var instance = Create<T>(key);
					this.GetReaderWriterDef.Add(key, instance);

					this.GetAllDelegates.Add(key, async () =>
					{
						List<T> item = await instance.GetAll();
						return item.AsEnumerable();
					});
				}
            }

            return (ITableReaderWriter<T>)this.GetReaderWriterDef[key];
        }

	    public object GetReaderWriter(string name)
	    {
		    if (this.GetReaderWriterDef.ContainsKey(name))
		    {
			    return this.GetReaderWriterDef[name];
		    }

			throw new ArgumentException("Missing value on key: " + name);
	    }

	    public ITableReaderWriter<T> GetReaderWriterOrThrow<T>(string name = null) where T : class
        {
            var key = name ?? typeof(T).Name;

            if (!Exist<T>())
            {
                throw new KeyNotFoundException("Missing View on key: " + key);
            }

            return (ITableReaderWriter<T>)GetReaderWriterDef[key];
        }

        public bool Exist<T>(string name = null)
        {
            var key = name ?? typeof(T).Name;

            return this.GetReaderWriterDef.ContainsKey(key);
        }

        protected abstract ITableReaderWriter<T> Create<T>(string viewname) where T : class;

        public Task<IEnumerable> GetAll(string name)
        {
            if (this.GetReaderWriterDef.ContainsKey(name) && this.GetAllDelegates.ContainsKey(name))
            {
                return this.GetAllDelegates[name]();
            }

            return Task.FromResult((IEnumerable)new object[0]);
        }

        public string[] Keys => this.GetReaderWriterDef.Keys.ToArray();

        public void Dispose()
        {
            this.GetAllDelegates.Clear();
            this.GetReaderWriterDef.Clear();
        }

        public bool ContainsName(string name)
        {
            return GetAllDelegates.ContainsKey(name);
        }        
    }
}