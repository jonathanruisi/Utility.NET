// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       IValidatable.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2016-01-14 @ 9:06 PM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JLR.Utility.NETFramework
{
	#region Enumerated Types
	public enum Validity
	{
		Valid   = 0,
		Unknown = 1,
		Invalid = 2
	}
	#endregion

	#region IValidatable
	/// <summary>
	/// Provides a means for implementers to validate their current state.
	/// This information is useful in cases where an invalid state is common
	/// and thus exception based handling of the invalid state is undesired.
	/// </summary>
	public interface IValidatable
	{
		/// <summary>
		/// Returns the validity of the current instance from the point at which
		/// <see cref="Validate()"/> was last called.
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// Causes the current instance to check its own validity and
		/// returns a <see cref="ValidationResult"/> object containing detailed validity information.
		/// </summary>
		/// <returns>A <see cref="ValidationResult"/> object containing detailed validity information.</returns>
		ValidationResult Validate();
	}
	#endregion

	#region ValidationResult
	/// <summary>
	/// Contains specific information regarding the validity of an object.
	/// </summary>
	public sealed class ValidationResult
	{
		#region Fields
		private          Validity     _validity;
		private readonly List<string> _reasons;
		#endregion

		#region Properties
		/// <summary>
		/// Gets a value indicating the current state of validity.
		/// </summary>
		public Validity Validity { get { return _validity; } private set { _validity = value; } }


		/// <summary>
		/// Gets a list of strings providing specific reasons as to the current state of validity.
		/// </summary>
		public List<string> Reasons => _reasons;
		#endregion

		#region Constructors
		public ValidationResult(Validity validity = Validity.Valid, params string[] reasons)
		{
			_validity = validity;
			_reasons  = new List<string>(reasons);
		}
		#endregion

		#region Public Methods
		public void EditValidity(Validity validity, string reason = null)
		{
			_validity = validity;
			if (_validity == Validity.Valid) Reasons.Clear();
			else if (!String.IsNullOrEmpty(reason)) Reasons.Add(reason);
		}

		/// <summary>
		/// Allows two <see cref="ValidationResult"/> objects to combine their reason lists
		/// if they indicate the same state of validity.
		/// </summary>
		/// <param name="other">The <see cref="ValidationResult"/> with which to merge.</param>
		public void Merge(ValidationResult other)
		{
			if (other._validity <= _validity) return;
			_validity = other._validity;
			Reasons.AddRange(other.Reasons);
		}
		#endregion

		#region Method Overrides (System.Object)
		public override bool Equals(object obj)
		{
			return _validity == (obj as ValidationResult)?._validity;
		}

		public override int GetHashCode()
		{
			return (int)Validity + Reasons.Sum(reason => reason.GetHashCode());
		}

		public override string ToString()
		{
			if (_validity == Validity.Valid)
				return Validity.Valid.ToString().ToUpper();

			var result = new StringBuilder(_validity.ToString().ToUpper());
			result.Append(": ");
			for (var i = 0; i < Reasons.Count; i++)
			{
				result.Append(Reasons[i]);
				if (i < Reasons.Count - 1)
					result.Append(", ");
			}

			return result.ToString();
		}
		#endregion

		#region Operator Overloads (Conversion)
		public static implicit operator bool(ValidationResult value)
		{
			return value?._validity == Validity.Valid;
		}
		#endregion
	}
	#endregion
}