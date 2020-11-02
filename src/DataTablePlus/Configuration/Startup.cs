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

#if NETSTANDARD20
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.Configuration
{
    /// <summary>
    /// Startup class that should be used in order to initialize the application and provide some required configurations
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// Provided EF DbContext
        /// </summary>
        public static DbContext DbContext { get; private set; }

        /// <summary>
        /// Provided ConnectionString
        /// </summary>
        public static string ConnectionString { get; private set; }

        /// <summary>
        /// Initializes the application providing a DbContext
        /// </summary>
        /// <typeparam name="T">Should be a DbContext</typeparam>
        /// <param name="dbContext">EF DbContext</param>
        public static void AddDbContext<T>(T dbContext) where T : DbContext
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");

#if NETSTANDARD20
            AddConnectionString(DbContext.Database?.GetDbConnection()?.ConnectionString);
#endif

#if NETFULL
            AddConnectionString(DbContext.Database?.Connection?.ConnectionString);
#endif
        }

        /// <summary>
        /// Initializes the application providing a connectionString
        /// </summary>
        /// <param name="connectionString">ConnectionString</param>
        public static void AddConnectionString(string connectionString) => ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString), $"{nameof(connectionString)} {CommonResources.CannotBeNull}");
    }
}
