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

using DataTablePlus.Extensions;
using System;
using System.Data;

namespace DataTablePlus.Mappings
{
    /// <summary>
    /// Class ColumnMapping.
    /// Implements the <see cref="DataTablePlus.Mappings.IColumnMapping" />
    /// </summary>
    /// <seealso cref="DataTablePlus.Mappings.IColumnMapping" />
    public class ColumnMapping : IColumnMapping
    {
        /// <summary>
        /// The name
        /// </summary>
        private string _name;

        /// <summary>
        /// The type
        /// </summary>
        private Type _type;

        /// <summary>
        /// The ordinal
        /// </summary>
        private int _ordinal;

        /// <summary>
        /// The default value
        /// </summary>
        private object _defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnMapping"/> class.
        /// </summary>
        public ColumnMapping()
        {
            EnsureDefaultValue();
        }

        /// <inheritdoc />
        public string Name
        {
            get => _name;
            set
            {
                ValidateName(value);
                _name = value;
            }
        }

        /// <inheritdoc />
        public Type Type
        {
            get => _type;
            set
            {
                ValidateType(value);
                _type = Nullable.GetUnderlyingType(value) ?? value;
            }
        }

        /// <inheritdoc />
        public int Ordinal
        {
            get => _ordinal;
            set
            {
                ValidateOrdinal(value);
                _ordinal = value;
            }
        }

        /// <inheritdoc />
        public bool IsPrimaryKey { get; set; }

        /// <inheritdoc />
        public bool AllowNull { get; set; }

        /// <inheritdoc />
        public object DefaultValue
        {
            get => _defaultValue;
            set
            {
                ValidateDataType(value);
                _defaultValue = value;
            }
        }

        /// <inheritdoc />
        public void Validate()
        {
            ValidateName();
            ValidateType();
            ValidateOrdinal();

            EnsureDefaultValue();
            ValidateAllowNull();

            ValidateDataType();
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        /// <returns>IColumnMapping.</returns>
        public static IColumnMapping Create()
        {
            return new ColumnMapping();
        }

        /// <inheritdoc />
        public IColumnMapping AddName(string name)
        {
            Name = name;

            return this;
        }

        /// <inheritdoc />
        public IColumnMapping AddType(Type type)
        {
            Type = type;

            return this;
        }

        /// <inheritdoc />
        public IColumnMapping AddOrdinal(int ordinal)
        {
            Ordinal = ordinal;

            return this;
        }

        /// <inheritdoc />
        public IColumnMapping PrimaryKey(bool primaryKey)
        {
            IsPrimaryKey = primaryKey;

            return this;
        }

        /// <inheritdoc />
        public IColumnMapping AcceptNull(bool acceptNull)
        {
            AllowNull = acceptNull;

            return this;
        }

        /// <inheritdoc />
        public IColumnMapping AddDefaultValue(object defaultValue)
        {
            DefaultValue = defaultValue;

            return this;
        }

        /// <inheritdoc />
        public DataColumn AsDataColumn()
        {
            var dataColumn = new DataColumn
            {
                ColumnName = Name,
                DataType = Type,
                AllowDBNull = AllowNull,
                DefaultValue = DefaultValue
            };

            return dataColumn;
        }

        /// <summary>
        /// Ensures the default value.
        /// </summary>
        private void EnsureDefaultValue()
        {
            if (Type != null && DefaultValue == null)
            {
                if (Type == typeof(string))
                {
                    DefaultValue = string.Empty;
                }
                else
                {
                    DefaultValue = Type.GetDefaultValue();
                }
            }
        }

        /// <summary>
        /// Validates the name.
        /// </summary>
        private void ValidateName()
        {
            ValidateName(Name);
        }

        /// <summary>
        /// Validates the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentException">name</exception>
        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }
        }

        /// <summary>
        /// Validates the type.
        /// </summary>
        private void ValidateType()
        {
            ValidateType(Type);
        }

        /// <summary>
        /// Validates the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentException">type</exception>
        private void ValidateType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentException(nameof(type));
            }
        }

        /// <summary>
        /// Validates the ordinal.
        /// </summary>
        private void ValidateOrdinal()
        {
            ValidateOrdinal(Ordinal);
        }

        /// <summary>
        /// Validates the ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <exception cref="ArgumentException">ordinal</exception>
        private void ValidateOrdinal(int ordinal)
        {
            if (ordinal < 0)
            {
                throw new ArgumentException(nameof(ordinal));
            }
        }

        /// <summary>
        /// Validates the allow null flag and the default value.
        /// </summary>
        /// <exception cref="ArgumentException">Null value is not allowed.</exception>
        private void ValidateAllowNull()
        {
            if (!AllowNull && DefaultValue == null)
            {
                throw new ArgumentException("Null value is not allowed.");
            }
        }

        /// <summary>
        /// Validates the type of the data.
        /// </summary>
        private void ValidateDataType()
        {
            ValidateDataType(DefaultValue);
        }

        /// <summary>
        /// Validates the type of the data.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentException">Data types do not match.</exception>
        private void ValidateDataType(object value)
        {
            if (value != null && Type != null && value.GetType() != Type)
            {
                throw new ArgumentException("Data types do not match.");
            }
        }
    }
}

