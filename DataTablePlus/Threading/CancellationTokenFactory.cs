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

using System.Threading;

namespace DataTablePlus.Threading
{
	/// <summary>
	/// An internal class that helps managing CancellationTokens
	/// </summary>
	internal static class CancellationTokenFactory
	{
		/// <summary>
		/// Creates a new CancellationToken
		/// </summary>
		/// <returns>A new CancellationToken</returns>
		public static CancellationToken Token()
		{
			var cancellationTokenSource = new CancellationTokenSource();

			var cancellationToken = cancellationTokenSource.Token;

			return cancellationToken;
		}
	}
}
