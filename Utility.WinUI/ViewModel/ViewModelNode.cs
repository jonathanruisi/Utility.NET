using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
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
    /// rely on data binding.<br/><see cref="ViewModelNode"/>
    /// inherits from <see cref="ViewModelElement"/>,
    /// and is therefore capable of fully automatic
    /// XML serialization and deserialization.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public abstract class ViewModelNode : ViewModelElement
    {
        #region Properties
        /// <summary>
        /// Gets a collection of this node's children
        /// </summary>
        [ViewModelCollection("Children")]
        public ObservableCollection<ViewModelElement> Children { get; }
        #endregion

        #region Constructor
        protected ViewModelNode()
        {
            Children = [];
            Children.CollectionChanged += ChildrenChanged;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Removes the first occurence of the specified <see cref="ViewModelElement"/> instance,
        /// if it exists, from any depth within this node's subtree.
        /// </summary>
        /// <param name="element">The <see cref="ViewModelElement"/> to remove.</param>
        /// <returns><c>true</c> if the specified node was removed, <c>false</c> otherwise</returns>
        public bool Remove(ViewModelElement element)
        {
            if (Children.Remove(element))
                return true;

            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i] is ViewModelNode node && node.Remove(element))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an enumerator that iterates over all nodes in the
        /// tree using a depth-first traversal algorithm.
        /// </summary>
        /// <remarks>
        /// The node on which <see cref="DepthFirstEnumerable"/>
        /// is called acts as the root node for the traversal.
        /// Only that node and nodes with greater depth
        /// will be returned.
        /// </remarks>
        /// <returns>
        /// An enumerator that traverses all nodes in the tree.
        /// </returns>
        public IEnumerable<ViewModelElement> DepthFirstEnumerable()
        {
            yield return this;

            foreach (var child in Children)
            {
                if (child is ViewModelNode node)
                {
                    var childEnumerator = node.DepthFirstEnumerable().GetEnumerator();
                    while (childEnumerator.MoveNext())
                    {
                        yield return childEnumerator.Current;
                    }
                }
                else
                {
                    yield return child;
                }
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles changes to the Children collection by updating parent references and notifying listeners of the
        /// change.
        /// </summary>
        /// <remarks>
        /// This method updates the parent references of affected child elements and sends a
        /// notification message to registered listeners. Override this method to customize how collection changes are
        /// handled in derived classes.
        /// </remarks>
        /// <param name="sender">The source of the collection changed event, typically the Children collection.</param>
        /// <param name="e">The event data containing information about the change to the collection.</param>
        protected virtual void ChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var collectionChangedMessage = new CollectionChangedMessage<ViewModelElement>(this, nameof(Children), e.Action)
            {
                OldStartingIndex = e.OldStartingIndex,
                NewStartingIndex = e.NewStartingIndex
            };

            if (e.OldItems != null)
            {
                foreach (ViewModelElement oldElement in e.OldItems)
                {
                    oldElement._parent = null;
                    collectionChangedMessage.OldValue.Add(oldElement);
                }
            }

            if (e.NewItems != null)
            {
                foreach (ViewModelElement newElement in e.NewItems)
                {
                    newElement._parent = this;
                    collectionChangedMessage.NewValue.Add(newElement);
                }
            }

            Messenger.Send(collectionChangedMessage, nameof(Children));
            NotifySerializedCollectionChanged(nameof(Children));
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