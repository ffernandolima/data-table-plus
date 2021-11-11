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

using DataTablePlus.DataAccess;
using DataTablePlus.DataAccess.Enums;
using DataTablePlus.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD || NET60
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
    /// Class DbContextExtensions.
    /// </summary>
    public static class DbContextExtensions
    {
        #region EF6 DataSpace Enum Explanation

        // C-Space - This is where the metadata about our conceptual model is found. Here we will get access to all Edm objects and the tables in our generated model.
        // S-Space - This is where metadata about the database is found. Here we will get access to all Sql objects and the tables in our database
        // O-Space - This is where metadata about the CLR types that map to our conceptual model is found
        // CS-Space - This is where metadata about mapping is found
        // OC-Space - This is where EF holds the mapping between our conceptual model (C-Space) and the CLR objects (O-Space).

        #endregion EF6 DataSpace Enum Explanation

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="ArgumentNullException">
        /// dbContext
        /// or
        /// entityType
        /// or
        /// entityTypeObject
        /// </exception>
        internal static string GetTableName(this DbContext dbContext, Type entityType)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
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

                    var schema = schemaMetadataProperty.Value.ToString();

                    var table = tableMetadataProperty.Value.ToString();

                    if (!string.IsNullOrWhiteSpace(schema))
                    {
                        tableName = $"{schema}.{table}";
                    }
                    else
                    {
                        tableName = table;
                    }
                }
            }
#endif

#if NETSTANDARD || NET60
            var entityTypeObject = dbContext.Model?.FindEntityType(entityType);

            if (entityTypeObject == null)
            {
                throw new ArgumentNullException(nameof(entityTypeObject));
            }

            if (!string.IsNullOrWhiteSpace(entityTypeObject.GetSchema()))
            {
                tableName = $"{entityTypeObject.GetSchema()}.{entityTypeObject.GetTableName()}";
            }
            else
            {
                tableName = entityTypeObject.GetTableName();
            }
#endif
            return tableName;
        }

        /// <summary>
        /// Gets the mappings.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>IDictionary&lt;PropertyInfo, System.String&gt;.</returns>
        /// <exception cref="ArgumentNullException">
        /// dbContext
        /// or
        /// entityType
        /// or
        /// entityTypeObject
        /// </exception>
        internal static IDictionary<PropertyInfo, string> GetMappings(this DbContext dbContext, Type entityType)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
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

#if NETSTANDARD || NET60
            var entityTypeObject = dbContext.Model?.FindEntityType(entityType);

            if (entityTypeObject == null)
            {
                throw new ArgumentNullException(nameof(entityTypeObject));
            }

            mappings = entityTypeObject.GetProperties().Where(property => !property.IsShadowProperty()).ToDictionary(property => property.PropertyInfo, property => property.GetColumnName());
#endif
            return mappings;
        }

        /// <summary>
        /// Gets the key names.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>IList&lt;System.String&gt;.</returns>
        /// <exception cref="ArgumentNullException">
        /// dbContext
        /// or
        /// entityType
        /// or
        /// entityTypeObject
        /// </exception>
        internal static IList<string> GetKeyNames(this DbContext dbContext, Type entityType)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
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

#if NETSTANDARD || NET60
            var entityTypeObject = dbContext.Model?.FindEntityType(entityType);

            if (entityTypeObject == null)
            {
                throw new ArgumentNullException(nameof(entityTypeObject));
            }

            var key = entityTypeObject.FindPrimaryKey();

            var keyNames = key.Properties.Where(property => !property.IsShadowProperty()).Select(property => property.Name).ToList();

            return keyNames;
#endif
        }

        /// <summary>
        /// Gets the database key names.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>IList&lt;System.String&gt;.</returns>
        /// <exception cref="ArgumentNullException">
        /// dbContext
        /// or
        /// entityType
        /// or
        /// entityTypeObject
        /// </exception>
        internal static IList<string> GetDbKeyNames(this DbContext dbContext, Type entityType)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
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

#if NETSTANDARD || NET60
            var entityTypeObject = dbContext.Model?.FindEntityType(entityType);

            if (entityTypeObject == null)
            {
                throw new ArgumentNullException(nameof(entityTypeObject));
            }

            var key = entityTypeObject.FindPrimaryKey();

            dbKeyNames = key.Properties.Where(property => !property.IsShadowProperty()).Select(property => property.GetColumnName()).ToList();
