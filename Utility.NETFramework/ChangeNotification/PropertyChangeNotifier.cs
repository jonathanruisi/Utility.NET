// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PropertyChangeNotifier.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-26 @ 12:00 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using JLR.Utility.NETFramework.ChangeNotification.EventArgs;

namespace JLR.Utility.NETFramework.ChangeNotification
{
	public interface IPropertyChangeNotification : INotifyPropertyChanged, INotifyPropertyChanging { }

	public abstract class PropertyChangeNotifier : IPropertyChangeNotification
	{
		#region Fields
		public event PropertyChangedEventHandler  PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;
		private bool                              _suspendPropertyChangeNotification;
		#endregion

		#region Properties
		public bool SuspendPropertyChangeNotification
		{
			get { return _suspendPropertyChangeNotification; }
			set { SetProperty(ref _suspendPropertyChangeNotification, value); }
		}
		#endregion

		#region Constructors
		protected PropertyChangeNotifier()
		{
			SuspendPropertyChangeNotification = false;
		}
		#endregion

		#region Protected Methods
		protected virtual bool SetProperty<T>(ref T backingStore, T newValue, [CallerMemberName] string propertyName = null)
		{
			if (Equals(backingStore, newValue) || NotifyPropertyChanging(backingStore, newValue, propertyName))
				return false;
			var oldValue = backingStore;
			backingStore = newValue;
			NotifyPropertyChanged(oldValue, backingStore, propertyName);
			return true;
		}

		protected virtual bool SetProperty<T>(T newValue,
											  Func<T> getter,
											  Action<T> setter,
											  [CallerMemberName] string propertyName = null)
		{
			var oldValue = getter();
			if (Equals(oldValue, newValue) || NotifyPropertyChanging(oldValue, newValue, propertyName))
				return false;
			setter(newValue);
			NotifyPropertyChanged(oldValue, getter(), propertyName);
			return true;
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanging"/> event.
		/// If the event handler sets <see cref="PropertyChangingAdvancedEventArgs.Cancel"/> = <code>true</code>,
		/// the value of the property will not change and no setter code will execute.
		/// </summary>
		/// <param name="currentValue">The value of the property before changes are made</param>
		/// <param name="newValue">The value of the property after changes are made</param>
		/// <param name="propertyName">
		/// The name of the property
		/// (Do not explicitly set this when calling from a property setter)
		/// </param>
		/// <returns><code>true</code> if property change was canceled, <code>false</code> otherwise</returns>
		protected bool NotifyPropertyChanging(object currentValue = null,
											  object newValue = null,
											  [CallerMemberName] string propertyName = null)
		{
			if (SuspendPropertyChangeNotification)
			{
				//Debug.Print("Property ChangING Notification: DISABLED");
				return false;
			}

			var args = new PropertyChangingAdvancedEventArgs(propertyName, currentValue, newValue);
			NotifyPropertyChanging(this, args);

			//if (currentValue == null && newValue != null)
			//	Debug.Print("{0} changING to {1}{2}", propertyName, newValue, args.Cancel ? " (CANCELLED)" : String.Empty);
			//else if (currentValue != null && newValue == null)
			//	Debug.Print("{0} changING from {1}{2}", propertyName, currentValue, args.Cancel ? " (CANCELLED)" : String.Empty);
			//else if (currentValue == null)
			//	Debug.Print("{0} changING{1}", propertyName, args.Cancel ? " (CANCELLED)" : String.Empty);
			//else
			//	Debug.Print("{0} changING from {1} to {2}{3}", propertyName, currentValue, newValue,
			//		args.Cancel ? " (CANCELLED)" : String.Empty);
			return args.Cancel;
		}

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="oldValue">The value of the property before changes were made</param>
		/// <param name="currentValue">The value of the property after changes were made</param>
		/// <param name="propertyName">
		/// The name of the property
		/// (Do not explicitly set this when calling from a property setter)
		/// </param>
		protected void NotifyPropertyChanged(object oldValue = null,
											 object currentValue = null,
											 [CallerMemberName] string propertyName = null)
		{
			if (SuspendPropertyChangeNotification)
			{
				//Debug.Print("Property ChangED Notification: DISABLED");
				return;
			}

			var args = new PropertyChangedAdvancedEventArgs(propertyName, oldValue, currentValue);

			//if (oldValue == null && currentValue != null)
			//	Debug.Print("{0} changED to {1}", propertyName, currentValue);
			//else if (oldValue != null && currentValue == null)
			//	Debug.Print("{0} changED from {1}", propertyName, oldValue);
			//else if (oldValue == null)
			//	Debug.Print("{0} changED", propertyName);
			//else
			//	Debug.Print("{0} changED from {1} to {2}", propertyName, oldValue, currentValue);
			NotifyPropertyChanged(this, args);
		}
		#endregion

		#region Private Methods
		private void NotifyPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			var handler = PropertyChanging;
			handler?.Invoke(sender, e);
		}

		private void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var handler = PropertyChanged;
			handler?.Invoke(sender, e);
		}
		#endregion
	}
}