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

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTablePlus.DataAccessContracts.Services
{
	/// <summary>
	/// MetadataService interface
	/// </summary>
	public interface IMetadataService : IDisposable
	{
		/// <summary>
		/// Gets the table name from the mapped entity on EF
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity</typeparam>
		/// <returns>Table name or null</returns>
		string GetTableName<T>() where T : class;

		/// <summary>
		/// Gets the table name from the mapped entity on EF
		/// </summary>
		/// <param name="type">Type of the mapped entity</param>
		/// <returns>Table name or null</returns>
		string GetTableName(Type type);

		/// <summary>
		/// Gets a mapping between the model properties and the mapped column names
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity</typeparam>
		/// <returns>Mapping or null</returns>
		IDictionary<PropertyInfo, string> GetMappings<T>() where T : class;

		/// <summary>
		/// Gets a mapping between the model properties and the mapped column names
		/// </summary>
		/// <param name="type">Type of the mapped entity</param>
		/// <returns>Mapping or null</returns>
		IDictionary<PropertyInfo, string> GetMappings(Type type);

		/// <summary>
		/// Gets the database table schema
		/// </summary>
		/// <param name="tableName">The name of the database table</param>
		/// <returns>A datatable that represents a mirror of the database table schema</returns>
		DataTable GetTableSchema(string tableName);
	}
}