#endif
            return dbKeyNames;
        }

        /// <summary>
        /// Executes the bulk insert.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="options">The options.</param>
        /// <param name="retrievePrimaryKeyValues">If set to <c>true</c>, it retrieves the primary key values.</param>
        /// <returns>IList&lt;T&gt;.</returns>
        public static IList<T> BulkInsert<T>(this DbContext dbContext, IList<T> entities, DbProvider? dbProvider = null, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null) where T : class
        {
            return BulkInsertInternal(dbContext, entities, dbProvider, batchSize, options, retrievePrimaryKeyValues);
        }

        /// <summary>
        /// Executes the bulk insert asynchronous.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="options">The options.</param>
        /// <param name="retrievePrimaryKeyValues">If set to <c>true</c>, it retrieves the primary key values.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task&lt;IList&lt;T&gt;&gt;.</returns>
        public static Task<IList<T>> BulkInsertAsync<T>(this DbContext dbContext, IList<T> entities, DbProvider? dbProvider = null, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null, CancellationToken cancellationToken = default) where T : class
        {
            return Task.Run(() => BulkInsert(dbContext, entities, dbProvider, batchSize, options, retrievePrimaryKeyValues), cancellationToken);
        }

        /// <summary>
        /// Executes the batch update.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="batchSize">Size of the batch.</param>
        public static void BatchUpdate<T>(this DbContext dbContext, IList<T> entities, string commandText, DbProvider? dbProvider = null, int batchSize = DataConstants.BatchSize) where T : class
        {
            BatchUpdateInternal(dbContext, entities, commandText, dbProvider, batchSize);
        }

        /// <summary>
        /// Executes the batch update asynchronous.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public static Task BatchUpdateAsync<T>(this DbContext dbContext, IList<T> entities, string commandText, DbProvider? dbProvider = null, int batchSize = DataConstants.BatchSize, CancellationToken cancellationToken = default) where T : class
        {
            return Task.Run(() => BatchUpdate(dbContext, entities, commandText, dbProvider, batchSize), cancellationToken);
        }

        /// <summary>
        /// Executes the bulk insert internally.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <param name="options">The options.</param>
        /// <param name="retrievePrimaryKeyValues">If set to <c>true</c>, it retrieves the primary key values.</param>
        /// <returns>IList&lt;T&gt;.</returns>
        /// <exception cref="ArgumentNullException">
        /// dbContext
        /// or
        /// entities
        /// </exception>
        private static IList<T> BulkInsertInternal<T>(DbContext dbContext, IList<T> entities, DbProvider? dbProvider = null, int batchSize = DataConstants.BatchSize, BulkCopyOptions? options = null, bool? retrievePrimaryKeyValues = null) where T : class
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities));
            }

            IList<string> primaryKeyNames = null;

            if (retrievePrimaryKeyValues.GetValueOrDefault())
            {
                var metadataService = MetadataServiceFactory.Instance.GetMetadataService(dbProvider, dbContext);

                try
                {
                    primaryKeyNames = metadataService?.GetDbKeyNames(entities.GetTypeFromEnumerable());
                }
                finally
                {
                    metadataService?.Dispose();
                }
            }

            var dataTable = entities.AsStronglyTypedDataTable(dbProvider, dbContext);

            var sqlService = SqlServiceFactory.Instance.GetSqlService(dbProvider, dbContext);

            try
            {
                dataTable = sqlService?.BulkInsert(dataTable, batchSize, options, primaryKeyNames);
            }
            finally
            {
                sqlService?.Dispose();
            }

            if (retrievePrimaryKeyValues.GetValueOrDefault())
            {
                return dataTable.ToList<T>();
            }

            return entities;
        }

        /// <summary>
        /// Executes the batch update internally.
        /// </summary>
        /// <typeparam name="T">The type of the T parameter.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entities">The entities.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="dbProvider">The database provider.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <exception cref="ArgumentNullException">dbContext</exception>
        /// <exception cref="ArgumentException">
        /// entities
        /// or
        /// commandText
        /// </exception>
        private static void BatchUpdateInternal<T>(DbContext dbContext, IList<T> entities, string commandText, DbProvider? dbProvider = null, int batchSize = DataConstants.BatchSize) where T : class
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            if (entities == null || !entities.Any())
            {
                throw new ArgumentException(nameof(entities));
            }

            if (string.IsNullOrWhiteSpace(commandText))
            {
                throw new ArgumentException(nameof(commandText));
            }

            var dataTable = entities.AsStronglyTypedDataTable(dbProvider, dbContext);

            var sqlService = SqlServiceFactory.Instance.GetSqlService(dbProvider, dbContext);

            try
            {
                sqlService?.BatchUpdate(dataTable, commandText, batchSize);
            }
            finally
            {
                sqlService?.Dispose();
            }
        }

#if NETFULL
        /// <summary>
        /// Gets the object context.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <returns>ObjectContext.</returns>
        /// <exception cref="ArgumentNullException">
        /// objectContextAdapter
        /// or
        /// objectContext
        /// </exception>
        private static ObjectContext GetObjectContext(this DbContext dbContext)
        {
            var objectContextAdapter = (dbContext as IObjectContextAdapter);

            if (objectContextAdapter == null)
            {
                throw new ArgumentNullException(nameof(objectContextAdapter));
            }

            var objectContext = objectContextAdapter.ObjectContext;

            if (objectContext == null)
            {
                throw new ArgumentNullException(nameof(objectContext));
            }

            return objectContext;
        }
#endif
    }
}
