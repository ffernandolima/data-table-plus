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
