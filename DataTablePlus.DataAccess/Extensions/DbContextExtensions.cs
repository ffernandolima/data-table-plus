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

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
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
		/// <summary>
		/// Tries to get a table name from a mapped entity
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entityType">Type of the entity</param>
		/// <returns>Table name or null</returns>
		public static string GetTableName(this DbContext dbContext, Type entityType)
		{
			var objectContextAdapter = (dbContext as IObjectContextAdapter);
			if (objectContextAdapter == null)
			{
				throw new ArgumentNullException("objectContextAdapter");
			}

			var objectContext = objectContextAdapter.ObjectContext;
			var metadataWorkspace = objectContext.MetadataWorkspace;

			var entitySetBase = metadataWorkspace.GetItemCollection(DataSpace.SSpace)
												 .GetItems<EntityContainer>()
												 .Single()
												 .BaseEntitySets.SingleOrDefault(x => x.Name == entityType.Name);

			return entitySetBase != null ? string.Concat(entitySetBase.MetadataProperties["Schema"].Value, ".", entitySetBase.MetadataProperties["Table"].Value) : null;
		}

		/// <summary>
		/// Tries to build a dictionary that contains a mapping between the model properties and the mapped column names
		/// </summary>
		/// <param name="dbContext">EF DbContext</param>
		/// <param name="entityType">Type of the entity</param>
		/// <returns>Dictionary that contains a mapping between the model properties and the mapped column names</returns>
		public static IDictionary<PropertyInfo, string> GetMappings(this DbContext dbContext, Type entityType)
		{
			var objectContextAdapter = (dbContext as IObjectContextAdapter);
			if (objectContextAdapter == null)
			{
				throw new ArgumentNullException("objectContextAdapter");
			}

			var objectContext = objectContextAdapter.ObjectContext;
			var metadataWorkspace = objectContext.MetadataWorkspace;

			var storageEntityType = metadataWorkspace.GetItems(DataSpace.SSpace)
													 .Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType)
													 .OfType<EntityType>()
													 .SingleOrDefault(x =>
														 x.Name == entityType.Name ||
														 (entityType.BaseType != null && x.Name == entityType.BaseType.Name)  // Considera herança entre objetos mapeados
													 );

			var objectEntityType = metadataWorkspace.GetItems(DataSpace.OSpace)
													.Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType)
													.OfType<EntityType>()
													.SingleOrDefault(x => x.Name == entityType.Name);

			if (storageEntityType != null && objectEntityType != null)
			{
				return (storageEntityType.Properties.Select((edmProperty, idx) => new
				{
					Property = entityType.GetProperty(objectEntityType.Members[idx].Name),
					edmProperty.Name

				}).ToDictionary(x => x.Property, x => x.Name));
			}

			return null;
		}
	}
}
