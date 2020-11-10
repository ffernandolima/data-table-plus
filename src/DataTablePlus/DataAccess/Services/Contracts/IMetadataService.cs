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
using System.Reflection;

namespace DataTablePlus.DataAccess.Services.Contracts
{
    /// <summary>
    /// Interface IMetadataService
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.Contracts.IServiceBase" />
    /// </summary>
    /// <seealso cref="DataTablePlus.DataAccess.Services.Contracts.IServiceBase" />
    public interface IMetadataService : IServiceBase
    {
        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <returns>System.String.</returns>
        string GetTableName<T>() where T : class;

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        string GetTableName(Type type);

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <returns>IDictionary&lt;PropertyInfo, System.String&gt;.</returns>
        IDictionary<PropertyInfo, string> GetMappings<T>() where T : class;

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IDictionary&lt;PropertyInfo, System.String&gt;.</returns>
        IDictionary<PropertyInfo, string> GetMappings(Type type);

        /// <summary>
        /// Gets the key names.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <returns>IList&lt;System.String&gt;.</returns>
        IList<string> GetKeyNames<T>() where T : class;

        /// <summary>
        /// Gets the key names.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IList&lt;System.String&gt;.</returns>
        IList<string> GetKeyNames(Type type);

        /// <summary>
        /// Gets the database key names.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <returns>IList&lt;System.String&gt;.</returns>
        IList<string> GetDbKeyNames<T>() where T : class;

        /// <summary>
        /// Gets the database key names.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IList&lt;System.String&gt;.</returns>
        IList<string> GetDbKeyNames(Type type);

        /// <summary>
        /// Gets the table schema.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>DataTable.</returns>
        DataTable GetTableSchema(string tableName);
    }
}
