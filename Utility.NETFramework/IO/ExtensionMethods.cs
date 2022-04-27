// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ExtensionMethods.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-26 @ 12:14 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLR.Utility.NETFramework.IO
{
	#region Enumerated Types
	[Flags]
	public enum StreamCapabilities
	{
		Read    = 0x1,
		Write   = 0x2,
		Seek    = 0x4,
		Timeout = 0x8
	}
	#endregion

	public static class ExtensionMethods
	{
		#region System.IO.Stream
		/// <summary>
		/// Determines if a stream supports a given set of operations
		/// </summary>
		/// <param name="stream">The stream to validate</param>
		/// <param name="requiredCapabilities">Flags that specify the capabilities to validate</param>
		public static void ValidateCapabilities(this Stream stream, StreamCapabilities requiredCapabilities)
		{
			StreamCapabilities missingCapabilities = 0;
			if (!stream.CanRead && ((requiredCapabilities & StreamCapabilities.Read) == StreamCapabilities.Read))
				missingCapabilities |= StreamCapabilities.Read;
			if (!stream.CanWrite && ((requiredCapabilities & StreamCapabilities.Write) == StreamCapabilities.Write))
				missingCapabilities |= StreamCapabilities.Write;
			if (!stream.CanSeek && ((requiredCapabilities & StreamCapabilities.Seek) == StreamCapabilities.Seek))
				missingCapabilities |= StreamCapabilities.Seek;
			if (!stream.CanTimeout && ((requiredCapabilities & StreamCapabilities.Timeout) == StreamCapabilities.Timeout))
				missingCapabilities |= StreamCapabilities.Timeout;

			if (missingCapabilities != 0)
			{
				var missingCapabilitiesList = missingCapabilities.GetSetFlagNames().ToList();
				var errorString             = new StringBuilder();
				errorString.Append("This stream does not support the following requirements: ");
				for (var i = 0; i < missingCapabilitiesList.Count; i++)
				{
					errorString.Append(missingCapabilitiesList[i]);
					if (i < missingCapabilitiesList.Count - 1)
						errorString.Append(", ");
				}

				throw new NotSupportedException(errorString.ToString());
			}
		}

		/// <summary>
		/// Determines if a stream is large enough to read/write an object of a given size at a given location
		/// </summary>
		/// <param name="stream">The stream to test</param>
		/// <param name="dataSize">The size (in bytes) of the object</param>
		/// <param name="dataOffset">
		/// The position of the object within the stream (relative to zero).
		/// If this parameter is <c>null</c>, the stream's current position will be used.
		/// </param>
		/// <returns>
		/// <c>true</c> if the stream is large enough to accommodate the object;
		/// otherwise <c>false</c>
		/// </returns>
		public static bool IsLargeEnough(this Stream stream, long dataSize, long? dataOffset = null)
		{
			if (dataOffset == null) dataOffset = stream.Position;
			return stream.Length - dataSize - dataOffset >= 0;
		}

		/// <summary>
		/// "Blanks" a specified number of bytes in a stream using a specified blanking value
		/// </summary>
		/// <param name="stream">The stream</param>
		/// <param name="blankingValue">The value that will be written to the blanked bytes</param>
		/// <param name="length">The number of bytes to blank</param>
		public static void Blank(this Stream stream, byte blankingValue, int length)
		{
			ValidateCapabilities(stream, StreamCapabilities.Write);
			var buffer = new byte[length];
			for (long i = 0; i < length; i++)
			{
				buffer[i] = blankingValue;
			}

			stream.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Asynchronously "blanks" a specified number of bytes in a stream
		/// using a specified blanking value
		/// </summary>
		/// <param name="stream">The stream</param>
		/// <param name="blankingValue">The value that will be written to the blanked bytes</param>
		/// <param name="length">The number of bytes to blank</param>
		public static async void BlankAsync(this Stream stream, byte blankingValue, int length)
		{
			ValidateCapabilities(stream, StreamCapabilities.Write);
			var buffer = new byte[length];
			for (long i = 0; i < length; i++)
			{
				buffer[i] = blankingValue;
			}

			await stream.WriteAsync(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Reads a specified number of bytes from a specified point in the current stream,
		/// and writes them to a specified position in another stream,
		/// using a specified buffer size.
		/// </summary>
		/// <param name="source">The source stream</param>
		/// <param name="destination">
		/// The stream to which the contents of the current stream will be copied</param>
		/// <param name="sourceStartPosition">
		/// A byte offset in the current stream from which to begin copying</param>
		/// <param name="destinationStartPosition">
		/// A byte offset in the destination stream to which data will be copied</param>
		/// <param name="totalBytesToCopy">The total number of bytes to copy</param>
		/// <param name="bufferSize">
		/// The size of the buffer used during the copying process.
		/// This value must be greater than zero.
		/// The default value is 4096.
		/// </param>
		/// <returns>The actual number of bytes copied</returns>
		public static long CopyTo(this Stream source,
								  Stream destination,
								  long sourceStartPosition,
								  long destinationStartPosition = 0,
								  long? totalBytesToCopy = null,
								  long bufferSize = 0x1000)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination), "The destination stream cannot be null");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(bufferSize), "The buffer size must be greater than 0");
			ValidateCapabilities(source,      StreamCapabilities.Read);
			ValidateCapabilities(destination, StreamCapabilities.Write);

			if (totalBytesToCopy == null)
				totalBytesToCopy = source.Length - sourceStartPosition;
			if (sourceStartPosition + totalBytesToCopy > source.Length)
				totalBytesToCopy = source.Length - sourceStartPosition;
			if (bufferSize > totalBytesToCopy)
				bufferSize = (long)totalBytesToCopy;

			source.Position = sourceStartPosition;
			var  buffer           = new byte[bufferSize];
			long totalBytesCopied = 0;
			int  bytesRead;
			for (var i = 0; i < totalBytesToCopy; i += bytesRead)
			{
				int bytesToRead;
				if (totalBytesToCopy - i >= buffer.Length)
					bytesToRead = buffer.Length;
				else
					bytesToRead = (int)((long)totalBytesToCopy - i);

				totalBytesCopied += bytesRead = source.Read(buffer, 0, bytesToRead);
				destination.Write(buffer, 0, bytesRead);
			}

			return totalBytesCopied;
		}

		/// <summary>
		/// Asynchronously reads a specified number of bytes from a specified point in the current stream,
		/// and asynchronously writes them to a specified position in another stream,
		/// using a specified buffer size.
		/// </summary>
		/// <param name="source">The source stream</param>
		/// <param name="destination">
		/// The stream to which the contents of the current stream will be copied</param>
		/// <param name="sourceStartPosition">
		/// A byte offset in the current stream from which to begin copying</param>
		/// <param name="destinationStartPosition">
		/// A byte offset in the destination stream to which data will be copied</param>
		/// <param name="totalBytesToCopy">The total number of bytes to copy</param>
		/// <param name="bufferSize">
		/// The size of the buffer used during the copying process.
		/// This value must be greater than zero.
		/// The default value is 4096.
		/// </param>
		/// <returns>The actual number of bytes copied</returns>
		public static async Task<long> CopyToAsync(this Stream source,
												   Stream destination,
												   long sourceStartPosition,
												   long destinationStartPosition = 0,
												   long? totalBytesToCopy = null,
												   long bufferSize = 0x1000)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination), "The destination stream cannot be null");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException(nameof(bufferSize), "The buffer size must be greater than 0");
			ValidateCapabilities(source,      StreamCapabilities.Read);
			ValidateCapabilities(destination, StreamCapabilities.Write);

			if (totalBytesToCopy == null)
				totalBytesToCopy = source.Length - sourceStartPosition;
			if (sourceStartPosition + totalBytesToCopy > source.Length)
				totalBytesToCopy = source.Length - sourceStartPosition;
			if (bufferSize > totalBytesToCopy)
				bufferSize = (long)totalBytesToCopy;

			source.Position = sourceStartPosition;
			var  buffer           = new byte[bufferSize];
			long totalBytesCopied = 0;
			int  bytesRead;
			for (var i = 0; i < totalBytesToCopy; i += bytesRead)
			{
				int bytesToRead;
				if (totalBytesToCopy - i >= buffer.Length)
					bytesToRead = buffer.Length;
				else
					bytesToRead = (int)((long)totalBytesToCopy - i);

				totalBytesCopied += bytesRead = await source.ReadAsync(buffer, 0, bytesToRead);
				await destination.WriteAsync(buffer, 0, bytesRead);
			}

			return totalBytesCopied;
		}
		#endregion

		#region System.IO.BinaryReader
		public static sbyte PeekSByte(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(sbyte))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadSByte();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static byte PeekByte(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(byte))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadByte();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static short PeekInt16(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(short))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadInt16();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static ushort PeekUInt16(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(ushort))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadUInt16();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static int PeekInt32(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(int))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadInt32();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static uint PeekUInt32(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(uint))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadUInt32();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static long PeekInt64(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(long))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadInt64();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static ulong PeekUInt64(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(ulong))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadUInt64();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static bool PeekBoolean(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(bool))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadBoolean();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static float PeekSingle(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(float))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadSingle();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static double PeekDouble(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(double))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadDouble();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static decimal PeekDecimal(this BinaryReader reader, long peekOffset = 0)
		{
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - sizeof(decimal))
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadDecimal();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}
		#endregion
	}
}