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
    /// rely on data binding.<br/><see cref="ViewModelNode"/>
    /// inherits from <see cref="ViewModelElement"/>,
    /// and is therefore capable of fully automatic
    /// XML serialization and deserialization.
    /// </summary>
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
            Children = new ObservableCollection<ViewModelElement>();
            Children.CollectionChanged += Children_CollectionChanged;
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
        #endregion

        #region Event Handlers
        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

        #region Interface Implementation (IEnumerable<T>)
        public IEnumerator<ViewModelElement> GetEnumerator()
        {
            return DepthFirstEnumerable().GetEnumerator();
        }

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