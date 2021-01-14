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
using System.Data;

namespace DataTablePlus.Mappings
{
    /// <summary>
    /// Interface IColumnMapping
    /// </summary>
    public interface IColumnMapping
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        Type Type { get; set; }

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it's primary key.
        /// </summary>
        /// <value><c>true</c> if it's primary key; otherwise, <c>false</c>.</value>
        bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it allows null.
        /// </summary>
        /// <value><c>true</c> if it allows null; otherwise, <c>false</c>.</value>
        bool AllowNull { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>The default value.</value>
        object DefaultValue { get; set; }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        void Validate();

        /// <summary>
        /// Adds the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IColumnMapping.</returns>
        IColumnMapping AddName(string name);

        /// <summary>
        /// Adds the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>IColumnMapping.</returns>
        IColumnMapping AddType(Type type);

        /// <summary>
        /// Adds the ordinal.
        /// </summary>
        /// <param name="ordinal">The ordinal.</param>
        /// <returns>IColumnMapping.</returns>
        IColumnMapping AddOrdinal(int ordinal);

        /// <summary>
        /// Sets the flag indicating whether it's the primary key.
        /// </summary>
        /// <param name="primaryKey">If set to <c>true</c> it's primary key.</param>
        /// <returns>IColumnMapping.</returns>
        IColumnMapping PrimaryKey(bool primaryKey);

        /// <summary>
        /// Sets the flag indicating whether it accepts null.
        /// </summary>
        /// <param name="acceptNull">If set to <c>true</c> it accepts null.</param>
        /// <returns>IColumnMapping.</returns>
        IColumnMapping AcceptNull(bool acceptNull);

        /// <summary>
        /// Adds the default value.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>IColumnMapping.</returns>
        IColumnMapping AddDefaultValue(object defaultValue);

        /// <summary>
        /// Converts the instance to DataColumn type.
        /// </summary>
        /// <returns>DataColumn.</returns>
        DataColumn AsDataColumn();
    }
}
