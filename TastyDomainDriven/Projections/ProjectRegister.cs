using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TastyDomainDriven.Projections
{
	/// <summary>
	/// Keeps track of any open generic interfaces, the Type has.
	/// </summary>
	public class ImplementOpenGeneric
	{
		private readonly Dictionary<TypeKey, List<Type>> list = new Dictionary<TypeKey, List<Type>>();

		internal class TypeKey
		{
			public TypeKey()
			{				
			}

			public TypeKey(Type genericType, Type[] arguments)
			{
				this.genericType = genericType;
				this.arguments = arguments;
			}

			public Type genericType;
			public Type[] arguments;
			
			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = (genericType != null ? genericType.GetHashCode() : 0);
					return arguments.Aggregate(hashCode, (current, a) => (current*397) ^ (a != null ? a.GetHashCode() : 0));
				}
			}

			public override string ToString()
			{
				return genericType.Name.Replace("`1", "<" + string.Join(",", arguments.Select(x => x.Name)) + ">");
			}

			public override bool Equals(object obj)
			{
				if (obj == null)
				{
					return false;
				}

				return this.GetHashCode().Equals(obj.GetHashCode());
			}
		}

		public void Add(Type lookfor, params Type[] types)
		{
			foreach (var t in types)
			{
				var interfaces = t.GetInterfaces().ToArray();

                var openGenerics = (from i in t.GetInterfaces() where i.IsGenericType && i.GetGenericTypeDefinition() == lookfor select i).ToArray();
				
				foreach (var i in openGenerics)
				{
					var genericArguments  = new TypeKey() { genericType = lookfor, arguments = i.GetGenericArguments() };

					if (!list.ContainsKey(genericArguments))
					{
						list.Add(genericArguments, new List<Type>());
					}

					list[genericArguments].Add(t);
				}
			}
		}

		public Type[] GetInvokeableTypes<TDelegate>()
		{
			return GetInvokeableTypes(typeof (TDelegate));
		}

		public Type[] GetInvokeableTypes(Type find)
		{
			if (find.IsGenericType)
			{
				var key = new TypeKey(find.GetGenericTypeDefinition(), find.GetGenericArguments());

				if (list.ContainsKey(key))
				{
					return list[key].ToArray();
				}
			}

			return new Type[0];
		}
	}
}