// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       RiffComponent.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2018-03-13 @ 7:06 PM
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

using JLR.Utility.NETFramework.Collections.Tree;
using JLR.Utility.NETFramework.IO;

namespace JLR.Utility.NETFramework.Multimedia.Audio.Riff
{
	#region IRiffComponent
	public interface IRiffComponent : IValidatable
	{
		long Size { get; }
	}
	#endregion

	public abstract class RiffComponent : TreeNodeBase<RiffComponent>, IRiffComponent
	{
		#region Properties
		public            bool IsValid    => Validate();
		protected virtual int  HeaderSize => 0;
		protected virtual bool IsPadded   => false;

		public virtual long Size       { get { return Children.Sum(child => child.ActualSize); } }
		public         long ActualSize => Size + HeaderSize + (IsPadded ? 1 : 0);

		public long Offset => ActualOffset + HeaderSize;

		public long ActualOffset
		{
			get
			{
				if (this == Root)
					return 0;
				if (this == Siblings.First)
					return Parent.Offset;
				return Previous.ActualOffset + Previous.ActualSize;
			}
		}
		#endregion

		#region Public Methods
		public abstract ValidationResult Validate();
		#endregion

		#region Internal Methods
		public virtual void Read(BinaryReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));
			reader.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Read);
		}

		public virtual void Write(BinaryWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			writer.BaseStream.ValidateCapabilities(StreamCapabilities.Seek | StreamCapabilities.Write);
		}
		#endregion
	}
}