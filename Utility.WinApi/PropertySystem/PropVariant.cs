// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       PropVariant.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:31 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using JLR.Utility.WinApi.Error;

namespace JLR.Utility.WinApi.PropertySystem
{
	#region Enumerated Types
	public enum StringType
	{
		Ansi,
		Bstr,
		Unicode
	}

	public enum ClipboardFormat : int
	{
		CF_FMTID                      = -3,
		CF_Macintosh                  = -2,
		CF_Windows                    = -1,
		None                          = 0,
		Text                          = 1,
		Bitmap                        = 2,
		WindowsMetafile               = 3,
		SYLK                          = 4,
		DIF                           = 5,
		TIFF                          = 6,
		OEMText                       = 7,
		DIB                           = 8,
		Palette                       = 9,
		PenData                       = 10,
		RIFF                          = 11,
		WAVE                          = 12,
		UnicodeText                   = 13,
		EnhancedMetafile              = 14,
		HDROP                         = 15,
		Locale                        = 16,
		DibV5                         = 17,
		DisplayFormatOwner            = 0x0080,
		DisplayFormatText             = 0x0081,
		DisplayFormatBitmap           = 0x0082,
		DisplayFormatWindowsMetafile  = 0x0083,
		DisplayFormatEnhancedMetafile = 0x008E
	}

	public enum VarType : ushort
	{
		EMPTY           = 0,      //				
		NULL            = 1,      //				
		I2              = 2,      //	V	A	R	
		I4              = 3,      //	V	A	R	
		R4              = 4,      //	V	A	R	
		R8              = 5,      //	V	A	R	
		CY              = 6,      //	V	A	R	
		Date            = 7,      //	V	A	R	
		BSTR            = 8,      //	V	A	R	
		Dispatch        = 9,      //		A	R	
		Error           = 10,     //	V	A	R	
		Bool            = 11,     //	V	A	R	
		Variant         = 12,     //	V	A	R	
		Unknown         = 13,     //		A	R	
		Decimal         = 14,     //		A	R	
		I1              = 16,     //	V	A	R	
		UI1             = 17,     //	V	A	R	
		UI2             = 18,     //	V	A	R	
		UI4             = 19,     //	V	A	R	
		I8              = 20,     //	V			
		UI8             = 21,     //	V			
		Int             = 22,     //		A	R	
		UInt            = 23,     //		A	R	
		Void            = 24,     //				Not used in PROPVARIANT
		HRESULT         = 25,     //				Not used in PROPVARIANT
		Pointer         = 26,     //				Not used in PROPVARIANT
		SafeArray       = 27,     //				Not used in PROPVARIANT
		CArray          = 28,     //				Not used in PROPVARIANT
		UserDefined     = 29,     //				Not used in PROPVARIANT
		LPSTR           = 30,     //	V			
		LPWSTR          = 31,     //	V			
		Record          = 36,     //				Not used in PROPVARIANT
		IntPtr          = 37,     //				Not used in PROPVARIANT
		UIntPtr         = 38,     //				Not used in PROPVARIANT
		FileTime        = 64,     //	V			
		Blob            = 65,     //				
		Stream          = 66,     //				
		Storage         = 67,     //				
		StreamedObject  = 68,     //				
		StoredObject    = 69,     //				
		BlobObject      = 70,     //				
		CF              = 71,     //	V			
		CLSID           = 72,     //	V			
		VersionedStream = 73,     //				
		Flag_Vector     = 0x1000, //
		Flag_Array      = 0x2000, //
		Flag_ByRef      = 0x4000, //
		Flag_TypeMask   = 0x0FFF, //
		Flag_FlagMask   = 0xF000  //
	}
	#endregion

