/*****************************************************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * See https://github.com/ffernandolima/data-table-plus for details.
 *
 * MIT License
 * 
 * Copyright (c) 2020 Fernando Luiz de Lima
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

using DataTablePlus.DataAccess.Enums;
using DataTablePlus.Factories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

#if NETSTANDARD || NET60
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

using ITableMapping = DataTablePlus.Mappings.ITableMapping;

namespace DataTablePlus.Extensions
{
    /// <summary>
    /// Class EnumerableExtensions.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts the objects to a strongly typed data table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>DataTable.</returns>
        internal static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, DbProvider? dbProvider, DbContext dbContext) where T : class
        {
            return AsStronglyTypedDataTable(objects, typeof(T), dbProvider, dbContext);
        }

        /// <summary>
        /// Converts the objects to a strongly typed data table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <param name="derivedObjectType">Type of the derived object.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>DataTable.</returns>
        /// <exception cref="ArgumentNullException">
        /// objects
        /// or
        /// derivedObjectType
        /// or
        /// dbContext
        /// </exception>
        /// <exception cref="ArgumentException">Columns</exception>
        internal static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, Type derivedObjectType, DbProvider? dbProvider, DbContext dbContext) where T : class
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            if (derivedObjectType == null)
            {
                throw new ArgumentNullException(nameof(derivedObjectType));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            var dataTable = GetTableSchemaFromDatabase(derivedObjectType, out var mappings, dbProvider, dbContext);

            if (dataTable.Columns == null || dataTable.Columns.Count <= 0)
            {
                throw new ArgumentException(nameof(dataTable.Columns));
            }

            dataTable.Populate(objects, mappings);

            dataTable.AcceptChanges();

            return dataTable;
        }

        /// <summary>
        /// Converts the objects to a strongly typed data table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="useDbContextMappings">If set to <c>true</c> it uses the database context mappings.</param>
        /// <returns>DataTable.</returns>
        public static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, DbProvider? dbProvider = null, bool? useDbContextMappings = true) where T : class
        {
            return AsStronglyTypedDataTable(objects, typeof(T), dbProvider, useDbContextMappings);
        }

        /// <summary>
        /// Converts the objects to a strongly typed data table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="objects">The objects.</param>
        /// <param name="derivedObjectType">Type of the derived object.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="useDbContextMappings">If set to <c>true</c> it uses the database context mappings.</param>
        /// <returns>DataTable.</returns>
        /// <exception cref="ArgumentNullException">
        /// objects
        /// or
        /// derivedObjectType
        /// </exception>
        /// <exception cref="ArgumentException">Columns</exception>
        public static DataTable AsStronglyTypedDataTable<T>(this IEnumerable<T> objects, Type derivedObjectType, DbProvider? dbProvider = null, bool? useDbContextMappings = true) where T : class
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            if (derivedObjectType == null)
            {
                throw new ArgumentNullException(nameof(derivedObjectType));
            }

            DataTable dataTable;

            IDictionary<PropertyInfo, string> mappings;

            if (useDbContextMappings.GetValueOrDefault())
            {
                dataTable = GetTableSchemaFromDatabase(derivedObjectType, out mappings, dbProvider);
            }
            else
            {
                dataTable = GetTableSchemaFromEntityStructure(derivedObjectType, out mappings);
            }

            if (dataTable.Columns == null || dataTable.Columns.Count <= 0)
            {
                throw new ArgumentException(nameof(dataTable.Columns));
            }

            dataTable.Populate(objects, mappings);

            dataTable.AcceptChanges();

            return dataTable;
        }

        /// <summary>
        /// Converts the objects to a strongly typed data table.
        /// </summary>
        /// <param name="objects">The objects.</param>
        /// <param name="tableMapping">The table mapping.</param>
        /// <returns>DataTable.</returns>
        /// <exception cref="ArgumentNullException">
        /// objects
        /// or
        /// tableMapping
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Objects have different lengths.
        /// or
        /// Columns
        /// </exception>
        public static DataTable AsStronglyTypedDataTable(this IEnumerable<object[]> objects, ITableMapping tableMapping)
        {
            if (objects == null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            if (tableMapping == null)
            {
                throw new ArgumentNullException(nameof(tableMapping));
            }

            if (objects.Any(x => (x?.Length).GetValueOrDefault() != tableMapping.ColumnMappings.Count))
            {
                throw new ArgumentException("Objects have different lengths.");
            }

            var dataTable = GetTableSchemaFromTableMapping(tableMapping);

            if (dataTable.Columns == null || dataTable.Columns.Count <= 0)
            {
                throw new ArgumentException(nameof(dataTable.Columns));
            }

            dataTable.Populate(objects, tableMapping);

            dataTable.AcceptChanges();

            return dataTable;
        }

        /// <summary>
        /// Gets the type from enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>Type.</returns>
        public static Type GetTypeFromEnumerable<T>(this IEnumerable<T> enumerable)
        {
            return typeof(T);
        }

        /// <summary>
        /// Gets the table schema from database.
        /// </summary>
        /// <param name="derivedObjectType">Type of the derived object.</param>
        /// <param name="mappings">The mappings.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>DataTable.</returns>
        /// <exception cref="ArgumentNullException">
        /// tableName
        /// or
        /// mappings
        /// or
        /// dataTable
        /// </exception>
        private static DataTable GetTableSchemaFromDatabase(Type derivedObjectType, out IDictionary<PropertyInfo, string> mappings, DbProvider? dbProvider = null, DbContext dbContext = null)
        {
            var metadataService = MetadataServiceFactory.Instance.GetMetadataService(dbProvider, dbContext);

            try
            {
                var tableName = metadataService?.GetTableName(derivedObjectType);

                if (string.IsNullOrWhiteSpace(tableName))
                {
                    throw new ArgumentNullException(nameof(tableName));
                }

                mappings = metadataService?.GetMappings(derivedObjectType);

                if (mappings == null)
                {
                    throw new ArgumentNullException(nameof(mappings));
                }

                var dataTable = metadataService?.GetTableSchema(tableName);

                if (dataTable == null)
                {
                    throw new ArgumentNullException(nameof(dataTable));
                }

                return dataTable;
            }
            finally
            {
                metadataService?.Dispose();
            }
        }

        /// <summary>
        /// Gets the table schema from entity structure.
        /// </summary>
        /// <param name="derivedObjectType">Type of the derived object.</param>
        /// <param name="mappings">The mappings.</param>
        /// <returns>DataTable.</returns>
        private static DataTable GetTableSchemaFromEntityStructure(Type derivedObjectType, out IDictionary<PropertyInfo, string> mappings)
        {
            var properties = derivedObjectType.GetPropertiesFromBindingFlags();

            mappings = properties?.ToDictionary(property => property, property => property.Name);

            var dataTable = new DataTable
            {
                TableName = derivedObjectType.Name
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
        /// Gets the table schema from table mapping.
        /// </summary>
        /// <param name="tableMapping">The table mapping.</param>
        /// <returns>DataTable.</returns>
        private static DataTable GetTableSchemaFromTableMapping(ITableMapping tableMapping)
        {
            tableMapping.Validate();

            string tableName;

            if (!string.IsNullOrWhiteSpace(tableMapping.Schema))
            {
                tableName = $"{tableMapping.Schema}.{tableMapping.TableName}";
            }
            else
            {
                tableName = tableMapping.TableName;
            }

            var dataTable = new DataTable
            {
                TableName = tableName
            };

            foreach (var columnMapping in tableMapping.ColumnMappings)
            {
                dataTable.Columns.Add(columnMapping.AsDataColumn());
            }

            return dataTable;
        }

        /// <summary>
        /// Populates the specified data table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <param name="objects">The objects.</param>
        /// <param name="mappings">The mappings.</param>
        /// <param name="enforceConstraints">If set to <c>true</c>, it enforces constraints.</param>
        private static void Populate<T>(this DataTable dataTable, IEnumerable<T> objects, IDictionary<PropertyInfo, string> mappings, bool enforceConstraints = false) where T : class
        {
            if (!enforceConstraints)
            {
                dataTable.Constraints.Clear();
            }

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
        /// Populates the specified data table.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="objects">The objects.</param>
        /// <param name="tableMapping">The table mapping.</param>
        /// <param name="enforceConstraints">If set to <c>true</c>, it enforces constraints.</param>
        private static void Populate(this DataTable dataTable, IEnumerable<object[]> objects, ITableMapping tableMapping, bool enforceConstraints = false)
        {
            if (!enforceConstraints)
            {
                dataTable.Constraints.Clear();
            }

            var internalObjects = objects.Where(item => item != null && item.Length > 0);

            foreach (var objectArray in internalObjects)
            {
                var dataRow = dataTable.NewRow();

                var columnMappings = tableMapping.ColumnMappings;

                for (var idx = 0; idx < columnMappings.Count; idx++)
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
