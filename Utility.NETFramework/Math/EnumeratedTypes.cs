namespace JLR.Utility.NETFramework.Math
{
	public enum EqualityMethods
	{
		/// <summary>
		/// Equality is determined based on the number of
		/// floating point values by which the arguments differ.
		/// </summary>
		Precision,

		/// <summary>
		/// Equality is determined by ignoring any differences
		/// present after a specified number of decimal places.
		/// </summary>
		FractionalSignificance
	}
}