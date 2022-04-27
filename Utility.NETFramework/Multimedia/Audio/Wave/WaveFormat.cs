// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       WaveFormat.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:03 PM
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

using JLR.Utility.NETFramework.Multimedia.Audio.Riff;

namespace JLR.Utility.NETFramework.Multimedia.Audio.Wave
{
	#region Enumerated Types
	public enum WaveFormatTag : ushort
	{
		Unknown    = 0x0000,
		Pcm        = 0x0001,
		IeeeFloat  = 0x0003,
		Extensible = 0xFFFE
	}

	[Flags]
	public enum SpeakerPositions : uint
	{
		None               = 0x00000000,
		FrontLeft          = 0x00000001,
		FrontRight         = 0x00000002,
		FrontCenter        = 0x00000004,
		LowFrequency       = 0x00000008,
		BackLeft           = 0x00000010,
		BackRight          = 0x00000020,
		FrontLeftOfCenter  = 0x00000040,
		FrontRightOfCenter = 0x00000080,
		BackCenter         = 0x00000100,
		SideLeft           = 0x00000200,
		SideRight          = 0x00000400,
		TopCenter          = 0x00000800,
		TopFrontLeft       = 0x00001000,
		TopFrontCenter     = 0x00002000,
		TopFrontRight      = 0x00004000,
		TopBackLeft        = 0x00008000,
		TopBackCenter      = 0x00010000,
		TopBackRight       = 0x00020000,
		All                = 0x80000000
	}
	#endregion

	#region MediaSubtypeGuid
	public static class MediaSubtypeGuid
	{
		public static readonly Guid Pcm = new Guid(
			0x00000001,
			0x0000,
			0x0010,
			0x80,
			0x00,
			0x00,
			0xAA,
			0x00,
			0x38,
			0x9B,
			0x71);

		public static readonly Guid IeeeFloat = new Guid(
			0x00000003,
			0x0000,
			0x0010,
			0x80,
			0x00,
			0x00,
			0xAA,
			0x00,
			0x38,
			0x9B,
			0x71);
	}
	#endregion

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
	public class WaveFormat : IRiffComponent, IEquatable<WaveFormat>
	{
		#region Constants
		protected const uint StandardSizePcm        = 16;
		protected const uint StandardSize           = 18;
		protected const uint StandardSizeExtensible = 40;
		#endregion

		#region Fields
		protected WaveFormatTag formatTag;
		protected short         channels;
		protected int           samplesPerSecond;
		protected int           averageBytesPerSecond;
		protected short         blockAlign;
		protected short         bitsPerSample;
		protected short         extraDataSize;
		#endregion

		#region Properties
		public WaveFormatTag FormatTag             => formatTag;
		public short         Channels              => channels;
		public int           SamplesPerSecond      => samplesPerSecond;
		public int           AverageBytesPerSecond => averageBytesPerSecond;
		public short         BlockAlign            => blockAlign;
		public short         BitsPerSample         => bitsPerSample;
		public short         ExtraDataSize         => extraDataSize;

		public long Size => ExtraDataSize + (FormatTag == WaveFormatTag.Pcm ? StandardSizePcm : StandardSize);

		public        bool       IsValid => Validate();
		public static WaveFormat Default => new WaveFormat();
		#endregion

		#region Constructors
		public WaveFormat() : this(WaveFormatTag.Pcm, 2, 44100, 16) { }

		public WaveFormat(WaveFormatTag formatTag, int channels, int samplesPerSecond, int bitsPerSample)
		{
			this.formatTag        = formatTag;
			this.channels         = (short)channels;
			this.samplesPerSecond = samplesPerSecond;
			this.bitsPerSample    = (short)bitsPerSample;
			this.extraDataSize    = 0;
			CalculateValues();
		}
		#endregion

