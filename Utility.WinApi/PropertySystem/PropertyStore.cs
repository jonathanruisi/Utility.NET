// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PropertyStore.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:30 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using JLR.Utility.WinApi.Error;

namespace JLR.Utility.WinApi.PropertySystem
{
	#region IPropertyStore COM Interface
	[ComImport]
	[Guid("886d8eeb-8cf2-4446-8d02-cdba1dbdcf99")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPropertyStore
	{
		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		int GetCount([Out] out uint count);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		int GetAt([In] uint index, out PropertyKey key);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		int GetValue([In] ref PropertyKey key, out PropVariant value);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		int SetValue([In] ref PropertyKey key, [In] ref PropVariant value);

		[PreserveSig]
		[return: MarshalAs(UnmanagedType.Error)]
		int Commit();
	}
	#endregion

	public sealed class PropertyStore : IDisposable, IEnumerable<KeyValuePair<PropertyKey, PropVariant>>
	{
		#region Fields
		private IPropertyStore _propertyStoreInterface;
		#endregion

		#region Properties and Indexers
		public int Count { get { return GetCount(); } }

		public PropVariant this[PropertyKey key]
		{
			get { return GetValue(key); }
			set { SetValue(new KeyValuePair<PropertyKey, PropVariant>(key, value)); }
		}

		public KeyValuePair<PropertyKey, PropVariant> this[int index]
		{
			get
			{
				var key   = GetAt(index);
				var value = GetValue(key);
				return new KeyValuePair<PropertyKey, PropVariant>(key, value);
			}
		}
		#endregion

		#region Constructors
		public PropertyStore(IPropertyStore propertyStoreInterface)
		{
			_propertyStoreInterface = propertyStoreInterface;
		}
		#endregion

		#region Public Methods
		public bool Contains(PropertyKey key)
		{
			for (var i = 0; i < Count; i++)
			{
				if (key.Equals(GetAt(i)))
					return true;
			}

			return false;
		}

		public PropertyKey GetAt(int index)
		{
			if ((index < 0) || (index >= Count))
				throw new IndexOutOfRangeException(
					String.Format("Index must be between 0 and {0} (inclusive) for this instance.", Count - 1));

			PropertyKey result;
			HResult.Try(_propertyStoreInterface.GetAt((uint)index, out result));
			return result;
		}

		public PropVariant GetValue(PropertyKey key)
		{
			if (Contains(key))
			{
				PropVariant result;
				HResult.Try(_propertyStoreInterface.GetValue(ref key, out result));
				return result;
			}
			else
			{
				return PropVariant.Empty;
			}
		}

		public void SetValue(KeyValuePair<PropertyKey, PropVariant> value)
		{
			if (Contains(value.Key))
			{
				var key       = value.Key;
				var propValue = value.Value;
				HResult.Try(_propertyStoreInterface.SetValue(ref key, ref propValue));
				propValue.Dispose();
			}
			else
			{
				throw new KeyNotFoundException(String.Format("The specified key ({0}) was not found.", value.Key.ToString()));
			}
		}

		public void Commit()
		{
			HResult.Try(_propertyStoreInterface.Commit());
		}
		#endregion

		#region Private Methods
		private int GetCount()
		{
			uint count;
			HResult.Try(_propertyStoreInterface.GetCount(out count));
			return (int)count;
		}
		#endregion

		#region IEnumerable<PropVariant> Implementation
		public IEnumerator<KeyValuePair<PropertyKey, PropVariant>> GetEnumerator()
		{
			for (var i = 0; i < Count; i++)
			{
				var key   = GetAt(i);
				var value = GetValue(key);
				yield return new KeyValuePair<PropertyKey, PropVariant>(key, value);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region IDisposable Implementation
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				// No managed disposables
			}

			if (_propertyStoreInterface != null)
			{
				Marshal.ReleaseComObject(_propertyStoreInterface);
				_propertyStoreInterface = null;
			}
		}

		~PropertyStore()
		{
			Dispose(false);
		}
		#endregion
	}
}