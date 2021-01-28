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
using DataTablePlus.DataAccess.Services.Contracts;
using DataTablePlus.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

#if NETSTANDARD
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.DataAccess.Services
{
    /// <summary>
    /// Class MetadataService.
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.ServiceBase" />
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.Contracts.IMetadataService" />
    /// </summary>
    /// <seealso cref="DataTablePlus.DataAccess.Services.ServiceBase" />
    /// <seealso cref="DataTablePlus.DataAccess.Services.Contracts.IMetadataService" />
    public abstract class MetadataService : ServiceBase, IMetadataService
    {
        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <value>The commands.</value>
        protected IDictionary<string, string> Commands { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataService"/> class.
        /// </summary>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        public MetadataService(DbProvider dbProvider, DbContext dbContext = null, string connectionString = null)
            : base(dbProvider, dbContext, connectionString)
        {
            Commands = BuildCommands();
        }

        /// <summary>
        /// Builds the commands.
        /// </summary>
        /// <returns>IDictionary&lt;System.String, System.String&gt;.</returns>
        protected abstract IDictionary<string, string> BuildCommands();

        /// <summary>
        /// Escapes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        protected abstract string Escape(string source);

        /// <inheritdoc />
        public string GetTableName<T>() where T : class
        {
            return GetTableName(typeof(T));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">type</exception>
        public string GetTableName(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return DbContext.GetTableName(type);
        }

        /// <inheritdoc />
        public IDictionary<PropertyInfo, string> GetMappings<T>() where T : class
        {
            return GetMappings(typeof(T));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">type</exception>
        public IDictionary<PropertyInfo, string> GetMappings(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return DbContext.GetMappings(type);
        }

        /// <inheritdoc />
        public IList<string> GetKeyNames<T>() where T : class
        {
            return GetKeyNames(typeof(T));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">type</exception>
        public IList<string> GetKeyNames(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return DbContext.GetKeyNames(type);
        }

        /// <inheritdoc />
        public IList<string> GetDbKeyNames<T>() where T : class
        {
            return GetDbKeyNames(typeof(T));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">type</exception>
        public IList<string> GetDbKeyNames(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return DbContext.GetDbKeyNames(type);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">tableName</exception>
        public DataTable GetTableSchema(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException(nameof(tableName));
            }

            DataTable dataTable;

            try
            {
                OpenConnection();

                var commandFormat = TryGetCommand("GetSchemaTable");
                var commandText = string.Format(commandFormat, Escape(tableName));

                using (var command = CreateCommand(commandText: commandText))
                using (var reader = command.ExecuteReader())
                {
                    dataTable = new DataTable(tableName);

                    dataTable.BeginLoadData();
                    dataTable.Load(reader);
                    dataTable.EndLoadData();
                }
            }
            finally
            {
                CloseConnection();
            }

            return dataTable;
        }

        /// <summary>
        /// Tries to get a command by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ArgumentException">
        /// key
        /// or
        /// Commands
        /// </exception>
        private string TryGetCommand(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }

            string command = null;

            if (!Commands?.TryGetValue(key, out command) ?? false)
            {
                throw new ArgumentException(nameof(Commands));
            }

            return command;
        }

        #region IDisposable Members

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    base.Dispose(true);
                }
            }

            _disposed = true;
        }

        #endregion IDisposable Members
    }
}
