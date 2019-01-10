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
using DataTablePlus.DataAccess.Services;
using DataTablePlus.DataAccessContracts;
using DataTablePlus.DataAccessContracts.Services;
using DataTablePlus.Threading;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD20
using Microsoft.EntityFrameworkCore;
#endif

#if NETFULL
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
#endif

namespace DataTablePlus.Extensions
{
	/// <summary>
	/// Class that contains DbContext extensions
	/// </summary>
	public static class DbContextExtensions
	{
		#region DataSpace Enum Explanation

		// C-Space - This is where the metadata about our conceptual model is found. Here we will get access to all Edm objects and the tables in our generated model.
		// S-Space - This is where metadata about the database is found. Here we will get access to all Sql objects and the tables in our database
		// O-Space - This is where metadata about the CLR types that map to our conceptual model is found
		// CS-Space - This is where metadata about mapping is found
		// OC-Space - This is where EF holds the mapping between our conceptual model (C-Space) and the CLR objects (O-Space).

		#endregion DataSpace Enum Explanation

		/// <summary>
		/// Tries to get a table name from a mapped entity
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entityType">Type of the entity</param>
		/// <returns>Table name or null</returns>
		internal static string GetTableName(this DbContext dbContext, Type entityType)
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");
			}

			if (entityType == null)
			{
				throw new ArgumentNullException(nameof(entityType), $"{nameof(entityType)} {CommonResources.CannotBeNull}");
			}

			string tableName = null;

#if NETFULL
			var objectContext = dbContext.GetObjectContext();

			var metadataWorkspace = objectContext.MetadataWorkspace;

			if (metadataWorkspace != null)
			{
				var entitySetBase = metadataWorkspace.GetItemCollection(DataSpace.SSpace)
													 .GetItems<EntityContainer>()
													 .Single()
													 .BaseEntitySets.SingleOrDefault(x => x.Name == entityType.Name);

				if (entitySetBase != null && entitySetBase.MetadataProperties != null && entitySetBase.MetadataProperties.Any())
				{
					var schemaMetadataProperty = entitySetBase.MetadataProperties["Schema"];

					var tableMetadataProperty = entitySetBase.MetadataProperties["Table"];

					tableName = $"[{schemaMetadataProperty.Value ?? Constants.DefaultSchema}].[{tableMetadataProperty.Value}]";
				}
			}
#endif

#if NETSTANDARD20
			var entityTypeObject = dbContext?.Model?.FindEntityType(entityType);

			if (entityTypeObject == null)
			{
				throw new ArgumentNullException(nameof(entityTypeObject), $"{nameof(entityTypeObject)} {CommonResources.CannotBeNull}");
			}

			var relationalEntityTypeAnnotations = entityTypeObject.Relational();

			tableName = $"[{relationalEntityTypeAnnotations.Schema ?? Constants.DefaultSchema}].[{relationalEntityTypeAnnotations.TableName}]";
#endif
			return tableName;
		}

		/// <summary>
		/// Tries to build a dictionary that contains a mapping between the model properties and the mapped column names
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entityType">Type of the entity</param>
		/// <returns>Dictionary that contains a mapping between the model properties and the mapped column names</returns>
		internal static IDictionary<PropertyInfo, string> GetMappings(this DbContext dbContext, Type entityType)
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");
			}

			if (entityType == null)
			{
				throw new ArgumentNullException(nameof(entityType), $"{nameof(entityType)} {CommonResources.CannotBeNull}");
			}

			IDictionary<PropertyInfo, string> mappings = null;

#if NETFULL
			var objectContext = dbContext.GetObjectContext();

			var metadataWorkspace = objectContext.MetadataWorkspace;

			if (metadataWorkspace != null)
			{
				var storageEntityType = metadataWorkspace.GetItems(DataSpace.SSpace)
														 .Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType)
														 .OfType<EntityType>()
														 .SingleOrDefault(x => x.Name == entityType.Name || (entityType.BaseType != null && x.Name == entityType.BaseType.Name)); // It considers inheritance between mapped objects

				var objectEntityType = metadataWorkspace.GetItems(DataSpace.OSpace)
														.Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType)
														.OfType<EntityType>()
														.SingleOrDefault(x => x.Name == entityType.Name);

				if (storageEntityType != null && objectEntityType != null)
				{
					// Enum properties usually are ordered at the end of the properties/members collection, so index based approach doesn't work
					if (!objectEntityType.Properties.Any(x => x.IsEnumType))
					{
						// Tries to get the mappings by property indexes
						mappings = storageEntityType.Properties.Select((edmProperty, idx) => new
						{
							Property = entityType.GetProperty(objectEntityType.Members[idx].Name),
							edmProperty.Name

						}).ToDictionary(x => x.Property, x => x.Name);
					}
					else
					{
						// Tries to get the mappings by property names since there may be enum properties
						mappings = storageEntityType.Properties.Select(edmProperty =>
						{
							var edmMemberResult = objectEntityType.Members.SingleOrDefault(edmMember => edmMember.Name.Equals(edmProperty.Name, StringComparison.OrdinalIgnoreCase));

							var mapping = new
							{
								Property = edmMemberResult != null ? entityType.GetProperty(edmMemberResult.Name) : null,
								edmProperty.Name
							};

							return mapping;

						}).ToDictionary(x => x.Property, x => x.Name);
					}
				}
			}
