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
using DataTablePlus.DataAccess.Services.Contracts;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

#if NETSTANDARD20
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
#endif

#if NETSTANDARD21
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
#endif

#if NETFULL
using MySql.Data.MySqlClient;
using System.Data.Entity;
#endif

namespace DataTablePlus.DataAccess.Services
{
    /// <summary>
    /// Class ServiceBase.
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.Contracts.IServiceBase" />
    /// </summary>
    /// <seealso cref="DataTablePlus.DataAccess.Services.Contracts.IServiceBase" />
    public abstract class ServiceBase : IServiceBase
    {
        /// <summary>
        /// Gets the database context.
        /// </summary>
        /// <value>The database context.</value>
        protected DbContext DbContext { get; private set; }

        /// <summary>
        /// Gets the database connection.
        /// </summary>
        /// <value>The database connection.</value>
        protected DbConnection DbConnection { get; private set; }

        /// <summary>
        /// Gets the database transaction.
        /// </summary>
        /// <value>The database transaction.</value>
        protected DbTransaction DbTransaction { get; private set; }

        /// <inheritdoc />
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        public ServiceBase(DbContext dbContext = null, string connectionString = null)
            : this(Startup.DbProvider, dbContext, connectionString)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase"/> class.
        /// </summary>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        public ServiceBase(DbProvider dbProvider, DbContext dbContext = null, string connectionString = null)
        {
            Construct(dbProvider, dbContext, connectionString);
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        protected void OpenConnection()
        {
            if (DbConnection.State != ConnectionState.Open)
            {
                DbConnection.Open();
            }
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        protected void CloseConnection()
        {
            if (DbConnection.State != ConnectionState.Closed)
            {
                DbConnection.Close();
            }
        }

        /// <summary>
        /// Determines whether this instance has a transaction.
        /// </summary>
        /// <returns><c>true</c> if this instance has a transaction; otherwise, <c>false</c>.</returns>
        public bool HasTransaction()
        {
            return DbTransaction != null;
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <param name="isolationLevel">The isolation level.</param>
        /// <returns>DbTransaction.</returns>
        /// <exception cref="InvalidOperationException">Cannot create more than one transaction.</exception>
        protected DbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (DbTransaction != null)
            {
                throw new InvalidOperationException("Cannot create more than one transaction.");
            }

            return (DbTransaction = DbConnection.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot commit a null transaction.</exception>
        protected void Commit()
        {
            try
            {
                if (DbTransaction == null)
                {
                    throw new InvalidOperationException("Cannot commit a null transaction.");
                }

                DbTransaction.Commit();
            }
            catch
            {
                Rollback();

                throw;
            }
            finally
            {
                DisposeTransaction();
            }
        }

        /// <summary>
        /// Rollbacks the current transaction.
        /// </summary>
        protected void Rollback()
        {
            try
            {
                DbTransaction?.Rollback();
            }
            catch
            {
                // ignored
            }
            finally
            {
                DisposeTransaction();
            }
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="useInternalTransaction">If set to <c>true</c> it uses an internal transaction.</param>
        /// <returns>DbCommand.</returns>
        protected DbCommand CreateCommand(string commandText = null, CommandType? commandType = null, DbParameter[] parameters = null, bool? useInternalTransaction = null)
        {
            DbTransaction transaction = null;

            if (useInternalTransaction.GetValueOrDefault())
            {
                transaction = BeginTransaction();
            }

            var dbCommand = DbConnection.CreateCommand();

            dbCommand.CommandTimeout = Convert.ToInt32(Timeout.TotalSeconds);
            dbCommand.CommandText = commandText;
            dbCommand.CommandType = commandType ?? CommandType.Text;

            if (parameters != null && parameters.Any())
            {
                dbCommand.Parameters.AddRange(parameters);
            }

            if (transaction != null)
            {
                dbCommand.Transaction = transaction;
            }

            return dbCommand;
        }

        /// <summary>
        /// Disposes the current transaction.
        /// </summary>
        private void DisposeTransaction()
        {
            if (DbTransaction != null)
            {
                DbTransaction.Dispose();
                DbTransaction = null;
            }
        }

        /// <summary>
        /// Constructs this instance.
        /// </summary>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="validateConnection"><c>true</c> if it should validate the connection; otherwise, <c>false</c>.</param>
        /// <exception cref="ArgumentNullException">Please configure this application through DataTablePlus.Configuration.Startup class.</exception>
        private void Construct(DbProvider dbProvider, DbContext dbContext = null, string connectionString = null, bool validateConnection = false)
        {
            dbContext ??= Startup.DbContext;
            connectionString ??= Startup.ConnectionString;

            if (dbContext != null)
            {
#if NETSTANDARD
                var dbConnection = dbContext.Database.GetDbConnection();

                if (validateConnection)
                {
                    ValidateConnection(dbConnection);
                }

                DbContext = dbContext;
                DbConnection = dbConnection;
#endif

#if NETFULL
                var dbConnection = dbContext.Database.Connection;

                if (validateConnection)
                {
                    ValidateConnection(dbConnection);
                }

                DbContext = dbContext;
                DbConnection = dbConnection;
#endif
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var dbConnection = GetConnection(connectionString, dbProvider);

                if (validateConnection)
                {
                    ValidateConnection(dbConnection);
                }

                DbConnection = dbConnection;
            }

            if (DbContext == null && DbConnection == null)
            {
                throw new ArgumentNullException("Please configure this application through DataTablePlus.Configuration.Startup class.");
            }
        }

        /// <summary>
        /// Validates the connection.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <exception cref="Exception">Invalid database connection.</exception>
        private static void ValidateConnection(IDbConnection dbConnection)
        {
            try
            {
                if (dbConnection.State != ConnectionState.Open)
                {
                    dbConnection.Open();
                }

                if (dbConnection.State != ConnectionState.Closed)
                {
                    dbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Invalid database connection.", ex);
            }
        }

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <returns>DbConnection.</returns>
        private static DbConnection GetConnection(string connectionString, DbProvider dbProvider)
        {
            DbConnection dbConnection = null;

            switch (dbProvider)
            {
                case DbProvider.SQLServer:
                    dbConnection = new SqlConnection(connectionString);
                    break;
                case DbProvider.MySQL:
                    dbConnection = new MySqlConnection(connectionString);
                    break;
                case DbProvider.None:
                default:
                    break;
            }

            return dbConnection;
        }

        #region IDisposable Members

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeTransaction();

                    DbContext = null;
                    DbConnection = null;
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}
