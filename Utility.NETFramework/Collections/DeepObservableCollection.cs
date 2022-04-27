// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       DeepObservableCollection.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-25 @ 11:53 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using JLR.Utility.NETFramework.ChangeNotification;
using JLR.Utility.NETFramework.ChangeNotification.EventArgs;
using JLR.Utility.NETFramework.Debug;
using JLR.Utility.NETFramework.Math;

namespace JLR.Utility.NETFramework.Collections
{
	/// <summary>
	/// Provides property and collection change notification for nested collections
	/// </summary>
	/// <typeparam name="T">The type of object in the collection</typeparam>
	public class DeepObservableCollection<T>
		: DeepPropertyChangeNotifier, INotifyCollectionChanged, IAdvancedList<T>, IList
	{
		#region Constants
		private const int    DefaultCapacity = 4;
		private const int    MaximumCapacity = 0x7FEFFFFF;
		private const string CountString     = "Count";
		private const string IndexerString   = "Item[]";
		#endregion

		#region Fields
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		private T[]                                      _items;
		private int                                      _count;
		private int                                      _version;
		private object                                   _syncRoot;

		private bool _suspendCollectionChangeNotification,
														_suspendChildCollectionChangeNotification,
														_notifyResetOnChildPropertyChange, _limitExcessFreeSpace;

		private readonly SimpleMonitor          _monitor;
		private readonly SynchronizationContext _syncContext;
		#endregion

		#region Properties
		public bool IsReadOnly => false;
		public int  Count      => _count;
		public int  Capacity   { get { return _items.Length; } set { SetProperty(value, () => _items.Length, SetCapacity); } }

		public bool LimitExcessFreeSpace
		{
			get { return _limitExcessFreeSpace; }
			set { SetProperty(ref _limitExcessFreeSpace, value); }
		}

		public bool SuspendCollectionChangeNotification
		{
			get { return _suspendCollectionChangeNotification; }
			set { SetProperty(ref _suspendCollectionChangeNotification, value); }
		}

		public bool SuspendChildCollectionChangeNotification
		{
			get { return _suspendChildCollectionChangeNotification; }
			set { SetProperty(ref _suspendChildCollectionChangeNotification, value); }
		}

		public bool NotifyResetOnChildPropertyChange
		{
			get { return _notifyResetOnChildPropertyChange; }
			set { SetProperty(ref _notifyResetOnChildPropertyChange, value); }
		}

		public T this[int index]
		{
			get
			{
				VerifyIndex(index);
				return _items[index];
			}
			set
			{
				VerifyIndex(index);
				ReplaceItem(index, value);
			}
		}

		public IEnumerable<T> this[int index, int? count]
		{
			get { return GetRange(index, count ?? _count - index); }
			set { SetRange(index, count == null ? value : value.Take((int)count)); }
		}
		#endregion

		#region Constructors
		public DeepObservableCollection() : this(null, DefaultCapacity) { }

		public DeepObservableCollection(int initialCapacity) : this(null, initialCapacity) { }

		public DeepObservableCollection(IEnumerable<T> items, SynchronizationContext syncContext = null) : this(
			syncContext,
			items.Count())
		{
			AddRange(items);
		}

		private DeepObservableCollection(SynchronizationContext syncContext, int initialCapacity)
		{
			if (initialCapacity < 0)
				throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity cannot be less than zero");

			_items                                    = new T[initialCapacity];
			_version                                  = 0;
			_count                                    = 0;
			_syncContext                              = syncContext ?? SynchronizationContext.Current;
			_monitor                                  = new SimpleMonitor();
			_suspendCollectionChangeNotification      = false;
			_suspendChildCollectionChangeNotification = false;
			_notifyResetOnChildPropertyChange         = true;
			_limitExcessFreeSpace                     = true;
		}
		#endregion

		#region Public Methods (IAdvancedList<T>)
		public void AddRange(IEnumerable<T> items)
		{
			InsertRange(_count, items);
		}

		public void InsertRange(int index, IEnumerable<T> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			var newItems = new List<T>(items);
			VerifyInsertionRange(index, newItems.Count);
			InsertItems(index, newItems);
		}

		public void Move(int fromIndex, int toIndex)
		{
			VerifyIndex(fromIndex);
			VerifyIndex(toIndex);
			MoveItem(fromIndex, toIndex);
		}

		public void MoveRange(int fromIndex, int toIndex, int count)
		{
			VerifyRange(fromIndex, count);
			VerifyRange(toIndex,   count);
			MoveItems(fromIndex, toIndex, count);
		}

		public void RemoveRange(int index, int count)
		{
			VerifyRange(index, count);
			RemoveItems(index, count);
		}

		public int RemoveAny(Predicate<T> criteria)
		{
			if (criteria == null)
				throw new ArgumentNullException(nameof(criteria));
			var indexList = new List<int>();
			for (var i = 0; i < _count; i++)
			{
				if (criteria(_items[i]))
					indexList.Add(i);
			}

			indexList.Sort();
			for (var i = 0; i < indexList.Count; i++)
			{
				RemoveAt(indexList[i] - i);
			}

			return indexList.Count;
		}

		public int IndexOf(Predicate<T> criteria)
		{
			if (criteria == null)
				throw new ArgumentNullException(nameof(criteria));
			for (var i = 0; i < _count; i++)
			{
				if (criteria(_items[i]))
					return i;
			}

			return -1;
		}

		public int IndexOfLast(Predicate<T> criteria)
		{
			if (criteria == null)
				throw new ArgumentNullException(nameof(criteria));
			for (var i = _count; i >= 0; i--)
			{
				if (criteria(_items[i]))
					return i;
			}

			return -1;
		}

		public int IndexOfLast(T item)
		{
			return IndexOf(item, 0, _count);
		}

		public int IndexOfLast(T item, int index, int count)
		{
			if (count == 0)
				return -1;
			VerifyIndex(index);
			VerifyRange(index, count);
			if (ReferenceEquals(item, null))
			{
				for (var i = _count - 1; i >= 0; i--)
				{
					if (_items[i].Equals(null))
						return i;
				}
			}
			else
			{
				for (var i = _count - 1; i >= 0; i--)
				{
					if (EqualityComparer<T>.Default.Equals(_items[i], item))
						return i;
				}
			}

			return -1;
		}
		#endregion

		#region Interface Implementation (IList<T>)
		public void Insert(int index, T item)
		{
			VerifyInsertionIndex(index);
			InsertItem(index, item);
		}

		public void RemoveAt(int index)
		{
			VerifyIndex(index);
			RemoveItem(index);
		}

		public int IndexOf(T item)
		{
			return IndexOf(item, 0, _count);
		}

		public int IndexOf(T item, int index, int count)
		{
			if (count == 0)
				return -1;
			VerifyIndex(index);
			VerifyRange(index, count);
			if (ReferenceEquals(item, null))
			{
				for (var i = 0; i < _count; i++)
				{
					if (_items[i].Equals(null))
						return i;
				}
			}
			else
			{
				for (var i = 0; i < _count; i++)
				{
					if (EqualityComparer<T>.Default.Equals(_items[i], item))
						return i;
				}
			}

			return -1;
		}
		#endregion

		#region Interface Implementation (ICollection<T>)
		public void Add(T item)
		{
			InsertItem(_count, item);
		}

		public bool Remove(T item)
		{
			var index = IndexOf(item);
			if (index < 0)
				return false;
			RemoveItem(index);
			return true;
		}

		public void Clear()
		{
			ClearItems();
		}

		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			Array.Copy(_items, 0, array, arrayIndex, _count);
		}
		#endregion

