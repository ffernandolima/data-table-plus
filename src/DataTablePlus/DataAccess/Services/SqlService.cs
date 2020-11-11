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
using System.Data.Common;
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
    /// Class SqlService.
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.ServiceBase" />
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.Contracts.ISqlService" />
    /// </summary>
    /// <seealso cref="DataTablePlus.DataAccess.Services.ServiceBase" />
    /// <seealso cref="DataTablePlus.DataAccess.Services.Contracts.ISqlService" />
    public abstract class SqlService : ServiceBase, ISqlService
    {
        /// <summary>
        /// The parameters regex
        /// </summary>
        private static readonly Regex ParametersRegex = new Regex(@"\@\w+", RegexOptions.Compiled);

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <value>The commands.</value>
        protected IDictionary<string, string> Commands { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlService"/> class.
        /// </summary>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        public SqlService(DbProvider dbProvider, DbContext dbContext = null, string connectionString = null)
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
        /// Creates the bulk copy.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="options">The options.</param>
        /// <param name="createColumnMappings">if set to <c>true</c> [create column mappings].</param>
        /// <returns>dynamic.</returns>
        protected abstract dynamic CreateBulkCopy(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, bool? createColumnMappings = true);

        /// <summary>
        /// Creates the database data adapter.
        /// </summary>
        /// <param name="updateCommand">The update command.</param>
        /// <returns>DbDataAdapter.</returns>
        protected abstract DbDataAdapter CreateDbDataAdapter(DbCommand updateCommand);

        /// <summary>
        /// Creates the database parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns>DbParameter.</returns>
        protected abstract DbParameter CreateDbParameter(string parameterName, object parameterValue);

        /// <summary>
        /// Creates the database parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="sourceColumn">The source column.</param>
        /// <returns>DbParameter.</returns>
        protected abstract DbParameter CreateDbParameter(string parameterName, string sourceColumn);

        /// <summary>
        /// Escapes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns>System.String.</returns>
        protected abstract string Escape(string source);

        /// <inheritdoc />
        public DataTable BulkInsert(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, IList<string> primaryKeyNames = null)
        {
            return BulkInsertInternal(dataTable, batchSize, options, primaryKeyNames);
        }

        /// <inheritdoc />
        public Task<DataTable> BulkInsertAsync(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, IList<string> primaryKeyNames = null, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() => BulkInsert(dataTable, batchSize, options, primaryKeyNames), cancellationToken);
        }

        /// <inheritdoc />
        public void BatchUpdate(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize)
        {
            BatchUpdateInternal(dataTable, commandText, batchSize);
        }

        /// <inheritdoc />
        public Task BatchUpdateAsync(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() => BatchUpdate(dataTable, commandText, batchSize), cancellationToken);
        }

        /// <summary>
        /// Executes the bulks insert internally.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="options">The options.</param>
        /// <param name="primaryKeyNames">The primary key names.</param>
        /// <returns>DataTable.</returns>
        private DataTable BulkInsertInternal(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, IList<string> primaryKeyNames = null)
        {
            ValidateBulkInsertParameters(dataTable);

            string trackerColumnName = null;

            try
            {
                OpenConnection();

                if (primaryKeyNames != null)
                {
                    primaryKeyNames = primaryKeyNames.Where(primaryKeyName => !string.IsNullOrWhiteSpace(primaryKeyName)).ToList();

                    if (primaryKeyNames.Any())
                    {
                        trackerColumnName = CreateTrackerColumn(dataTable);

                        DropIndex(dataTable.TableName);

                        CreateIndex(dataTable.TableName, trackerColumnName);
                    }
                }

                SetState(dataTable, DataRowState.Added);

                var bulkCopy = CreateBulkCopy(dataTable, batchSize, options);

                try
                {
                    bulkCopy.WriteToServer(dataTable);

                    if (HasTransaction())
                    {
                        Commit();
                    }
                }
                catch
                {
                    if (HasTransaction())
                    {
                        Rollback();
                    }

                    throw;
                }
                finally
                {
                    if (bulkCopy is IDisposable disposableBulkCopy)
                    {
                        disposableBulkCopy.Dispose();
                    }
                }

                if (primaryKeyNames != null && primaryKeyNames.Any())
                {
                    SetReadOnlyFalse(dataTable, primaryKeyNames);

                    RetrievePrimaryKeyValues(dataTable, trackerColumnName, primaryKeyNames);
                }
            }
            finally
            {
                DropIndex(dataTable.TableName);

                DropDbTrackerColumn(dataTable.TableName, trackerColumnName);

                RemoveTrackerColumn(dataTable, trackerColumnName);

                CloseConnection();
            }

            return dataTable;
        }

        /// <summary>
        /// Executes the batch update internally.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="batchSize">Size of the batch.</param>
        private void BatchUpdateInternal(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize)
        {
            ValidateBatchUpdateParameters(dataTable, commandText);

            try
            {
                OpenConnection();

                var parameters = BuildUpdateParameters(commandText);

                var updateCommand = CreateCommand(commandText: commandText, parameters: parameters, useInternalTransaction: true);

                updateCommand.UpdatedRowSource = UpdateRowSource.None;

                var dataAdapter = CreateDbDataAdapter(updateCommand);
#if NETFULL
                dataAdapter.UpdateBatchSize = batchSize;
#endif
                SetState(dataTable, DataRowState.Modified);

                using (updateCommand)
                using (dataAdapter)
                {
                    dataAdapter.Update(dataTable);

                    if (HasTransaction())
                    {
                        Commit();
                    }
                }
            }
            catch
            {
                if (HasTransaction())
                {
                    Rollback();
                }

                throw;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Validates the bulk insert parameters.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <exception cref="ArgumentException">TableName</exception>
        private void ValidateBulkInsertParameters(DataTable dataTable)
        {
            dataTable.ValidateParameters();

            if (string.IsNullOrWhiteSpace(dataTable.TableName))
            {
                throw new ArgumentException(nameof(dataTable.TableName));
            }
        }

        /// <summary>
        /// Validates the batch update parameters.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="commandText">The command text.</param>
        /// <exception cref="ArgumentException">commandText</exception>
        private void ValidateBatchUpdateParameters(DataTable dataTable, string commandText)
        {
            dataTable.ValidateParameters();

            if (string.IsNullOrWhiteSpace(commandText))
            {
                throw new ArgumentException(nameof(commandText));
            }
        }

        /// <summary>
        /// Drops the index.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        private void DropIndex(string tableName)
        {
            try
            {
                var commandFormat = TryGetCommand("DropIndex");
                var commandText = string.Format(commandFormat, Escape(tableName));

                using (var command = CreateCommand(commandText: commandText))
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
        /// Creates the index.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="trackerColumnName">Name of the tracker column.</param>
        private void CreateIndex(string tableName, string trackerColumnName)
        {
            try
            {
                var commandFormat = TryGetCommand("CreateIndex");
                var commandText = string.Format(commandFormat, Escape(tableName), Escape(trackerColumnName));

                using (var command = CreateCommand(commandText: commandText))
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
        /// Creates the tracker column.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns>System.String.</returns>
        private string CreateTrackerColumn(DataTable dataTable)
        {
            const string trackerColumnName = "TrackerColumn";

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

            CreateDbTrackerColumn(dataTable.TableName, trackerColumnName);

            return trackerColumnName;
        }

        /// <summary>
        /// Creates the database tracker column.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="trackerColumnName">Name of the tracker column.</param>
        private void CreateDbTrackerColumn(string tableName, string trackerColumnName)
        {
            var commandFormat = TryGetCommand("AddTrackerColumn");
            var commandText = string.Format(commandFormat, Escape(tableName), Escape(trackerColumnName));

            using (var command = CreateCommand(commandText: commandText))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Sets the read only false.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="primaryKeyNames">The primary key names.</param>
        private void SetReadOnlyFalse(DataTable dataTable, IList<string> primaryKeyNames)
        {
            var dataColumns = dataTable.Columns.Cast<DataColumn>().Where(dataColumn => primaryKeyNames.Contains(dataColumn.ColumnName));

            foreach (var dataColumn in dataColumns)
            {
                dataColumn.ReadOnly = false;
            }
        }

        /// <summary>
        /// Retrieves the primary key values.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="trackerColumnName">Name of the tracker column.</param>
        /// <param name="primaryKeyNames">The primary key names.</param>
        private void RetrievePrimaryKeyValues(DataTable dataTable, string trackerColumnName, IList<string> primaryKeyNames)
        {
            var fields = new List<string>(primaryKeyNames)
            {
                trackerColumnName
            };

            var fieldNames = fields.Select(primaryKey => $"{Escape(primaryKey)}");

            var selectClause = string.Join(",", fieldNames);

            var parameters = BuildInsertParameters(dataTable, trackerColumnName);

            var commandFormat = TryGetCommand("SelectPrimaryKeys");

            var commandText = string.Format(commandFormat, selectClause, Escape(dataTable.TableName), Escape(trackerColumnName), "@MinParam", "@MaxParam");

            using (var command = CreateCommand(commandText: commandText, parameters: parameters))
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
        /// Builds the insert parameters.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="trackerColumnName">Name of the tracker column.</param>
        /// <returns>DbParameter[].</returns>
        private DbParameter[] BuildInsertParameters(DataTable dataTable, string trackerColumnName)
        {
            var dataRows = dataTable.Rows.Cast<DataRow>();

            var parameters = new[]
            {
                CreateDbParameter("@MinParam", dataRows.Min(dataRow => Convert.ToInt32(dataRow[trackerColumnName]))),
                CreateDbParameter("@MaxParam", dataRows.Max(dataRow => Convert.ToInt32(dataRow[trackerColumnName])))
            };

            return parameters;
        }

        /// <summary>
        /// Drops the database tracker column.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="trackerColumnName">Name of the tracker column.</param>
        private void DropDbTrackerColumn(string tableName, string trackerColumnName)
        {
            if (string.IsNullOrWhiteSpace(trackerColumnName))
            {
                return;
            }

            try
            {
                var commandFormat = TryGetCommand("DropTrackerColumn");
                var commandText = string.Format(commandFormat, Escape(tableName), Escape(trackerColumnName));

                using (var command = CreateCommand(commandText: commandText))
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
        /// Removes the tracker column.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="trackerColumnName">Name of the tracker column.</param>
        private void RemoveTrackerColumn(DataTable dataTable, string trackerColumnName)
        {
            if (string.IsNullOrWhiteSpace(trackerColumnName))
            {
                return;
            }

            try
            {
                dataTable.Columns.Remove(trackerColumnName);
            }
            catch
            {
                // ignored
            }
        }

        /// <summary>
        /// Builds the update parameters.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>DbParameter[].</returns>
        private DbParameter[] BuildUpdateParameters(string commandText)
        {
            var parameters = ParametersRegex.Matches(commandText)
                                             .Cast<Match>()
                                             .Select(match =>
                                             {
                                                 var parameterName = match.Value;
                                                 var sourceColumn = match.Value.Replace("@", string.Empty);

                                                 var parameter = CreateDbParameter(parameterName, sourceColumn);

                                                 return parameter;

                                             }).ToArray();

            return parameters;
        }

        /// <summary>
        /// Sets the state.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="dataRowState">State of the data row.</param>
        private void SetState(DataTable dataTable, DataRowState dataRowState)
        {
            dataTable.AcceptChanges();

            var dataRows = dataTable.Rows.Cast<DataRow>().Where(dataRow => dataRow.RowState == DataRowState.Unchanged);

            foreach (var dataRow in dataRows)
            {
                SetStateInternal(dataRow, dataRowState);
            }
        }

        /// <summary>
        /// Sets the state internally.
        /// </summary>
        /// <param name="dataRow">The data row.</param>
        /// <param name="dataRowState">State of the data row.</param>
        private void SetStateInternal(DataRow dataRow, DataRowState dataRowState)
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
