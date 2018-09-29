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
		public ServiceBase(DbContext dbContext = null, string connectionString = null) => this.Construct(dbContext, connectionString);

		/// <summary>
		/// Opens the current connection
		/// </summary>
		protected void OpenConnection()
		{
			if (this.SqlConnection.State != ConnectionState.Open)
			{
				this.SqlConnection.Open();
			}
		}

		/// <summary>
		/// Closes the current connection
		/// </summary>
		protected void CloseConnection()
		{
			if (this.SqlConnection.State != ConnectionState.Closed)
			{
				this.SqlConnection.Close();
			}
		}

		/// <summary>
		/// Creates a new transaction from the current connection
		/// </summary>
		/// <param name="isolationLevel">Kind of isolation level to create the transaction</param>
		/// <returns>An active transaction</returns>
		protected SqlTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			if (this.SqlTransaction != null)
			{
				throw new InvalidOperationException(DataResources.MoreThanOneTransaction);
			}

			return (this.SqlTransaction = this.SqlConnection.BeginTransaction(isolationLevel));
		}

		/// <summary>
		/// Commits the current transaction if there's an active one
		/// </summary>
		protected void Commit()
		{
			try
			{
				if (this.SqlTransaction == null)
				{
					throw new InvalidOperationException(DataResources.TransactionIsNull);
				}

				this.SqlTransaction.Commit();
			}
			catch
			{
				this.Rollback();

				throw;
			}
		}

		/// <summary>
		/// Rollbacks the current transaction if there's an active one
		/// </summary>
		protected void Rollback()
		{
			try
			{
				if (this.SqlTransaction != null)
				{
					this.SqlTransaction.Rollback();
				}
			}
			catch
			{
				// ignored
			}
		}

		/// <summary>
		/// Creates a command based on the provided parameters
		/// </summary>
		/// <param name="commandText">Commnad text to be executed on database</param>
		/// <param name="commandType">Type of the provided command text</param>
		/// <param name="parameters">command parameters</param>
		/// <param name="useInternalTransaction">A flag that indicates if an internal transaction shall be created</param>
		/// <returns>A new command</returns>
		protected SqlCommand CreateCommand(string commandText = null, CommandType? commandType = null, SqlParameter[] parameters = null, bool? useInternalTransaction = null)
		{
			SqlTransaction transaction = null;

			if (useInternalTransaction.GetValueOrDefault())
			{
				transaction = this.BeginTransaction();
			}

			var sqlCommand = this.SqlConnection.CreateCommand();

			sqlCommand.CommandTimeout = Convert.ToInt32(this.Timeout.TotalSeconds);

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
		/// <param name="dataTable">The table that contains the data to execute the bulk insert on database</param>
		/// <param name="batchSize">the size of batch</param>
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

			#endregion

			const SqlBulkCopyOptions SqlBulkCopyOptions = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction;

			var sqlBulkCopy = new SqlBulkCopy(this.SqlConnection, options ?? SqlBulkCopyOptions, null)
			{
				BatchSize = batchSize,
				DestinationTableName = dataTable.TableName,
				BulkCopyTimeout = Convert.ToInt32(this.Timeout.TotalSeconds)
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
		/// Fill the properties out using the values from the Ctor or from the Startup class
		/// </summary>
		/// <param name="dbContext">Db Context</param>
		/// <param name="connectionString">Connection string</param>
		private void Construct(DbContext dbContext = null, string connectionString = null)
		{
			dbContext = dbContext ?? Startup.DbContext;
			connectionString = connectionString ?? Startup.ConnectionString;

			if (dbContext != null)
			{
				this.DbContext = dbContext;

#if NETSTANDARD20
				ValidateConnectionString(this.DbContext.Database.GetDbConnection().ConnectionString);
				this.SqlConnection = this.DbContext.Database.GetDbConnection() as SqlConnection;
#endif

#if NETFULL
				ValidateConnectionString(this.DbContext.Database.Connection.ConnectionString);
				this.SqlConnection = this.DbContext.Database.Connection as SqlConnection;
#endif
			}

			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				ValidateConnectionString(connectionString);

				this.SqlConnection = new SqlConnection(connectionString);
			}

			if (this.DbContext == null && this.SqlConnection == null)
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
			if (!this._disposed)
			{
				if (disposing)
				{
					if (this.DbContext != null)
					{
						this.DbContext = null;
					}

					if (this.SqlConnection != null)
					{
						this.SqlConnection.Dispose();
						this.SqlConnection = null;
					}
				}
			}

			this._disposed = true;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
