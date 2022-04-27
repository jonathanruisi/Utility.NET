// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       LinkedListBase.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 10:08 PM
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

namespace JLR.Utility.NETFramework.Collections.LinkedList
{
	#region Enumerated Types
	public enum IterationMode
	{
		Linear,
		Circular,
		Harmonic
	}

	public enum IterationDirection
	{
		Forward,
		Reverse
	}
	#endregion

	public abstract class LinkedListBase<T> : IList<T> where T : LinkedListNodeBase<T>
	{
		#region Fields
		private T   _first;
		private int _count;
		#endregion

		#region Properties
		public T First => _first;
		public T Last  => First?._previous;
		#endregion

		#region Indexers
		public T this[int index]
		{
			get { return GetNodeAt(index); }
			set
			{
				ValidateNewNode(value);
				Replace(GetNodeAt(index), value);
			}
		}
		#endregion

		#region Constructors
		protected LinkedListBase(IEnumerable<T> collection = null)
		{
			_first = null;
			_count = 0;

			if (collection == null) return;
			foreach (var item in collection)
			{
				AddLast(item);
			}
		}
		#endregion

		#region Public Methods
		public void AddFirst(T newNode)
		{
			AddFirst(newNode, false);
		}

		protected void AddFirst(T newNode, bool bypassNewNodeValidation)
		{
			if (!bypassNewNodeValidation) ValidateNewNode(newNode);
			if (_first == null) AddFirstNode(newNode);
			else
			{
				InsertNodeBefore(_first, newNode);
				_first = newNode;
			}

			if (!newNode.IsRelated)
				SetRelationships(newNode);
		}

		public void AddLast(T newNode)
		{
			AddLast(newNode, false);
		}

		protected void AddLast(T newNode, bool bypassNewNodeValidation)
		{
			if (!bypassNewNodeValidation) ValidateNewNode(newNode);
			if (_first == null) AddFirstNode(newNode);
			else InsertNodeBefore(_first, newNode);
			if (!newNode.IsRelated)
				SetRelationships(newNode);
		}

		public void InsertBefore(T node, T newNode)
		{
			InsertBefore(node, newNode, false);
		}

		protected void InsertBefore(T node, T newNode, bool bypassNewNodeValidation)
		{
			ValidateExistingNode(node);
			if (!bypassNewNodeValidation) ValidateNewNode(newNode);
			InsertNodeBefore(node, newNode);
			if (!newNode.IsRelated)
				SetRelationships(newNode);
			if (node == _first) _first = newNode;
		}

		public void InsertAfter(T node, T newNode)
		{
			InsertAfter(node, newNode, false);
		}

		protected void InsertAfter(T node, T newNode, bool bypassNewNodeValidation)
		{
			ValidateExistingNode(node);
			if (!bypassNewNodeValidation) ValidateNewNode(newNode);
			InsertNodeBefore(node._next, newNode);
			if (!newNode.IsRelated)
				SetRelationships(newNode);
		}

		public void Replace(T node, T newNode)
		{
			Replace(node, newNode, false);
		}

		protected void Replace(T node, T newNode, bool bypassNewNodeValidation)
		{
			ValidateExistingNode(node);
			if (!bypassNewNodeValidation) ValidateNewNode(newNode);
			ReplaceNode(node, newNode);
			if (!newNode.IsRelated)
				SetRelationships(newNode);
		}
		#endregion

		#region Interface Implementation (IList<>)
		public void Insert(int index, T node)
		{
			if (index < 0 || index > _count)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (index == _count) AddLast(node);
			else InsertBefore(GetNodeAt(index), node);
		}

		public void RemoveAt(int index)
		{
			Remove(GetNodeAt(index));
		}

		public int IndexOf(T node)
		{
			var i = 0;
			foreach (var item in this)
			{
				if (item == node)
					return i;
				i++;
			}

			return -1;
		}
		#endregion

		#region Interface Implementation (ICollection<>)
		public int  Count      => _count;
		public bool IsReadOnly => false;

		public bool Contains(T node)
		{
			return this.Any(item => node == item);
		}

		public void Add(T node)
		{
			AddLast(node, false);
		}

