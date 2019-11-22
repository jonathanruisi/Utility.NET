using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JLR.Utility.NET.ChangeNotification
{
	public abstract class ObservableBase : INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region Fields
		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;
		#endregion

		#region Methods (Protected)
		protected virtual void Set<T>(ref T backingStore, T newValue, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, newValue))
				return;
			NotifyPropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			backingStore = newValue;
			NotifyPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void Set<T>(T newValue,
		                              Func<T> getter,
		                              Action<T> setter,
		                              [CallerMemberName] string propertyName = null)
		{
			var currentValue = getter();
			if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
				return;
			NotifyPropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			setter(newValue);
			NotifyPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#region Methods (Private)
		private void NotifyPropertyChanging(object sender, PropertyChangingEventArgs args)
		{
			var handler = PropertyChanging;
			handler?.Invoke(sender, args);
		}

		private void NotifyPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			var handler = PropertyChanged;
			handler?.Invoke(sender, args);
		}
		#endregion
	}
}