#endif

#if NETSTANDARD20
			var entityTypeObject = dbContext?.Model?.FindEntityType(entityType);

			if (entityTypeObject == null)
			{
				throw new ArgumentNullException(nameof(entityTypeObject), $"{nameof(entityTypeObject)} {CommonResources.CannotBeNull}");
			}

			mappings = entityTypeObject.GetProperties().Where(property => !property.IsShadowProperty).ToDictionary(property => property.PropertyInfo, property => property.Relational().ColumnName);
#endif
			return mappings;
		}

		/// <summary>
		/// Tries to create a string array containing the entity keys
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entityType">Type of the entity</param>
		/// <returns>String array that contains the entity keys</returns>
		internal static IList<string> GetKeyNames(this DbContext dbContext, Type entityType)
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");
			}

			if (entityType == null)
			{
				throw new ArgumentNullException(nameof(entityType), $"{nameof(entityType)} {CommonResources.CannotBeNull}");
			}

#if NETFULL
			var objectContext = dbContext.GetObjectContext();

			var objectContextType = objectContext.GetType();

			var methodInfo = objectContextType.GetMethod("CreateObjectSet", Type.EmptyTypes);

			var genericMethodInfo = methodInfo.MakeGenericMethod(entityType);

			dynamic objectSet = genericMethodInfo.Invoke(objectContext, null);

			IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

			var keyNames = keyMembers.Select(keyMember => (string)keyMember.Name).ToList();

			return keyNames;
#endif

#if NETSTANDARD20
			var entityTypeObject = dbContext?.Model?.FindEntityType(entityType);

			if (entityTypeObject == null)
			{
				throw new ArgumentNullException(nameof(entityTypeObject), $"{nameof(entityTypeObject)} {CommonResources.CannotBeNull}");
			}

			var key = entityTypeObject.FindPrimaryKey();

			var keyNames = key.Properties.Where(property => !property.IsShadowProperty).Select(property => property.Name).ToList();

			return keyNames;
#endif
		}

		/// <summary>
		/// Tries to create a string array containing the db entity keys
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entityType">Type of the entity</param>
		/// <returns>String array that contains the db entity keys</returns>
		internal static IList<string> GetDbKeyNames(this DbContext dbContext, Type entityType)
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");
			}

			if (entityType == null)
			{
				throw new ArgumentNullException(nameof(entityType), $"{nameof(entityType)} {CommonResources.CannotBeNull}");
			}

			IList<string> dbKeyNames = null;

#if NETFULL
			var keyNames = dbContext.GetKeyNames(entityType);
			var mappings = dbContext.GetMappings(entityType);

			if (keyNames != null && mappings != null)
			{
				dbKeyNames = mappings.Where(mapping => keyNames.Contains(mapping.Key.Name)).Select(mapping => mapping.Value).ToList();
			}
#endif

#if NETSTANDARD20
			var entityTypeObject = dbContext?.Model?.FindEntityType(entityType);

			if (entityTypeObject == null)
			{
				throw new ArgumentNullException(nameof(entityTypeObject), $"{nameof(entityTypeObject)} {CommonResources.CannotBeNull}");
			}

			var key = entityTypeObject.FindPrimaryKey();

			dbKeyNames = key.Properties.Where(property => !property.IsShadowProperty).Select(property => property.Relational().ColumnName).ToList();
