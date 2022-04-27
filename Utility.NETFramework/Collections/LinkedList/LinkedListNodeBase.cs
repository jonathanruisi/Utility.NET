// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       LinkedListNodeBase.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 10:07 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

namespace JLR.Utility.NETFramework.Collections.LinkedList
{
	public abstract class LinkedListNodeBase<T> : Disposable where T : LinkedListNodeBase<T>
	{
		#region Fields
		internal T                 _previous;
		internal T                 _next;
		internal LinkedListBase<T> _siblings;
		#endregion

		#region Properties
		public int Index
		{
			get
			{
				EnforceDisposalState();
				return (Previous?.Index ?? -1) + 1;
			}
		}

		public T Previous
		{
			get
			{
				EnforceDisposalState();
				if (_previous != null && this != _siblings.First)
					return _previous;
				return null;
			}
		}

		public T Next
		{
			get
			{
				EnforceDisposalState();
				if (_next != null && _next != _siblings.First)
					return _next;
				return null;
			}
		}

		public LinkedListBase<T> Siblings
		{
			get
			{
				EnforceDisposalState();
				return _siblings;
			}
		}

		internal bool IsRelated
		{
			get
			{
				EnforceDisposalState();
				return _siblings != null;
			}
		}
		#endregion

		#region Constructors
		public LinkedListNodeBase(LinkedListBase<T> siblings = null)
		{
			_previous = null;
			_next     = null;
			_siblings = siblings;
		}
		#endregion

		#region Method Overrides (Disposable)
		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					_siblings = null;
					_previous = null;
					_next     = null;
				}
			}

			IsDisposed = true;
		}
		#endregion
	}
}