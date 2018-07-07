/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * See https://github.com/ffernandolima/data-table-plus for details.
 *
 * Copyright (C) 2018 Fernando Luiz de Lima
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 *
 * The GNU Lesser General Public License can be viewed at http://www.opensource.org/licenses/lgpl-license.php
 * If you unfamiliar with this license or have questions about it, here is a FAQ: http://www.gnu.org/licenses/gpl-faq.html
 *
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 * 
 *******************************************************************************/

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
		internal static PropertyInfo[] GetPropertiesFromBindingFlags(this Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
		{
			var properties = type.GetProperties(bindingFlags);

			return properties;
		}

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
