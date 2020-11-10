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
using System.Linq;

namespace DataTablePlus.Mappings
{
    /// <summary>
    /// Class TableMapping.
    /// Implements the <see cref="DataTablePlus.Mappings.ITableMapping" />
    /// </summary>
    /// <seealso cref="DataTablePlus.Mappings.ITableMapping" />
    public class TableMapping : ITableMapping
    {
        /// <summary>
        /// The schema
        /// </summary>
        private string _schema;

        /// <summary>
        /// The name of the table
        /// </summary>
        private string _tableName;

        /// <summary>
        /// The column mappings
        /// </summary>
        private readonly IList<IColumnMapping> _columnMappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableMapping"/> class.
        /// </summary>
        public TableMapping()
        {
            _columnMappings = new List<IColumnMapping>();
        }

        /// <inheritdoc />
        public string Schema
        {
            get => _schema;
            set
            {
                ValidateSchema(value);
                _schema = value;
            }
        }

        /// <inheritdoc />
        public string TableName
        {
            get => _tableName;
            set
            {
                ValidateTableName(value);
                _tableName = value;
            }
        }

        /// <inheritdoc />
        public IList<string> PrimaryKeyNames { get => ColumnMappings.Where(mapping => mapping != null && mapping.IsPrimaryKey).Select(mapping => mapping.Name).ToList(); }

        /// <inheritdoc />
        public IReadOnlyList<IColumnMapping> ColumnMappings { get => _columnMappings.Where(mapping => mapping != null).OrderBy(mapping => mapping.Ordinal).ToList().AsReadOnly(); }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">_columnMappings</exception>
        public void Validate()
        {
            ValidateTableName();

            if (!_columnMappings.Any())
            {
                throw new ArgumentException(nameof(_columnMappings));
            }

            foreach (var columnMapping in _columnMappings)
            {
                columnMapping.Validate();
            }
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>ITableMapping.</returns>
        public static ITableMapping Create()
        {
            return new TableMapping();
        }

        /// <inheritdoc />
        public ITableMapping AddSchema(string schema)
        {
            Schema = schema;

            return this;
        }

        /// <inheritdoc />
        public ITableMapping AddTableName(string tableName)
        {
            TableName = tableName;

            return this;
        }

        /// <inheritdoc />
        public ITableMapping AddColumnMapping(string columnName, Type columnType, int? ordinal = null, bool? isPrimaryKey = null, bool? allowNull = null, object defaultValue = null)
        {
            IColumnMapping mapping = new ColumnMapping
            {
                Name = columnName,
                Type = columnType,
                Ordinal = ordinal.GetValueOrDefault(),
                IsPrimaryKey = isPrimaryKey.GetValueOrDefault(),
                AllowNull = allowNull.GetValueOrDefault(),
                DefaultValue = defaultValue
            };

            return AddColumnMapping(mapping);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">columnMapping</exception>
        public ITableMapping AddColumnMapping(IColumnMapping columnMapping)
        {
            if (columnMapping == null)
            {
                throw new ArgumentNullException(nameof(columnMapping));
            }

            if (!_columnMappings.Any())
            {
                columnMapping.Ordinal = 0;
            }
            else
            {
                var last = _columnMappings.Where(mapping => mapping != null).OrderBy(mapping => mapping.Ordinal).Last();

                columnMapping.Ordinal = last.Ordinal;
                columnMapping.Ordinal++;
            }

            _columnMappings.Add(columnMapping);

            return this;
        }

        /// <summary>
        /// Validates the schema.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <exception cref="ArgumentException">schema</exception>
        private void ValidateSchema(string schema)
        {
            if (string.IsNullOrWhiteSpace(schema))
            {
                throw new ArgumentException(nameof(schema));
            }
        }

        /// <summary>
        /// Validates the name of the table.
        /// </summary>
        private void ValidateTableName()
        {
            ValidateTableName(TableName);
        }

        /// <summary>
        /// Validates the name of the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <exception cref="ArgumentException">tableName</exception>
        private void ValidateTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException(nameof(tableName));
            }
        }
    }
}
