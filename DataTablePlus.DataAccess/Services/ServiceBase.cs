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

using DataTablePlus.Common;
using DataTablePlus.Configuration;
using DataTablePlus.DataAccessContracts.Services;
using System;
using System.Data.Entity;
using System.Data.SqlClient;

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
		/// Ctor
		/// </summary>
		public ServiceBase()
		{
			this.Construct();
		}

		/// <summary>
		/// Fill the properties out using the values from the Startup
		/// </summary>
		private void Construct()
		{
			var dbContext = Startup.DbContext;
			var connectionString = Startup.ConnectionString;

			if (dbContext != null)
			{
				this.DbContext = dbContext;

				ValidateConnectionString(this.DbContext.Database.Connection.ConnectionString);

				this.SqlConnection = this.DbContext.Database.Connection as SqlConnection;
			}

			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				ValidateConnectionString(connectionString);

				this.SqlConnection = new SqlConnection(connectionString);
			}

			if (this.DbContext == null && this.SqlConnection == null)
			{
				throw new ArgumentNullException($"{CommonResources.App_MissingConfiguration}");
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
				throw new Exception($"{CommonResources.App_InvalidConnectionString}");
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
