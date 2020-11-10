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

using DataTablePlus.Configuration;
using DataTablePlus.DataAccess.Enums;
using DataTablePlus.DataAccess.Services;
using DataTablePlus.DataAccess.Services.Contracts;
using System;

#if NETSTANDARD20
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.Factories
{
    /// <summary>
    /// Class SqlServiceFactory.
    /// </summary>
    public class SqlServiceFactory
    {
        /// <summary>
        /// The factory
        /// </summary>
        private static readonly Lazy<SqlServiceFactory> Factory = new Lazy<SqlServiceFactory>(() => new SqlServiceFactory(), isThreadSafe: true);

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static SqlServiceFactory Instance { get => Factory.Value; }

        /// <summary>
        /// Prevents a default instance of the <see cref="SqlServiceFactory"/> class from being created.
        /// </summary>
        private SqlServiceFactory()
        { }

        /// <summary>
        /// Gets the SQL service.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>ISqlService.</returns>
        public ISqlService GetSqlService(DbContext dbContext = null, string connectionString = null)
        {
            ISqlService sqlService = null;

            switch (Startup.DbProvider)
            {
                case DbProvider.SQLServer:
                    sqlService = new SqlServerService(dbContext, connectionString);
                    break;
                case DbProvider.MySQL:
                    sqlService = new MySqlService(dbContext, connectionString);
                    break;
                case DbProvider.None:
                default:
                    break;
            }

            return sqlService;
        }
    }
}