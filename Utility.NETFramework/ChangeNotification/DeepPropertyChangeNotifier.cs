// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       DeepPropertyChangeNotifier.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-26 @ 12:01 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System.ComponentModel;

namespace JLR.Utility.NET.ChangeNotification
{
	public abstract class DeepPropertyChangeNotifier : PropertyChangeNotifier
	{
		#region Fields
		private bool _suspendChildPropertyChangeNotification;
		#endregion

		#region Properties
		public bool SuspendChildPropertyChangeNotification
		{
			get { return _suspendChildPropertyChangeNotification; }
			set { SetProperty(ref _suspendChildPropertyChangeNotification, value); }
		}
		#endregion

		#region Constructors
		protected DeepPropertyChangeNotifier()
		{
			SuspendChildPropertyChangeNotification = false;
		}
		#endregion

		#region Protected Methods
		protected virtual void OnChildPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (SuspendPropertyChangeNotification || SuspendChildPropertyChangeNotification)
			{
				//Debug.Print("Child Property ChangeING Notification: DISABLED");
				return;
			}

			NotifyPropertyChanging(sender, e);
		}

		protected virtual void OnChildPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (SuspendPropertyChangeNotification || SuspendChildPropertyChangeNotification)
			{
				//Debug.Print("Child Property ChangeED Notification: DISABLED");
				return;
			}

			NotifyPropertyChanged(sender, e);
		}

		protected void AttachChangeEventHandlers<T>(T child)
		{
			var asChanging = child as INotifyPropertyChanging;
			if (asChanging != null)
				asChanging.PropertyChanging += OnChildPropertyChanging;
			var asChanged = child as INotifyPropertyChanged;
			if (asChanged != null)
				asChanged.PropertyChanged += OnChildPropertyChanged;
		}

		protected void DetachChangeEventHandlers<T>(T child)
		{
			var asChanging = child as INotifyPropertyChanging;
			if (asChanging != null)
				asChanging.PropertyChanging -= OnChildPropertyChanging;
			var asChanged = child as INotifyPropertyChanged;
			if (asChanged != null)
				asChanged.PropertyChanged -= OnChildPropertyChanged;
		}
		#endregion
	}
}