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
using System;
using System.Data.Entity;

namespace DataTablePlus.Configuration
{
	/// <summary>
	/// Startup class that should be used in order to initialize the application and provide some required configurations
	/// </summary>
	public static class Startup
	{
		/// <summary>
		/// Provided EF DbContext
		/// </summary>
		public static DbContext DbContext { get; private set; }

		/// <summary>
		/// Provided ConnectionString
		/// </summary>
		public static string ConnectionString { get; private set; }

		/// <summary>
		/// Initializes the application providing a DbContext
		/// </summary>
		/// <typeparam name="T">Should be a DbContext</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		public static void AddDbContext<T>(T dbContext) where T : DbContext
		{
			DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");

			AddConnectionString(DbContext.Database.Connection.ConnectionString);
		}

		/// <summary>
		/// Initializes the application providing a connectionString
		/// </summary>
		/// <param name="connectionString">ConnectionString</param>
		public static void AddConnectionString(string connectionString)
		{
			ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString), $"{nameof(connectionString)} {CommonResources.CannotBeNull}");
		}
	}
}
