// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       CollectionHelper.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:52 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JLR.Utility.NETFramework.Collections
{
	public static class CollectionHelper
	{
		/// <summary>
		/// Distributes the contents of two source collections into a single collection containing pairs of the source items.
		/// A tuple is created for every possible pair of items between the collections.
		/// The first item in any tuple will always be a member of collection 1,
		/// as well as the second item in any tuple will always be a member of collection 2.
		/// </summary>
		/// <typeparam name="T1">The type of items in collection #1</typeparam>
		/// <typeparam name="T2">The type of items in collection #2</typeparam>
		/// <param name="collection1">Collection #1</param>
		/// <param name="collection2">Collection #2</param>
		/// <param name="maxCount">
		/// Limits the size of the returned collection.
		/// The default value of zero allows for a collection of unlimited size.
		/// </param>
		/// <returns>ICollection&lt;Tuple&lt;T1, T2&gt;&gt;.</returns>
		public static ICollection<Tuple<T1, T2>> DistributeCollections<T1, T2>(
			ICollection<T1> collection1,
			ICollection<T2> collection2,
			int maxCount = 0)
		{
			var result = new Collection<Tuple<T1, T2>>();
			foreach (var item1 in collection1)
			{
				foreach (var item2 in collection2)
				{
					result.Add(new Tuple<T1, T2>(item1, item2));
					if (maxCount > 0 && result.Count == maxCount)
						return result;
				}
			}

			return result;
		}

		/// <summary>
		/// Merges the contents of a set of lists into one list.
		/// Each item in the merged list is a list of all source items sharing that index.
		/// For example, if three source lists are provided,
		/// index zero of the merged list is a list of the first item in each source list.
		/// Source lists of different lengths are handled appropriately.
		/// </summary>
		/// <param name="lists">An array of lists to merge</param>
		/// <returns>IList&lt;IList&lt;T&gt;&gt;.</returns>
		public static IList<IList<T>> ListMerge<T>(params IList<T>[] lists)
		{
			var result = new List<IList<T>>();

			var maxCount = lists.Select(t => t.Count).Max();
			for (var i = 0; i < maxCount; i++)
			{
				result.Add(new List<T>());
				foreach (var list in lists)
				{
					if (i < list.Count)
						result[i].Add(list[i]);
				}
			}

			return result;
		}
	}
}