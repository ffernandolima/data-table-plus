﻿/*******************************************************************************
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
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <returns>Returns the data table filled out with primary keys or not, depends on the primaryKeyNames parameter</returns>
		DataTable BulkInsert(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null);

		/// <summary>
		/// Executes an async bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options></param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <returns>Returns a task and as a result after running the bulk insert a data table filled out with primary keys or not will be returned</returns>
		Task<DataTable> BulkInsertAsync(DataTable dataTable, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null);

		/// <summary>
		/// Executes an async bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options></param>
		/// <param name="primaryKeyNames">Primary key names to retrieve their values after the bulk insert</param>
		/// <returns>Returns a task and as a result after running the bulk insert a data table filled out with primary keys or not will be returned</returns>
		Task<DataTable> BulkInsertAsync(DataTable dataTable, CancellationToken cancellationToken, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, IList<string> primaryKeyNames = null);

		/// <summary>
		/// Executes a batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		void BatchUpdate(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize);

		/// <summary>
		/// Executes an async batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		/// <returns>Returns a task which will be processing the update</returns>
		Task BatchUpdateAsync(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize);

		/// <summary>
		/// Executes an async batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <param name="dataTable">Data table that contains the data</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <param name="batchSize">The batch number that will be considered while updating</param>
		/// <returns>Returns a task which will be processing the update</returns>
		Task BatchUpdateAsync(DataTable dataTable, string commandText, CancellationToken cancellationToken, int batchSize = DataConstants.BatchSize);
	}
}
