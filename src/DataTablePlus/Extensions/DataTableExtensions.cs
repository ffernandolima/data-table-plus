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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataTablePlus.Extensions
{
    /// <summary>
    /// Class DataTableExtensions.
    /// </summary>
    public static class DataTableExtensions
    {
        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <exception cref="ArgumentNullException">dataTable</exception>
        /// <exception cref="ArgumentException">
        /// Columns
        /// or
        /// Rows
        /// </exception>
        internal static void ValidateParameters(this DataTable dataTable)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            if (dataTable.Columns == null || dataTable.Columns.Count <= 0)
            {
                throw new ArgumentException(nameof(dataTable.Columns));
            }

            if (dataTable.Rows == null || dataTable.Rows.Count <= 0)
            {
                throw new ArgumentException(nameof(dataTable.Rows));
            }
        }

        /// <summary>
        /// Converts to list.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <returns>IList&lt;T&gt;.</returns>
        public static IList<T> ToList<T>(this DataTable dataTable) where T : class 
        {
            return Transform<T>(dataTable).ToList(); 
        }

        /// <summary>
        /// Converts to array.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <returns>T[].</returns>
        public static T[] ToArray<T>(this DataTable dataTable) where T : class 
        {
            return Transform<T>(dataTable).ToArray(); 
        }

        /// <summary>
        /// Transforms the specified data table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        private static IEnumerable<T> Transform<T>(DataTable dataTable) where T : class
        {
            dataTable.ValidateParameters();

            return TransformInternal<T>(dataTable);
        }

        /// <summary>
        /// Transforms the specified data table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dataTable">The data table.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        private static IEnumerable<T> TransformInternal<T>(DataTable dataTable) where T : class
        {
            var entities = new List<T>();

            var entityType = typeof(T);

            var dataColumnNames = dataTable.Columns.Cast<DataColumn>().Select(dataColumn => dataColumn.ColumnName);

            var properties = entityType.GetPropertiesFromBindingFlags();

            foreach (var dataRow in dataTable.Rows.Cast<DataRow>())
            {
                var entity = (T)Activator.CreateInstance(entityType);

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
