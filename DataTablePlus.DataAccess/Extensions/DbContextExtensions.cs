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
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace DataTablePlus.DataAccess.Extensions
{
	/// <summary>
	/// Class that contains DbContext extensions
	/// </summary>
	internal static class DbContextExtensions
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
		public static string GetTableName(this DbContext dbContext, Type entityType)
		{
			ValidateParameters(dbContext, entityType);

			const string Schema = "Schema";
			const string Table = "Table";

			string tableName = null;

			var objectContext = dbContext.GetObjectContext();

			var metadataWorkspace = objectContext.MetadataWorkspace;

			if (metadataWorkspace != null)
			{
				var entitySetBase = metadataWorkspace.GetItemCollection(DataSpace.SSpace)
													 .GetItems<EntityContainer>()
													 .Single()
													 .BaseEntitySets.SingleOrDefault(x => x.Name == entityType.Name);

				if (entitySetBase != null)
				{
					var schema = entitySetBase.MetadataProperties[Schema].Value;

					var table = entitySetBase.MetadataProperties[Table].Value;

					tableName = string.Concat(Constants.LeftSquareBracket, schema, Constants.RigthSquareBracket, Constants.FullStop, Constants.LeftSquareBracket, table, Constants.RigthSquareBracket);
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
		public static IDictionary<PropertyInfo, string> GetMappings(this DbContext dbContext, Type entityType)
		{
			ValidateParameters(dbContext, entityType);

			var objectContext = dbContext.GetObjectContext();

			var metadataWorkspace = objectContext.MetadataWorkspace;

			if (metadataWorkspace != null)
			{
				var storageEntityType = metadataWorkspace.GetItems(DataSpace.SSpace)
														 .Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType)
														 .OfType<EntityType>()
														 .SingleOrDefault(x =>
															 x.Name == entityType.Name ||
															 (entityType.BaseType != null && x.Name == entityType.BaseType.Name)  // It considers inheritance between mapped objects
														 );

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
		public static IList<string> GetKeyNames(this DbContext dbContext, Type entityType)
		{
			ValidateParameters(dbContext, entityType);

			const string MethodName = "CreateObjectSet";

			var objectContext = dbContext.GetObjectContext();

			var objectContextType = objectContext.GetType();

			var methodInfo = objectContextType.GetMethod(MethodName, Type.EmptyTypes);

			var genericMethodInfo = methodInfo.MakeGenericMethod(entityType);

			dynamic objectSet = genericMethodInfo.Invoke(objectContext, null);

			IEnumerable<dynamic> keyMembers = objectSet.EntitySet.ElementType.KeyMembers;

			var keyNames = keyMembers.Select(keyMember => (string)keyMember.Name).ToList();

			return keyNames;
		}

		/// <summary>
		/// Generic method that validates the provided parameters to avoid any kind of problem during the execution
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entityType">Entity type</param>
		private static void ValidateParameters(DbContext dbContext, Type entityType)
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException(nameof(dbContext), $"{nameof(dbContext)} {CommonResources.CannotBeNull}");
			}

			if (entityType == null)
			{
				throw new ArgumentNullException(nameof(entityType), $"{nameof(entityType)} {CommonResources.CannotBeNull}");
			}
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
	}
}
