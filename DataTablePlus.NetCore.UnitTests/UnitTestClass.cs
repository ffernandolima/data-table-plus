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

using DataTablePlus.Configuration;
using DataTablePlus.DataAccess.Services;
using DataTablePlus.DataAccessContracts.Services;
using DataTablePlus.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;

namespace DataTablePlus.NetCore.UnitTests
{
	public class UnitTestClass
	{
		public UnitTestClass()
		{
			// Sets the culture to invariant in order to avoid some exception details in another language
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

			var configurationBuilder = new ConfigurationBuilder();

			// Gets the connection string from the configuration file
			var configuration = configurationBuilder.AddJsonFile("appsettings.json").Build();
			var connectionString = configuration.GetConnectionString("Context");

			var dbContextOptionsBuilder = new DbContextOptionsBuilder<Context>();

			dbContextOptionsBuilder.UseSqlServer(connectionString);
			dbContextOptionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

			// Creates the DbContext
			var context = new Context(dbContextOptionsBuilder.Options);

			// Adds the DbContext to DataTablePlus configurations
			Startup.AddDbContext(context);

			// Also, a connection string can be added to these configurations 
			// You should specify at least one of them (DbContext and/or a ConnectionString) and just a reminder: if a DbContext was not provided, the EF extensions will not be available

			// Adds the connection string to DataTablePlus configurations
			Startup.AddConnectionString(connectionString);
		}

		[Fact]
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
			//	- About 1 minute retrieving the primary key values
			//	- About 5 seconds whitout retrieving the primary key values

			// Batch Update time spent: 
			//	- About 50 seconds updating 1 000 000 of rows

			// The measurement was taken while running some tests in Debug mode, so in Release mode it should be faster
			// To sum up, although it was taken in Debug mode, it is still faster than Entity Framework (much faster)

			// Creates a list of Users
			IList<User> entities = new List<User>();

			for (int i = 0; i < 100; i++)
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
			using (IMetadataService metadataService = new MetadataService())
			using (ISqlService sqlService = new SqlService())
			{
				// Overrides the default timeout setting 2 minutes to ensure that the data will be inserted successfully
				// ps.: Default timeout is 1 minute
				sqlService.Timeout = TimeSpan.FromMinutes(2);

				// Setting the primary key names and passing them as parameter, their values will be retrieved from the database after the bulk insert execution
				// It is optional, does not need to be set
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

				// var bulkInsertTask = sqlService.BulkInsertAsync(dataTable: dataTable, primaryKeyNames: databaseKeyNames);

				// You can do something here while waiting for the task completion

				// Waits for the task completion
				// dataTable = bulkInsertTask.Result;

				dataTable = sqlService.BulkInsert(dataTable: dataTable, primaryKeyNames: databaseKeyNames);

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

					// var batchUpdateTask = sqlService.BatchUpdateAsync(dataTable, "Update [User] SET [Name] = 'Batch Update Usage Example' WHERE [Id] = @Id");

					// You can do something here while waiting for the task completion

					// Waits for the task completion
					// batchUpdateTask.Wait();

					sqlService.BatchUpdate(dataTable, "Update [User] SET [Name] = 'Batch Update Usage Example' WHERE [Id] = @Id");

					// Stops the Stopwatch
					stopwatch.Stop();

					// Gets the total of time spent
					Debug.WriteLine($"Batch Update Elapsed Time: {stopwatch.Elapsed}");
				}

				// Transforms back the data table into a list of objects
				entities = dataTable.ToList<User>();
			}
		}
	}

	/// <summary>
	/// Sample DbContext class
	/// </summary>
	internal class Context : DbContext
	{
		public Context(DbContextOptions<Context> options)
		: base(options)
		{
			this.Database.AutoTransactionsEnabled = false;
			this.ChangeTracker.AutoDetectChangesEnabled = false;
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.ApplyConfiguration(new UserMap());
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
	public class UserMap : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			// Primary Key
			builder.HasKey(t => t.Id);

			// Properties
			builder.Property(t => t.Name)
				.HasMaxLength(250)
				.IsRequired();

			builder.Property(t => t.Email)
				.HasMaxLength(150)
				.IsRequired();

			builder.Property(t => t.Password)
				.HasMaxLength(255)
				.IsRequired();

			// Table & Column Mappings
			builder.ToTable("User");
			builder.Property(t => t.Id).HasColumnName("Id");
			builder.Property(t => t.Name).HasColumnName("Name");
			builder.Property(t => t.Email).HasColumnName("Email");
			builder.Property(t => t.Password).HasColumnName("Password");

			// Relationships
		}
	}
}
