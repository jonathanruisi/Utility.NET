using System;

using Windows.Storage;

using JLR.Utility.NET;

namespace JLR.Utility.UWP.Models
{
	public abstract class MediaFile
	{
		#region Properties
		public StorageFile File { get; }
		public MIMETypes FileType { get; protected set; }
		#endregion

		#region Constructors
		protected MediaFile(StorageFile file)
		{
			File = file ?? throw new ArgumentNullException(nameof(file));
		}
		#endregion

		#region Static Methods
		public static MediaFile Create(StorageFile file)
		{
			if (file == null)
				return null;

			if (file.ContentType.Contains("audio"))
				return new AudioFile(file);
			if (file.ContentType.Contains("image"))
				return new ImageFile(file);
			if (file.ContentType.Contains("video"))
				return new VideoFile(file);
			return null;
		}
		#endregion

		#region Method Overrides (System.Object)
		public override string ToString()
		{
			return File.DisplayName;
		}
		#endregion
	}

	public sealed class AudioFile : MediaFile
	{
		public AudioFile(StorageFile file) : base(file)
		{
			FileType = MIMETypes.Audio;
		}
	}

	public sealed class ImageFile : MediaFile
	{
		public ImageFile(StorageFile file) : base(file)
		{
			FileType = MIMETypes.Image;
		}
	}

	public sealed class VideoFile : MediaFile
	{
		public VideoFile(StorageFile file) : base(file)
		{
			FileType = MIMETypes.Video;
		}
	}
}