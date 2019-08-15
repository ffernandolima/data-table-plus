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
using DataTablePlus.DataAccess.Resources;
using DataTablePlus.DataAccessContracts;
using DataTablePlus.DataAccessContracts.Services;
using DataTablePlus.Extensions;
using DataTablePlus.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD20
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.DataAccess.Services
{
	/// <summary>
	/// Service that should be used in order to ingest or update a large amount of data
	/// </summary>
	public class SqlService : ServiceBase, ISqlService
	{
		private static readonly Regex PARAMETERS_REGEX = new Regex(@"\@\w+", RegexOptions.Compiled);

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="dbContext">Db Context</param>
		/// <param name="connectionString">Connection String</param>
		public SqlService(DbContext dbContext = null, string connectionString = null)
			: base(dbContext, connectionString)
		{
		}

		/// <summary>
		/// Executes a bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="batchSize">The batch number which will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <returns>Returns the data table filled out with primary keys or not, depends on the primaryKeyNames parameter</returns>
		public DataTable BulkInsert(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null) => this.BulkInsertInternal(dataTable, batchSize, options, primaryKeyNames);

		/// <summary>
		/// Executes an async bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="batchSize">The batch number which will be considered while inserting</param>
		/// <param name="options">Bulk insert options></param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <returns>Returns the data table filled out with primary keys or not, depends on the primaryKeyNames parameter</returns>
		public Task<DataTable> BulkInsertAsync(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null, CancellationToken cancellationToken = default)
		{
			if (cancellationToken == null || cancellationToken == default)
			{
				cancellationToken = CancellationTokenFactory.Token();
			}

			var task = Task.Factory.StartNew(() => this.BulkInsert(dataTable, batchSize, options, primaryKeyNames), cancellationToken);

			return task;
		}

		/// <summary>
		/// Executes a batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		public void BatchUpdate(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize) => this.BatchUpdateInternal(dataTable, commandText, batchSize);

		/// <summary>
		/// Executes an async batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <returns>Returns a task which will be processing the update</returns>
		public Task BatchUpdateAsync(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize, CancellationToken cancellationToken = default)
		{
			if (cancellationToken == null || cancellationToken == default)
			{
				cancellationToken = CancellationTokenFactory.Token();
			}

			var task = Task.Factory.StartNew(() => this.BatchUpdate(dataTable, commandText, batchSize), cancellationToken);

			return task;
		}

		/// <summary>
		/// Executes a bulk insert in order to get a high performance level while inserting a lot of data (internal method)
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <returns>Returns the data table filled out with primary keys or not, depends on the primaryKeyNames parameter</returns>
		private DataTable BulkInsertInternal(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null)
		{
			this.ValidateBulkInsertParameters(dataTable);

			string trackerColumnName = null;

			try
			{
				this.OpenConnection();

				if (primaryKeyNames != null)
				{
					primaryKeyNames = primaryKeyNames.Where(primaryKeyName => !string.IsNullOrWhiteSpace(primaryKeyName)).ToList();

					if (primaryKeyNames.Any())
					{
						trackerColumnName = this.CreateTrackerColumn(dataTable);

						this.DropIndex(dataTable.TableName);

						this.CreateIndex(dataTable.TableName, trackerColumnName);
					}
				}

				this.SetStatus(dataTable, DataRowState.Added);

				using (var sqlBulkCopy = this.CreateSqlBulkCopy(dataTable, batchSize, options))
				{
					sqlBulkCopy.WriteToServer(dataTable);
				}

				if (primaryKeyNames != null && primaryKeyNames.Any())
				{
					this.SetReadOnlyFalse(dataTable, primaryKeyNames);

					this.RetrievePrimaryKeyValues(dataTable, trackerColumnName, primaryKeyNames);
				}
			}
			finally
			{
				this.DropIndex(dataTable.TableName);

				this.DropDbTrackerColumn(dataTable.TableName, trackerColumnName);

				this.RemoveTrackerColumn(dataTable, trackerColumnName);

				this.CloseConnection();
			}

			return dataTable;
		}

		/// <summary>
		/// Executes a batch update in order to get a high performance level while updating a lot of data (internal method)
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		private void BatchUpdateInternal(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize)
		{
			this.ValidateBatchUpdateParameters(dataTable, commandText);

			try
			{
				this.OpenConnection();

				var parameters = this.BuildUpdateParameters(commandText);

				var updateCommand = this.CreateCommand(commandText: commandText, parameters: parameters, useInternalTransaction: true);

				updateCommand.UpdatedRowSource = UpdateRowSource.None;

				var sqlDataAdapter = new SqlDataAdapter
				{
					UpdateCommand = updateCommand
				};
#if NETFULL
				sqlDataAdapter.UpdateBatchSize = batchSize;
#endif
				this.SetStatus(dataTable, DataRowState.Modified);

				using (updateCommand)
				using (sqlDataAdapter)
				{
					sqlDataAdapter.Update(dataTable);

					this.Commit();
				}
			}
			catch
			{
				this.Rollback();

				throw;
			}
			finally
			{
				this.CloseConnection();
			}
		}

		/// <summary>
		/// Validates the provided parameters to avoid some problems during the bulk insert
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		private void ValidateBulkInsertParameters(DataTable dataTable)
		{
			dataTable.ValidateParameters();

			if (string.IsNullOrWhiteSpace(dataTable.TableName))
			{
				throw new ArgumentException($"{nameof(dataTable.TableName)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(dataTable.TableName));
			}
		}

		/// <summary>
		/// Validates the provided parameters to avoid some problems during the batch update
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="commandText">Commnad text to update the data</param>
		private void ValidateBatchUpdateParameters(DataTable dataTable, string commandText)
		{
			dataTable.ValidateParameters();

			if (string.IsNullOrWhiteSpace(commandText))
			{
				throw new ArgumentException($"{nameof(commandText)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(commandText));
			}
		}

		/// <summary>
		/// Drops the datatabse non clustered index if it exists
		/// </summary>
		/// <param name="tableName">Name of the table that has the index</param>
		private void DropIndex(string tableName)
		{
			try
			{
				var commandText = string.Format(DataResources.DropNonClusteredIndex, tableName);

				using (var command = this.CreateCommand(commandText: commandText))
				{
					command.ExecuteNonQuery();
				}
			}
			catch
			{
				// ignored
			}
		}

		/// <summary>
		/// Create the datatabse non clustered index
		/// </summary>
		/// <param name="tableName">Name of the table that is going to receive the non clustered index</param>
		/// <param name="trackerColumnName">Name of the column that is going to receive the non clustered index</param>
		private void CreateIndex(string tableName, string trackerColumnName)
		{
			try
			{
				var commandText = string.Format(DataResources.CreateNonClusteredIndex, tableName, trackerColumnName);

				using (var command = this.CreateCommand(commandText: commandText))
				{
					command.ExecuteNonQuery();
				}
			}
			catch
			{
				// ignored
			}
		}

		/// <summary>
		/// Creates a column in the current data table to track the inserted values
		/// </summary>
		/// <param name="dataTable"></param>
		/// <returns></returns>
		private string CreateTrackerColumn(DataTable dataTable)
		{
			var trackerColumnName = DataResources.TrackerColumnName;

			var trackerColumn = new DataColumn
			{
				ColumnName = trackerColumnName,
				DataType = typeof(int),
				AllowDBNull = true
			};

			dataTable.Columns.Add(trackerColumn);

			var idx = 0;

			foreach (var dataRow in dataTable.Rows.Cast<DataRow>())
			{
				idx++;
				dataRow[trackerColumnName] = idx;
			}

			this.CreateDbTrackerColumn(dataTable.TableName, trackerColumnName);

			return trackerColumnName;
		}

		/// <summary>
		/// Creates a column in the current database table to track the inserted values
		/// </summary>
		/// <param name="tableName">Database table name</param>
		/// <param name="trackerColumnName">The name of the column</param>
		private void CreateDbTrackerColumn(string tableName, string trackerColumnName)
		{
			var commandText = string.Format(DataResources.AddTrackerColumnStatement, tableName, trackerColumnName);

			using (var command = this.CreateCommand(commandText: commandText))
			{
				command.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Allows the primary key columns to receive some data
		/// </summary>
		/// <param name="dataTable">Table with data</param>
		/// <param name="primaryKeyNames">The name of the primary keys</param>
		private void SetReadOnlyFalse(DataTable dataTable, IList<string> primaryKeyNames)
		{
			var dataColumns = dataTable.Columns.Cast<DataColumn>().Where(dataColumn => primaryKeyNames.Contains(dataColumn.ColumnName));

			foreach (var dataColumn in dataColumns)
			{
				dataColumn.ReadOnly = false;
			}
		}

		/// <summary>
		/// Gets the primary key values back from the database
		/// </summary>
		/// <param name="dataTable">Table with data</param>
		/// <param name="trackerColumnName">The name of the column</param>
		/// <param name="primaryKeyNames">The name of the primary keys</param>
		private void RetrievePrimaryKeyValues(DataTable dataTable, string trackerColumnName, IList<string> primaryKeyNames)
		{
			var fields = new List<string>(primaryKeyNames)
			{
				trackerColumnName
			};

			var fieldNames = fields.Select(primaryKey => $"[{primaryKey}]");

			var selectClause = string.Join(Constants.Comma, fieldNames);

			var parameters = this.BuildInsertParameters(dataTable, trackerColumnName);

			var commandText = string.Format(DataResources.SelectPrimaryKeysStatement, selectClause, dataTable.TableName, trackerColumnName, DataResources.MinParameterName, DataResources.MaxParameterName);

			using (var command = this.CreateCommand(commandText: commandText, parameters: parameters))
			{
				using (var reader = command.ExecuteReader())
				{
					foreach (var dataRow in dataTable.Rows.Cast<DataRow>())
					{
						if (!reader.Read())
						{
							continue;
						}
#if DEBUG
						var drTrackerColumnValue = Convert.ToString(dataRow[trackerColumnName]);
						var dbTrackerColumnValue = Convert.ToString(reader[trackerColumnName]);

						if (!string.Equals(drTrackerColumnValue, dbTrackerColumnValue, StringComparison.OrdinalIgnoreCase))
						{
							Debug.WriteLine($"DataRowTrackerColumnValue '{drTrackerColumnValue}' and DatabaseTrackerColumnValue '{dbTrackerColumnValue}' are not equal.");
							continue;
						}
#endif
						foreach (var primaryKeyName in primaryKeyNames)
						{
							dataRow[primaryKeyName] = reader[primaryKeyName];
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates the bulk insert parameters
		/// </summary>
		/// <param name="dataTable">Table with data</param>
		/// <param name="trackerColumnName">The name of the column</param>
		/// <returns></returns>
		private SqlParameter[] BuildInsertParameters(DataTable dataTable, string trackerColumnName)
		{
			var dataRows = dataTable.Rows.Cast<DataRow>();

			var parameters = new[]
			{
				new SqlParameter
				{
					ParameterName = DataResources.MinParameterName,
					Value = dataRows.Min(dataRow => Convert.ToInt32(dataRow[trackerColumnName]))
				},

				new SqlParameter
				{
					ParameterName = DataResources.MaxParameterName,
					Value = dataRows.Max(dataRow => Convert.ToInt32(dataRow[trackerColumnName]))
				}
			};

			return parameters;
		}

		/// <summary>
		/// Drops the tracker column from the database table
		/// </summary>
		/// <param name="tableName">Database table name</param>
		/// <param name="trackerColumnName">The name of the column</param>
		private void DropDbTrackerColumn(string tableName, string trackerColumnName)
		{
			if (!string.IsNullOrWhiteSpace(trackerColumnName))
			{
				try
				{
					var commandText = string.Format(DataResources.DropTrackerColumnStatement, tableName, trackerColumnName);

					using (var command = this.CreateCommand(commandText: commandText))
					{
						command.ExecuteNonQuery();
					}
				}
				catch
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Drops the tracker column from the current data table
		/// </summary>
		/// <param name="dataTable">Table with data</param>
		/// <param name="trackerColumnName">The name of the column</param>
		private void RemoveTrackerColumn(DataTable dataTable, string trackerColumnName)
		{
			if (!string.IsNullOrWhiteSpace(trackerColumnName))
			{
				try
				{
					dataTable.Columns.Remove(trackerColumnName);
				}
				catch
				{
					// ignored
				}
			}
		}

		/// <summary>
		/// Creates the batch update parameters
		/// </summary>
		/// <param name="commandText">Cmmand text which will be used to update the data</param>
		/// <returns></returns>
		private SqlParameter[] BuildUpdateParameters(string commandText)
		{
			var parameters = PARAMETERS_REGEX.Matches(commandText)
											 .Cast<Match>()
											 .Select(match =>
											 {
												 var parameterName = match.Value;
												 var sourceColumn = match.Value.Replace("@", string.Empty);

												 var parameter = new SqlParameter
												 {
													 ParameterName = parameterName,
													 SourceColumn = sourceColumn
												 };

												 return parameter;

											 }).ToArray();

			return parameters;
		}

		/// <summary>
		/// Sets the provided status to the current data table rows
		/// </summary>
		/// <param name="dataTable">Table with data</param>
		/// <param name="dataRowState">The row state to be set</param>
		private void SetStatus(DataTable dataTable, DataRowState dataRowState)
		{
			dataTable.AcceptChanges();

			var dataRows = dataTable.Rows.Cast<DataRow>().Where(dataRow => dataRow.RowState == DataRowState.Unchanged);

			foreach (var dataRow in dataRows)
			{
				SetStatusInternal(dataRow, dataRowState);
			}
		}

		/// <summary>
		/// Sets the provided status to the current data table rows (internal method)
		/// </summary>
		/// <param name="dataRow">Data Row which will receive the current state</param>
		/// <param name="dataRowState">The row state to be set</param>
		private void SetStatusInternal(DataRow dataRow, DataRowState dataRowState)
		{
			switch (dataRowState)
			{
				case DataRowState.Added:
					dataRow.SetAdded();
					break;

				case DataRowState.Modified:
					dataRow.SetModified();
					break;

				default:
					break;
			}
		}

		#region IDisposable Members

		private bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
					base.Dispose(true);
				}
			}

			this._disposed = true;
		}

		#endregion
	}
}
