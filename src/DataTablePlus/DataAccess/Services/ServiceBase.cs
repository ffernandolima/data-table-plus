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
using DataTablePlus.Configuration;
using DataTablePlus.DataAccess.Resources;
using DataTablePlus.DataAccessContracts;
using DataTablePlus.DataAccessContracts.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

#if NETSTANDARD20
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.DataAccess.Services
{
    /// <summary>
    /// Service base that controls database objects
    /// </summary>
    public class ServiceBase : IServiceBase
    {
        /// <summary>
        /// EF DbContext
        /// </summary>
        protected DbContext DbContext { get; private set; }

        /// <summary>
        /// Sql Connection
        /// </summary>
        protected SqlConnection SqlConnection { get; private set; }

        /// <summary>
        /// Sql Transaction
        /// </summary>
        protected SqlTransaction SqlTransaction { get; private set; }

        /// <summary>
        /// Sql command Timeout
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="dbContext">Db Context</param>
        /// <param name="connectionString">Connection String</param>
        public ServiceBase(DbContext dbContext = null, string connectionString = null) => Construct(dbContext, connectionString);

        /// <summary>
        /// Opens the current connection
        /// </summary>
        protected void OpenConnection()
        {
            if (SqlConnection.State != ConnectionState.Open)
            {
                SqlConnection.Open();
            }
        }

        /// <summary>
        /// Closes the current connection
        /// </summary>
        protected void CloseConnection()
        {
            if (SqlConnection.State != ConnectionState.Closed)
            {
                SqlConnection.Close();
            }
        }

        /// <summary>
        /// Creates a new transaction from the current connection
        /// </summary>
        /// <param name="isolationLevel">Kind of isolation level to create the transaction</param>
        /// <returns>An active transaction</returns>
        protected SqlTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (SqlTransaction != null)
            {
                throw new InvalidOperationException(DataResources.MoreThanOneTransaction);
            }

            return (SqlTransaction = SqlConnection.BeginTransaction(isolationLevel));
        }

        /// <summary>
        /// Commits the current transaction if there's an active one
        /// </summary>
        protected void Commit()
        {
            try
            {
                if (SqlTransaction == null)
                {
                    throw new InvalidOperationException(DataResources.TransactionIsNull);
                }

                SqlTransaction.Commit();
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
        /// Rollbacks the current transaction if there's an active one
        /// </summary>
        protected void Rollback()
        {
            try
            {
                if (SqlTransaction != null)
                {
                    SqlTransaction.Rollback();
                }
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
        /// Creates a command based on the provided parameters
        /// </summary>
        /// <param name="commandText">Command text to be executed on database</param>
        /// <param name="commandType">Type of the provided command text</param>
        /// <param name="parameters">command parameters</param>
        /// <param name="useInternalTransaction">A flag that indicates if an internal transaction shall be created</param>
        /// <returns>A new command</returns>
        protected SqlCommand CreateCommand(string commandText = null, CommandType? commandType = null, SqlParameter[] parameters = null, bool? useInternalTransaction = null)
        {
            SqlTransaction transaction = null;

            if (useInternalTransaction.GetValueOrDefault())
            {
                transaction = BeginTransaction();
            }

            var sqlCommand = SqlConnection.CreateCommand();

            sqlCommand.CommandTimeout = Convert.ToInt32(Timeout.TotalSeconds);

            sqlCommand.CommandText = commandText;

            sqlCommand.CommandType = commandType ?? CommandType.Text;

            if (parameters != null && parameters.Any())
            {
                sqlCommand.Parameters.AddRange(parameters);
            }

            if (transaction != null)
            {
                sqlCommand.Transaction = transaction;
            }

            return sqlCommand;
        }

        /// <summary>
        /// Creates a new SqlBulkCopy based on the provided parameters
        /// </summary>
        /// <param name="dataTable">The data table with data to execute the bulk insert on database</param>
        /// <param name="batchSize">the batch size</param>
        /// <param name="options">Options to be used while ingesting the amount of data</param>
        /// <param name="createColumnMappings">A flag that indicates if the mappings shall be created</param>
        /// <returns></returns>
        protected SqlBulkCopy CreateSqlBulkCopy(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, bool? createColumnMappings = true)
        {
            #region SqlBulkCopyOptions

            // src: https://msdn.microsoft.com/pt-br/library/system.data.sqlclient.sqlbulkcopyoptions(v=vs.110).aspx
            //
            // CheckConstraints: Check constraints while data is being inserted. By default, constraints are not checked.
            // KeepNulls: Preserve null values in the destination table regardless of the settings for default values. When not specified, null values are replaced by default values where applicable.
            // TableLock: Obtain a bulk update lock for the duration of the bulk copy operation. When not specified, row locks are used.
            // UseInternalTransaction: When specified, each batch of the bulk-copy operation will occur within a transaction. If you indicate this option and also provide a SqlTransaction object to the constructor, an ArgumentException occurs.

            #endregion SqlBulkCopyOptions

            const SqlBulkCopyOptions SqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction;

            var sqlBulkCopy = new SqlBulkCopy(SqlConnection, options ?? SqlBulkCopyOptions, null)
            {
                BatchSize = batchSize,
                DestinationTableName = dataTable.TableName,
                BulkCopyTimeout = Convert.ToInt32(Timeout.TotalSeconds)
            };

            if (createColumnMappings.GetValueOrDefault())
            {
                foreach (var dataColumn in dataTable.Columns.Cast<DataColumn>())
                {
                    sqlBulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                }
            }

            return sqlBulkCopy;
        }

        /// <summary>
        /// Disposes the current transaction
        /// </summary>
        private void DisposeTransaction()
        {
            if (SqlTransaction != null)
            {
                SqlTransaction.Dispose();
                SqlTransaction = null;
            }
        }

        /// <summary>
        /// Fill the properties out using the values from the Ctor or from the Startup class
        /// </summary>
        /// <param name="dbContext">Db Context</param>
        /// <param name="connectionString">Connection string</param>
        private void Construct(DbContext dbContext = null, string connectionString = null)
        {
            dbContext ??= Startup.DbContext;
            connectionString ??= Startup.ConnectionString;

            if (dbContext != null)
            {
                DbContext = dbContext;

#if NETSTANDARD20
                ValidateConnectionString(DbContext.Database.GetDbConnection().ConnectionString);
                SqlConnection = DbContext.Database.GetDbConnection() as SqlConnection;
#endif

#if NETFULL
                ValidateConnectionString(DbContext.Database.Connection.ConnectionString);
                SqlConnection = DbContext.Database.Connection as SqlConnection;
#endif
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                ValidateConnectionString(connectionString);

                SqlConnection = new SqlConnection(connectionString);
            }

            if (DbContext == null && SqlConnection == null)
            {
                throw new ArgumentNullException($"{CommonResources.MissingConfiguration}");
            }
        }

        /// <summary>
        /// Validates the connection string which has been provided
        /// </summary>
        /// <param name="connectionString">ConnectionString to be tested</param>
        private static void ValidateConnectionString(string connectionString)
        {
            try
            {
                using (var sqlconnection = new SqlConnection(connectionString))
                {
                    sqlconnection.Open();
                }
            }
            catch
            {
                throw new Exception($"{CommonResources.InvalidConnectionString}");
            }
        }

        #region IDisposable Members

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeTransaction();

                    if (DbContext != null)
                    {
                        DbContext = null;
                    }

                    if (SqlConnection != null)
                    {
                        SqlConnection.Dispose();
                        SqlConnection = null;
                    }
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
