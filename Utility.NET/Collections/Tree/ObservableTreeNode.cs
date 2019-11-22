using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using JLR.Utility.NET.ChangeNotification;

namespace JLR.Utility.NET.Collections.Tree
{
	public abstract class ObservableTreeNode : ObservableBase
	{
		#region Fields
		private ObservableTreeNode _parent;
		#endregion

		#region Properties
		public ObservableTreeNode Parent
		{
			get => _parent;
			set => Set(value, () => _parent, newValue =>
			{
				_parent?.Children.Remove(this);
				newValue.Children.Add(this);
			});
		}

		public ObservableCollection<ObservableTreeNode> Children { get; }

		public int Depth => _parent?.Depth + 1 ?? 0;

		public ObservableTreeNode Root => _parent == null ? this : _parent.Root;

		public IEnumerable<ObservableTreeNode> Siblings =>
			from sibling in _parent?.Children
			where sibling != this
			select sibling;
		#endregion

		#region Constructors
		protected ObservableTreeNode()
		{
			Children                   =  new ObservableCollection<ObservableTreeNode>();
			Children.CollectionChanged += Children_CollectionChanged;
		}
		#endregion

		#region Methods (Public)
		public bool DeepRemove(ObservableTreeNode node)
		{
			if (Children.Remove(node))
				return true;

			var result = false;
			for (var i = 0; result == false && i < Children.Count; i++)
			{
				result = Children[i].DeepRemove(node);
			}

			return result;
		}
		#endregion

		#region Event Handlers
		private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				{
					foreach (ObservableTreeNode node in e.NewItems)
						node._parent = this;
					break;
				}

				case NotifyCollectionChangedAction.Remove:
				{
					foreach (ObservableTreeNode node in e.OldItems)
						node._parent = null;
					break;
				}

				case NotifyCollectionChangedAction.Replace:
				{
					foreach (ObservableTreeNode oldNode in e.OldItems)
						oldNode._parent = null;
					foreach (ObservableTreeNode newNode in e.NewItems)
						newNode._parent = this;
					break;
				}
			}
		}
		#endregion
	}
}