// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       TreeNode.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 10:22 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;

namespace JLR.Utility.NETFramework.Collections.Tree
{
	#region Tree
	public class Tree<T> : TreeNode<T>
	{
		public Tree(T value = default(T)) : base(value) { }
	}
	#endregion

	public class TreeNode<T> : TreeNodeBase<TreeNode<T>>, IEquatable<T>
	{
		#region Fields & Properties
		private T _value;

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
		public TreeNode(T value = default(T), TreeNode<T> parent = null) : base(parent)
		{
			_value = value;
		}
		#endregion

		#region Interface Implementation (IEquatable<>)
		public virtual bool Equals(T other)
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
			if (obj is TreeNode<T>)
				return Equals((obj as TreeNode<T>)._value);
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