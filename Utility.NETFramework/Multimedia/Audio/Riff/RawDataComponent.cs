// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       RawDataComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:09 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.IO;
using System.Runtime.InteropServices;

using JLR.Utility.NET.IO;

namespace JLR.Utility.NET.Multimedia.Audio.Riff
{
	//TODO: Debug this class - it may not work properly...
	public sealed unsafe class RawDataComponent : RiffComponent
	{
		#region Fields
		private          UnmanagedMemoryStream _data;
		private readonly IntPtr                _dataIntPtr;
		#endregion

		#region Properties
		public          UnmanagedMemoryStream DataStream  => _data;
		public          IntPtr                DataPointer => _dataIntPtr;
		public override long                  Size        => _data.Length;
		#endregion

		#region Constructors
		public RawDataComponent(long dataSize)
		{
			_dataIntPtr = Marshal.AllocHGlobal((IntPtr)dataSize);
			var dataPtr = (byte*)_dataIntPtr.ToPointer();
			_data = new UnmanagedMemoryStream(dataPtr, dataSize, dataSize, FileAccess.ReadWrite);
		}

		public RawDataComponent(byte[] data) : this(data.LongLength)
		{
			_data.Write(data, 0, data.Length);
		}

		public RawDataComponent(BinaryReader reader, long dataSize) : this(dataSize)
		{
			Read(reader);
		}
		#endregion

		#region Methods
		public override void Read(BinaryReader reader)
		{
			reader.BaseStream.CopyTo(_data, reader.BaseStream.Position, 0, _data.Capacity);
		}

		public override void Write(BinaryWriter writer)
		{
			_data.CopyTo(writer.BaseStream, 0, writer.BaseStream.Position);
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public override ValidationResult Validate()
		{
			return new ValidationResult();
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return $"[Raw Data] Size: {Size}B";
		}
		#endregion

		#region Method Overrides (Disposable)
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!IsDisposed)
			{
				_data.Close();
				_data = null;
				Marshal.FreeHGlobal(_dataIntPtr);
			}

			IsDisposed = true;
		}
		#endregion
	}
}