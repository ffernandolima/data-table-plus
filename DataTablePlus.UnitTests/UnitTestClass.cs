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

using DataTablePlus.Configuration;
using DataTablePlus.DataAccess.Services;
using DataTablePlus.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DataTablePlus.UnitTests
{
	[TestClass]
	public class UnitTestClass
	{
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			// Sets the culture to invariant culture in order to avoid some exception details in another language
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			// Creates the DbContext
			var context = new Context();

			// Adds the DbContext to DataTablePlus configurations
			Startup.AddDbContext(context);

			// Also, a connection string can be added to these configurations 
			// You should specify at least one of them (DbContext and/or a ConnectionString) and just a reminder: if a DbContext was not provided, the EF extensions will not be available

			// Gets the connection string from the configuration file
			var connectionString = ConfigurationManager.ConnectionStrings["Context"].ConnectionString;

			// Adds the connection string to DataTablePlus configurations
			Startup.AddConnectionString(connectionString);
		}

		[TestMethod]
		public void GeneralTestMethod()
		{
			// Some things should be done before running this test method:
			//
			// - Configure the EF and the ConnectionString in the App.config
			// - Create a new database table
			// - Create a new data model and a new mapping configuration that represent the database table
			// - Add this configuration to the DbContext configurations

			// Notes:
			// this example builds a list of 1 000 000 objects, executes a bulk insert and after that a batch update

			// Bulk Insert time spent: 
			//	- About 1 minute retrieving the primary key names
			//	- About 5 seconds whitout retrieving the primary key names

			// Batch Update time spent: 
			//	- About 50 seconds updating 1 000 000 of rows

			// The measurement was taken while running some tests in Debug mode, so in Release mode it should be faster
			// To sum up, although it was taken in Debug mode, still faster than Entity Framework (much faster)

			// Creates a list of User objects
			var entities = new List<User>();

			for (int i = 0; i < 1000000; i++)
			{
				var entity = new User
				{
					Name = $"John Doe {i}",
					Email = $"johndoe{i}@gmail.com",
					Password = "rH&n&}eEB7!v5d&}"
				};

				entities.Add(entity);
			}

			// Creates the data table using the extensions method
			// You can also construct your data table by another way, but this method can simplify it
			var dataTable = entities.AsStronglyTypedDataTable();

			// Creates the services
			// ps.: The MetadataService is needed only to get the primary key names, if you do not want to get them automatically, do not need to create this instance
			using (var metadataService = new MetadataService())
			using (var sqlService = new SqlService())
			{
				// Overrides the default timeout setting 2 minutes to ensure that the data will be inserted successfully
				// ps.: Default timeout is 1 minute
				sqlService.Timeout = TimeSpan.FromMinutes(2);

				// Setting the primary key names and passing them as parameter, their values will be retrieved from the database after the bulk insert execution
				// It's optional, does not need to be set
				// Not setting them can save a lot of time

				// Gets the primary key names from the entity mapping
				var databaseKeyNames = metadataService.GetDbKeyNames(entities.GetTypeFromEnumerable());

				// You can specify the primary key names directly, get from another source or pass null
				// var databaseKeyNames = new List<string> { "Id" };
				// Or
				// IList<string> databaseKeyNames = null;

				// Creates a Stopwatch, just to know the time which was spent during the execution
				var stopwatch = Stopwatch.StartNew();

				// Invokes the BulkInsert method
				// You can also pass the BatchSize and the SqlBulkCopyOptions parameters to this method

				// BatchSize will be used to flush the values against the database table
				// SqlBulkCopyOptions can be mixed up to get a lot of advantages, by default some options will be set

				var bulkInsertTask = sqlService.BulkInsertAsync(dataTable: dataTable, primaryKeyNames: databaseKeyNames);

				// You can do something here while waiting for the task completion

				// Waits for the task completion
				dataTable = bulkInsertTask.Result;

				// Stops the Stopwatch
				stopwatch.Stop();

				// Gets the total of time spent
				Debug.WriteLine($"Bulk Insert Elapsed Time: {stopwatch.Elapsed}");

				if (databaseKeyNames != null && databaseKeyNames.Any())
				{
					// Reestarts the Stopwatch
					stopwatch.Restart();

					// Invokes the BatchUpdate method
					// You can also pass the BatchSize parameter to this method
					// BatchSize will be used to flush the values against the database table

					var batchUpdateTask = sqlService.BatchUpdateAsync(dataTable, "Update [User] SET [Name] = 'Batch Update Usage Example' WHERE [Id] = @Id");

					// You can do something here while waiting for the task completion

					// Waits for the task completion
					batchUpdateTask.Wait();

					// Stops the Stopwatch
					stopwatch.Stop();

					// Gets the total of time spent
					Debug.WriteLine($"Batch Update Elapsed Time: {stopwatch.Elapsed}");
				}
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
			modelBuilder.Configurations.Add(new UserMap());
		}
	}

	/// <summary>
	/// Sample POCO class
	/// </summary>
	public partial class User
	{
		public User()
		{ }

		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
	}

	/// <summary>
	/// Sample POCO class EF mapping
	/// </summary>
	public class UserMap : EntityTypeConfiguration<User>
	{
		public UserMap()
		{
			// Primary Key
			this.HasKey(t => t.Id);

			// Properties
			this.Property(t => t.Name)
				.HasMaxLength(250)
				.IsRequired();

			this.Property(t => t.Email)
				.HasMaxLength(150)
				.IsRequired();

			this.Property(t => t.Password)
				.HasMaxLength(255)
				.IsRequired();

			// Table & Column Mappings
			this.ToTable("User");
			this.Property(t => t.Id).HasColumnName("Id");
			this.Property(t => t.Name).HasColumnName("Name");
			this.Property(t => t.Email).HasColumnName("Email");
			this.Property(t => t.Password).HasColumnName("Password");

			// Relationships
		}
	}
}
