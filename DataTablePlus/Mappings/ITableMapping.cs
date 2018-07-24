/*******************************************************************************
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
