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

using DataTablePlus.Configuration;
using DataTablePlus.DataAccess.Enums;
using DataTablePlus.DataAccess.Services;
using DataTablePlus.DataAccess.Services.Contracts;
using System;

#if NETSTANDARD
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.Factories
{
    /// <summary>
    /// Class MetadataServiceFactory.
    /// </summary>
    public class MetadataServiceFactory
    {
        /// <summary>
        /// The factory
        /// </summary>
        private static readonly Lazy<MetadataServiceFactory> Factory = new(() => new MetadataServiceFactory(), isThreadSafe: true);

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static MetadataServiceFactory Instance { get => Factory.Value; }

        /// <summary>
        /// Prevents a default instance of the <see cref="MetadataServiceFactory"/> class from being created.
        /// </summary>
        private MetadataServiceFactory()
        { }

        /// <summary>
        /// Gets the metadata service.
        /// </summary>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>IMetadataService.</returns>
        public IMetadataService GetMetadataService(DbProvider? dbProvider = null, DbContext dbContext = null, string connectionString = null)
        {
            IMetadataService metadataService = null;

            switch (dbProvider ?? Startup.DbProvider)
            {
                case DbProvider.SQLServer:
                    metadataService = new SqlServerMetadataService(dbContext, connectionString);
                    break;
                case DbProvider.MySQL:
                    metadataService = new MySqlMetadataService(dbContext, connectionString);
                    break;
                case DbProvider.None:
                default:
                    break;
            }

            return metadataService;
        }
    }
}
