// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ComObject.cs
// ┃  PROJECT:    Utility.WinApi
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 7:50 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;

using JLR.Utility.NETFramework;

namespace JLR.Utility.WinApi.COM
{
	/// <summary>
	/// Provides a contract for .NET types compatible (can be marshalled) with COM interfaces.
	/// </summary>
	public interface IComInterface { }


	/// <summary>
	/// Provides a contract that guarantees implementing types support the COM QueryInterface method.
	/// </summary>
	public abstract class ComObject : Disposable
	{
		#region Fields
		private readonly IComInterface _comInterface;
		#endregion

		#region Constructor
		protected internal ComObject(IComInterface comInterface)
		{
			if (comInterface == null)
				throw new ArgumentNullException(nameof(comInterface));
			_comInterface = comInterface;
		}
		#endregion

		#region Public Methods
		public IComInterface QueryInterface()
		{
			EnforceDisposalState();
			return _comInterface;
		}
		#endregion
	}
}