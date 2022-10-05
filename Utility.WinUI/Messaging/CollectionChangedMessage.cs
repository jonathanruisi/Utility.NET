using System.Collections.Generic;
using System.Collections.Specialized;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace JLR.Utility.WinUI.Messaging
{

    public class CollectionChangedMessage<T> : PropertyChangedMessage<IList<T>>
    {
        #region Properties
        public NotifyCollectionChangedAction Action { get; set; }
        public int OldStartingIndex { get; set; }
        public int NewStartingIndex { get; set; }
        #endregion

        #region Constructors
        public CollectionChangedMessage(object sender, string propertyName, NotifyCollectionChangedAction action)
            : this(sender, propertyName, action, new List<T>(), new List<T>()) { }

        public CollectionChangedMessage(object sender,
                                        string propertyName,
                                        NotifyCollectionChangedAction action,
                                        IList<T> removedItems,
                                        IList<T> addedItems)
            : base(sender, propertyName, removedItems, addedItems)
        {
            Action = action;
            OldStartingIndex = 0;
            NewStartingIndex = 0;
        }
        #endregion
    }
}