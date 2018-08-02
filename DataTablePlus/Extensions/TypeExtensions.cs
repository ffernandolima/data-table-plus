/*****************************************************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * See https://github.com/ffernandolima/data-table-plus for details.
 *
 * MIT License
 * 
 * Copyright (c) 2018 Fernando Luiz de Lima
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial 
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
 * LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 ****************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace DataTablePlus.Extensions
{
	/// <summary>
	/// Class that contains Type extensions
	/// </summary>
	internal static class TypeExtensions
	{
		private static Dictionary<Type, object> DefaultValueTypes = new Dictionary<Type, object>();

		/// <summary>
		/// Gets the properties according to the BindingFlags passed as parameter
		/// </summary>
		/// <param name="type">Type for getting the properties</param>
		/// <param name="bindingFlags">BindingFlags for getting the properties</param>
		/// <returns>An array of property info</returns>
		internal static PropertyInfo[] GetPropertiesFromBindingFlags(this Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance) => type.GetProperties(bindingFlags);

		/// <summary>
		/// Gets the default value according to its type
		/// </summary>
		/// <param name="type">Type for getting the default value</param>
		/// <returns>he default value</returns>
		public static object GetDefaultValue(this Type type)
		{
			if (!type.IsValueType)
			{
				return null;
			}

			if (DefaultValueTypes.TryGetValue(type, out var defaultValue))
			{
				return defaultValue;
			}

			defaultValue = Activator.CreateInstance(type);

			Dictionary<Type, object> snapshot, newCache;

			do
			{
				snapshot = DefaultValueTypes;
				newCache = new Dictionary<Type, object>(DefaultValueTypes) { [type] = defaultValue };

			} while (!ReferenceEquals(Interlocked.CompareExchange(ref DefaultValueTypes, newCache, snapshot), snapshot));

			return defaultValue;
		}
	}
}
