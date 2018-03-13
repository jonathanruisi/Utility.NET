// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       Disposable.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-23 @ 4:40 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;

namespace JLR.Utility.NET
{
	/// <summary>
	/// Implements some of the boilerplate code associated with the chainable dispose pattern.
	/// </summary>
	public abstract class Disposable : IDisposable
	{
		protected bool IsDisposed = false;

		/// <summary>
		/// Use this method to close or release unmanaged resources such as files, streams,
		/// and handles held by an instance of a derived class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Call this method at the beginning of methods and property accessors in a derived class to
		/// prevent use of an already-disposed object.
		/// </summary>
		protected void EnforceDisposalState()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		/// <summary>
		/// Override in derived class to implement the chainable dispose pattern.
		/// </summary>
		/// <param name="disposing">
		/// True when consumer has called Dispose() on the object, false when the object is being finalized.
		/// </param>
		protected abstract void Dispose(bool disposing);

		/// <summary>
		/// Finalizer. Guarantees that unmanaged resources will be freed.
		/// </summary>
		~Disposable()
		{
			Dispose(false);
		}
	}
}