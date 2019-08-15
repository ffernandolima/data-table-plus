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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DataTablePlus.DataAccessContracts.Services
{
	/// <summary>
	/// SqlService interface
	/// </summary>
	public interface ISqlService : IServiceBase, IDisposable
	{
		/// <summary>
		/// Executes a bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="batchSize">The batch number which will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <returns>Returns the data table filled out with primary keys or not, depends on the primaryKeyNames parameter</returns>
		DataTable BulkInsert(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null);

		/// <summary>
		/// Executes an async bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="batchSize">The batch number which will be considered while inserting</param>
		/// <param name="options">Bulk insert options></param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <returns>Returns the data table filled out with primary keys or not, depends on the primaryKeyNames parameter</returns>
		Task<DataTable> BulkInsertAsync(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Executes a batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		void BatchUpdate(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize);

		/// <summary>
		/// Executes an async batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table with data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <returns>Returns a task which will be processing the update</returns>
		Task BatchUpdateAsync(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize, CancellationToken cancellationToken = default);
	}
}
