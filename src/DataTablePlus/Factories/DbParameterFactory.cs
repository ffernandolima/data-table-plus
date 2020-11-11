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

using System;
using System.Data;
using System.Data.Common;

namespace DataTablePlus.Factories
{
    /// <summary>
    /// Class DbParameterFactory.
    /// </summary>
    public class DbParameterFactory
    {
        /// <summary>
        /// The factory
        /// </summary>
        private static readonly Lazy<DbParameterFactory> Factory = new Lazy<DbParameterFactory>(() => new DbParameterFactory(), isThreadSafe: true);

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static DbParameterFactory Instance { get => Factory.Value; }

        /// <summary>
        /// Prevents a default instance of the <see cref="DbParameterFactory"/> class from being created.
        /// </summary>
        private DbParameterFactory()
        { }

        /// <summary>
        /// Creates the database parameter.
        /// </summary>
        /// <typeparam name="TParameter">The type of the T parameter.</typeparam>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterValue">The parameter value.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>DbParameter.</returns>
        /// <exception cref="ArgumentException">parameterName</exception>
        public DbParameter CreateDbParameter<TParameter>(string parameterName, object parameterValue, ParameterDirection direction = ParameterDirection.Input) where TParameter : DbParameter
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(nameof(parameterName));
            }

            var parameter = Activator.CreateInstance<TParameter>();
            {
                parameter.ParameterName = parameterName;
                parameter.Value = parameterValue ?? DBNull.Value;
                parameter.Direction = direction;
            }

            return parameter;
        }

        /// <summary>
        /// Creates the database parameter.
        /// </summary>
        /// <typeparam name="TParameter">The type of the T parameter.</typeparam>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="sourceColumn">The source column.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>DbParameter.</returns>
        /// <exception cref="ArgumentException">parameterName</exception>
        public DbParameter CreateDbParameter<TParameter>(string parameterName, string sourceColumn, ParameterDirection direction = ParameterDirection.Input) where TParameter : DbParameter
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException(nameof(parameterName));
            }

            var parameter = Activator.CreateInstance<TParameter>();
            {
                parameter.ParameterName = parameterName;
                parameter.SourceColumn = sourceColumn ?? string.Empty;
                parameter.Direction = direction;
            }

            return parameter;
        }
    }
}
