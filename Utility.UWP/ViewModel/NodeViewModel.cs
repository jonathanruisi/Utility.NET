using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml;

namespace JLR.Utility.UWP.ViewModel
{
	/// <summary>
	/// Represents a node in a hierarchical structure.
	/// <see cref="NodeViewModel"/> types are capable of advanced
	/// data binding scenarios in which change notification is required.
	/// <see cref="NodeViewModel"/> types are capable of
	/// serialization to and deserialization from XML.
	/// </summary>
	public abstract class NodeViewModel : XmlViewModel, IEnumerable<NodeViewModel>
	{
		#region Fields
		private NodeViewModel _parent;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets a reference to this node's parent node
		/// </summary>
		public NodeViewModel Parent
		{
			get => _parent;
			set =>
				Set(value, () => _parent, newValue =>
				{
					_parent?.Children.Remove(this);
					newValue.Children.Add(this);
				});
		}

		/// <summary>
		/// Gets a collection of this node's child nodes
		/// </summary>
		public ObservableCollection<NodeViewModel> Children { get; }

		/// <summary>
		/// Gets the depth of this node within the tree relative to the root node.
		/// A value of zero indicates that this node is the root.
		/// </summary>
		public int Depth => _parent?.Depth + 1 ?? 0;

		/// <summary>
		/// Gets a reference to the root node
		/// </summary>
		public NodeViewModel Root => _parent == null ? this : _parent.Root;

		/// <summary>
		/// Exposes an enumerator which iterates over all nodes that share this node's parent
		/// </summary>
		public IEnumerable<NodeViewModel> Siblings =>
			from sibling in _parent?.Children
			where sibling != this
			select sibling;
		#endregion

		#region Constructor
		protected NodeViewModel()
		{
			Children                   =  new ObservableCollection<NodeViewModel>();
			Children.CollectionChanged += Children_CollectionChanged;
		}
		#endregion

		#region Methods (Public)
		/// <summary>
		/// Removes the first occurence of the specified <see cref="NodeViewModel"/> instance,
		/// if it exists, from any depth within this node's subtree.
		/// </summary>
		/// <param name="node">The <see cref="NodeViewModel"/> to remove</param>
		/// <returns><code>true</code> if the specified node was removed, <code>false</code> otherwise</returns>
		public bool Remove(NodeViewModel node)
		{
			if (Children.Remove(node))
				return true;

			var result = false;
			for (var i = 0; result == false && i < Children.Count; i++)
			{
				result = Children[i].Remove(node);
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
					foreach (NodeViewModel node in e.NewItems)
						node._parent = this;
					break;
				}

				case NotifyCollectionChangedAction.Remove:
				{
					foreach (NodeViewModel node in e.OldItems)
						node._parent = null;
					break;
				}

				case NotifyCollectionChangedAction.Replace:
				{
					foreach (NodeViewModel oldNode in e.OldItems)
						oldNode._parent = null;
					foreach (NodeViewModel newNode in e.NewItems)
						newNode._parent = this;
					break;
				}
			}
		}
		#endregion

		#region Method Overrides (XmlViewModel)
		protected override void ProcessMemberElement(XmlViewModel element, string flag)
		{
			if (flag == "Child" && element is NodeViewModel node)
				Children.Add(node);
		}

		public override void WriteXml(XmlWriter writer)
		{
			// Verify that this node's type is registered for XML serialization
			var type = GetType();
			ValidateTypeForXmlSerialization(type);

			// Write start tag
			writer.WriteStartElement(SerializationInfo[type]);

			// If this is a root node, explicitly indicate it is not a child.
			// This allows for derived classes to include their own type as
			// serializable members which are not children in its hierarchy.
			// If this is a child node and its parent is a different type,
			// explicitly indicate that it is a child.
			if (this == Root && Children.Count == 0)
				writer.WriteAttributeString(ViewModelFlagName, "Member");
			else if (Parent != null &&
					 SerializationInfo[Parent.GetType()]
				  != SerializationInfo[GetType()])
				writer.WriteAttributeString(ViewModelFlagName, "Child");

			// Serialize this node's attributes and non-child elements
			WriteAttributes(writer);
			WriteElements(writer);

			// Recursively serialize all child nodes
			foreach (var child in Children)
			{
				child.WriteXml(writer);
			}

			// Write end tag
			writer.WriteEndElement();
		}
		#endregion

		#region Interface Implementation (IEnumerable<T>)
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<NodeViewModel> GetEnumerator()
		{
			yield return this;

			foreach (var child in Children)
			{
				IEnumerator<NodeViewModel> childEnumerator = child.GetEnumerator();
				while (childEnumerator.MoveNext())
				{
					yield return childEnumerator.Current;
				}

				childEnumerator.Dispose();
			}
		}
		#endregion
	}
}