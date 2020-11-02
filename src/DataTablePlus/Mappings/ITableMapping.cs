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

using System;
using System.Collections.Generic;

namespace DataTablePlus.Mappings
{
    /// <summary>
    /// Interface that represents the contract of TableMapping object
    /// </summary>
    public interface ITableMapping
    {
        /// <summary>
        /// Database schema
        /// </summary>
        string Schema { get; set; }

        /// <summary>
        /// Datatabase table name
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// Datatabase table primary key names
        /// </summary>
        IList<string> PrimaryKeyNames { get; }

        /// <summary>
        /// Database table column mappings ordered by ordinal property
        /// </summary>
        IReadOnlyList<IColumnMapping> ColumnMappings { get; }

        /// <summary>
        /// Adds a schema 
        /// </summary>
        /// <param name="schema">Schema</param>
        /// <returns>TableMapping object (Builder pattern)</returns>
        ITableMapping AddSchema(string schema);

        /// <summary>
        /// Adds a table name
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>TableMapping object (Builder pattern)</returns>
        ITableMapping AddTableName(string tableName);

        /// <summary>
        /// Allows adding a new column mapping
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="columnType">Column type</param>
        /// <param name="ordinal">Ordinal</param>
        /// <param name="isPrimaryKey">Is primary key</param>
        /// <param name="allowNull">Allows null</param>
        /// <param name="defaultValue">Default value</param>
        /// <returns>TableMapping object (Builder pattern)</returns>
        ITableMapping AddColumnMapping(string columnName, Type columnType, int? ordinal = null, bool? isPrimaryKey = null, bool? allowNull = null, object defaultValue = null);

        /// <summary>
        /// Allows adding a new column mapping
        /// </summary>
        /// <param name="columnMapping">Column mapping object</param>
        /// <returns>TableMapping object (Builder pattern)</returns>
        ITableMapping AddColumnMapping(IColumnMapping columnMapping);

        /// <summary>
        /// Validates the whole table mapping object including the column mappings
        /// </summary>
        void Validate();
    }
}
