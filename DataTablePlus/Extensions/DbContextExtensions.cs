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
using DataTablePlus.DataAccess.Services;
using DataTablePlus.DataAccessContracts;
using DataTablePlus.DataAccessContracts.Services;
using DataTablePlus.Threading;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

					tableName = $"[{schemaMetadataProperty.Value}].[{tableMetadataProperty.Value}]";
				}
			}

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
					var mappings = (storageEntityType.Properties.Select((edmProperty, idx) => new
					{
						Property = entityType.GetProperty(objectEntityType.Members[idx].Name),
						edmProperty.Name

					}).ToDictionary(x => x.Property, x => x.Name));

					return mappings;
				}
			}

			return null;
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

			var objectContext = dbContext.GetObjectContext();

			var objectContextType = objectContext.GetType();

			var methodInfo = objectContextType.GetMethod("CreateObjectSet", Type.EmptyTypes);

			var genericMethodInfo = methodInfo.MakeGenericMethod(entityType);

			dynamic objectSet = genericMethodInfo.Invoke(objectContext, null);

			IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

			var keyNames = keyMembers.Select(keyMember => (string)keyMember.Name).ToList();

			return keyNames;
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
		/// <returns>List of objects filled with the primary key values or not</returns>
		public static Task<IList<T>> BulkInsertAsync<T>(this DbContext dbContext, IList<T> entities, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null) where T : class => BulkInsertAsync(dbContext, entities, CancellationTokenFactory.Token(), batchSize, options, retrievePrimaryKeyValues);

		/// <summary>
		/// Executes an async bulk insert in order to get a high performance level while inserting a lot of data
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		/// <param name="options">Bulk insert options</param>
		/// <param name="retrievePrimaryKeyValues">A flag that indicates if the primary key values should be retrieved after the bulk insert</param>
		/// <returns>List of objects filled with the primary key values or not</returns>
		public static Task<IList<T>> BulkInsertAsync<T>(this DbContext dbContext, IList<T> entities, CancellationToken cancellationToken, int batchSize = DataConstants.BatchSize, SqlBulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null) where T : class
		{
			if (cancellationToken == null)
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
		public static Task BatchUpdateAsync<T>(this DbContext dbContext, IList<T> entities, string commandText, int batchSize = DataConstants.BatchSize) where T : class => BatchUpdateAsync(dbContext, entities, commandText, CancellationTokenFactory.Token(), batchSize);

		/// <summary>
		/// Executes an async batch update in order to get a high performance level while updating a lot of data
		/// </summary>
		/// <typeparam name="T">Type of the objects</typeparam>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entities">List of objects which contain the values to insert into the database</param>
		/// <param name="commandText">The sql command text that will be used to update the data</param>
		/// <param name="cancellationToken">A token for stopping the task if needed</param>
		/// <param name="batchSize">The batch number that will be considered while inserting</param>
		public static Task BatchUpdateAsync<T>(this DbContext dbContext, IList<T> entities, string commandText, CancellationToken cancellationToken, int batchSize = DataConstants.BatchSize) where T : class
		{
			if (cancellationToken == null)
			{
				cancellationToken = CancellationTokenFactory.Token();
			}

			var task = Task.Factory.StartNew(() => BatchUpdate(dbContext, entities, commandText, batchSize), cancellationToken);

			return task;
		}

		/// <summary>
		/// Gets ObjectContext from EF DbContext
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
	}
}
