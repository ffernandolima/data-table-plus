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
using DataTablePlus.DataAccessContracts.Services;
using DataTablePlus.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

#if NETSTANDARD20
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
#endif

namespace DataTablePlus.DataAccess.Services
{
	/// <summary>
	/// Service that should be used to get some metadata
	/// </summary>
	public class MetadataService : ServiceBase, IMetadataService
	{
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="dbContext">Db Context</param>
		/// <param name="connectionString">Connection String</param>
		public MetadataService(DbContext dbContext = null, string connectionString = null)
			: base(dbContext, connectionString)
		{
		}

		/// <summary>
		/// Gets the table name from the mapped entity on EF
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity</typeparam>
		/// <returns>Table name or null</returns>
		public string GetTableName<T>() where T : class => this.GetTableName(typeof(T));

		/// <summary>
		/// Gets the table name from the mapped entity on EF
		/// </summary>
		/// <param name="type">Type of the mapped entity</param>
		/// <returns>Table name or null</returns>
		public string GetTableName(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), $"{nameof(type)} {CommonResources.CannotBeNull}");
			}

			return this.DbContext.GetTableName(type);
		}

		/// <summary>
		/// Gets a mapping between the model properties and the mapped column names
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity on EF</typeparam>
		/// <returns>Mapping or null</returns>
		public IDictionary<PropertyInfo, string> GetMappings<T>() where T : class => this.GetMappings(typeof(T));

		/// <summary>
		/// Gets a mapping between the model properties and the mapped column names
		/// </summary>
		/// <param name="type">Type of the mapped entity on EF</param>
		/// <returns>Mapping or null</returns>
		public IDictionary<PropertyInfo, string> GetMappings(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), $"{nameof(type)} {CommonResources.CannotBeNull}");
			}

			return this.DbContext.GetMappings(type);
		}

		/// <summary>
		/// Gets the entity keys from the mapped entity on EF
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity on EF</typeparam>
		/// <returns>A list that contains the entity keys</returns>
		public IList<string> GetKeyNames<T>() where T : class => this.GetKeyNames(typeof(T));

		/// <summary>
		/// Gets the entity keys from the mapped entity on EF
		/// </summary>
		/// <param name="type">Type of the mapped entity on EF</param>
		/// <returns>A list that contains the entity keys</returns>
		public IList<string> GetKeyNames(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), $"{nameof(type)} {CommonResources.CannotBeNull}");
			}

			return this.DbContext.GetKeyNames(type);
		}

		/// <summary>
		/// Gets the database keys based on the EF mappings
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity on EF</typeparam>
		/// <returns>A list that contains the database keys</returns>
		public IList<string> GetDbKeyNames<T>() where T : class => this.GetDbKeyNames(typeof(T));

		/// <summary>
		/// Gets the database keys based on the EF mappings
		/// </summary>
		/// <param name="type">Type of the mapped entity on EF</param>
		/// <returns>A list that contains the database keys</returns>
		public IList<string> GetDbKeyNames(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), $"{nameof(type)} {CommonResources.CannotBeNull}");
			}

			return this.DbContext.GetDbKeyNames(type);
		}

		/// <summary>
		/// Gets the database table schema
		/// </summary>
		/// <param name="tableName">The name of the database table</param>
		/// <returns>A datatable that represents a mirror of the database table schema</returns>
		public DataTable GetTableSchema(string tableName)
		{
			if (string.IsNullOrWhiteSpace(tableName))
			{
				throw new ArgumentException($"{nameof(tableName)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(tableName));
			}

			DataTable dataTable;

			try
			{
				this.OpenConnection();

				var commandText = string.Format(DataResources.GetSchemaTable, tableName);

				using (var command = this.CreateCommand(commandText: commandText))
				using (var reader = command.ExecuteReader())
				{
					dataTable = new DataTable
					{
						TableName = tableName
					};

					dataTable.BeginLoadData();
					dataTable.Load(reader);
					dataTable.EndLoadData();
				}
			}
			finally
			{
				this.CloseConnection();
			}

			return dataTable;
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
