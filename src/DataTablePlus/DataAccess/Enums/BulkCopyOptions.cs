﻿/*****************************************************************************************************************
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

namespace DataTablePlus.DataAccess.Enums
{
    /// <summary>
    /// Enum BulkCopyOptions
    /// </summary>
    [Flags]
    public enum BulkCopyOptions : int
    {
        /// <summary>
        /// The default
        /// </summary>
        Default = 0,

        /// <summary>
        /// The keep identity
        /// </summary>
        KeepIdentity = 1,

        /// <summary>
        /// The check constraints
        /// </summary>
        CheckConstraints = 2,

        /// <summary>
        /// The table lock
        /// </summary>
        TableLock = 4,

        /// <summary>
        /// The keep nulls
        /// </summary>
        KeepNulls = 8,

        /// <summary>
        /// The fire triggers
        /// </summary>
        FireTriggers = 16,

        /// <summary>
        /// The use internal transaction
        /// </summary>
        UseInternalTransaction = 32
    }
}