	#region Structures
	[StructLayout(LayoutKind.Sequential)]
	public struct ClipData
	{
		public ClipboardFormat Format { get; internal set; }
		public byte[]          Data   { get; internal set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SafeArrayBound
	{
		public uint NumElements { get; }
		public int  LowerBound  { get; }

		public SafeArrayBound(int numElements, int lowerBound)
		{
			NumElements = (uint)numElements;
			LowerBound  = lowerBound;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct PropVariantDataUnion
	{
		public IntPtr Pointer1;
		public IntPtr Pointer2;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct PropVariantHandleUnion
	{
		private IntPtr   placeholder;
		public  GCHandle handle;
	}
	#endregion

	[StructLayout(LayoutKind.Explicit)]
	public struct PropVariant : IDisposable, IFormattable
	{
		#region Fields
		// Reserved
		[FieldOffset(2)] private ushort reserved1;

		[FieldOffset(4)] private ushort reserved2;

		[FieldOffset(6)] private ushort reserved3;

		// Value Type
		[FieldOffset(0)] private VarType varType;

		// Pointers to Value Data
		[FieldOffset(8)] private PropVariantHandleUnion handleUnion;

		[FieldOffset(8)] private PropVariantDataUnion pointerUnion;

		// Value Data (Specific)
		[FieldOffset(8)] private sbyte data_i1;

		[FieldOffset(8)] private byte data_ui1;

		[FieldOffset(8)] private short data_i2;

		[FieldOffset(8)] private ushort data_ui2;

		[FieldOffset(8)] private int data_i4;

		[FieldOffset(8)] private uint data_ui4;

		[FieldOffset(8)] private long data_i8;

		[FieldOffset(8)] private ulong data_ui8;

		[FieldOffset(8)] private float data_r4;

		[FieldOffset(8)] private double data_r8;

		[FieldOffset(0)] private decimal data_decimal;
		#endregion

		#region Properties
		public VarType VarType       => varType;
		public bool    IsNullOrEmpty => (varType == VarType.EMPTY || varType == VarType.NULL);
		public object  Value         => GetValue();
		#endregion

		#region Static Properties
		public static PropVariant Empty => new PropVariant();
		#endregion

		#region Public Methods
		public List<VarType> GetVarTypes()
		{
			var result = new List<VarType>();
			if ((varType & VarType.Flag_FlagMask) != VarType.EMPTY)
				result.Add(varType & VarType.Flag_FlagMask);
			result.Add(varType & VarType.Flag_TypeMask);
			return result;
		}
		#endregion

		#region Private Methods
		private T[] GetVector<T>()
		{
			var result = new T[data_i4];
			var size   = Marshal.SizeOf(typeof(T));
			for (var i = 0; i < data_i4; i++)
			{
				result[i] = (T)Marshal.PtrToStructure(IntPtr.Add(pointerUnion.Pointer2, i * size), typeof(T));
			}

			return result;
		}

		private void SetVector<T>(IReadOnlyList<T> value, VarType elementType)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var size = Marshal.SizeOf(typeof(T));
			varType               = VarType.Flag_Vector | elementType;
			data_i4               = value.Count;
			pointerUnion.Pointer2 = Marshal.AllocCoTaskMem(value.Count * size);
			for (var i = 0; i < value.Count; i++)
			{
				Marshal.StructureToPtr(value[i], IntPtr.Add(pointerUnion.Pointer2, i * size), false);
			}
		}

		private T[] GetSafeArray<T>()
		{
			var dimCount = (uint)HResult.Try(NativeMethods.OleAut32.SafeArrayGetDim(pointerUnion.Pointer1)).SuccessValue;
			if (dimCount > 1)
				throw new NotSupportedException("Multidimensional SafeArrays are not supported.");

			int lBound, uBound;
			HResult.Try(NativeMethods.OleAut32.SafeArrayGetLBound(pointerUnion.Pointer1, dimCount, out lBound));
			HResult.Try(NativeMethods.OleAut32.SafeArrayGetUBound(pointerUnion.Pointer1, dimCount, out uBound));
			var elemCount = uBound - lBound + 1;

			var result = new T[elemCount];
			var index  = new int[1];
			for (var i = 0; i < elemCount; i++)
			{
				index[0] = lBound + i;
				object element;
				HResult.Try(NativeMethods.OleAut32.SafeArrayGetElement(pointerUnion.Pointer1, index, out element));
				result[i] = (T)element;
			}

			return result;
		}

		private void SetSafeArray(Array value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (value.Rank > 1)
				throw new NotSupportedException("Multidimensional arrays are not supported.");

			var bounds = new SafeArrayBound[1];
			bounds[0] = new SafeArrayBound(value.GetLength(0), value.GetLowerBound(0));
			var    arrayPtr = NativeMethods.OleAut32.SafeArrayCreate(VarType.Unknown, 1, bounds);
			IntPtr arrayDataPtr;
			HResult.Try(NativeMethods.OleAut32.SafeArrayAccessData(arrayPtr, out arrayDataPtr));

			try
			{
				for (var i = bounds[0].LowerBound; i < bounds[0].LowerBound + bounds[0].NumElements; i++)
				{
					var element    = value.GetValue(i);
					var elementPtr = (element != null) ? Marshal.GetIUnknownForObject(element) : IntPtr.Zero;
					Marshal.WriteIntPtr(arrayDataPtr, i * IntPtr.Size, elementPtr);
				}
			}
			finally
			{
				HResult.Try(NativeMethods.OleAut32.SafeArrayUnaccessData(arrayPtr));
			}

			varType               = VarType.Flag_Array | VarType.Unknown;
			pointerUnion.Pointer1 = arrayPtr;
		}

		private T GetByRefValue<T>()
		{
			if (handleUnion.handle.IsAllocated)
				return (T)handleUnion.handle.Target;
			else
			{
				return default(T);
			}
		}

		private void SetByRefValue<T>(T value, VarType valueType)
		{
			SetByRefValue(ref value, valueType);
		}

		private void SetByRefValue<T>(ref T value, VarType valueType)
		{
			handleUnion.handle    = GCHandle.Alloc(value, GCHandleType.Pinned);
			pointerUnion.Pointer1 = handleUnion.handle.AddrOfPinnedObject();
			varType               = VarType.Flag_ByRef | valueType;
		}

		private byte[] GetBlobData()
		{
			var result = new byte[data_ui4];
			Marshal.Copy(pointerUnion.Pointer2, result, 0, result.Length);
			return result;
		}

		private void SetBlobData(byte[] value)
		{
			varType               = VarType.Blob;
			data_ui4              = (uint)value.Length;
			pointerUnion.Pointer2 = Marshal.AllocCoTaskMem(value.Length);
			Marshal.Copy(value, 0, pointerUnion.Pointer2, value.Length);
		}

		private ClipData GetClipData(IntPtr ptr)
		{
			var result = new ClipData();
			var size   = (uint)Marshal.ReadInt32(ptr);
			result.Format = (ClipboardFormat)Marshal.ReadInt32(IntPtr.Add(ptr, sizeof(uint)));
			Marshal.Copy(IntPtr.Add(ptr, sizeof(uint) + sizeof(int)), result.Data, 0, (int)(size - sizeof(int)));
			return result;
		}

		private void SetClipData(ClipData value, IntPtr ptr)
		{
			var size = (uint)value.Data.Length + sizeof(int);
			ptr = Marshal.AllocCoTaskMem((int)size + sizeof(uint));
			Marshal.WriteInt32(ptr,                                    (int)size);
			Marshal.WriteInt32(IntPtr.Add(ptr,          sizeof(uint)), (int)value.Format);
			Marshal.Copy(value.Data, 0, IntPtr.Add(ptr, sizeof(uint) + sizeof(int)), value.Data.Length);
		}
		#endregion

		#region GetValue Method
		private object GetValue()
		{
			switch (varType)
			{
				// Single Types                                                                        
				case VarType.EMPTY:
				case VarType.NULL:
					return null;
				case VarType.I1:
					return data_i1;
				case VarType.UI1:
					return data_ui1;
				case VarType.I2:
					return data_i2;
				case VarType.UI2:
					return data_ui2;
				case VarType.I4:
				case VarType.Int:
					return data_i4;
				case VarType.UI4:
				case VarType.UInt:
					return data_ui4;
				case VarType.I8:
					return data_i8;
				case VarType.UI8:
					return data_ui8;
				case VarType.R4:
					return data_r4;
				case VarType.R8:
					return data_r8;
				case VarType.Decimal:
					return data_decimal;
				case VarType.Bool:
					return data_i2 != 0;
				case VarType.Error:
					return new HResult(data_ui4);
				case VarType.CY:
					return decimal.FromOACurrency(data_i8);
				case VarType.Date:
					return DateTime.FromOADate(data_r8);
				case VarType.FileTime:
					return DateTime.FromFileTime(data_i8);
				case VarType.CLSID:
					return (Guid)Marshal.PtrToStructure(pointerUnion.Pointer1, typeof(Guid));
				case VarType.CF:
					return GetClipData(pointerUnion.Pointer1);
				case VarType.BSTR:
					return Marshal.PtrToStringBSTR(pointerUnion.Pointer1);
				case VarType.Blob:
					return GetBlobData();
				case VarType.BlobObject: // ?
					goto default;
				case VarType.LPSTR:
					return Marshal.PtrToStringAnsi(pointerUnion.Pointer1);
				case VarType.LPWSTR:
					return Marshal.PtrToStringUni(pointerUnion.Pointer1);
				case VarType.Unknown:
					return Marshal.GetObjectForIUnknown(pointerUnion.Pointer1);
				case VarType.Dispatch:        // ?
				case VarType.Stream:          // ?
				case VarType.StreamedObject:  // ?
				case VarType.Storage:         // ?
				case VarType.StoredObject:    // ?
				case VarType.VersionedStream: // ?
					goto default;

				// SafeArray                                                                           
				case VarType.Flag_Array | VarType.I1:
					return GetSafeArray<sbyte>();
				case VarType.Flag_Array | VarType.UI1:
					return GetSafeArray<byte>();
				case VarType.Flag_Array | VarType.I2:
					return GetSafeArray<short>();
				case VarType.Flag_Array | VarType.UI2:
					return GetSafeArray<ushort>();
				case VarType.Flag_Array | VarType.I4:
				case VarType.Flag_Array | VarType.Int:
					return GetSafeArray<int>();
				case VarType.Flag_Array | VarType.UI4:
				case VarType.Flag_Array | VarType.UInt:
					return GetSafeArray<uint>();
				case VarType.Flag_Array | VarType.R4:
					return GetSafeArray<float>();
				case VarType.Flag_Array | VarType.R8:
					return GetSafeArray<double>();
				case VarType.Flag_Array | VarType.Decimal:
					return GetSafeArray<decimal>();
				case VarType.Flag_Array | VarType.Bool:
				{
					var temp   = GetSafeArray<short>();
					var result = new bool[temp.Length];
					for (var i = 0; i < temp.Length; i++)
					{
						result[i] = temp[i] != 0;
					}

					return result;
				}
				case VarType.Flag_Array | VarType.Error:
					return GetSafeArray<HResult>();
				case VarType.Flag_Array | VarType.CY:
				{
					var temp   = GetSafeArray<long>();
					var result = new decimal[temp.Length];
					for (var i = 0; i < temp.Length; i++)
					{
						result[i] = decimal.FromOACurrency(temp[i]);
					}

					return result;
				}
				case VarType.Flag_Array | VarType.Date:
				{
					var temp   = GetSafeArray<double>();
					var result = new DateTime[temp.Length];
					for (var i = 0; i < temp.Length; i++)
					{
						result[i] = DateTime.FromOADate(temp[i]);
					}

					return result;
				}
				case VarType.Flag_Array | VarType.BSTR:
					return GetSafeArray<string>();
				case VarType.Flag_Array | VarType.Unknown:
					return GetSafeArray<object>();
				case VarType.Flag_Array | VarType.Dispatch: // ?
					goto default;
				case VarType.Flag_Array | VarType.Variant:
					return GetSafeArray<PropVariant>();

				// Vector                                                                              
				case VarType.Flag_Vector | VarType.I1:
					return GetVector<sbyte>();
				case VarType.Flag_Vector | VarType.UI1:
					return GetVector<byte>();
				case VarType.Flag_Vector | VarType.I2:
					return GetVector<short>();
				case VarType.Flag_Vector | VarType.UI2:
					return GetVector<ushort>();
				case VarType.Flag_Vector | VarType.I4:
					return GetVector<int>();
				case VarType.Flag_Vector | VarType.UI4:
					return GetVector<uint>();
				case VarType.Flag_Vector | VarType.I8:
					return GetVector<long>();
				case VarType.Flag_Vector | VarType.UI8:
					return GetVector<ulong>();
				case VarType.Flag_Vector | VarType.R4:
					return GetVector<float>();
				case VarType.Flag_Vector | VarType.R8:
					return GetVector<double>();
				case VarType.Flag_Vector | VarType.Bool:
				{
					var temp   = GetVector<short>();
					var result = new bool[data_i4];
					for (var i = 0; i < temp.Length; i++)
					{
						result[i] = temp[i] == 0 ? false : true;
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.Error:
					return GetVector<HResult>();
				case VarType.Flag_Vector | VarType.CY:
				{
					var temp   = GetVector<long>();
					var result = new decimal[data_i4];
					for (var i = 0; i < temp.Length; i++)
					{
						result[i] = decimal.FromOACurrency(temp[i]);
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.Date:
				{
					var temp   = GetVector<double>();
					var result = new DateTime[data_i4];
					for (var i = 0; i < temp.Length; i++)
					{
						result[i] = DateTime.FromOADate(temp[i]);
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.FileTime:
				{
					var temp   = GetVector<long>();
					var result = new DateTime[data_i4];
					for (var i = 0; i < temp.Length; i++)
					{
						result[i] = DateTime.FromFileTime(temp[i]);
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.CLSID:
					return GetVector<Guid>();
				case VarType.Flag_Vector | VarType.CF:
				{
					var resultPtrs = new IntPtr[data_i4];
					var result     = new ClipData[data_i4];
					Marshal.Copy(pointerUnion.Pointer2, resultPtrs, 0, data_i4);
					for (var i = 0; i < resultPtrs.Length; i++)
					{
						result[i] = GetClipData(resultPtrs[i]);
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.BSTR:
				{
					var resultPtrs = new IntPtr[data_i4];
					var result     = new string[data_i4];
					Marshal.Copy(pointerUnion.Pointer2, resultPtrs, 0, data_i4);
					for (var i = 0; i < resultPtrs.Length; i++)
					{
						result[i] = Marshal.PtrToStringBSTR(resultPtrs[i]);
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.LPSTR:
				{
					var resultPtrs = new IntPtr[data_i4];
					var result     = new string[data_i4];
					Marshal.Copy(pointerUnion.Pointer2, resultPtrs, 0, data_i4);
					for (var i = 0; i < resultPtrs.Length; i++)
					{
						result[i] = Marshal.PtrToStringAnsi(resultPtrs[i]);
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.LPWSTR:
				{
					var resultPtrs = new IntPtr[data_i4];
					var result     = new string[data_i4];
					Marshal.Copy(pointerUnion.Pointer2, resultPtrs, 0, data_i4);
					for (var i = 0; i < resultPtrs.Length; i++)
					{
						result[i] = Marshal.PtrToStringUni(resultPtrs[i]);
					}

					return result;
				}
				case VarType.Flag_Vector | VarType.Variant:
					return GetVector<PropVariant>();

				// ByRef                                                                               
				case VarType.Flag_ByRef | VarType.I1:
					return GetByRefValue<sbyte>();
				case VarType.Flag_ByRef | VarType.UI1:
					return GetByRefValue<byte>();
				case VarType.Flag_ByRef | VarType.I2:
					return GetByRefValue<short>();
				case VarType.Flag_ByRef | VarType.UI2:
					return GetByRefValue<ushort>();
				case VarType.Flag_ByRef | VarType.I4:
				case VarType.Flag_ByRef | VarType.Int:
					return GetByRefValue<int>();
				case VarType.Flag_ByRef | VarType.UI4:
				case VarType.Flag_ByRef | VarType.UInt:
					return GetByRefValue<uint>();
				case VarType.Flag_ByRef | VarType.R4:
					return GetByRefValue<float>();
				case VarType.Flag_ByRef | VarType.R8:
					return GetByRefValue<double>();
				case VarType.Flag_ByRef | VarType.Bool:
					return GetByRefValue<short>() != 0;
				case VarType.Flag_ByRef | VarType.Error:
					return GetByRefValue<HResult>();
				case VarType.Flag_ByRef | VarType.CY:
					return decimal.FromOACurrency(GetByRefValue<long>());
				case VarType.Flag_ByRef | VarType.Date:
					return DateTime.FromOADate(GetByRefValue<double>());
				case VarType.Flag_ByRef | VarType.BSTR:
					return Marshal.PtrToStringBSTR(GetByRefValue<IntPtr>());
				case VarType.Flag_ByRef | VarType.Unknown:
					return Marshal.GetObjectForIUnknown(GetByRefValue<IntPtr>());
				case VarType.Flag_ByRef | VarType.Dispatch: // ?
					goto default;
				case VarType.Flag_ByRef | VarType.Decimal:
					return GetByRefValue<decimal>();
				case VarType.Flag_ByRef | VarType.Variant:
					return GetByRefValue<PropVariant>();
				case VarType.Flag_ByRef | VarType.Flag_Array:
					return GetSafeArray<Array>();

				// All other VarType combinations                                                      
				default:
				{
					var message = new StringBuilder();
					message.Append("The specified VarType (");
					var varTypes = GetVarTypes();
					for (var i = 0; i < varTypes.Count; i++)
					{
						message.Append(Enum.GetName(typeof(VarType), varTypes[i]));
						if (i < varTypes.Count - 1)
							message.Append(" | ");
					}

					message.Append(") is not supported by the managed PropVariant.");
					throw new NotSupportedException(message.ToString());
				}
			}
		}
		#endregion

		#region Factory Methods
		public static PropVariant Create(sbyte value)
		{
			var result = new PropVariant { data_i1 = value, varType = VarType.I1 };
			return result;
		}

		public static PropVariant Create(ref sbyte value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.I1);
			return result;
		}

		public static PropVariant Create(sbyte[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.I1);
			return result;
		}

		public static PropVariant Create(byte value)
		{
			var result = new PropVariant { data_ui1 = value, varType = VarType.UI1 };
			return result;
		}

		public static PropVariant Create(ref byte value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.UI1);
			return result;
		}

		public static PropVariant Create(byte[] value, bool isBlob)
		{
			var result = new PropVariant();
			if (isBlob)
			{
				result.SetBlobData(value);
			}
			else
			{
				result.SetVector(value, VarType.UI1);
			}

			return result;
		}

		public static PropVariant Create(short value)
		{
			var result = new PropVariant { data_i2 = value, varType = VarType.I2 };
			return result;
		}

		public static PropVariant Create(ref short value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.I2);
			return result;
		}

		public static PropVariant Create(short[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.I2);
			return result;
		}

		public static PropVariant Create(ushort value)
		{
			var result = new PropVariant { data_ui2 = value, varType = VarType.UI2 };
			return result;
		}

		public static PropVariant Create(ref ushort value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.UI2);
			return result;
		}

		public static PropVariant Create(ushort[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.UI2);
			return result;
		}

		public static PropVariant Create(int value)
		{
			var result = new PropVariant { data_i4 = value, varType = VarType.I4 };
			return result;
		}

		public static PropVariant Create(ref int value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.I4);
			return result;
		}

		public static PropVariant Create(int[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.I4);
			return result;
		}

		public static PropVariant Create(uint value)
		{
			var result = new PropVariant { data_ui4 = value, varType = VarType.UI4 };
			return result;
		}

		public static PropVariant Create(ref uint value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.UI4);
			return result;
		}

		public static PropVariant Create(uint[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.UI4);
			return result;
		}

		public static PropVariant Create(long value)
		{
			var result = new PropVariant { data_i8 = value, varType = VarType.I8 };
			return result;
		}

		public static PropVariant Create(long[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.I8);
			return result;
		}

		public static PropVariant Create(ulong value)
		{
			var result = new PropVariant { data_ui8 = value, varType = VarType.UI8 };
			return result;
		}

		public static PropVariant Create(ulong[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.UI8);
			return result;
		}

		public static PropVariant Create(float value)
		{
			var result = new PropVariant { data_r4 = value, varType = VarType.R4 };
			return result;
		}

		public static PropVariant Create(ref float value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.R4);
			return result;
		}

		public static PropVariant Create(float[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.R4);
			return result;
		}

		public static PropVariant Create(double value)
		{
			var result = new PropVariant { data_r8 = value, varType = VarType.R8 };
			return result;
		}

		public static PropVariant Create(ref double value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.R8);
			return result;
		}

		public static PropVariant Create(double[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.R8);
			return result;
		}

		public static PropVariant Create(decimal value, bool isCurrency)
		{
			var result = new PropVariant();
			if (isCurrency)
			{
				result.data_i8 = decimal.ToOACurrency(value);
				result.varType = VarType.CY;
			}
			else
			{
				result.data_decimal = value;
				result.varType      = VarType.Decimal;
			}

			return result;
		}

		public static PropVariant Create(ref decimal value, bool isCurrency)
		{
			var result = new PropVariant();
			if (isCurrency)
			{
				result.SetByRefValue(decimal.ToOACurrency(value), VarType.CY);
			}
			else
			{
				result.SetByRefValue(ref value, VarType.Decimal);
			}

			return result;
		}

		public static PropVariant Create(decimal[] value, bool isCurrency)
		{
			var result = new PropVariant();
			if (isCurrency)
			{
				var temp = new long[value.Length];
				for (var i = 0; i < value.Length; i++)
				{
					temp[i] = decimal.ToOACurrency(value[i]);
				}

				result.SetVector(temp, VarType.CY);
			}
			else
			{
				result.SetVector(value, VarType.Decimal);
			}

			return result;
		}

		public static PropVariant Create(bool value)
		{
			var result = new PropVariant { data_i2 = value ? (short)-1 : (short)0, varType = VarType.Bool };
			return result;
		}

		public static PropVariant Create(ref bool value)
		{
			var result = new PropVariant();
			result.SetByRefValue(value ? (short)-1 : (short)0, VarType.Bool);
			return result;
		}

		public static PropVariant Create(bool[] value)
		{
			var result = new PropVariant();
			var temp   = new short[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				temp[i] = value[i] ? (short)-1 : (short)0;
			}

			result.SetVector(temp, VarType.Bool);
			return result;
		}

		public static PropVariant Create(HResult value)
		{
			var result = new PropVariant { data_ui4 = value, varType = VarType.Error };
			return result;
		}

		public static PropVariant Create(ref HResult value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.Error);
			return result;
		}

		public static PropVariant Create(HResult[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.Error);
			return result;
		}

		public static PropVariant Create(DateTime value, bool isFileTime)
		{
			var result = new PropVariant();
			if (isFileTime)
			{
				result.data_r8 = value.ToOADate();
				result.varType = VarType.Date;
			}
			else
			{
				result.data_i8 = value.ToFileTime();
				result.varType = VarType.FileTime;
			}

			return result;
		}

		public static PropVariant Create(ref DateTime value)
		{
			var result = new PropVariant();
			result.SetByRefValue(value.ToOADate(), VarType.Date);
			return result;
		}

		public static PropVariant Create(DateTime[] value, bool isFileTime)
		{
			var result = new PropVariant();
			if (isFileTime)
			{
				var temp = new double[value.Length];
				for (var i = 0; i < value.Length; i++)
				{
					temp[i] = value[i].ToOADate();
				}

				result.SetVector(temp, VarType.Date);
			}
			else
			{
				var temp = new long[value.Length];
				for (var i = 0; i < value.Length; i++)
				{
					temp[i] = value[i].ToFileTime();
				}

				result.SetVector(temp, VarType.FileTime);
			}

			return result;
		}

		public static PropVariant Create(Guid value)
		{
			var result = new PropVariant { pointerUnion = { Pointer1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(value)) } };
			Marshal.StructureToPtr(value, result.pointerUnion.Pointer1, false);
			result.varType = VarType.CLSID;
			return result;
		}

		public static PropVariant Create(Guid[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.CLSID);
			return result;
		}

		public static PropVariant Create(ClipData value)
		{
			var result = new PropVariant();
			result.SetClipData(value, result.pointerUnion.Pointer1);
			result.varType = VarType.CF;
			return result;
		}

		public static PropVariant Create(string value, StringType stringType)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var result = new PropVariant();
			switch (stringType)
			{
				case StringType.Ansi:
				{
					result.pointerUnion.Pointer1 = Marshal.StringToCoTaskMemAnsi(value);
					result.varType               = VarType.LPSTR;
					break;
				}
				case StringType.Bstr:
				{
					result.pointerUnion.Pointer1 = Marshal.StringToBSTR(value);
					result.varType               = VarType.BSTR;
					break;
				}
				case StringType.Unicode:
				{
					result.pointerUnion.Pointer1 = Marshal.StringToCoTaskMemUni(value);
					result.varType               = VarType.LPWSTR;
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(stringType), stringType, null);
			}

			return result;
		}

		public static PropVariant Create(ref string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var result = new PropVariant();
			result.SetByRefValue(Marshal.StringToBSTR(value), VarType.BSTR);
			return result;
		}

		public static PropVariant Create(string[] value, StringType stringType)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var result = new PropVariant();
			switch (stringType)
			{
				case StringType.Ansi:
				{
					var resultPtrs = new IntPtr[value.Length];
					for (var i = 0; i < value.Length; i++)
					{
						resultPtrs[i] = Marshal.StringToCoTaskMemAnsi(value[i]);
					}

					result.SetVector(resultPtrs, VarType.LPSTR);
					break;
				}
				case StringType.Bstr:
				{
					var resultPtrs = new IntPtr[value.Length];
					for (var i = 0; i < value.Length; i++)
					{
						resultPtrs[i] = Marshal.StringToBSTR(value[i]);
					}

					result.SetVector(resultPtrs, VarType.BSTR);
					break;
				}
				case StringType.Unicode:
				{
					var resultPtrs = new IntPtr[value.Length];
					for (var i = 0; i < value.Length; i++)
					{
						resultPtrs[i] = Marshal.StringToCoTaskMemUni(value[i]);
					}

					result.SetVector(resultPtrs, VarType.LPWSTR);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(stringType), stringType, null);
			}

			return result;
		}

		public static PropVariant Create(object value, ComInterfaceType interfaceType = ComInterfaceType.InterfaceIsIUnknown)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var result = new PropVariant();
			switch (interfaceType)
			{
				case ComInterfaceType.InterfaceIsIDispatch:
					result.pointerUnion.Pointer1 = Marshal.GetIDispatchForObject(value);
					result.varType               = VarType.Dispatch;
					break;
				case ComInterfaceType.InterfaceIsIUnknown:
					result.pointerUnion.Pointer1 = Marshal.GetIUnknownForObject(value);
					result.varType               = VarType.Unknown;
					break;
				default:
					throw new NotSupportedException("Only IDispatch and IUnknown objects are supported.");
			}

			return result;
		}

		public static PropVariant Create(ref object value)
		{
			return Create(ref value, ComInterfaceType.InterfaceIsIUnknown);
		}

		public static PropVariant Create(ref object value, ComInterfaceType interfaceType)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			var result = new PropVariant();
			switch (interfaceType)
			{
				case ComInterfaceType.InterfaceIsIDispatch:
					result.SetByRefValue(Marshal.GetIDispatchForObject(value), VarType.Dispatch);
					break;
				case ComInterfaceType.InterfaceIsIUnknown:
					result.SetByRefValue(Marshal.GetIUnknownForObject(value), VarType.Unknown);
					break;
				default:
					throw new NotSupportedException("Only IDispatch and IUnknown objects are supported.");
			}

			return result;
		}

		public static PropVariant Create(ref PropVariant value)
		{
			var result = new PropVariant();
			result.SetByRefValue(ref value, VarType.Variant);
			return result;
		}

		public static PropVariant Create(PropVariant[] value)
		{
			var result = new PropVariant();
			result.SetVector(value, VarType.Variant);
			return result;
		}

		public static PropVariant Create(Array value)
		{
			var result = new PropVariant();
			result.SetSafeArray(value);
			return result;
		}
		#endregion

		#region IFormattable Implementation
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (String.IsNullOrEmpty(format))
				return String.Empty;

			var str      = new StringBuilder();
			var fmtT     = new StringBuilder();
			var fmtV     = new StringBuilder();
			var isEscape = false;
			var isModeT  = false;
			var isModeV  = false;

			foreach (var ch in format)
			{
				if (ch == '`')
				{
					isEscape = true;
				}
				else if (ch == 'T' && isEscape)
				{
					if (isModeT)
					{
						var strT     = new StringBuilder();
						var varTypes = GetVarTypes();
						for (var i = 0; i < varTypes.Count; i++)
						{
							strT.Append(Enum.GetName(typeof(VarType), varTypes[i]));
							if (i < varTypes.Count - 1)
								strT.Append(fmtT);
						}

						str.Append(strT);
						isModeT = false;
					}
					else
					{
						fmtT    = new StringBuilder();
						isModeT = true;
					}

					isEscape = false;
				}
				else if (ch == 'V' && isEscape)
				{
					if (isModeV)
					{
						var strV = new StringBuilder();
						if (Value is Array)
						{
							var valArray = Value as Array;
							for (var i = 0; i < valArray.Length; i++)
							{
								strV.Append(valArray.GetValue(i));
								if (i < valArray.Length - 1)
									strV.Append(fmtV);
							}
						}
						else
						{
							strV.Append(Value);
						}

						str.Append(strV);
						isModeV = false;
					}
					else
					{
						fmtV    = new StringBuilder();
						isModeV = true;
					}

					isEscape = false;
				}
				else
				{
					if (isModeT)
					{
						fmtT.Append(ch);
					}
					else if (isModeV)
					{
						fmtV.Append(ch);
					}
					else
					{
						str.Append(ch);
					}
				}
			}

			return str.ToString();
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return ToString("(`T | `T) = `V, `V", null);
		}
		#endregion

		#region IDisposable Implementation
		public void Dispose()
		{
			// Free pinned managed resources
			if ((varType & VarType.Flag_FlagMask) == VarType.Flag_ByRef)
			{
				if (handleUnion.handle.IsAllocated)
					handleUnion.handle.Free();
			}

			// Free native resources
			unsafe
			{
				fixed (PropVariant* thisPtr = &this)
				{
					HResult.Try(NativeMethods.Ole32.PropVariantClear(thisPtr));
				}
			}

			// Suppress finalization
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}