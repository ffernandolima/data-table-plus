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

using DataTablePlus.DataAccess.Enums;
using System;

#if NETSTANDARD || NET60
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.Configuration
{
    /// <summary>
    /// Class Startup.
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <value>The database context.</value>
        public static DbContext DbContext { get; private set; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public static string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the database provider.
        /// </summary>
        /// <value>The database provider.</value>
        public static DbProvider DbProvider { get; private set; }

        /// <summary>
        /// Adds the database context.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <exception cref="ArgumentNullException">dbContext</exception>
        public static void AddDbContext<T>(T dbContext) where T : DbContext
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

#if NETSTANDARD || NET60
            var connection = DbContext.Database.GetDbConnection();
            AddConnectionString(connection.ConnectionString);
#endif

#if NETFULL
            var connection = DbContext.Database.Connection;
            AddConnectionString(connection.ConnectionString);
#endif
        }

        /// <summary>
        /// Adds the connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="ArgumentException">connectionString</exception>
        public static void AddConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            ConnectionString = connectionString;
        }

        /// <summary>
        /// Adds the database provider.
        /// </summary>
        /// <param name="dbProvider">The database provider.</param>
        /// <exception cref="ArgumentException">dbProvider</exception>
        public static void AddDbProvider(DbProvider dbProvider)
        {
            if (dbProvider == DbProvider.None)
            {
                throw new ArgumentException(nameof(dbProvider));
            }

            DbProvider = dbProvider;
        }
    }
}
