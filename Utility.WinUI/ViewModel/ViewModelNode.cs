using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.Messaging;

using JLR.Utility.WinUI.Messaging;

namespace JLR.Utility.WinUI.ViewModel
{
    /// <summary>
    /// Represents a node in a hierarchical structure ideal for
    /// use in user interfaces and other scenarios that
    /// rely on data binding. <see cref="ViewModelNode"/>
    /// inherits from <see cref="ViewModelElement"/>,
    /// and is therefore capable of fully automatic
    /// XML serialization and deserialization.
    /// </summary>
    public abstract class ViewModelNode : ViewModelElement
    {
        #region Fields
        private ViewModelNode _parent;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a reference to this node's parent node
        /// </summary>
        public ViewModelNode Parent
        {
            get => _parent;
            set => SetProperty(_parent, value, newValue =>
            {
                _parent?.Children.Remove(this);
                newValue?.Children.Add(this);
                _parent = newValue;
            }, true);
        }

        /// <summary>
        /// Gets a collection of this node's children
        /// </summary>
        [ViewModelCollection("Children")]
        public ObservableCollection<ViewModelNode> Children { get; }

        /// <summary>
        /// Gets the depth of this node within the tree relative to the root node.
        /// A value of zero indicates that this node is the root.
        /// </summary>
        public int Depth => _parent?.Depth + 1 ?? 0;

        /// <summary>
        /// Gets a reference to the root node
        /// </summary>
        public ViewModelNode Root => _parent == null ? this : _parent.Root;

        /// <summary>
        /// Exposes an enumerator which iterates over all nodes that share this node's parent
        /// </summary>
        public IEnumerable<ViewModelNode> Siblings => from sibling in _parent?.Children
                                                      where sibling != this && sibling.Depth == Depth
                                                      select sibling;
        #endregion

        #region Constructor
        protected ViewModelNode()
        {
            Children = new ObservableCollection<ViewModelNode>();
            Children.CollectionChanged += Children_CollectionChanged;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Removes the first occurence of the specified <see cref="ViewModelNode"/> instance,
        /// if it exists, from any depth within this node's subtree.
        /// </summary>
        /// <param name="node">The <see cref="ViewModelNode"/> to remove</param>
        /// <returns><code>true</code> if the specified node was removed, <code>false</code> otherwise</returns>
        public bool Remove(ViewModelNode node)
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
            var collectionChangedMessage = new CollectionChangedMessage<ViewModelNode>(this, nameof(Children), e.Action)
            {
                OldStartingIndex = e.OldStartingIndex,
                NewStartingIndex = e.NewStartingIndex
            };

            if (e.OldItems != null)
            {
                foreach (ViewModelNode oldNode in e.OldItems)
                {
                    oldNode._parent = null;
                    collectionChangedMessage.OldValue.Add(oldNode);
                }
            }

            if (e.NewItems != null)
            {
                foreach (ViewModelNode newNode in e.NewItems)
                {
                    newNode._parent = this;
                    collectionChangedMessage.NewValue.Add(newNode);
                }
            }

            Messenger.Send(collectionChangedMessage, nameof(Children));
            NotifySerializedCollectionChanged(nameof(Children));
        }
        #endregion

        #region Interface Implementation (IEnumerable<T>)
        public IEnumerator<ViewModelNode> GetEnumerator()
        {
            return DepthFirstEnumerable().GetEnumerator();
        }

        public IEnumerable<ViewModelNode> DepthFirstEnumerable()
        {
            yield return this;

            foreach (var child in Children)
            {
                var childEnumerator = child.DepthFirstEnumerable().GetEnumerator();
                while (childEnumerator.MoveNext())
                {
                    yield return childEnumerator.Current;
                }
            }
        }
        #endregion

        #region Method Overrides (System.Object)
        public override string ToString()
        {
            var str = new StringBuilder(base.ToString());
            if (Children.Count > 0)
            {
                str.Append($" ({Children.Count} ");
                str.Append(Children.Count == 1 ? "Child" : "Children");
                str.Append(')');
            }
            return str.ToString();
        }
        #endregion
    }
}