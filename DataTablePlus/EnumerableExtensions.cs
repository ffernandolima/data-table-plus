/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 *
 * DataTablePlus provides some extensions in order to transform list of objects in data tables
 * based on the object mappings (Mappings which come from EntityFramework configurations) and also some sql helpers which perform
 * some batch operations using the data tables previously built.
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
using DataTablePlus.DataAccessContracts;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTablePlus.Extensions
{
	/// <summary>
	/// Class that contains Enumerable extensions
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Transforms a list of objects into a data table
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="objects">List of objects</param>
		/// <returns>A data table</returns>
		public static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects) where T : class
		{
			if (objects == null)
			{
				throw new ArgumentNullException(nameof(objects));
			}

			return AsStronglyTypedDataTable(objects, typeof(T));
		}

		/// <summary>
		/// Transforms a list of objects into a data table
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="objects">List of objects</param>
		/// <param name="derivedObjectType">Derived type of the objects if there's polymorphism</param>
		/// <returns>A data table</returns>
		public static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, Type derivedObjectType) where T : class
		{
			if (objects == null)
			{
				throw new ArgumentNullException(nameof(objects));
			}

			if (derivedObjectType == null)
			{
				throw new ArgumentNullException(nameof(derivedObjectType));
			}

			MetadataService metadataService = null;

			try
			{
				metadataService = new MetadataService();

				var tableName = metadataService.GetTableName(derivedObjectType);

				var mappings = metadataService.GetMappings(derivedObjectType);

				if (string.IsNullOrWhiteSpace(tableName))
				{
					throw new ArgumentNullException(nameof(tableName));
				}

				if (mappings == null)
				{
					throw new ArgumentNullException(nameof(mappings));
				}

				var dataTable = metadataService.GetTableSchema(tableName);

				if (dataTable == null || dataTable.Columns == null)
				{
					throw new ArgumentNullException(nameof(dataTable));
				}

				var dataColumn = new IgnoredDataColumn
				{
					ColumnName = "end_line",
					DataType = typeof(string)
				};

				dataTable.Columns.Add(dataColumn);

				return FillDataTable(objects, dataTable, mappings);
			}
			finally
			{
				if (metadataService != null)
				{
					metadataService.Dispose();
				}
			}
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
			if (objects == null)
			{
				throw new ArgumentNullException(nameof(objects));
			}

			if (dataTable == null)
			{
				throw new ArgumentNullException(nameof(dataTable));
			}

			if (mappings == null)
			{
				throw new ArgumentNullException(nameof(mappings));
			}

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

				dataRow[dataTable.Columns.Count - 1] = Constants.END_LINE;

				dataTable.Rows.Add(dataRow);
			}

			dataTable.AcceptChanges();

			return dataTable;
		}
	}
}
