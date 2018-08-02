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
using System.Linq;

namespace DataTablePlus.Mappings
{
	/// <summary>
	/// Class that allows creating some mappings which represent a database table
	/// </summary>
	public class TableMapping : ITableMapping
	{
		/// <summary>
		/// Database schema field
		/// </summary>
		private string _schema;

		/// <summary>
		/// Datatabase table name field
		/// </summary>
		private string _tableName;

		/// <summary>
		/// Database table column mappings field
		/// </summary>
		private readonly IList<IColumnMapping> _columnMappings;

		/// <summary>
		/// Ctor
		/// </summary>
		public TableMapping()
		{
			this.Schema = Constants.DefaultSchema;
			this._columnMappings = new List<IColumnMapping>();
		}

		/// <summary>
		/// Database schema
		/// </summary>
		public string Schema
		{
			get => this._schema;
			set
			{
				this.ValidateSchema(value);
				this._schema = value;
			}
		}

		/// <summary>
		/// Datatabase table name
		/// </summary>
		public string TableName
		{
			get => this._tableName;
			set
			{
				this.ValidateTableName(value);
				this._tableName = value;
			}
		}

		/// <summary>
		/// Datatabase table primary key names
		/// </summary>
		public IList<string> PrimaryKeyNames => this.ColumnMappings.Where(mapping => mapping != null && mapping.IsPrimaryKey).Select(mapping => mapping.Name).ToList();

		/// <summary>
		/// Database table column mappings ordered by ordinal property
		/// </summary>
		public IReadOnlyList<IColumnMapping> ColumnMappings => this._columnMappings.Where(mapping => mapping != null).OrderBy(mapping => mapping.Ordinal).ToList().AsReadOnly();

		/// <summary>
		/// Validates the whole table mapping object including the column mappings
		/// </summary>
		public void Validate()
		{
			this.ValidateTableName();
			this.ValidateSchema();

			if (!this._columnMappings.Any())
			{
				throw new ArgumentException($"{nameof(this._columnMappings)} {CommonResources.CannotBeEmpty}", nameof(this._columnMappings));
			}

			foreach (var columnMapping in this._columnMappings)
			{
				columnMapping.Validate();
			}
		}

		/// <summary>
		/// Creates a new TableMapping object
		/// </summary>
		/// <returns>TableMapping object (Builder pattern)</returns>
		public static ITableMapping Create() => new TableMapping();

		/// <summary>
		/// Adds a schema 
		/// </summary>
		/// <param name="schema">Schema</param>
		/// <returns>TableMapping object (Builder pattern)</returns>
		public ITableMapping AddSchema(string schema)
		{
			this.Schema = schema;

			return this;
		}

		/// <summary>
		/// Adds a table name
		/// </summary>
		/// <param name="tableName">Table name</param>
		/// <returns>TableMapping object (Builder pattern)</returns>
		public ITableMapping AddTableName(string tableName)
		{
			this.TableName = tableName;

			return this;
		}

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

			return this.AddColumnMapping(mapping);
		}

		/// <summary>
		/// Allows adding a new column mapping
		/// </summary>
		/// <param name="columnMapping">Column mapping object</param>
		/// <returns>TableMapping object (Builder pattern)</returns>
		public ITableMapping AddColumnMapping(IColumnMapping columnMapping)
		{
			if (columnMapping == null)
			{
				throw new ArgumentNullException(nameof(columnMapping), $"{nameof(columnMapping)} {CommonResources.CannotBeNull}");
			}

			if (!this._columnMappings.Any())
			{
				columnMapping.Ordinal = 0;
			}
			else
			{
				var last = this._columnMappings.Where(mapping => mapping != null).OrderBy(mapping => mapping.Ordinal).Last();

				columnMapping.Ordinal = last.Ordinal;
				columnMapping.Ordinal++;
			}

			this._columnMappings.Add(columnMapping);

			return this;
		}

		/// <summary>
		/// Validates the provided schema
		/// </summary>
		private void ValidateSchema() => this.ValidateSchema(this.Schema);

		/// <summary>
		/// Validates the provided schema
		/// </summary>
		/// <param name="schema">Schema name</param>
		private void ValidateSchema(string schema)
		{
			if (string.IsNullOrWhiteSpace(schema))
			{
				throw new ArgumentException($"{nameof(schema)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(schema));
			}
		}

		/// <summary>
		/// Validates the provided table name
		/// </summary>
		private void ValidateTableName() => this.ValidateTableName(this.TableName);

		/// <summary>
		/// Validates the provided table name
		/// </summary>
		/// <param name="tableName">Table name</param>
		private void ValidateTableName(string tableName)
		{
			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"{nameof(tableName)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(tableName));
			}
		}
	}
}
