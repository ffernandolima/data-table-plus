﻿/*******************************************************************************
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
using DataTablePlus.Mappings;
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
		/// <typeparam name="T">Type of the enumerable values</typeparam>
		/// <param name="enumerable">Current enumerable of objects</param>
		/// <returns>Returns the entity type from the enumerable</returns>
		public static Type GetTypeFromEnumerable<T>(this IEnumerable<T> enumerable) => typeof(T);

		/// <summary>
		/// Transforms a list of objects into a data table
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="objects">List of objects</param>
		/// <param name="useDbContextMappings">A flag that indicates if it shall use DbContext mappings</param>
		/// <returns>A data table</returns>
		public static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, bool? useDbContextMappings = true) where T : class => AsStronglyTypedDataTable(objects, typeof(T), useDbContextMappings);

		/// <summary>
		/// Transforms a list of objects into a data table
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="objects">List of objects</param>
		/// <param name="derivedObjectType">Derived type of objects if there's polymorphism</param>
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

			dataTable.Populate(objects, mappings);

			dataTable.AcceptChanges();

			return dataTable;
		}

		/// <summary>
		/// Transforms a list of object arrays into a data table
		/// </summary>
		/// <param name="objects">List of object arrays</param>
		/// <param name="tableMapping">An object that contains the table mapping as well as its columns and so on</param>
		/// <returns>A data table</returns>
		public static DataTable AsStronglyTypedDataTable(this IEnumerable<object[]> objects, TableMapping tableMapping)
		{
			if (objects == null)
			{
				throw new ArgumentNullException(nameof(objects), $"{nameof(objects)} {CommonResources.CannotBeNull}");
			}

			if (tableMapping == null)
			{
				throw new ArgumentNullException(nameof(tableMapping), $"{nameof(tableMapping)} {CommonResources.CannotBeNull}");
			}

			if (objects.Any(x => (x?.Length).GetValueOrDefault() != tableMapping.ColumnMappings.Count))
			{
				throw new ArgumentException(CommonResources.InvalidLength);
			}

			var dataTable = GetTableSchemaFromTableMapping(tableMapping);

			if (dataTable.Columns == null || dataTable.Columns.Count <= 0)
			{
				throw new ArgumentException($"{nameof(dataTable.Columns)} {CommonResources.CannotBeNullOrEmpty}", nameof(dataTable.Columns));
			}

			dataTable.Populate(objects, tableMapping);

			dataTable.AcceptChanges();

			return dataTable;
		}

		/// <summary>
		/// Gets the table schema from database using DbContext mappings for getting some information as well
		/// </summary>
		/// <param name="derivedObjectType">Derived type of objects if there's polymorphism</param>
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
		/// <param name="derivedObjectType">Derived type of objects if there's polymorphism</param>
		/// <param name="mappings">Mappings between the model properties and the mapped column names</param>
		/// <returns>A data table</returns>
		private static DataTable GetTableSchemaFromEntityStructure(Type derivedObjectType, out IDictionary<PropertyInfo, string> mappings)
		{
			var properties = derivedObjectType.GetPropertiesFromBindingFlags();

			mappings = properties?.ToDictionary(property => property, property => property.Name);

			var dataTable = new DataTable
			{
				TableName = $"[{Constants.DefaultSchema}].[{derivedObjectType.Name}]"
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
		/// Gets the table schema from table mapping object
		/// </summary>
		/// <param name="tableMapping">Table mappings as well as its columns and so on</param>
		/// <returns>A data table</returns>
		private static DataTable GetTableSchemaFromTableMapping(TableMapping tableMapping)
		{
			tableMapping.Validate();

			var dataTable = new DataTable
			{
				TableName = $"[{tableMapping.Schema}].[{tableMapping.TableName}]"
			};

			foreach (var columnMapping in tableMapping.ColumnMappings)
			{
				dataTable.Columns.Add(columnMapping.AsDataColumn());
			}

			return dataTable;
		}

		/// <summary>
		/// Fill out the data table getting the values from the list of objects
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dataTable">Data table to be filled out</param>
		/// <param name="objects">List of objects</param>
		/// <param name="mappings">Mappings between the model properties and the mapped column names</param>
		private static void Populate<T>(this DataTable dataTable, IEnumerable<T> objects, IDictionary<PropertyInfo, string> mappings) where T : class
		{
			var internalObjects = objects.Where(item => item != null);

			foreach (var item in internalObjects)
			{
				var dataRow = dataTable.NewRow();

				foreach (var mapping in mappings)
				{
					var property = mapping.Key;

					var columnName = mapping.Value;

					var value = property.GetValue(item);

					if (value != null)
					{
						if (property.PropertyType.IsEnum)
						{
							dataRow[columnName] = value.GetHashCode();
						}
						else
						{
							var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

							value = Convert.ChangeType(value, underlyingType);

							dataRow[columnName] = value;
						}
					}
					else
					{
						var column = dataTable.Columns[columnName];

						if (!column.AllowDBNull)
						{
							if (column.DataType == typeof(string))
							{
								dataRow[columnName] = string.Empty;
							}
							else
							{
								dataRow[columnName] = column.DataType.GetDefaultValue();
							}
						}
						else
						{
							dataRow[columnName] = DBNull.Value;
						}
					}
				}

				dataTable.Rows.Add(dataRow);
			}
		}

		/// <summary>
		/// Fill out the data table getting the values from the list of object arrays
		/// </summary>
		/// <param name="dataTable">Data table to be filled out</param>
		/// <param name="objects">List of object arrays</param>
		/// <param name="tableMapping">Table mappings as well as its columns and so on</param>
		private static void Populate(this DataTable dataTable, IEnumerable<object[]> objects, TableMapping tableMapping)
		{
			var internalObjects = objects.Where(item => item != null && item.Length > 0);

			foreach (var objectArray in internalObjects)
			{
				var dataRow = dataTable.NewRow();

				var columnMappings = tableMapping.ColumnMappings;

				for (int idx = 0; idx < columnMappings.Count; idx++)
				{
					var columnMapping = columnMappings[idx];

					var columnName = columnMapping.Name;

					var value = objectArray[idx];

					if (value != null)
					{
						if (columnMapping.Type.IsEnum)
						{
							dataRow[columnName] = value.GetHashCode();
						}
						else
						{
							value = Convert.ChangeType(value, columnMapping.Type);

							dataRow[columnName] = value;
						}
					}
					else
					{
						if (!columnMapping.AllowNull)
						{
							if (columnMapping.Type == typeof(string))
							{
								dataRow[columnName] = string.Empty;
							}
							else
							{
								dataRow[columnName] = columnMapping.DefaultValue;
							}
						}
						else
						{
							dataRow[columnName] = DBNull.Value;
						}
					}
				}

				dataTable.Rows.Add(dataRow);
			}
		}
	}
}
