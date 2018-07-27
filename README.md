# DataTablePlus for .NET

DataTablePlus provides some extensions in order to transform object lists into data tables based on the object mappings (it's able to use the entity framework mappings or just the objects structure) and also some sql helpers which perform some batch operations using the previously built data tables. This application is focused on solving some performance issues while ingesting or updating a lot of data (represented as objects).

# Dependencies

- EntityFramework >= 6.2.0

# Nuget Status

[![NuGet Version](https://img.shields.io/nuget/v/DataTablePlus.svg)](https://www.nuget.org/packages/DataTablePlus/ "NuGet Version")
[![NuGet Downloads](https://img.shields.io/nuget/dt/DataTablePlus.svg)](https://www.nuget.org/packages/DataTablePlus/ "NuGet Downloads")

# Getting Started

- Configure the EF and the ConnectionString in the App.config:

```XML

<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="entityFramework" type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" requirePermission="false"/>
	</configSections>
	<connectionStrings>
		<add name="Context" providerName="System.Data.SqlClient" connectionString="YourConnectionString"/>
	</connectionStrings>
	<entityFramework>
		<defaultConnectionFactory type="System.Data.Entity.Infrastructure.LocalDbConnectionFactory, EntityFramework">
		</defaultConnectionFactory>
		<providers>
			<provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer"/>
		</providers>
	</entityFramework>
</configuration>

```

- Create a new database table:

```PLSQL

	CREATE TABLE [dbo].[User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](250) NOT NULL,
	[Email] [varchar](150) NOT NULL,
	[Password] [varchar](255) NOT NULL,
 	CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
	(
		[UserId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	GO
  
 ```

- Create a new data model and a new mapping configuration that represent the database table:

```C#

    public partial class User
    {
        public User()
        { }

        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
  
    public class UserMap : EntityTypeConfiguration<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.UserId);

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
            this.Property(t => t.UserId).HasColumnName("UserId");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.Password).HasColumnName("Password");

            // Relationships
        }
    }
  
```   
      
- Add this configuration to the DbContext configurations:

```C#

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
			modelBuilder.Configurations.Add(new UserMap());
		}
	}
  
```
  
- Code Examples:

```C#
	
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
			//	- About 1 minute retrieving the primary key values
			//	- About 5 seconds whitout retrieving the primary key values

			// Batch Update time spent: 
			//	- About 50 seconds updating 1 000 000 of rows

			// The measurement was taken while running some tests in Debug mode, so in Release mode it should be faster
			// To sum up, although it was taken in Debug mode, it is still faster than Entity Framework (much faster)
			
			// Sets the culture to invariant in order to avoid some exception details in another language
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			
			// Creates the DbContext
			var context = new Context();

			// Adds the DbContext to DataTablePlus configurations
			Startup.AddDbContext(context);

			// Also, a connection string can be added to these configurations 
			// You should specify at least one of them (DbContext and/or a ConnectionString) 
			// Just a reminder: if a DbContext was not provided, the EF extensions will not be available

			// Gets the connection string from the configuration file
			var connectionString = ConfigurationManager.ConnectionStrings["Context"].ConnectionString;

			// Adds the connection string to DataTablePlus configurations
			Startup.AddConnectionString(connectionString);

			// Creates a list of Users
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
				// It is optional, does not need to be set
				// Not setting them can save a lot of time

				// Gets the primary key names from the entity mapping
				var databaseKeyNames = metadataService.GetDbKeyNames(entities.GetTypeFromCollection());

				// You can specify the primary key names directly, get from another source or pass null
				// var databaseKeyNames = new List<string> { "UserId" };
				// Or
				// IList<string> databaseKeyNames = null;

				// Creates a Stopwatch, just to know the time which was spent during the execution
				var stopwatch = Stopwatch.StartNew();

				// Invokes the BulkInsert method
				// You can also pass the BatchSize and the SqlBulkCopyOptions parameters to this method

				// BatchSize will be used to flush the values against the database table
				// SqlBulkCopyOptions can be mixed up to get lots of advantages, by default some options will be set

				sqlService.BulkInsert(dataTable: dataTable, primaryKeyNames: databaseKeyNames);

				// Stops the Stopwatch
				stopwatch.Stop();

				// Gets the total time spent
				Debug.WriteLine($"Bulk Insert Elapsed Time: {stopwatch.Elapsed}");

				if (databaseKeyNames != null && databaseKeyNames.Any())
				{
					// Reestarts the Stopwatch
					stopwatch.Restart();

					// Invokes the BatchUpdate method
					// You can also pass the BatchSize parameter to this method
					// BatchSize will be used to flush the values against the database table

					sqlService.BatchUpdate(dataTable, "Update [User] SET [Name] = 'Batch Update Usage Example' WHERE [UserId] = @UserId");

					// Stops the Stopwatch
					stopwatch.Stop();

					// Gets the total time spent
					Debug.WriteLine($"Batch Update Elapsed Time: {stopwatch.Elapsed}");
				}
			}
		}
  
```
