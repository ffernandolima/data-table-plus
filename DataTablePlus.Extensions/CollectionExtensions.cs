using System;
using System.Collections.Generic;

namespace DataTablePlus.Extensions
{
	public static class CollectionExtensions
	{
		public static Type GetTypeFromCollection<T>(this ICollection<T> collection)
		{
			return typeof(T);
		}
	}
}
