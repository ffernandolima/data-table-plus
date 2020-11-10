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

namespace DataTablePlus.Mappings
{
    /// <summary>
    /// Interface ITableMapping
    /// </summary>
    public interface ITableMapping
    {
        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>The schema.</value>
        string Schema { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        string TableName { get; set; }

        /// <summary>
        /// Gets the primary key names.
        /// </summary>
        /// <value>The primary key names.</value>
        IList<string> PrimaryKeyNames { get; }

        /// <summary>
        /// Gets the column mappings.
        /// </summary>
        /// <value>The column mappings.</value>
        IReadOnlyList<IColumnMapping> ColumnMappings { get; }

        /// <summary>
        /// Adds the schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns>ITableMapping.</returns>
        ITableMapping AddSchema(string schema);

        /// <summary>
        /// Adds the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>ITableMapping.</returns>
        ITableMapping AddTableName(string tableName);

        /// <summary>
        /// Adds the column mapping.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="columnType">Type of the column.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="isPrimaryKey">if set to <c>true</c> it's primary key.</param>
        /// <param name="allowNull">if set to <c>true</c> it allows null.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>ITableMapping.</returns>
        ITableMapping AddColumnMapping(string columnName, Type columnType, int? ordinal = null, bool? isPrimaryKey = null, bool? allowNull = null, object defaultValue = null);

        /// <summary>
        /// Adds the column mapping.
        /// </summary>
        /// <param name="columnMapping">The column mapping.</param>
        /// <returns>ITableMapping.</returns>
        ITableMapping AddColumnMapping(IColumnMapping columnMapping);

        /// <summary>
        /// Validates this instance.
        /// </summary>
        void Validate();
    }
}
