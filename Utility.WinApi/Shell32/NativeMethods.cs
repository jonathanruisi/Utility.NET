// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       NativeMethods.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 8:22 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Runtime.InteropServices;

namespace JLR.Utility.WinApi.Shell32
{
	internal static class NativeMethods
	{
		#region Shell32.dll
		internal static class Shell32
		{
			/// <summary>
			/// Retrieves a handle to an icon from the specified executable file, DLL, or icon file.
			/// </summary>
			/// <param name="hInst">A handle to the instance of the application calling the function.</param>
			/// <param name="lpszExeFileName">The name of an executable file, DLL, or icon file.</param>
			/// <param name="nIconIndex">
			/// The zero-based index of the icon to retrieve.
			/// For example, if this value is 0, the function returns a handle to the first icon in the specified file.
			/// </param>
			/// <returns>
			/// The return value is a handle to an icon.
			/// If the file specified was not an executable file, DLL, or icon file, the return is 1.
			/// If no icons were found in the file, the return value is <c>null</c>.
			/// </returns>
			[DllImport(
				"shell32.dll",
				EntryPoint    = "ExtractIconA",
				CharSet       = CharSet.Ansi,
				SetLastError  = true,
				ExactSpelling = true)]
			internal static extern IntPtr ExtractIcon(int hInst, string lpszExeFileName, int nIconIndex);

			/// <summary>
			/// Creates an array of handles to large or small icons
			/// extracted from the specified executable file, DLL, or icon file.
			/// </summary>
			/// <param name="szFileName">
			/// The name of an executable file, DLL, or icon file from which icons will be extracted.
			/// </param>
			/// <param name="nIconIndex">
			/// The zero-based index of the first icon to extract.
			/// For example, if this value is zero, the function extracts the first icon in the specified file.
			/// </param>
			/// <param name="phiconLarge">
			/// An array of icon handles that receives handles to the large icons extracted from the file.
			/// If this parameter is <c>null</c>, no large icons are extracted from the file.
			/// </param>
			/// <param name="phiconSmall">
			/// An array of icon handles that receives handles to the small icons extracted from the file.
			/// If this parameter is <c>null</c>, no small icons are extracted from the file.
			/// </param>
			/// <param name="nIcons">The number of icons to be extracted from the file.</param>
			/// <returns>
			/// If the <paramref name="nIconIndex"/> parameter is -1,
			/// the <paramref name="phiconLarge"/> parameter is <c>null</c>,
			/// and the <paramref name="phiconSmall"/> parameter is <c>null</c>,
			/// then the return value is the number of icons contained in the specified file.
			/// Otherwise, the return value is the number of icons successfully extracted from the file.
			/// </returns>
			[DllImport("shell32.dll", CharSet = CharSet.Auto)]
			internal static extern uint ExtractIconEx(string szFileName,
													  int nIconIndex,
													  IntPtr[] phiconLarge,
													  IntPtr[] phiconSmall,
													  uint nIcons);
		}
		#endregion

		#region User32.dll
		internal static class User32
		{
			/// <summary>
			/// Destroys an icon and frees any memory the icon occupied.
			/// </summary>
			/// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use.</param>
			/// <returns><c>true</c> if the function succeeds, <c>false</c> otherwise.</returns>
			[DllImport("user32.dll", SetLastError = true)]
			internal static extern bool DestroyIcon(IntPtr hIcon);
		}
		#endregion
	}
}