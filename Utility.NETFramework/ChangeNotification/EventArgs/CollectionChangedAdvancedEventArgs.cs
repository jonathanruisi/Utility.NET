// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       CollectionChangedAdvancedEventArgs.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-26 @ 12:10 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace JLR.Utility.NETFramework.ChangeNotification.EventArgs
{
	public sealed class CollectionChangedAdvancedEventArgs<T> : NotifyCollectionChangedEventArgs
	{
		#region Properties
		public new NotifyCollectionChangedAction Action           { get; }
		public new IList<T>                      NewItems         { get; }
		public new IList<T>                      OldItems         { get; }
		public new int                           NewStartingIndex { get; }
		public new int                           OldStartingIndex { get; }
		#endregion

		#region Constructors
		private CollectionChangedAdvancedEventArgs(NotifyCollectionChangedAction action,
												   IList<T> oldItems,
												   IList<T> newItems,
												   int oldIndex,
												   int newIndex) : base(NotifyCollectionChangedAction.Reset)
		{
			Action           = action;
			OldStartingIndex = oldIndex;
			NewStartingIndex = newIndex;
			OldItems         = oldItems;
			NewItems         = newItems;
		}
		#endregion

		#region Static Methods
		public static CollectionChangedAdvancedEventArgs<T> CreateResetAction()
		{
			return new CollectionChangedAdvancedEventArgs<T>(NotifyCollectionChangedAction.Reset, null, null, -1, -1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateAddAction(T item)
		{
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Add,
				null,
				new List<T> { item },
				-1,
				-1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateAddAction(IList<T> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			return new CollectionChangedAdvancedEventArgs<T>(NotifyCollectionChangedAction.Add, null, items, -1, -1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateInsertAction(T item, int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Add,
				null,
				new List<T> { item },
				-1,
				index);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateInsertAction(IList<T> items, int index)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(NotifyCollectionChangedAction.Add, null, items, -1, index);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateRemoveAction(T item)
		{
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Remove,
				new List<T> { item },
				null,
				-1,
				-1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateRemoveAction(IList<T> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			return new CollectionChangedAdvancedEventArgs<T>(NotifyCollectionChangedAction.Remove, items, null, -1, -1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateRemoveAtAction(T item, int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Remove,
				new List<T> { item },
				null,
				index,
				-1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateRemoveAtAction(IList<T> items, int index)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(NotifyCollectionChangedAction.Remove, items, null, index, -1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateReplaceAction(T oldItem, T newItem)
		{
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Replace,
				new List<T> { oldItem },
				new List<T> { newItem },
				-1,
				-1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateReplaceAction(IList<T> oldItems, IList<T> newItems)
		{
			if (oldItems == null)
				throw new ArgumentNullException(nameof(oldItems));
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			return new CollectionChangedAdvancedEventArgs<T>(NotifyCollectionChangedAction.Replace, oldItems, newItems, -1, -1);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateReplaceAtAction(T oldItem, T newItem, int index)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Replace,
				new List<T> { oldItem },
				new List<T> { newItem },
				index,
				index);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateReplaceAtAction(
			IList<T> oldItems,
			IList<T> newItems,
			int index)
		{
			if (oldItems == null)
				throw new ArgumentNullException(nameof(oldItems));
			if (newItems == null)
				throw new ArgumentNullException(nameof(newItems));
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Replace,
				oldItems,
				newItems,
				index,
				index);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateMoveAction(T item, int oldIndex, int newIndex)
		{
			if (oldIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(oldIndex), "Index cannot be negative");
			if (newIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(newIndex), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Move,
				new List<T> { item },
				new List<T> { item },
				oldIndex,
				newIndex);
		}

		public static CollectionChangedAdvancedEventArgs<T> CreateMoveAction(IList<T> items, int oldIndex, int newIndex)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (oldIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(oldIndex), "Index cannot be negative");
			if (newIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(newIndex), "Index cannot be negative");
			return new CollectionChangedAdvancedEventArgs<T>(
				NotifyCollectionChangedAction.Move,
				items,
				items,
				oldIndex,
				newIndex);
		}
		#endregion
	}
}