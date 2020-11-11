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
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DataTablePlus.DataAccess.Services.Contracts
{
    /// <summary>
    /// Interface ISqlService
    /// Implements the <see cref="DataTablePlus.DataAccess.Services.Contracts.IServiceBase" />
    /// </summary>
    /// <seealso cref="DataTablePlus.DataAccess.Services.Contracts.IServiceBase" />
    public interface ISqlService : IServiceBase
    {
        /// <summary>
        /// Executes the bulk insert.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="options">The options.</param>
        /// <param name="primaryKeyNames">The primary key names.</param>
        /// <returns>DataTable.</returns>
        DataTable BulkInsert(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, IList<string> primaryKeyNames = null);

        /// <summary>
        /// Executes the bulk insert asynchronous.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="options">The options.</param>
        /// <param name="primaryKeyNames">The primary key names.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;DataTable&gt;.</returns>
        Task<DataTable> BulkInsertAsync(DataTable dataTable, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, IList<string> primaryKeyNames = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the batch update.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="batchSize">Size of the batch.</param>
        void BatchUpdate(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize);

        /// <summary>
        /// Executes the batch update asynchronous.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task BatchUpdateAsync(DataTable dataTable, string commandText, int batchSize = DataConstants.BatchSize, CancellationToken cancellationToken = default);
    }
}
