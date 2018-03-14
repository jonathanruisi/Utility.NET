// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       NativeMethods.cs
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
using System.Runtime.InteropServices;

namespace JLR.Utility.WinApi.PropertySystem
{
	internal static class NativeMethods
	{
		#region Ole32.dll
		internal static class Ole32
		{
			[DllImport("ole32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int FreePropVariantArray(uint numElements, [In] ref PropVariant propertyVariantArray);

			[DllImport("ole32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern unsafe int PropVariantClear(void* propVariant);

			[DllImport("ole32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantCopy(ref PropVariant destination, [In] ref PropVariant source);
		}
		#endregion

		#region OleAut32.dll
		internal static class OleAut32
		{
			[DllImport("OleAut32.dll", PreserveSig = true)]
			internal static extern IntPtr SafeArrayCreate(VarType varType, uint numDimensions, [In] SafeArrayBound[] bounds);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayDestroy(IntPtr arrayPointer);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			internal static extern IntPtr SafeArrayCreateVector(VarType varType, int lowerBound, uint numElements);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayAccessData(IntPtr arrayPointer, out IntPtr arrayDataPointer);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayUnaccessData(IntPtr arrayPointer);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			internal static extern uint SafeArrayGetDim(IntPtr arrayPointer);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayGetLBound(IntPtr arrayPointer, uint dimNumber, out int lowerBound);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayGetUBound(IntPtr arrayPointer, uint dimNumber, out int upperBound);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayGetElement(IntPtr arrayPointer,
														   int[] indices,
														   [MarshalAs(UnmanagedType.IUnknown)] out object element);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayPutElement(IntPtr arrayPointer,
														   int[] indices,
														   [MarshalAs(UnmanagedType.IUnknown)] ref object element);

			[DllImport("OleAut32.dll", PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int SafeArrayGetVartype(IntPtr arrayPointer, out VarType varType);
		}
		#endregion

		#region propsys.dll
		internal static class Propsys
		{
			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.I4)]
			internal static extern uint PropVariantGetElementCount([In] ref PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetBooleanElem(PropVariant propVariant,
																 uint elementIndex,
																 [MarshalAs(UnmanagedType.Bool)] out bool value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetInt16Elem(PropVariant propVariant, uint elementIndex, out short value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetUInt16Elem(PropVariant propVariant, uint elementIndex, out ushort value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetInt32Elem(PropVariant propVariant, uint elementIndex, out int value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetUInt32Elem(PropVariant propVariant, uint elementIndex, out uint value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetInt64Elem(PropVariant propVariant, uint elementIndex, out long value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetUInt64Elem(PropVariant propVariant, uint elementIndex, out ulong value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetDoubleElem(PropVariant propVariant, uint elementIndex, out double value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetFileTimeElem(PropVariant propVariant,
																  uint elementIndex,
																  [MarshalAs(UnmanagedType.Struct)] out System.Runtime.InteropServices.ComTypes.FILETIME value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int PropVariantGetStringElem(PropVariant propVariant,
																uint elementIndex,
																[MarshalAs(UnmanagedType.LPWStr)] ref string value);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromFileTime(ref System.Runtime.InteropServices.ComTypes.FILETIME value,
																   [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromBooleanVector(
				[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool)]
				bool[] source,
				uint numElements,
				[Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromInt16Vector(short[] source,
																	  uint numElements,
																	  [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromUInt16Vector(ushort[] source,
																	   uint numElements,
																	   [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromInt32Vector(int[] source,
																	  uint numElements,
																	  [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromUInt32Vector(uint[] source,
																	   uint numElements,
																	   [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromInt64Vector(long[] source,
																	  uint numElements,
																	  [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromUInt64Vector(ulong[] source,
																	   uint numElements,
																	   [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromDoubleVector(double[] source,
																	   uint numElements,
																	   [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromFileTimeVector(
				System.Runtime.InteropServices.ComTypes.FILETIME[] source,
				uint numElements,
				[Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromStringVector(string[] source,
																	   uint numElements,
																	   [Out] PropVariant propVariant);

			[DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = true)]
			[return: MarshalAs(UnmanagedType.Error)]
			internal static extern int InitPropVariantFromPropVariantVectorElem(PropVariant source,
																				uint elementIndex,
																				[Out] PropVariant destination);
		}
		#endregion
	}
}