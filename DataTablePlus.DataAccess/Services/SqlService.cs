/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 * 
 * See https://github.com/ffernandolima/data-table-plus for details.
 *
 * Copyright (C) 2018 Fernando Luiz de Lima
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 *
 * The GNU Lesser General Public License can be viewed at http://www.opensource.org/licenses/lgpl-license.php
 * If you unfamiliar with this license or have questions about it, here is a FAQ: http://www.gnu.org/licenses/gpl-faq.html
 *
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 * 
 *******************************************************************************/

using DataTablePlus.DataAccessContracts;
using DataTablePlus.DataAccessContracts.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace DataTablePlus.DataAccess.Services
{
	/// <summary>
	/// Service that should be used in order to ingest or update a large amount of data
	/// </summary>
	public class SqlService : ServiceBase, ISqlService
	{
		private static readonly Regex PARAMETERS_REGEX = new Regex(@"\@\w+", RegexOptions.Compiled);
		private static readonly TimeSpan DEFAULT_TIMEOUT = TimeSpan.FromMinutes(1);

		/// <summary>
		/// Sql command Timeout
		/// </summary>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// Parameterless Ctor
		/// </summary>
		public SqlService()
		{
			this.Timeout = DEFAULT_TIMEOUT;
		}

		/// <summary>
		/// Executes a bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		public void BulkInsert(DataTable dataTable, int batchSize = DataConstants.BATCHSIZE, SqlBulkCopyOptions? options = null)
		{
			if (dataTable == null)
			{
				throw new ArgumentNullException(nameof(dataTable));
			}

			try
			{
				if (this.SqlConnection.State != ConnectionState.Open)
				{
					this.SqlConnection.Open();
				}

				#region SqlBulkCopyOptions

				// src: https://msdn.microsoft.com/pt-br/library/system.data.sqlclient.sqlbulkcopyoptions(v=vs.110).aspx
				//
				// CheckConstraints: Check constraints while data is being inserted. By default, constraints are not checked.
				// KeepNulls: Preserve null values in the destination table regardless of the settings for default values. When not specified, null values are replaced by default values where applicable.
				// TableLock: Obtain a bulk update lock for the duration of the bulk copy operation. When not specified, row locks are used.
				// UseInternalTransaction: When specified, each batch of the bulk-copy operation will occur within a transaction. If you indicate this option and also provide a SqlTransaction object to the constructor, an ArgumentException occurs.

				#endregion

				const SqlBulkCopyOptions SQL_BULK_COPY_OPTIONS = SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepNulls | SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction;

				var sqlBulkCopy = new SqlBulkCopy(this.SqlConnection, options ?? SQL_BULK_COPY_OPTIONS, null)
				{
					BatchSize = batchSize,
					DestinationTableName = dataTable.TableName,
					BulkCopyTimeout = Convert.ToInt32(this.Timeout.TotalSeconds)
				};

				foreach (var dataColumn in dataTable.Columns.Cast<DataColumn>().Where(c => !(c is IgnoredDataColumn)))
				{
					sqlBulkCopy.ColumnMappings.Add(dataColumn.ColumnName, dataColumn.ColumnName);
				}

				dataTable.AcceptChanges();

				foreach (var dataRow in dataTable.Rows.Cast<DataRow>().Where(dataRow => dataRow.RowState == DataRowState.Unchanged))
				{
					dataRow.SetAdded();
				}

				using (sqlBulkCopy)
				{
					sqlBulkCopy.WriteToServer(dataTable);
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				if (this.SqlConnection.State != ConnectionState.Closed)
				{
					this.SqlConnection.Close();
				}
			}
		}

		/// <summary>
		/// Executes a batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		public void BatchUpdate(DataTable dataTable, string commandText, int batchSize = DataConstants.BATCHSIZE)
		{
			if (dataTable == null)
			{
				throw new ArgumentNullException(nameof(dataTable));
			}

			if (string.IsNullOrWhiteSpace(commandText))
			{
				throw new ArgumentException(nameof(commandText));
			}

			SqlTransaction transaction = null;

			try
			{
				if (this.SqlConnection.State != ConnectionState.Open)
				{
					this.SqlConnection.Open();
				}

				transaction = this.SqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);

				var updateCommand = new SqlCommand(commandText, this.SqlConnection, transaction)
				{
					UpdatedRowSource = UpdateRowSource.None,
					CommandTimeout = Convert.ToInt32(this.Timeout.TotalSeconds)
				};

				var parameters = PARAMETERS_REGEX.Matches(commandText)
												 .Cast<Match>()
												 .Select(x => new SqlParameter
												 {
													 ParameterName = x.Value,
													 SourceColumn = x.Value.Replace("@", string.Empty)

												 }).ToList();

				updateCommand.Parameters.AddRange(parameters.ToArray());

				var sqlDataAdapter = new SqlDataAdapter
				{
					UpdateCommand = updateCommand,
					UpdateBatchSize = batchSize
				};

				dataTable.AcceptChanges();

				foreach (var dataRow in dataTable.Rows.Cast<DataRow>().Where(dataRow => dataRow.RowState == DataRowState.Unchanged))
				{
					dataRow.SetModified();
				}

				using (updateCommand)
				using (sqlDataAdapter)
				{
					sqlDataAdapter.Update(dataTable);
					transaction.Commit();
				}
			}
			catch (Exception)
			{
				if (transaction != null)
				{
					try
					{
						transaction.Rollback();
					}
					catch
					{
						// ignored
					}
				}

				throw;
			}
			finally
			{
				if (this.SqlConnection.State != ConnectionState.Closed)
				{
					this.SqlConnection.Close();
				}
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
