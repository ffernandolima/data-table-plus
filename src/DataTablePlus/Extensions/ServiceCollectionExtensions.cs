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

#if NETSTANDARD
using DataTablePlus.Configuration;
using DataTablePlus.DataAccess.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DataTablePlus.Extensions
{
    /// <summary>
    /// Class ServiceCollectionExtensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the data table plus.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <returns>IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">services</exception>
        /// <exception cref="ArgumentException">dbProvider</exception>
        public static IServiceCollection AddDataTablePlus(this IServiceCollection services, DbProvider dbProvider)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (dbProvider == DbProvider.None)
            {
                throw new ArgumentException(nameof(dbProvider));
            }

            Startup.AddDbProvider(dbProvider);

            return services;
        }

        /// <summary>
        /// Adds the data table plus.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <returns>IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">
        /// services
        /// or
        /// dbContext
        /// </exception>
        /// <exception cref="ArgumentException">dbProvider</exception>
        public static IServiceCollection AddDataTablePlus(this IServiceCollection services, DbProvider dbProvider, DbContext dbContext)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (dbProvider == DbProvider.None)
            {
                throw new ArgumentException(nameof(dbProvider));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            Startup.AddDbProvider(dbProvider);
            Startup.AddDbContext(dbContext);

            return services;
        }

        /// <summary>
        /// Adds the data table plus.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">services</exception>
        /// <exception cref="ArgumentException">
        /// dbProvider
        /// or
        /// connectionString
        /// </exception>
        public static IServiceCollection AddDataTablePlus(this IServiceCollection services, DbProvider dbProvider, string connectionString)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (dbProvider == DbProvider.None)
            {
                throw new ArgumentException(nameof(dbProvider));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            Startup.AddDbProvider(dbProvider);
            Startup.AddConnectionString(connectionString);

            return services;
        }


        /// <summary>
        /// Adds the data table plus.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>IServiceCollection.</returns>
        /// <exception cref="ArgumentNullException">
        /// services
        /// or
        /// dbContext
        /// </exception>
        /// <exception cref="ArgumentException">
        /// dbProvider
        /// or
        /// connectionString
        /// </exception>
        public static IServiceCollection AddDataTablePlus(this IServiceCollection services, DbProvider dbProvider, DbContext dbContext, string connectionString)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (dbProvider == DbProvider.None)
            {
                throw new ArgumentException(nameof(dbProvider));
            }

            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException(nameof(connectionString));
            }

            Startup.AddDbProvider(dbProvider);
            Startup.AddDbContext(dbContext);
            Startup.AddConnectionString(connectionString);

            return services;
        }
    }
}
#endif
