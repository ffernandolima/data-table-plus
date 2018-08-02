﻿/*****************************************************************************************************************
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
using DataTablePlus.Extensions;
using System;
using System.Data;

namespace DataTablePlus.Mappings
{
	/// <summary>
	/// Class that allows creating some mappings which represent a database table column
	/// </summary>
	public class ColumnMapping : IColumnMapping
	{
		/// <summary>
		/// Database table column name field
		/// </summary>
		private string _name;

		/// <summary>
		/// Database table column data type field
		/// </summary>
		private Type _type;

		/// <summary>
		/// Database table column order field
		/// </summary>
		private int _ordinal;

		/// <summary>
		/// Database table column default value field
		/// It will be used if the column doesn't allow null values
		/// </summary>
		private object _defaultValue;

		/// <summary>
		/// Ctor
		/// </summary>
		public ColumnMapping() => this.EnsureDefaultValue();

		/// <summary>
		/// Database table column name
		/// </summary>
		public string Name
		{
			get => this._name;
			set
			{
				this.ValidateName(value);
				this._name = value;
			}
		}

		/// <summary>
		/// Database table column data type
		/// </summary>
		public Type Type
		{
			get => this._type;
			set
			{
				this.ValidateType(value);
				this._type = Nullable.GetUnderlyingType(value) ?? value;
			}
		}

		/// <summary>
		/// Database table column order
		/// </summary>
		public int Ordinal
		{
			get => this._ordinal;
			set
			{
				this.ValidateOrdinal(value);
				this._ordinal = value;
			}
		}

		/// <summary>
		/// A flag that indicates if it is the primary key
		/// </summary>
		public bool IsPrimaryKey { get; set; }

		/// <summary>
		/// A flag that indicates if it allows null values
		/// </summary>
		public bool AllowNull { get; set; }

		/// <summary>
		/// Database table column default value
		/// It will be used if the column doesn't allow null values
		/// </summary>
		public object DefaultValue
		{
			get => this._defaultValue;
			set
			{
				this.ValidateDataType(value);
				this._defaultValue = value;
			}
		}

		/// <summary>
		/// Validates the whole column mapping object
		/// </summary>
		public void Validate()
		{
			this.ValidateName();
			this.ValidateType();
			this.ValidateOrdinal();

			this.EnsureDefaultValue();
			this.ValidateAllowNull();

			this.ValidateDataType();
		}

		/// <summary>
		/// Creates a new ColumnMapping object
		/// </summary>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		public static IColumnMapping Create() => new ColumnMapping();

		/// <summary>
		/// Adds a column name
		/// </summary>
		/// <param name="name">Column name</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		public IColumnMapping AddName(string name)
		{
			this.Name = name;

			return this;
		}

		/// <summary>
		/// Adds a column type
		/// </summary>
		/// <param name="type">Column type</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		public IColumnMapping AddType(Type type)
		{
			this.Type = type;

			return this;
		}

		/// <summary>
		/// Adds a column ordinal
		/// </summary>
		/// <param name="ordinal">Column ordinal</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		public IColumnMapping AddOrdinal(int ordinal)
		{
			this.Ordinal = ordinal;

			return this;
		}

		/// <summary>
		/// Indicates if it is the primary key
		/// </summary>
		/// <param name="primaryKey">Primary key flag</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		public IColumnMapping PrimaryKey(bool primaryKey)
		{
			this.IsPrimaryKey = primaryKey;

			return this;
		}

		/// <summary>
		/// Indicates if it accepts null values
		/// </summary>
		/// <param name="acceptNull">Accept null flag</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		public IColumnMapping AcceptNull(bool acceptNull)
		{
			this.AllowNull = acceptNull;

			return this;
		}

		/// <summary>
		/// Adds a column default value
		/// </summary>
		/// <param name="defaultValue">Column default value</param>
		/// <returns>ColumnMapping object (Builder pattern)</returns>
		public IColumnMapping AddDefaultValue(object defaultValue)
		{
			this.DefaultValue = defaultValue;

			return this;
		}

		/// <summary>
		/// Transforms this object to a new DataColumn object
		/// </summary>
		/// <returns>A new DataColumn object</returns>
		public DataColumn AsDataColumn()
		{
			var dataColumn = new DataColumn
			{
				ColumnName = this.Name,
				DataType = this.Type,
				AllowDBNull = this.AllowNull,
				DefaultValue = this.DefaultValue
			};

			return dataColumn;
		}

		/// <summary>
		/// Tries to set a default value to DefaultValue property based on the data type
		/// </summary>
		private void EnsureDefaultValue()
		{
			if (this.Type != null && this.DefaultValue == null)
			{
				if (this.Type == typeof(string))
				{
					this.DefaultValue = string.Empty;
				}
				else
				{
					this.DefaultValue = this.Type.GetDefaultValue();
				}
			}
		}

		/// <summary>
		/// Validates the provided column name
		/// </summary>
		private void ValidateName() => this.ValidateName(this.Name);

		/// <summary>
		/// Validates the provided column name
		/// </summary>
		/// <param name="name">Column name</param>
		private void ValidateName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"{nameof(name)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(name));
			}
		}

		/// <summary>
		/// Validates the provided data type
		/// </summary>
		private void ValidateType() => this.ValidateType(this.Type);

		/// <summary>
		/// Validates the provided data type
		/// </summary>
		/// <param name="type">Data type</param>
		private void ValidateType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentException($"{nameof(type)} {CommonResources.CannotBeNull}", nameof(type));
			}
		}

		/// <summary>
		/// Validates the provided ordinal value
		/// </summary>
		private void ValidateOrdinal() => this.ValidateOrdinal(this.Ordinal);

		/// <summary>
		/// Validates the provided ordinal value
		/// </summary>
		/// <param name="ordinal">Ordinal value</param>
		private void ValidateOrdinal(int ordinal)
		{
			if (ordinal < 0)
			{
				throw new ArgumentException($"{nameof(ordinal)} {CommonResources.CannotBeLessThanZero}", nameof(ordinal));
			}
		}

		/// <summary>
		/// Validates if it doesn't allow null values and also doesn't have a default value
		/// </summary>
		private void ValidateAllowNull()
		{
			if (!this.AllowNull && this.DefaultValue == null)
			{
				throw new ArgumentException(CommonResources.NullValueIsNotAllowed);
			}
		}

		/// <summary>
		/// Validates if the default value type has the same type of the property data type
		/// </summary>
		private void ValidateDataType() => this.ValidateDataType(this.DefaultValue);

		/// <summary>
		/// Validates if the default value type has the same type of the property data type
		/// </summary>
		/// <param name="value">Default value</param>
		private void ValidateDataType(object value)
		{
			if (value != null && this.Type != null && value.GetType() != this.Type)
			{
				throw new ArgumentException(CommonResources.DataTypesDoNotMatch);
			}
		}
	}
}

