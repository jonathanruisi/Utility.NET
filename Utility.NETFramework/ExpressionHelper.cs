using System;
using System.Linq.Expressions;

namespace JLR.Utility.NETFramework
{
	public static class ExpressionHelper
	{
		public static Func<T> GetConstructor<T>()
		{
			var type = typeof(T);
			var body = Expression.New(type);
			return Expression.Lambda<Func<T>>(body).Compile();
		}
	}
}