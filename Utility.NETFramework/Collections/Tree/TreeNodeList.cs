// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       TreeNodeList.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 10:19 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using JLR.Utility.NETFramework.Collections.LinkedList;

namespace JLR.Utility.NETFramework.Collections.Tree
{
	public sealed class TreeNodeList<T> : LinkedListBase<T> where T : TreeNodeBase<T>
	{
		#region Fields / Properties
		private readonly T _parent;
		public           T Parent => _parent;
		#endregion

		#region Constructor
		public TreeNodeList(T parent = null)
		{
			_parent = parent;
		}
		#endregion

		#region Method Overrides
		protected override void SetRelationships(T node)
		{
			base.SetRelationships(node);
			node.Parent          = _parent;
			node.Parent.Children = this;
		}
		#endregion
	}
}