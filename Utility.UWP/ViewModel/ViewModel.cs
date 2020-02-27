using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using Windows.Devices.AllJoyn;

namespace JLR.Utility.UWP.ViewModel
{
	/// <summary>
	/// Represents types that participate in data binding,
	/// in which property change notification is required.
	/// </summary>
	public abstract class ViewModel : INotifyPropertyChanged, INotifyPropertyChanging
	{
		#region Fields
		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;
		#endregion

		#region Methods (Protected)
		/// <summary>
		/// Sets the value of a property in a derived type and raises property change/changing events.
		/// Calls to this method are typically made from a CLR property setter.
		/// </summary>
		/// <typeparam name="T">The type of the property</typeparam>
		/// <param name="backingStore">
		/// Reference to the backing store in which the value of this property is stored
		/// </param>
		/// <param name="newValue">The value to which the property is being set</param>
		/// <param name="propertyName">
		/// Do not modify this parameter from its default value of <c>null</c>
		/// </param>
		protected virtual void Set<T>(ref T backingStore, T newValue, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, newValue))
				return;
			NotifyPropertyChanging(this, new PropertyChangingEventArgs(propertyName));
			backingStore = newValue;
			NotifyPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Sets the value of a property in a derived type using caller-specified setter logic
		/// and raises property change/changing events.
		/// Calls to this method are typically made from a CLR property setter.
		/// </summary>
		/// <typeparam name="T">The type of the property</typeparam>
		/// <param name="newValue">The value to which the property is being set</param>
		/// <param name="getter">Method which returns the current value of the property</param>
		/// <param name="setter">Method which sets the property top its new value</param>
		/// <param name="propertyName">
		/// Do not modify this parameter from its default value of <c>null</c>
		/// </param>
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