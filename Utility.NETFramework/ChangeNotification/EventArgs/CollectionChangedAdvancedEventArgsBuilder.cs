// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       CollectionChangedAdvancedEventArgsBuilder.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-26 @ 12:08 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections;
using System.Collections.Specialized;

namespace JLR.Utility.NETFramework.ChangeNotification.EventArgs
{
	public static class CollectionChangedEventArgsBuilder
	{
		public static NotifyCollectionChangedEventArgs CreateResetAction()
		{
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
		}

		public static NotifyCollectionChangedEventArgs CreateSingleItemAddAction(object item)
		{
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
		}

		public static NotifyCollectionChangedEventArgs CreateMultipleItemAddAction(IList items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items);
		}

		public static NotifyCollectionChangedEventArgs CreateSingleItemInsertAction(object item, int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index);
		}

		public static NotifyCollectionChangedEventArgs CreateMultipleItemInsertAction(IList items, int index)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index);
		}

		public static NotifyCollectionChangedEventArgs CreateSingleItemRemoveAction(object item)
		{
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);
		}

		public static NotifyCollectionChangedEventArgs CreateMultipleItemRemoveAction(IList items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items);
		}

		public static NotifyCollectionChangedEventArgs CreateSingleItemRemoveAtAction(object item, int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index);
		}

		public static NotifyCollectionChangedEventArgs CreateMultipleItemRemoveAtAction(IList items, int index)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, index);
		}

		public static NotifyCollectionChangedEventArgs CreateSingleItemReplaceAction(object oldItem, object newItem)
		{
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem);
		}

		public static NotifyCollectionChangedEventArgs CreateMultipleItemReplaceAction(IList oldItems, IList newItems)
		{
			if (oldItems == null)
				throw new ArgumentNullException(nameof(oldItems));
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems);
		}

		public static NotifyCollectionChangedEventArgs CreateSingleItemReplaceAtAction(
			object oldItem,
			object newItem,
			int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index);
		}

		public static NotifyCollectionChangedEventArgs CreateMultipleItemReplaceAtAction(
			IList oldItems,
			IList newItems,
			int index)
		{
			if (oldItems == null)
				throw new ArgumentNullException(nameof(oldItems));
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, index);
		}

		public static NotifyCollectionChangedEventArgs CreateSingleItemMoveAction(object item, int oldIndex, int newIndex)
		{
			if (oldIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(oldIndex), "Index cannot be negative");
			if (newIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(newIndex), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
		}

		public static NotifyCollectionChangedEventArgs CreateMultipleItemMoveAction(IList items, int oldIndex, int newIndex)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (oldIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(oldIndex), "Index cannot be negative");
			if (newIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(newIndex), "Index cannot be negative");
			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex);
		}
	}
}