// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Shell32Helper.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:24 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Microsoft.Win32;

namespace JLR.Utility.WinApi.Shell32
{
	public static class Shell32Helper
	{
		public static List<FileTypeInfo> EnumerateKnownFileTypesAndIcons()
		{
			var fileTypeList = new List<FileTypeInfo>();

			// Get an array of all subkey names under HKEY_CLASSES_ROOT registry key
			var rootKey      = Registry.ClassesRoot;
			var rootKeyNames = rootKey.GetSubKeyNames();

			// Get the location of the file containing the icon for all file extensions
			foreach (var keyName in rootKeyNames)
			{
				// Skip any keys that do not represent a file extension
				if (String.IsNullOrEmpty(keyName))
					continue;
				if (keyName.IndexOf('.') != 0)
					continue;

				// Get the default subkey
				var fileTypeKey  = rootKey.OpenSubKey(keyName);
				var defaultValue = fileTypeKey?.GetValue(String.Empty);
				if (defaultValue == null)
					continue;

				// Get the subkey that contains the default icon location
				var iconKey = rootKey.OpenSubKey(defaultValue + "\\DefaultIcon");
				if (iconKey != null)
				{
					// Get the file that contains the icon and the icon's index within that file
					var iconInfo = iconKey.GetValue(String.Empty);
					if (iconInfo != null)
					{
						var index           = 0;
						var iconInfoStrings = iconInfo.ToString().Replace("\"", String.Empty).Split(',');

						//var fileStrings = iconInfoStrings[0].Split('.');
						//var extension = fileStrings[fileStrings.Length - 1];
						if (iconInfoStrings.Length > 1)
							int.TryParse(iconInfoStrings[1], out index);

						try
						{
							Icon smallIcon, largeIcon;
							ExtractIconFromFile(iconInfoStrings[0], index, out smallIcon, out largeIcon);
							var fileTypeInfo = new FileTypeInfo
							{
								Extension     = keyName,
								Description   = String.Empty,
								SmallIcon     = smallIcon,
								LargeIcon     = largeIcon,
								IconFile      = iconInfoStrings[0],
								IconFileIndex = index
							};
							fileTypeList.Add(fileTypeInfo);
						}
						catch (Exception ex)
						{
							throw new Exception("Unable to extract icon from file", ex);
						}
					}

					iconKey.Close();
				}

				fileTypeKey.Close();
			}

			rootKey.Close();
			return fileTypeList;
		}

		public static void ExtractIconFromFile(string filePath, int index, out Icon smallIcon, out Icon largeIcon)
		{
			smallIcon = null;
			largeIcon = null;
			var smallIconPtrs = new[] { IntPtr.Zero };
			var largeIconPtrs = new[] { IntPtr.Zero };

			try
			{
				var iconCount = NativeMethods.Shell32.ExtractIconEx(filePath, 0, largeIconPtrs, smallIconPtrs, 1);
				if (iconCount > 0)
				{
					if (smallIconPtrs[0] != IntPtr.Zero)
					{
						smallIcon = (Icon)Icon.FromHandle(smallIconPtrs[0]).Clone();
					}

					if (largeIconPtrs[0] != IntPtr.Zero)
					{
						largeIcon = (Icon)Icon.FromHandle(largeIconPtrs[0]).Clone();
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Unable to extract icon from file", ex);
			}
			finally
			{
				foreach (var ptr in smallIconPtrs.Where(ptr => ptr != IntPtr.Zero))
				{
					NativeMethods.User32.DestroyIcon(ptr);
				}

				foreach (var ptr in largeIconPtrs.Where(ptr => ptr != IntPtr.Zero))
				{
					NativeMethods.User32.DestroyIcon(ptr);
				}
			}
		}
	}
}