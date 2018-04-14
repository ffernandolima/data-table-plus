/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 *
 * DataTablePlus provides some extensions in order to transform list of objects in data tables
 * based on the object mappings (Mappings which come from EntityFramework configurations) and also some sql helpers which perform
 * some batch operations using the data tables previously built.
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

using DataTablePlus.Configuration;
using DataTablePlus.DataAccess.Services;
using DataTablePlus.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.Entity;

namespace DataTablePlus.UnitTests
{
	[TestClass]
	public class UnitTestClass
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			var context = new Context();

			Startup.AddDbContext(context);
		}

		[TestMethod]
		public void GeneralTestMethod()
		{
			// Some things should be done before running this test method.
			//
			// - Configure the EF and the ConnectionString in the App.config
			// - Create a new database table
			// - Create a new data model and a new mapping configuration that represent the database table
			// - Add this configuration to the DbContext configurations
			// - Change the list type in order to use the data model type that has been created in the previous steps

			var entities = new List<object>();

			var dataTable = entities.AsStronglyTypedDataTable();

			using (var sqlService = new SqlService())
			{
				sqlService.BulkInsert(dataTable);

				sqlService.BatchUpdate(dataTable, string.Empty);
			}
		}
	}

	/// <summary>
	/// Sample DbContext class
	/// </summary>
	internal class Context : DbContext
	{
		static Context()
		{
			Database.SetInitializer<Context>(null);
		}

		public Context()
			: this("Name=Context")
		{
		}

		protected Context(string connectionStringName)
			: base(connectionStringName)
		{
			this.Configuration.LazyLoadingEnabled = false;
			this.Configuration.ProxyCreationEnabled = false;
			this.Configuration.AutoDetectChangesEnabled = false;
			this.Configuration.ValidateOnSaveEnabled = false;
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			// Add the configurations here like this:
			//
			// modelBuilder.Configurations.Add(new YourConfigurationMap());
		}
	}
}
