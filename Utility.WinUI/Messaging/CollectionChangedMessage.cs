using System.Collections.Generic;

using CommunityToolkit.Mvvm.Messaging.Messages;

namespace JLR.Utility.WinUI.Messaging
{

    public class CollectionChangedMessage<T> : PropertyChangedMessage<IList<T>>
    {
        public CollectionChangedMessage(object sender, string propertyName)
            : this(sender, propertyName, new List<T>(), new List<T>()) { }

        public CollectionChangedMessage(object sender, string propertyName, IList<T> removedItems, IList<T> addedItems)
            : base(sender, propertyName, removedItems, addedItems) { }
    }
}