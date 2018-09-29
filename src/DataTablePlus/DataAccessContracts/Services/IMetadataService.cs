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

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace DataTablePlus.DataAccessContracts.Services
{
	/// <summary>
	/// MetadataService interface
	/// </summary>
	public interface IMetadataService : IServiceBase, IDisposable
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
		/// <typeparam name="T">Type of the mapped entity on EF</typeparam>
		/// <returns>Mapping or null</returns>
		IDictionary<PropertyInfo, string> GetMappings<T>() where T : class;

		/// <summary>
		/// Gets a mapping between the model properties and the mapped column names
		/// </summary>
		/// <param name="type">Type of the mapped entity on EF</param>
		/// <returns>Mapping or null</returns>
		IDictionary<PropertyInfo, string> GetMappings(Type type);

		/// <summary>
		/// Gets the entity keys from the mapped entity on EF
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity on EF</typeparam>
		/// <returns>A list that contains the entity keys</returns>
		IList<string> GetKeyNames<T>() where T : class;

		/// <summary>
		/// Gets the entity keys from the mapped entity on EF
		/// </summary>
		/// <param name="type">Type of the mapped entity on EF</param>
		/// <returns>A list that contains the entity keys</returns>
		IList<string> GetKeyNames(Type type);

		/// <summary>
		/// Gets the database keys based on the EF mappings
		/// </summary>
		/// <typeparam name="T">Type of the mapped entity on EF</typeparam>
		/// <returns>A list that contains the database keys</returns>
		IList<string> GetDbKeyNames<T>() where T : class;

		/// <summary>
		/// Gets the database keys based on the EF mappings
		/// </summary>
		/// <param name="type">Type of the mapped entity on EF</param>
		/// <returns>A list that contains the database keys</returns>
		IList<string> GetDbKeyNames(Type type);

		/// <summary>
		/// Gets the database table schema
		/// </summary>
		/// <param name="tableName">The name of the database table</param>
		/// <returns>A datatable that represents a mirror of the database table schema</returns>
		DataTable GetTableSchema(string tableName);
	}
}
