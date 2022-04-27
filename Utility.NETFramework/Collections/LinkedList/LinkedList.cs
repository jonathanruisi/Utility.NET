// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       LinkedList.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 10:16 PM
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
using System.Linq;
using System.Threading;

namespace JLR.Utility.NETFramework.Collections.LinkedList
{
	public class LinkedList<T> : LinkedListBase<LinkedListNode<T>>, ILinkedList<T>, ICollection
	{
		#region Properties
		public  bool   IsSynchronized => false;
		private object _syncRoot;

		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null)
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}
		#endregion

		#region Indexers
		public LinkedListNode<T> this[T item]
		{
			get { return Find(item); }
			set
			{
				ValidateNewNode(value);
				var node = Find(item);
				if (node != null)
					Replace(node, value);
			}
		}

		public new T this[int index] { get { return base[index].Value; } set { base[index] = new LinkedListNode<T>(value); } }
		#endregion

		#region Constructors
		public LinkedList(IEnumerable<T> collection = null)
		{
			if (collection != null)
			{
				foreach (var item in collection)
				{
					AddLast(item);
				}
			}
		}
		#endregion

		#region Public Methods
		public LinkedListNode<T> AddFirst(T value)
		{
			var newNode = new LinkedListNode<T>(value, this);
			AddFirst(newNode, true);
			return newNode;
		}

		public LinkedListNode<T> AddLast(T value)
		{
			var newNode = new LinkedListNode<T>(value, this);
			AddLast(newNode, true);
			return newNode;
		}

		public LinkedListNode<T> InsertBefore(LinkedListNode<T> node, T value)
		{
			var newNode = new LinkedListNode<T>(value, this);
			InsertBefore(node, newNode, true);
			return newNode;
		}

		public LinkedListNode<T> InsertAfter(LinkedListNode<T> node, T value)
		{
			var newNode = new LinkedListNode<T>(value, this);
			InsertAfter(node, newNode, true);
			return newNode;
		}

		public LinkedListNode<T> Replace(LinkedListNode<T> node, T value)
		{
			var newNode = new LinkedListNode<T>(value, this);
			Replace(node, newNode, true);
			return newNode;
		}

		public void RemoveFirst()
		{
			if (First == null)
				throw new InvalidOperationException("List is empty");
			RemoveNode(First);
		}

		public void RemoveLast()
		{
			if (First == null)
				throw new InvalidOperationException("List is empty");
			RemoveNode(Last);
		}

		public LinkedListNode<T> Find(T value)
		{
			var testNode = First;
			if (testNode != null)
			{
				do
				{
					if (EqualityComparer<T>.Default.Equals(testNode.Value, value))
						return testNode;
					testNode = testNode._next;
				} while (!ReferenceEquals(testNode, First));
			}

			return null;
		}

		public LinkedListNode<T> FindLast(T value)
		{
			if (First != null)
			{
				var previousNode = First._previous;
				var testNode     = previousNode;
				if (testNode != null)
				{
					do
					{
						if (EqualityComparer<T>.Default.Equals(testNode.Value, value))
							return testNode;
						testNode = testNode._previous;
					} while (!ReferenceEquals(testNode, previousNode));
				}
			}

			return null;
		}
		#endregion

		#region Interface Implementation (IList<>)
		public void Add(T value)
		{
			AddLast(value);
		}

		public void Insert(int index, T value)
		{
			if (index < 0 || index > Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (index == Count)
				AddLast(value);
			else
				InsertBefore(GetNodeAt(index), value);
		}

		public bool Remove(T value)
		{
			var node = Find(value);
			if (node != null)
			{
				RemoveNode(node);
				return true;
			}

			return false;
		}

		public bool Contains(T value)
		{
			return Find(value) != null;
		}

		public int IndexOf(T value)
		{
			var i = 0;
			foreach (var item in this)
			{
				if (EqualityComparer<T>.Default.Equals(item, value))
					return i;
				i++;
			}

			return -1;
		}

		public void CopyTo(T[] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (index < 0 || index > array.Length)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (array.Length - index < Count)
				throw new ArgumentException("There is insufficient space to copy the list to the array at the specified index");

			var node = First;
			if (node != null)
			{
				do
				{
					array[index++] = node.Value;
					node           = node._next;
				} while (!ReferenceEquals(node, First));
			}
		}
		#endregion

		#region Interface Implementation (ICollection)
		public void CopyTo(Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			if (array.Rank != 1)
				throw new ArgumentException("Array must have only 1 rank");

			if (array.GetLowerBound(0) != 0)
				throw new ArgumentException("Array must have a lower bound equal to 0");

			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (array.Length - index < Count)
				throw new ArgumentException("There is insufficient space to copy the list to the array at the specified index");

			var localArray = array as T[];
			if (localArray != null)
			{
				CopyTo(localArray, index);
			}
			else
			{
				var elementType = array.GetType().GetElementType();
				var listType    = typeof(T);
				if (!elementType.IsAssignableFrom(listType) && !listType.IsAssignableFrom(elementType))
					throw new ArgumentException("Invalid array type");

				var objArray = array as object[];
				if (objArray == null)
					throw new ArgumentException("Invalid array type");

				var node = First;
				try
				{
					if (node != null)
					{
						do
						{
							objArray[index++] = node.Value;
							node              = node._next;
						} while (!ReferenceEquals(node, First));
					}
				}
				catch (ArrayTypeMismatchException)
				{
					throw new ArgumentException("Invalid array type");
				}
			}
		}
		#endregion

		#region Interface Implementation (IEnumerable<>)
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public new IEnumerator<T> GetEnumerator()
		{
			return LinearEnumerable(IterationDirection.Forward).Select(node => node.Value).GetEnumerator();
		}

		public IEnumerable<T> GetItemEnumerable(IterationMode mode = IterationMode.Linear,
												IterationDirection direction = IterationDirection.Forward)
		{
			switch (mode)
			{
				case IterationMode.Linear:
					foreach (var node in LinearEnumerable(direction))
					{
						yield return node.Value;
					}

					yield break;

				case IterationMode.Circular:
					foreach (var node in CircularEnumerable(direction))
					{
						yield return node.Value;
					}

					yield break;

				case IterationMode.Harmonic:
					foreach (var node in HarmonicEnumerable(direction))
					{
						yield return node.Value;
					}

					yield break;

				default:
					yield break;
			}
		}

		public IEnumerable<LinkedListNode<T>> GetNodeEnumerable(IterationMode mode = IterationMode.Linear,
																IterationDirection direction = IterationDirection.Forward)
		{
			switch (mode)
			{
				case IterationMode.Linear:
					return LinearEnumerable(direction);
				case IterationMode.Circular:
					return CircularEnumerable(direction);
				case IterationMode.Harmonic:
					return HarmonicEnumerable(direction);
				default:
					return null;
			}
		}
		#endregion
	}
}