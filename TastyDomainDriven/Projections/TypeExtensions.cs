namespace TastyDomainDriven.Projections
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class TypeExtensions
	{
		public static Type[] GetImplementedGenericType(this object source, Type type)
		{
			return (from i in source.GetType().GetInterfaces() where i.IsGenericType && i.GetGenericTypeDefinition() == type select i.GetGenericArguments()[0]).ToArray();
		}

		public static T[] AddToArray<T>(this T[] array, T item)
		{
			var newarray = new List<T>(array);
			newarray.Add(item);
			return newarray.ToArray();
		}

		public static T[] RemoveToArray<T>(this T[] array, T item)
		{
			var newarray = new List<T>(array);
			newarray.Remove(item);
			return newarray.ToArray();
		}
	}
}