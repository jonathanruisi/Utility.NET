// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       TreeNodeBase.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 10:18 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.Collections.Generic;

using JLR.Utility.NET.Collections.LinkedList;

namespace JLR.Utility.NET.Collections.Tree
{
	#region Enumerated Types
	public enum TraversalMode
	{
		DepthFirst,
		BreadthFirst
	}

	public enum TraversalDirection
	{
		TopDown,
		BottomUp
	}
	#endregion

	public abstract class TreeNodeBase<T> : LinkedListNodeBase<T> where T : TreeNodeBase<T>
	{
		#region Fields
		private T               _parent;
		private TreeNodeList<T> _children;
		#endregion

		#region Properties
		public TreeNodeList<T> Children
		{
			get
			{
				EnforceDisposalState();
				return _children;
			}
			internal set
			{
				EnforceDisposalState();
				_children = value;
			}
		}

		public T Parent
		{
			get
			{
				EnforceDisposalState();
				return _parent;
			}
			internal set
			{
				EnforceDisposalState();
				if (value == _parent) return;
				_parent?._children.Remove(this as T);
				if (value != null && !value._children.Contains(this as T))
					value._children.AddLast(this as T);
				_parent = value;
			}
		}

		public T Root
		{
			get
			{
				EnforceDisposalState();
				return _parent == null ? this as T : _parent.Root;
			}
		}

		public int Depth
		{
			get
			{
				EnforceDisposalState();
				return (_parent?.Depth ?? -1) + 1;
			}
		}
		#endregion

		#region Constructors
		protected TreeNodeBase(T parent = null) : base(parent?._children)
		{
			Parent    = parent; // NOTE: Changed from _parent = parent
			_children = new TreeNodeList<T>(this as T);
		}
		#endregion

		#region Interface Implementation (IEnumerable<>)
		public IEnumerable<T> GetEnumerable(TraversalMode mode, TraversalDirection direction)
		{
			switch (mode)
			{
				case TraversalMode.DepthFirst:
					return DepthFirstEnumerable(direction);
				case TraversalMode.BreadthFirst:
					return BreadthFirstEnumerable(direction);
				default:
					return null;
			}
		}

		private IEnumerable<T> DepthFirstEnumerable(TraversalDirection direction)
		{
			if (direction == TraversalDirection.TopDown)
				yield return this as T;

			foreach (var child in _children)
			{
				var childEnumerator = child.DepthFirstEnumerable(direction).GetEnumerator();
				while (childEnumerator.MoveNext())
				{
					yield return childEnumerator.Current;
				}
			}

			if (direction == TraversalDirection.BottomUp)
				yield return this as T;
		}

		private IEnumerable<T> BreadthFirstEnumerable(TraversalDirection direction)
		{
			if (direction == TraversalDirection.TopDown)
			{
				var queue = new Queue<T>();
				queue.Enqueue(this as T);

				while (0 < queue.Count)
				{
					var node = queue.Dequeue();
					foreach (var child in node._children)
					{
						queue.Enqueue(child);
					}

					yield return node;
				}

				yield break;
			}

			var stack = new Stack<T>();
			foreach (var node in BreadthFirstEnumerable(TraversalDirection.TopDown))
			{
				stack.Push(node);
			}

			while (stack.Count > 0)
			{
				yield return stack.Pop();
			}
		}
		#endregion

		#region Method Overrides (Disposable)
		protected override void Dispose(bool disposing)
		{
			// BUG: Causes infinite loop!
			if (!IsDisposed)
			{
				if (disposing)
				{
					_children.Clear();
					_children = null;
					_parent   = null;
				}
			}

			IsDisposed = true;
			base.Dispose(disposing);
		}
		#endregion
	}
}