		#region Interface Implementation (IEnumerable<T>, IEnumerable)
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}
		#endregion

		#region Legacy Interface Implementation (IList)
		bool IList.IsFixedSize => false;

		object IList.this[int index]
		{
			get { return this[index]; }
			set
			{
				try
				{
					this[index] = (T)value;
				}
				catch (InvalidCastException)
				{
					throw new ArgumentException($"Value must be of type {typeof(T)}", nameof(value));
				}
			}
		}

		int IList.Add(object item)
		{
			try
			{
				Add((T)item);
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException($"Value must be of type {typeof(T)}", nameof(item));
			}

			return _count - 1;
		}

		void IList.Insert(int index, object item)
		{
			try
			{
				Insert(index, (T)item);
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException($"Value must be of type {typeof(T)}", nameof(item));
			}
		}

		void IList.Remove(object item)
		{
			if (IsCompatibleObject(item))
				Remove((T)item);
		}

		bool IList.Contains(object item)
		{
			return IsCompatibleObject(item) && Contains((T)item);
		}

		int IList.IndexOf(object item)
		{
			if (IsCompatibleObject(item))
				return IndexOf((T)item);
			return -1;
		}
		#endregion

		#region Legacy Interface Implementation (ICollection)
		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array != null && array.Rank != 1)
				throw new ArgumentException("Multi-dimensional array are not supported", nameof(array));

			try
			{
				Array.Copy(_items, 0, array, arrayIndex, _count);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Invalid array type", nameof(array));
			}
		}
		#endregion

		#region Methods (Property/Collection Change Notification)
		protected virtual void OnItemAdded(T item)
		{
			var observableCollection = item as INotifyCollectionChanged;
			if (observableCollection != null)
				observableCollection.CollectionChanged += OnChildCollectionChanged;
			AttachChangeEventHandlers(item);
		}

		protected virtual void OnItemRemoved(T item)
		{
			var observableCollection = item as INotifyCollectionChanged;
			if (observableCollection != null)
				observableCollection.CollectionChanged -= OnChildCollectionChanged;
			DetachChangeEventHandlers(item);
		}

		protected void NotifyCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			// Gives derived classes the chance to modify added and removed items before the event is raised
			if (e.OldItems != null)
			{
				foreach (var item in e.OldItems.OfType<T>())
				{
					OnItemRemoved(item);
				}
			}

			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems.OfType<T>())
				{
					OnItemAdded(item);
				}
			}

			OnCollectionChanged(this, e);
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var handler = CollectionChanged;
			if (handler == null || SuspendCollectionChangeNotification)
			{
				//if (SuspendCollectionChangeNotification)
				//	System.Diagnostics.Debug.Print("Collection Notification: DISABLED");
				return;
			}

			//CollectionChangeDebugInfo(sender, e);
			using (BlockReentrancy())
				handler(sender, e);
		}

		private void OnChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (SuspendChildCollectionChangeNotification)
			{
				//System.Diagnostics.Debug.Print("Child Collection Notification: DISABLED");
				return;
			}

			OnCollectionChanged(sender, e);
		}

		protected override void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnChildPropertyChanged(sender, e);
			if (NotifyResetOnChildPropertyChange && e.PropertyName != CountString && e.PropertyName != IndexerString &&
				e.PropertyName != "Capacity")
			{
				//System.Diagnostics.Debug.Print("Child Property Change: NOTIFIFYING RESET");
				NotifyCollectionChanged(CollectionChangedEventArgsBuilder.CreateResetAction());
			}

			//if (!NotifyResetOnChildPropertyChange)
			//{
			//	System.Diagnostics.Debug.Print("Child Property Change: RESET DISABLED");
			//}
		}
		#endregion

		#region Private Methods (Collection Manipulation)
		private IEnumerable<T> GetRange(int index, int count)
		{
			VerifyRange(index, count);
			var result = new T[count];
			for (var i = 0; i < count; i++)
			{
				result[i] = _items[index + i];
			}

			return result;
		}

		private void SetRange(int index, IEnumerable<T> items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			var newItems = new List<T>(items);
			VerifyRange(index, newItems.Count);
			ReplaceItems(index, newItems);
		}

		private void ReplaceItem(int index, T item)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						ReplaceItemImplementation(index, item);
					},
					null);
			}
			else
			{
				ReplaceItemImplementation(index, item);
			}
		}

		private void ReplaceItemImplementation(int index, T item)
		{
			CheckReentrancy();

			//System.Diagnostics.Debug.IndentLevel++;
			//System.Diagnostics.Debug.Print("BEGIN REPLACE (Single Item)");
			var oldItem = _items[index];
			_items[index] = item;
			_version++;
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(CollectionChangedEventArgsBuilder.CreateSingleItemReplaceAtAction(oldItem, item, index));

			//System.Diagnostics.Debug.Print("END REPLACE (Single Item)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void ReplaceItems(int index, IList<T> items)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						ReplaceItemsImplementation(index, items);
					},
					null);
			}
			else
			{
				ReplaceItemsImplementation(index, items);
			}
		}

		private void ReplaceItemsImplementation(int index, IList<T> items)
		{
			CheckReentrancy();

			//System.Diagnostics.Debug.IndentLevel++;
			//System.Diagnostics.Debug.Print("BEGIN REPLACE (Multiple Items)");
			var oldItems = new List<T>();
			for (var i = 0; i < items.Count; i++)
			{
				oldItems.Add(_items[index + i]);
				_items[index + i] = items[i];
			}

			_version++;
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(
				CollectionChangedEventArgsBuilder.CreateMultipleItemReplaceAtAction(oldItems, items.ToArray(), index));

			//System.Diagnostics.Debug.Print("END REPLACE (Multiple Items)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void InsertItem(int index, T item, bool isPartOfMove = false)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						InsertItemImplementation(index, item, isPartOfMove);
					},
					null);
			}
			else
			{
				InsertItemImplementation(index, item, isPartOfMove);
			}
		}

		private void InsertItemImplementation(int index, T item, bool isPartOfMove)
		{
			if (!isPartOfMove)
			{
				CheckReentrancy();

				//System.Diagnostics.Debug.IndentLevel++;
				//System.Diagnostics.Debug.Print("BEGIN INSERT (Single Item)");
			}

			MakeRoom(1);
			if (index < _count)
				Array.Copy(_items, index, _items, index + 1, _count - index);
			_items[index] = item;
			_count++;
			if (isPartOfMove) return;
			_version++;
			NotifyPropertyChanged(propertyName: CountString);
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(CollectionChangedEventArgsBuilder.CreateSingleItemInsertAction(item, index));

			//System.Diagnostics.Debug.Print("END INSERT (Single Item)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void InsertItems(int index, IList<T> items, bool isPartOfMove = false)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						InsertItemsImplementation(index, items, isPartOfMove);
					},
					null);
			}
			else
			{
				InsertItemsImplementation(index, items, isPartOfMove);
			}
		}

		private void InsertItemsImplementation(int index, IList<T> items, bool isPartOfMove)
		{
			if (items.Count == 0) return;
			if (!isPartOfMove)
			{
				CheckReentrancy();

				//System.Diagnostics.Debug.IndentLevel++;
				//System.Diagnostics.Debug.Print("BEGIN INSERT (Multiple Items)");
			}

			MakeRoom(items.Count);
			if (index < _count)
			{
				Array.Copy(_items, index, _items, index + items.Count, _count - index);
			}

			if (Equals(items, this))
			{
				Array.Copy(_items, 0,                   _items, index,     index);
				Array.Copy(_items, index + items.Count, _items, index * 2, _count - index);
			}
			else
			{
				Array.Copy(items.ToArray(), 0, _items, index, items.Count);
			}

			_count += items.Count;
			if (isPartOfMove) return;
			_version++;
			NotifyPropertyChanged(propertyName: CountString);
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(CollectionChangedEventArgsBuilder.CreateMultipleItemInsertAction(items.ToArray(), index));

			//System.Diagnostics.Debug.Print("END INSERT (Multiple Items)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void RemoveItem(int index, bool isPartOfMove = false)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						RemoveItemImplementation(index, isPartOfMove);
					},
					null);
			}
			else
			{
				RemoveItemImplementation(index, isPartOfMove);
			}
		}

		private void RemoveItemImplementation(int index, bool isPartOfMove)
		{
			if (!isPartOfMove)
			{
				CheckReentrancy();

				//System.Diagnostics.Debug.IndentLevel++;
				//System.Diagnostics.Debug.Print("BEGIN REMOVE (Single Item)");
			}

			var oldItem = _items[index];
			_count--;
			if (index < _count)
			{
				Array.Copy(_items, index + 1, _items, index, _count - index);
			}

			_items[_count] = default(T);
			if (isPartOfMove) return;
			_version++;
			RecoverFreeSpace();
			NotifyPropertyChanged(propertyName: CountString);
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(CollectionChangedEventArgsBuilder.CreateSingleItemRemoveAtAction(oldItem, index));

			//System.Diagnostics.Debug.Print("END REMOVE (Single Item)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void RemoveItems(int index, int count, bool isPartOfMove = false)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						RemoveItemsImplementation(index, count, isPartOfMove);
					},
					null);
			}
			else
			{
				RemoveItemsImplementation(index, count, isPartOfMove);
			}
		}

		private void RemoveItemsImplementation(int index, int count, bool isPartOfMove)
		{
			var oldItems = new List<T>();
			if (!isPartOfMove)
			{
				CheckReentrancy();

				//System.Diagnostics.Debug.IndentLevel++;
				//System.Diagnostics.Debug.Print("BEGIN REMOVE (Multiple Items)");
				for (var i = 0; i < count; i++)
				{
					oldItems.Add(_items[index + i]);
				}
			}

			_count -= count;
			if (index < _count)
			{
				Array.Copy(_items, index + count, _items, index, _count - index);
			}

			Array.Clear(_items, _count, count);
			if (isPartOfMove) return;
			_version++;
			RecoverFreeSpace();
			NotifyPropertyChanged(propertyName: CountString);
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(CollectionChangedEventArgsBuilder.CreateMultipleItemRemoveAtAction(oldItems, index));

			//System.Diagnostics.Debug.Print("END REMOVE (Multiple Items)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void MoveItem(int fromIndex, int toIndex)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						MoveItemImplementation(fromIndex, toIndex);
					},
					null);
			}
			else
			{
				MoveItemImplementation(fromIndex, toIndex);
			}
		}

		private void MoveItemImplementation(int fromIndex, int toIndex)
		{
			CheckReentrancy();

			//System.Diagnostics.Debug.IndentLevel++;
			//System.Diagnostics.Debug.Print("BEGIN MOVE (Single Item)");
			var itemToMove = _items[fromIndex];
			RemoveItemImplementation(fromIndex, true);
			InsertItemImplementation(toIndex, itemToMove, true);
			_version++;
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(
				CollectionChangedEventArgsBuilder.CreateSingleItemMoveAction(itemToMove, fromIndex, toIndex));

			//System.Diagnostics.Debug.Print("END MOVE (Single Item)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void MoveItems(int fromIndex, int toIndex, int count)
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						MoveItemsImplementation(fromIndex, toIndex, count);
					},
					null);
			}
			else
			{
				MoveItemsImplementation(fromIndex, toIndex, count);
			}
		}

		private void MoveItemsImplementation(int fromIndex, int toIndex, int count)
		{
			CheckReentrancy();

			//System.Diagnostics.Debug.IndentLevel++;
			//System.Diagnostics.Debug.Print("BEGIN MOVE (Multiple Items)");
			var itemsToMove = new T[count];
			for (var i = 0; i < count; i++)
			{
				itemsToMove[i] = _items[fromIndex + i];
			}

			RemoveItemsImplementation(fromIndex, count, true);
			InsertItemsImplementation(toIndex, itemsToMove, true);
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(
				CollectionChangedEventArgsBuilder.CreateMultipleItemMoveAction(itemsToMove, fromIndex, toIndex));

			//System.Diagnostics.Debug.Print("END MOVE (Multiple Items)");
			//System.Diagnostics.Debug.IndentLevel--;
		}

		private void ClearItems()
		{
			if (_syncContext != null)
			{
				_syncContext.Send(
					delegate
					{
						ClearItemsImplementation();
					},
					null);
			}
			else
			{
				ClearItemsImplementation();
			}
		}

		private void ClearItemsImplementation()
		{
			CheckReentrancy();

			//System.Diagnostics.Debug.IndentLevel++;
			//System.Diagnostics.Debug.Print("BEGIN CLEAR");
			if (_count <= 0) return;
			Array.Clear(_items, 0, _count);
			_count = 0;
			_version++;
			RecoverFreeSpace();
			NotifyPropertyChanged(propertyName: CountString);
			NotifyPropertyChanged(propertyName: IndexerString);
			NotifyCollectionChanged(CollectionChangedEventArgsBuilder.CreateResetAction());

			//System.Diagnostics.Debug.Print("END CLEAR");
			//System.Diagnostics.Debug.IndentLevel--;
		}
		#endregion

		#region Private Methods (Verification)
		private void VerifyIndex(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException(
					nameof(index),
					"Specified index must be between zero and the number of items in the collection minus one.");
		}

		private void VerifyInsertionIndex(int index)
		{
			if (index < 0 || index > _count)
				throw new ArgumentOutOfRangeException(
					nameof(index),
					"Specified index must be between zero and the number of items in the collection.");
		}

		private void VerifyRange(int index, int count)
		{
			VerifyIndex(index);
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), "The specified count must be greater than 0");
			if (count > _count || _count - index < count)
				throw new ArgumentException("The specified range falls outside the bounds of the collection", nameof(count));
		}

		private void VerifyInsertionRange(int index, int count)
		{
			VerifyInsertionIndex(index);
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count), "The specified count must be greater than 0");
		}
		#endregion

		#region Private Methods
		private IDisposable BlockReentrancy()
		{
			_monitor.Enter();
			return _monitor;
		}

		private void CheckReentrancy()
		{
			var handler = CollectionChanged;
			if (_monitor.IsBusy && handler != null && handler.GetInvocationList().Length > 1)
				throw new InvalidOperationException("Reentrancy not allowed for collections derived from DeepObservableCollection");
		}

		private void MakeRoom(int requiredSpace, int currentSpaceToRequiredSpaceRatio = 2)
		{
			if (requiredSpace < 0)
				throw new ArgumentException("Space required cannot be less than zero", nameof(requiredSpace));
			if (requiredSpace == 0 || (Capacity - _count) / requiredSpace > currentSpaceToRequiredSpaceRatio)
				return;

			SetCapacity(
				Capacity + requiredSpace < DefaultCapacity ? DefaultCapacity :
				Capacity * 2 > MaximumCapacity             ? MaximumCapacity :
				Capacity * 2 >= requiredSpace              ? Capacity * 2 : requiredSpace);
		}

		private void RecoverFreeSpace(double freeSpaceThresholdPercentage = 0.25)
		{
			if (freeSpaceThresholdPercentage < 0 || freeSpaceThresholdPercentage > 1)
				throw new ArgumentOutOfRangeException(
					nameof(freeSpaceThresholdPercentage),
					"The free space threshold percentage must be between 0 and 1 (inclusive)");

			if (_count < Capacity * (1.0 - freeSpaceThresholdPercentage))
				SetCapacity(_count + DefaultCapacity);
		}

		private void SetCapacity(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentException("Specified capacity cannot be less than zero", nameof(capacity));
			if (capacity > MaximumCapacity)
				capacity = MaximumCapacity;
			if (capacity > 0)
			{
				var newArray = new T[capacity];
				if (_count > 0)
					Array.Copy(_items, newArray, MathHelper.Min(_count, capacity));
				_items = newArray;
			}
			else
			{
				_items = new T[0];
			}
		}

		[Conditional("DEBUG")]
		private void CollectionChangeDebugInfo(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					System.Diagnostics.Debug.Print(sender == this ? "ADD:" : "(Child) ADD");
					System.Diagnostics.Debug.IndentLevel++;
					DebugHelper.Print.Table(true, new PrintTableColumn<object>(e.NewItems, "New Items"));
					System.Diagnostics.Debug.IndentLevel--;
					break;
				case NotifyCollectionChangedAction.Remove:
					System.Diagnostics.Debug.Print(sender == this ? "REMOVE:" : "(Child) REMOVE");
					System.Diagnostics.Debug.IndentLevel++;
					DebugHelper.Print.Table(true, new PrintTableColumn<object>(e.OldItems, "Old Items"));
					System.Diagnostics.Debug.IndentLevel--;
					break;
				case NotifyCollectionChangedAction.Replace:
					System.Diagnostics.Debug.Print(sender == this ? "REPLACE:" : "(Child) REPLACE");
					System.Diagnostics.Debug.IndentLevel++;
					DebugHelper.Print.Table(
						true,
						new PrintTableColumn<object>(e.OldItems, "Old Items"),
						new PrintTableColumn<object>(e.NewItems, "New Items"));
					System.Diagnostics.Debug.IndentLevel--;
					break;
				case NotifyCollectionChangedAction.Move:
					System.Diagnostics.Debug.Print(sender == this ? "MOVE:" : "(Child) MOVE");
					System.Diagnostics.Debug.IndentLevel++;
					DebugHelper.Print.Table(
						true,
						new PrintTableColumn<object>(e.OldItems, "Old Items"),
						new PrintTableColumn<object>(e.NewItems, "New Items"));
					System.Diagnostics.Debug.IndentLevel--;
					break;
				case NotifyCollectionChangedAction.Reset:
					System.Diagnostics.Debug.Print(sender == this ? "RESET:" : "(Child) RESET");
					break;
			}
		}

		private static bool IsCompatibleObject(object value)
		{
			return value is T || ((value == null) && (default(T) == null));
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return $"{typeof(T).Name} (Count={_count})";
		}
		#endregion

		#region Nested Types
		[StructLayout(LayoutKind.Sequential)]
		private struct Enumerator : IEnumerator<T>
		{
			#region Fields
			private readonly DeepObservableCollection<T> _collection;
			private readonly int                         _version;
			private          int                         _index;
			private          T                           _current;
			#endregion

			#region Properties
			public T Current => _current;

			object IEnumerator.Current
			{
				get
				{
					if (_index == 0 || _index == _collection._count + 1)
						throw new InvalidOperationException(
							"Attempting to get Current property before MoveNext has been called for the first time, " +
							"or all items have been iterated over");
					return _current;
				}
			}
			#endregion

			#region Constructors
			internal Enumerator(DeepObservableCollection<T> collection)
			{
				_collection = collection;
				_index      = 0;
				_version    = _collection._version;
				_current    = default(T);
			}
			#endregion

			#region Public Methods
			void IDisposable.Dispose() { }

			public bool MoveNext()
			{
				if (_version == _collection._version && _index < _collection._count)
				{
					_current = _collection._items[_index];
					_index++;
					return true;
				}

				if (_version != _collection._version)
					throw new InvalidOperationException(
						"Collection versions do not match. Has the collection changed during iteration?");
				_index   = _collection._count + 1;
				_current = default(T);
				return false;
			}

			void IEnumerator.Reset()
			{
				if (_version != _collection._version)
					throw new InvalidOperationException(
						"Collection versions do not match. Has the collection changed during iteration?");
				_index   = 0;
				_current = default(T);
			}
			#endregion
		}

		private class SimpleMonitor : IDisposable
		{
			private int  _busyCount;
			public  bool IsBusy => _busyCount > 0;

			public void Enter()
			{
				_busyCount++;
			}

			public void Dispose()
			{
				_busyCount--;
			}
		}
		#endregion
	}
}