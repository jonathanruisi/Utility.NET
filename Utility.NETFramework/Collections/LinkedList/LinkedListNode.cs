// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       LinkedListNode.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 10:14 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;

namespace JLR.Utility.NET.Collections.LinkedList
{
	public class LinkedListNode<T> : LinkedListNodeBase<LinkedListNode<T>>, IEquatable<T>
	{
		#region Fields
		private T _value;
		#endregion

		#region Properties
		public T Value
		{
			get
			{
				EnforceDisposalState();
				return _value;
			}
			set
			{
				EnforceDisposalState();
				_value = value;
			}
		}
		#endregion

		#region Constructors
		public LinkedListNode(T value = default(T), LinkedListBase<LinkedListNode<T>> siblings = null) : base(siblings)
		{
			_value = value;
		}
		#endregion

		#region Interface Implementation (IEquatable<>)
		public bool Equals(T other)
		{
			EnforceDisposalState();
			return EqualityComparer<T>.Default.Equals(_value, other);
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			EnforceDisposalState();
			if (obj is T)
				return Equals((T)obj);
			if (obj is LinkedListNode<T>)
				return Equals((obj as LinkedListNode<T>)._value);
			return false;
		}

		public override int GetHashCode()
		{
			EnforceDisposalState();
			return EqualityComparer<T>.Default.GetHashCode(Value);
		}

		public override string ToString()
		{
			EnforceDisposalState();
			return !ReferenceEquals(_value, null) ? _value.ToString() : String.Empty;
		}
		#endregion
	}
}