		#region Static Methods
		public static WaveFormat FromStream(BinaryReader reader)
		{
			try
			{
				var streamPosition = reader.BaseStream.Position;
				var result         = new WaveFormat();
				result.Read(reader);
				if (result.FormatTag == WaveFormatTag.Extensible)
				{
					reader.BaseStream.Position = streamPosition;
					return FromStream(reader, StandardSizeExtensible);
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new RiffException("Unable to read WaveFormat data", ex);
			}
			finally
			{
				reader = null;
			}
		}

		public static WaveFormat FromStream(BinaryReader reader, uint size)
		{
			try
			{
				var result = new WaveFormat();
				if (size == StandardSize || size == StandardSizePcm) result.Read(reader);
				else if (size >= StandardSizeExtensible)
				{
					result = new WaveFormatExtensible();
					result.Read(reader);
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new RiffException("Unable to read WaveFormat data", ex);
			}
			finally
			{
				reader = null;
			}
		}

		public static WaveFormat MarshalFromPtr(IntPtr pointer)
		{
			var waveFormat = (WaveFormat)Marshal.PtrToStructure(pointer, typeof(WaveFormat));
			switch (waveFormat.formatTag)
			{
				case WaveFormatTag.Pcm:
					waveFormat.extraDataSize = 0;
					break;

				case WaveFormatTag.Extensible:
					waveFormat = (WaveFormatExtensible)Marshal.PtrToStructure(pointer, typeof(WaveFormatExtensible));
					break;

				default:
					if (waveFormat.extraDataSize > 0)
					{
						throw new FormatException("This WaveFormat has extra data > 0. TODO: Implement WaveFormatExtraData");
					}

					break;
			}

			return waveFormat;
		}

		public static WaveFormat FromBlob(byte[] blob)
		{
			if (blob == null)
			{
				throw new ArgumentNullException(nameof(blob));
			}

			var reader = new BinaryReader(new MemoryStream(blob));
			if (reader.BaseStream.Length < 18)
				throw new FormatException("The BLOB is too small to create a WaveFormat object");

			// Read WaveFormat fields
			var result = new WaveFormatExtensible
			{
				formatTag             = (WaveFormatTag)reader.ReadInt16(),
				channels              = reader.ReadInt16(),
				samplesPerSecond      = reader.ReadInt32(),
				averageBytesPerSecond = reader.ReadInt32(),
				blockAlign            = reader.ReadInt16(),
				bitsPerSample         = reader.ReadInt16(),
				extraDataSize         = reader.ReadInt16()
			};

			// Check if extensible data exists
			if (result.extraDataSize != 22) return result;
			if (reader.BaseStream.Length < 40)
				throw new FormatException("The BLOB is too small to create a WaveFormatExtensible object");
			result.ValidBitsPerSample = reader.ReadInt16();
			result.ChannelMask        = (SpeakerPositions)reader.ReadUInt32();
			result.SubFormat          = new Guid(reader.ReadBytes(16));
			return result;
		}
		#endregion

		#region Protected Methods
		internal virtual void Read(BinaryReader reader)
		{
			try
			{
				formatTag             = (WaveFormatTag)reader.ReadUInt16();
				channels              = reader.ReadInt16();
				samplesPerSecond      = reader.ReadInt32();
				averageBytesPerSecond = reader.ReadInt32();
				blockAlign            = reader.ReadInt16();
				bitsPerSample         = reader.ReadInt16();
				extraDataSize         = reader.ReadInt16();

				if (extraDataSize != 0 && extraDataSize != StandardSizeExtensible - StandardSize)
				{
					switch (formatTag)
					{
						case WaveFormatTag.Pcm:
							extraDataSize = 0;
							reader.BaseStream.Seek(-2, SeekOrigin.Current);
							break;
						case WaveFormatTag.Extensible:
							extraDataSize = (short)(StandardSizeExtensible - StandardSize);
							break;
					}
				}

				CalculateValues();
			}
			catch (Exception ex)
			{
				throw new IOException("Failed to read WaveFormat data from stream", ex);
			}
		}

		internal virtual void Write(BinaryWriter writer)
		{
			try
			{
				writer.Write((short)formatTag);
				writer.Write(channels);
				writer.Write(samplesPerSecond);
				writer.Write(averageBytesPerSecond);
				writer.Write(blockAlign);
				writer.Write(bitsPerSample);
				writer.Write(extraDataSize);
			}
			catch (Exception ex)
			{
				throw new IOException("Failed to write WaveFormat data to stream", ex);
			}
		}
		#endregion

		#region Private Methods
		private void CalculateValues()
		{
			blockAlign            = (short)(channels * (bitsPerSample / 8));
			averageBytesPerSecond = samplesPerSecond * blockAlign;
		}
		#endregion

		#region Interface Implementation(IEquatable<>)
		public virtual bool Equals(WaveFormat other)
		{
			if (other == null)
				return false;

			return formatTag == other.formatTag && channels == other.channels && samplesPerSecond == other.samplesPerSecond &&
				averageBytesPerSecond == other.averageBytesPerSecond && blockAlign == other.blockAlign &&
				bitsPerSample == other.bitsPerSample && extraDataSize == other.extraDataSize;
		}
		#endregion

		#region Interface Implementation (IValidatable)
		public virtual ValidationResult Validate()
		{
			var result = new ValidationResult();

			// FormatTag
			if (formatTag == WaveFormatTag.Unknown)
				result.EditValidity(Validity.Invalid, "\"FormatTag\" = 'Unknown' is invalid");
			else if (!Enum.IsDefined(typeof(WaveFormatTag), formatTag))
				result.EditValidity(Validity.Invalid, $"The specified \"FormatTag\" ({formatTag}) is not recognized");

			// Channels
			if (channels < 1)
				result.EditValidity(Validity.Invalid, "\"Channels\" must be = 1");
			/*else if (channels > 2 && formatTag != WaveFormatTag.Extensible)
				result.EditValidity(Validity.Invalid, String.Format(
					"The specified \"FormatTag\" ({0}) does not support \"Channels\" > 2", formatTag));*/

			// SamplesPerSecond
			if (samplesPerSecond <= 0)
				result.EditValidity(Validity.Invalid, "\"SamplesPerSecond\" must be > zero");

			// BitsPerSample
			if (bitsPerSample <= 0)
				result.EditValidity(Validity.Invalid, "\"BitsPerSample\" must be > zero");
			else if (bitsPerSample % 8 != 0)
				result.EditValidity(Validity.Invalid, "\"BitsPerSample\" must be byte-aligned");
			/*else if (bitsPerSample > 16 && formatTag != WaveFormatTag.Extensible)
				result.EditValidity(Validity.Invalid, String.Format(
					"The specified \"FormatTag\" ({0}) does not support \"BitsPerSample\" > 16", formatTag));*/

			// ExtraDataSize
			if (extraDataSize != 0 && extraDataSize != StandardSizeExtensible - StandardSize)
				result.EditValidity(Validity.Invalid, "\"ExtraDataSize\" must have a value of 0 or 22");

			return result;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			return Equals(obj as WaveFormat);
		}

		public override int GetHashCode()
		{
			return (int)FormatTag + Channels + SamplesPerSecond + AverageBytesPerSecond + BlockAlign + BitsPerSample +
				ExtraDataSize;
		}

		public override string ToString()
		{
			return IsValid
				? $"{Enum.GetName(typeof(WaveFormatTag), formatTag)}: {bitsPerSample}bits @ {samplesPerSecond / 1000}kHz X {channels}ch"
				: Validate().ToString();
		}
		#endregion
	}
}