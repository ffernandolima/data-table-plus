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

using DataTablePlus.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataTablePlus.Extensions
{
	/// <summary>
	/// Class that contains DataTable extensions
	/// </summary>
	public static class DataTableExtensions
	{
		/// <summary>
		/// Generic method that validates the provided parameters to avoid any kind of problem during the execution
		/// </summary>
		/// <param name="dataTable">Current data table to be validated</param>
		internal static void ValidateParameters(this DataTable dataTable)
		{
			if (dataTable == null)
			{
				throw new ArgumentNullException(nameof(dataTable), $"{nameof(dataTable)} {CommonResources.CannotBeNull}");
			}

			if (dataTable.Columns == null || dataTable.Columns.Count <= 0)
			{
				throw new ArgumentException($"{nameof(dataTable.Columns)} {CommonResources.CannotBeNullOrEmpty}", nameof(dataTable.Columns));
			}

			if (dataTable.Rows == null || dataTable.Rows.Count <= 0)
			{
				throw new ArgumentException($"{nameof(dataTable.Rows)} {CommonResources.CannotBeNullOrEmpty}", nameof(dataTable.Rows));
			}
		}

		/// <summary>
		/// Transforms a data table into a list of objects
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dataTable">Current data table to be transformed</param>
		/// <returns>A new list of objects</returns>
		public static IList<T> ToList<T>(this DataTable dataTable) where T : class, new()
		{
			var entities = Transform<T>(dataTable);

			return entities.ToList();
		}

		/// <summary>
		/// Transforms a data table into an array of objects
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dataTable">Current data table to be transformed</param>
		/// <returns>A new array of objects</returns>
		public static T[] ToArray<T>(this DataTable dataTable) where T : class, new()
		{
			var entities = Transform<T>(dataTable);

			return entities.ToArray();
		}

		/// <summary>
		/// Transforms a data table into an enumerable of objects
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dataTable">Current data table to be transformed</param>
		/// <returns>A new enumerable of objects</returns>
		private static IEnumerable<T> Transform<T>(DataTable dataTable) where T : class, new()
		{
			dataTable.ValidateParameters();

			return TransformInternal<T>(dataTable);
		}

		/// <summary>
		/// Transforms a data table into an enumerable of objects (internal)
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dataTable">Current data table to be transformed</param>
		/// <returns>A new enumerable of objects</returns>
		private static IEnumerable<T> TransformInternal<T>(DataTable dataTable) where T : class, new()
		{
			var entities = new List<T>();

			var entityType = typeof(T);

			var dataColumnNames = dataTable.Columns.Cast<DataColumn>().Select(dataColumn => dataColumn.ColumnName);

			var properties = entityType.GetPropertiesFromBindingFlags();

			foreach (var dataRow in dataTable.Rows.Cast<DataRow>())
			{
				var entity = new T();

				foreach (var property in properties.Where(property => dataColumnNames.Contains(property.Name)))
				{
					var dataRowValue = dataRow[property.Name];

					if (dataRowValue == null || dataRowValue == DBNull.Value)
					{
						property.SetValue(entity, null, null);
					}
					else
					{
						var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

						var value = Convert.ChangeType(dataRowValue, underlyingType);

						property.SetValue(entity, value, null);
					}
				}

				entities.Add(entity);
			}

			return entities;
		}
	}
}