#endif
			return dbKeyNames;
		}

		/// <summary>
		/// Executes a bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="retrievePrimaryKeyValues">A flag that indicates if the primary key values should be retrieved after the bulk insert</param>
		/// <returns>List of objects filled with the primary key values or not</returns>
		public static IList<T> BulkInsert<T>(this DbContext dbContext, IList<T> entities, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null) where T : class => BulkInsertInternal(dbContext, entities, batchSize, options, retrievePrimaryKeyValues);

		/// <summary>
		/// Executes an async bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="retrievePrimaryKeyValues">A flag that indicates if the primary key values should be retrieved after the bulk insert</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <returns>List of objects filled with the primary key values or not</returns>
		public static Task<IList<T>> BulkInsertAsync<T>(this DbContext dbContext, IList<T> entities, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
		{
			if (cancellationToken == null || cancellationToken == default(CancellationToken))
			{
				cancellationToken = CancellationTokenFactory.Token();
			}

			var task = Task.Factory.StartNew(() => BulkInsert(dbContext, entities, batchSize, options, retrievePrimaryKeyValues), cancellationToken);

			return task;
		}

		/// <summary>
		/// Executes a batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		public static void BatchUpdate<T>(this DbContext dbContext, IList<T> entities, string commandText, int batchSize = DataConstants.BatchSize) where T : class => BatchUpdateInternal(dbContext, entities, commandText, batchSize);

		/// <summary>
		/// Executes an async batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param> 
		public static Task BatchUpdateAsync<T>(this DbContext dbContext, IList<T> entities, string commandText, int batchSize = DataConstants.BatchSize, CancellationToken cancellationToken = default(CancellationToken)) where T : class
		{
			if (cancellationToken == null || cancellationToken == default(CancellationToken))
			{
				cancellationToken = CancellationTokenFactory.Token();
			}

			var task = Task.Factory.StartNew(() => BatchUpdate(dbContext, entities, commandText, batchSize), cancellationToken);

			return task;
		}

		/// <summary>
		/// Executes a bulk insert in order to get a high performance level while inserting a lot of data (internal method)
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="retrievePrimaryKeyValues">A flag that indicates if the primary key values should be retrieved after the bulk insert</param>
		/// <returns>List of objects filled with the primary key values or not</returns>
		private static IList<T> BulkInsertInternal<T>(DbContext dbContext, IList<T> entities, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null) where T : class
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");
			}

			if (entities == null || !entities.Any())
			{
				throw new ArgumentNullException(nameof(entities), $"{nameof(entities)} {CommonResources.CannotBeNullOrEmpty}");
			}

			IList<string> primaryKeyNames = null;

			if (retrievePrimaryKeyValues.GetValueOrDefault())
			{
				using (IMetadataService metadataService = new MetadataService(dbContext))
				{
					primaryKeyNames = metadataService.GetDbKeyNames(entities.GetTypeFromEnumerable());
				}
			}

			var dataTable = entities.AsStronglyTypedDataTable(dbContext);

			using (ISqlService sqlService = new SqlService(dbContext))
			{
				dataTable = sqlService.BulkInsert(dataTable, batchSize, options, primaryKeyNames);
			}

			if (retrievePrimaryKeyValues.GetValueOrDefault())
			{
				return dataTable.ToList<T>();
			}

			return entities;
		}

		/// <summary>
		/// Executes a batch update in order to get a high performance level while updating a lot of data (internal method)
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		private static void BatchUpdateInternal<T>(DbContext dbContext, IList<T> entities, string commandText, int batchSize = DataConstants.BatchSize) where T : class
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");
			}

			if (entities == null || !entities.Any())
			{
				throw new ArgumentException($"{nameof(entities)} {CommonResources.CannotBeNullOrEmpty}", nameof(entities));
			}

			if (string.IsNullOrWhiteSpace(commandText))
			{
				throw new ArgumentException($"{nameof(commandText)} {CommonResources.CannotBeNullOrWhiteSpace}", nameof(commandText));
			}

			var dataTable = entities.AsStronglyTypedDataTable(dbContext);

			using (ISqlService sqlService = new SqlService(dbContext))
			{
				sqlService.BatchUpdate(dataTable, commandText, batchSize);
			}
		}

#if NETFULL
		/// <summary>
		/// Gets the ObjectContext from EF DbContext
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <returns>ObjectContext from EF DbContext</returns>
		private static ObjectContext GetObjectContext(this DbContext dbContext)
		{
			var objectContextAdapter = (dbContext as IObjectContextAdapter);

			if (objectContextAdapter == null)
			{
				throw new ArgumentNullException(nameof(objectContextAdapter), $"{nameof(objectContextAdapter)} {CommonResources.CannotBeNull}");
			}

			var objectContext = objectContextAdapter.ObjectContext;

			if (objectContext == null)
			{
				throw new ArgumentNullException(nameof(objectContext), $"{nameof(objectContext)} {CommonResources.CannotBeNull}");
			}

			return objectContext;
		}
#endif
	}
}
