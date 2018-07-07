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
using DataTablePlus.DataAccess.Services;
using DataTablePlus.DataAccessContracts.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DataTablePlus.Extensions
{
	/// <summary>
	/// Class that contains Enumerable extensions
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Gets the entity type from the enumerable
		/// </summary>
		/// <typeparam name="T">Type of the enumerable</typeparam>
		/// <param name="enumerable">Current enumerable of objects</param>
		/// <returns>Returns the entity type from the enumerable</returns>
		public static Type GetTypeFromEnumerable<T>(this IEnumerable<T> enumerable)
		{
			return typeof(T);
		}

		/// <summary>
		/// Transforms a list of objects into a data table
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="objects">List of objects</param>
		/// <param name="useDbContextMappings">A flag that indicates if it shall use DbContext mappings</param>
		/// <returns>A data table</returns>
		public static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, bool? useDbContextMappings = true) where T : class
		{
			return AsStronglyTypedDataTable(objects, typeof(T), useDbContextMappings);
		}

		/// <summary>
		/// Transforms a list of objects into a data table
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="objects">List of objects</param>
		/// <param name="derivedObjectType">Derived type of the objects if there's polymorphism</param>
		/// <param name="useDbContextMappings">A flag that indicates if it shall use DbContext mappings</param>
		/// <returns>A data table</returns>
		public static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, Type derivedObjectType, bool? useDbContextMappings = true) where T : class
		{
			if (objects == null)
			{
				throw new ArgumentNullException(nameof(objects), $"{nameof(objects)} {CommonResources.CannotBeNull}");
			}

			if (derivedObjectType == null)
			{
				throw new ArgumentNullException(nameof(derivedObjectType), $"{nameof(derivedObjectType)} {CommonResources.CannotBeNull}");
			}

			DataTable dataTable = null;

			IDictionary<PropertyInfo, string> mappings = null;

			if (useDbContextMappings.GetValueOrDefault())
			{
				dataTable = GetTableSchemaFromDatabase(derivedObjectType, out mappings);
			}
			else
			{
				dataTable = GetTableSchemaFromEntityStructure(derivedObjectType, out mappings);

			}

			if (dataTable.Columns == null || dataTable.Columns.Count <= 0)
			{
				throw new ArgumentException($"{nameof(dataTable.Columns)} {CommonResources.CannotBeNullOrEmpty}", nameof(dataTable.Columns));
			}

			FillDataTable(objects, dataTable, mappings);

			dataTable.AcceptChanges();

			return dataTable;
		}

		/// <summary>
		/// Gets the table schema from database using DbContext mappings for getting some information as well
		/// </summary>
		/// <param name="derivedObjectType">Derived type of the objects if there's polymorphism</param>
		/// <param name="mappings">Mappings between the model properties and the mapped column names</param>
		/// <returns>A data table</returns>
		private static DataTable GetTableSchemaFromDatabase(Type derivedObjectType, out IDictionary<PropertyInfo, string> mappings)
		{
			using (IMetadataService metadataService = new MetadataService())
			{
				var tableName = metadataService.GetTableName(derivedObjectType);

				if (string.IsNullOrWhiteSpace(tableName))
				{
					throw new ArgumentNullException(nameof(tableName), $"{nameof(tableName)} {CommonResources.CannotBeNull}");
				}

				mappings = metadataService.GetMappings(derivedObjectType);

				if (mappings == null)
				{
					throw new ArgumentNullException(nameof(mappings), $"{nameof(mappings)} {CommonResources.CannotBeNull}");
				}

				var dataTable = metadataService.GetTableSchema(tableName);

				if (dataTable == null)
				{
					throw new ArgumentNullException(nameof(dataTable), $"{nameof(dataTable)} {CommonResources.CannotBeNull}");
				}

				return dataTable;
			}
		}

		/// <summary>
		/// Gets the table schema from entity structure
		/// </summary>
		/// <param name="derivedObjectType">Derived type of the objects if there's polymorphism</param>
		/// <param name="mappings">Mappings between the model properties and the mapped column names</param>
		/// <returns>A data table</returns>
		private static DataTable GetTableSchemaFromEntityStructure(Type derivedObjectType, out IDictionary<PropertyInfo, string> mappings)
		{
			const string Schema = "dbo";

			var properties = derivedObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			mappings = properties?.ToDictionary(property => property, property => property.Name);

			var dataTable = new DataTable
			{
				TableName = string.Concat(Constants.LeftSquareBracket, Schema, Constants.RigthSquareBracket, Constants.FullStop, Constants.LeftSquareBracket, derivedObjectType.Name, Constants.RigthSquareBracket)
			};

			if (properties != null && properties.Any())
			{
				foreach (var property in properties)
				{
					var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

					dataTable.Columns.Add(property.Name, propertyType);
				}
			}

			return dataTable;
		}

		/// <summary>
		/// Fill out the data table getting the values from the list of objects
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="objects">List of objects</param>
		/// <param name="dataTable">Data table to be filled out</param>
		/// <param name="mappings">Mappings between the model properties and the mapped column names</param>
		/// <returns></returns>
		private static DataTable FillDataTable<T>(IEnumerable<T> objects, DataTable dataTable, IDictionary<PropertyInfo, string> mappings) where T : class
		{
			foreach (var obj in objects)
			{
				var dataRow = dataTable.NewRow();

				foreach (var mapping in mappings)
				{
					var property = mapping.Key;

					var value = property.GetValue(obj);

					if (value != null)
					{
						if (property.PropertyType.IsEnum)
						{
							dataRow[mapping.Value] = value.GetHashCode();
						}
						else
						{
							dataRow[mapping.Value] = value;
						}
					}
					else
					{
						var column = dataTable.Columns[mapping.Value];

						if (!column.AllowDBNull)
						{
							if (column.DataType == typeof(string))
							{
								dataRow[mapping.Value] = string.Empty;
							}
							else
							{
								dataRow[mapping.Value] = column.DataType.GetDefaultValue();
							}
						}
						else
						{
							dataRow[mapping.Value] = DBNull.Value;
						}
					}
				}

				dataTable.Rows.Add(dataRow);
			}

			return dataTable;
		}
	}
}
