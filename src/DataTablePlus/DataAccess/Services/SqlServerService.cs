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
using DataTablePlus.Factories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
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
    /// Class SqlServerService.
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.SqlService" />
    /// </summary>
    /// <seealso cref="DataTablePlus.DataAccess.Services.SqlService" />
    public class SqlServerService : SqlService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        public SqlServerService(DbContext dbContext = null, string connectionString = null)
            : base(DbProvider.SQLServer, dbContext, connectionString)
        { }

        /// <inheritdoc />
        protected override IDictionary<string, string> BuildCommands()
        {
            var commands = new Dictionary<string, string>
            {
                ["DropNonClusteredIndex"] = "IF EXISTS (SELECT name FROM sys.indexes WHERE name = N'IX_TrackerColumn') DROP INDEX IX_TrackerColumn ON {0}",
                ["CreateNonClusteredIndex"] = "CREATE NONCLUSTERED INDEX IX_TrackerColumn ON {0} ({1})",
                ["AddTrackerColumnStatement"] = "ALTER TABLE {0} ADD {1} INT NULL",
                ["DropTrackerColumnStatement"] = "ALTER TABLE {0} DROP COLUMN {1}",
            };

            return new ReadOnlyDictionary<string, string>(commands);
        }

        /// <inheritdoc />
        protected override dynamic CreateBulkCopy(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, bool? createColumnMappings = true)
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

            var connection = DbConnection as SqlConnection;
            var copyOptions = options.HasValue ? (SqlBulkCopyOptions)options.Value.GetHashCode() : SqlBulkCopyOptions;

            var bulkCopy = new SqlBulkCopy(connection, copyOptions, externalTransaction: null)
            {
                BatchSize = batchSize,
                DestinationTableName = dataTable.TableName,
                BulkCopyTimeout = Convert.ToInt32(Timeout.TotalSeconds)
            };

            if (createColumnMappings.GetValueOrDefault())
            {
                foreach (var dataColumn in dataTable.Columns.Cast<DataColumn>())
                {
                    bulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
                }
            }

            return bulkCopy;
        }

        /// <inheritdoc />
        protected override DbDataAdapter CreateDbDataAdapter(DbCommand updateCommand)
        {
            var dataAdapter = new SqlDataAdapter
            {
                UpdateCommand = updateCommand as SqlCommand
            };

            return dataAdapter;
        }

        /// <inheritdoc />
        protected override DbParameter CreateDbParameter(string parameterName, object parameterValue)
        {
            var parameter = DbParameterFactory.Instance.CreateDbParameter<SqlParameter>(parameterName, parameterValue);

            return parameter;
        }

        /// <inheritdoc />
        protected override DbParameter CreateDbParameter(string parameterName, string sourceColumn)
        {
            var parameter = DbParameterFactory.Instance.CreateDbParameter<SqlParameter>(parameterName, sourceColumn);

            return parameter;
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