		public bool Remove(T node)
		{
			if (!Contains(node)) return false;
			RemoveNode(node);
			return true;
		}

		public void Clear()
		{
			var node = _first;
			while (node != null)
			{
				var previousNode = node;
				node = node._next;
				previousNode.Dispose();
			}

			_first = null;
			_count = 0;
		}

		public void CopyTo(T[] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException(nameof(array));
			if (index < 0 || index > array.Length)
				throw new ArgumentOutOfRangeException(nameof(index));
			if (array.Length - index < _count)
				throw new ArgumentException("There is insufficient space to copy the list to the array at the specified index");

			var node = _first;
			if (node == null) return;
			do
			{
				array[index++] = node;
				node           = node._next;
			} while (node != _first);
		}
		#endregion

		#region Interface Implementation (IEnumerable<>)
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return LinearEnumerable(IterationDirection.Forward).GetEnumerator();
		}

		protected IEnumerable<T> LinearEnumerable(IterationDirection direction)
		{
			var current = direction == IterationDirection.Forward ? _first : _first._previous;
			if (current == null) yield break;

			if (direction == IterationDirection.Forward)
			{
				do
				{
					yield return current;
					current = current._next;
				} while (current != _first);
			}
			else
			{
				do
				{
					yield return current;
					current = current._previous;
				} while (current != _first._previous);
			}
		}

		protected IEnumerable<T> CircularEnumerable(IterationDirection direction)
		{
			var current = direction == IterationDirection.Forward ? _first : _first._previous;
			if (current == null) yield break;

			while (true)
			{
				yield return current;
				current = direction == IterationDirection.Forward ? current._next : current._previous;
			}
		}

		protected IEnumerable<T> HarmonicEnumerable(IterationDirection direction)
		{
			var current = direction == IterationDirection.Forward ? _first : _first._previous;
			if (current == null) yield break;
			var isOpposite = false;

			while (true)
			{
				yield return current;
				if ((direction == IterationDirection.Forward && !isOpposite) ||
					(direction == IterationDirection.Reverse && isOpposite))
				{
					current = current._next;
					if (current == _first._previous)
						isOpposite = direction == IterationDirection.Forward;
				}
				else if ((direction == IterationDirection.Forward && isOpposite) ||
					(direction == IterationDirection.Reverse && !isOpposite))
				{
					current = current._previous;
					if (current == _first)
						isOpposite = direction == IterationDirection.Reverse;
				}
			}
		}
		#endregion

		#region Private Methods
		protected void AddFirstNode(T newNode)
		{
			newNode._next     = newNode;
			newNode._previous = newNode;
			_first            = newNode;
			_count++;
		}

		protected void InsertNodeBefore(T node, T newNode)
		{
			newNode._next        = node;
			newNode._previous    = node._previous;
			node._previous._next = newNode;
			node._previous       = newNode;
			_count++;
		}

		protected void ReplaceNode(T node, T newNode)
		{
			newNode._next        = node._next;
			newNode._previous    = node._previous;
			node._previous._next = newNode;
			node._next._previous = newNode;
			if (node == _first) _first = newNode;
			node.Dispose();
		}

		protected void RemoveNode(T node)
		{
			if (node._next == node) _first = null;
			else
			{
				node._previous._next = node._next;
				node._next._previous = node._previous;
				if (node == _first) _first = node._next;
			}

			node.Dispose();
			_count--;
		}

		protected T GetNodeAt(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException(nameof(index));

			var current = _first;
			if (index < _count / 2)
			{
				for (var i = 1; i <= index; i++)
					current = current._next;
			}
			else
			{
				for (var i = _count; i > index; i--)
					current = current._previous;
			}

			return current;
		}

		protected void ValidateNewNode(T newNode)
		{
			if (newNode == null) throw new ArgumentNullException(nameof(newNode));
			if (newNode._siblings != null)
				throw new InvalidOperationException("Node must not already belong to another list");
		}

		protected void ValidateExistingNode(T node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));
			if (node._siblings != this)
				throw new InvalidOperationException("Node must be a member of this list");
		}

		protected virtual void SetRelationships(T node)
		{
			node._siblings = this;
		}
		#endregion
	}
}