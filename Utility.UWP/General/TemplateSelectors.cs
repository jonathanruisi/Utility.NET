using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using JLR.Utility.NET;
using JLR.Utility.UWP.ViewModels;

namespace JLR.Utility.UWP
{
	public class MediaItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate FolderTemplate { get; set; }
		public DataTemplate ImageFileTemplate { get; set; }
		public DataTemplate VideoFileTemplate { get; set; }

		protected override DataTemplate SelectTemplateCore(object item)
		{
			switch (item)
			{
				case MediaTreeFolder _:
					return FolderTemplate;
				case MediaTreeFile image when image.File.FileType == MIMETypes.Image:
					return ImageFileTemplate;
				case MediaTreeFile video when video.File.FileType == MIMETypes.Video:
					return VideoFileTemplate;
				default:
					return null;
			}
		}
	}
}