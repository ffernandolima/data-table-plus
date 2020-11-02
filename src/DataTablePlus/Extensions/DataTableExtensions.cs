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
        public static IList<T> ToList<T>(this DataTable dataTable) where T : class => Transform<T>(dataTable).ToList();

        /// <summary>
        /// Transforms a data table into an array of objects 
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <param name="dataTable">Current data table to be transformed</param>
        /// <returns>A new array of objects</returns>
        public static T[] ToArray<T>(this DataTable dataTable) where T : class => Transform<T>(dataTable).ToArray();

        /// <summary>
        /// Transforms a data table into an enumerable of objects
        /// </summary>
        /// <typeparam name="T">Type of the objects</typeparam>
        /// <param name="dataTable">Current data table to be transformed</param>
        /// <returns>A new enumerable of objects</returns>
        private static IEnumerable<T> Transform<T>(DataTable dataTable) where T : class
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
