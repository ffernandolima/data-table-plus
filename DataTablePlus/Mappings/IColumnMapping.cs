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
using System.Data;

namespace DataTablePlus.Mappings
{
	/// <summary>
	/// Interface that represents the contract of ColumnMapping object
	/// </summary>
	public interface IColumnMapping
	{
		/// <summary>
		/// Database table column name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Database table column data type
		/// </summary>
		Type Type { get; set; }

		/// <summary>
		/// Database table column order
		/// </summary>
		int Ordinal { get; set; }

		/// <summary>
		/// A flag that indicates if it is the primary key
		/// </summary>
		bool IsPrimaryKey { get; set; }

		/// <summary>
		/// A flag that indicates if it allows null values
		/// </summary>
		bool AllowNull { get; set; }

		/// <summary>
		/// Database table column default value
		/// It will be used if the column doesn't allow null values
		/// </summary>
		object DefaultValue { get; set; }

		/// <summary>
		/// Validates the whole column mapping object
		/// </summary>
		void Validate();

		/// <summary>
		/// Adds a column name
		/// </summary>
		/// <param name="name">Column name</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		IColumnMapping AddName(string name);

		/// <summary>
		/// Adds a column type
		/// </summary>
		/// <param name="type">Column type</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		IColumnMapping AddType(Type type);

		/// <summary>
		/// Adds a column ordinal
		/// </summary>
		/// <param name="ordinal">Column ordinal</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		IColumnMapping AddOrdinal(int ordinal);

		/// <summary>
		/// Indicates if it is the primary key
		/// </summary>
		/// <param name="primaryKey">Primary key flag</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		IColumnMapping PrimaryKey(bool primaryKey);

		/// <summary>
		/// Indicates if it accepts null values
		/// </summary>
		/// <param name="acceptNull">Accept null flag</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		IColumnMapping AcceptNull(bool acceptNull);

		/// <summary>
		/// Adds a column default value
		/// </summary>
		/// <param name="defaultValue">Column default value</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		IColumnMapping AddDefaultValue(object defaultValue);

		/// <summary>
		/// Transforms this object to a new DataColumn object
		/// </summary>
		/// <returns>A new DataColumn object</returns>
		DataColumn AsDataColumn();
	}
}
