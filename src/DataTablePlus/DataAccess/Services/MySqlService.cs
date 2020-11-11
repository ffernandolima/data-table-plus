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
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
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
    /// Class MySqlService.
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.SqlService" />
    /// </summary>
    /// <seealso cref="DataTablePlus.DataAccess.Services.SqlService" />
    public class MySqlService : SqlService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionString">The connection string.</param>
        public MySqlService(DbContext dbContext = null, string connectionString = null)
            : base(DbProvider.MySQL, dbContext, connectionString)
        { }

        /// <inheritdoc />
        protected override IDictionary<string, string> BuildCommands()
        {
            var commands = new Dictionary<string, string>
            {
                ["DropIndex"] = "DROP INDEX IX_TrackerColumn ON {0};",
                ["CreateIndex"] = "CREATE INDEX IX_TrackerColumn ON {0} ({1});",
                ["AddTrackerColumn"] = "ALTER TABLE {0} ADD COLUMN {1} INT NULL;",
                ["SelectPrimaryKeys"] = "SELECT {0} FROM {1} WHERE {2} >= {3} AND {2} <= {4} ORDER BY {2};",
                ["DropTrackerColumn"] = "ALTER TABLE {0} DROP COLUMN {1};",
            };

            return new ReadOnlyDictionary<string, string>(commands);
        }

        /// <inheritdoc />
        protected override dynamic CreateBulkCopy(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, bool? createColumnMappings = true)
        {
            const BulkCopyOptions BulkCopyOptions = BulkCopyOptions.UseInternalTransaction;

            var connection = DbConnection as MySqlConnection;
            var copyOptions = options ?? BulkCopyOptions;

            DbTransaction dbTransaction = null;

            if (copyOptions.HasFlag(BulkCopyOptions.UseInternalTransaction))
            {
                dbTransaction = BeginTransaction();
            }

            var transaction = dbTransaction as MySqlTransaction;

            var bulkCopy = new MySqlBulkCopy(connection, transaction)
            {
                DestinationTableName = Escape(dataTable.TableName),
                BulkCopyTimeout = Convert.ToInt32(Timeout.TotalSeconds)
            };

            if (createColumnMappings.GetValueOrDefault())
            {
                foreach (var dataColumn in dataTable.Columns.Cast<DataColumn>())
                {
                    var columnMapping = new MySqlBulkCopyColumnMapping(dataColumn.Ordinal, dataColumn.ColumnName);
                    bulkCopy.ColumnMappings.Add(columnMapping);
                }
            }

            return bulkCopy;
        }

        /// <inheritdoc />
        protected override DbDataAdapter CreateDbDataAdapter(DbCommand updateCommand)
        {
            var dataAdapter = new MySqlDataAdapter
            {
                UpdateCommand = updateCommand as MySqlCommand
            };

            return dataAdapter;
        }

        /// <inheritdoc />
        protected override DbParameter CreateDbParameter(string parameterName, object parameterValue)
        {
            var parameter = DbParameterFactory.Instance.CreateDbParameter<MySqlParameter>(parameterName, parameterValue);

            return parameter;
        }

        /// <inheritdoc />
        protected override DbParameter CreateDbParameter(string parameterName, string sourceColumn)
        {
            var parameter = DbParameterFactory.Instance.CreateDbParameter<MySqlParameter>(parameterName, sourceColumn);

            return parameter;
        }

        /// <inheritdoc />
        protected override string Escape(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            const string Separator = ".";

            return string.Join(Separator, source.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries).Select(value => $"`{value}`"));
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
