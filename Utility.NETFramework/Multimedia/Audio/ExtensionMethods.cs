// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ExtensionMethods.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 6:55 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.IO;

using JLR.Utility.NETFramework.Multimedia.Audio.Riff;

namespace JLR.Utility.NETFramework.Multimedia.Audio
{
	public static class ExtensionMethods
	{
		#region System.IO.BinaryReader
		public static FourCc ReadFourCc(this BinaryReader reader)
		{
			return FourCc.FromUInt32(reader.ReadUInt32());
		}

		public static FourCc PeekFourCc(this BinaryReader reader, long peekOffset = 0)
		{
			var initialPosition = reader.BaseStream.Position;
			var peekPosition    = reader.BaseStream.Position + peekOffset;
			if (peekPosition < 0 || peekPosition > reader.BaseStream.Length - 4)
				throw new ArgumentOutOfRangeException(nameof(peekOffset));

			try
			{
				reader.BaseStream.Position = peekPosition;
				var result = reader.ReadFourCc();
				return result;
			}
			finally
			{
				reader.BaseStream.Position = initialPosition;
			}
		}

		public static bool SeekNextFourCc(this BinaryReader reader, FourCc value)
		{
			var initialPosition = reader.BaseStream.Position;
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				if (reader.ReadFourCc() == value)
					return true;
				if (reader.BaseStream.Length - reader.BaseStream.Position < 4)
					break;
			}

			reader.BaseStream.Position = initialPosition;
			return false;
		}
		#endregion
	}
}