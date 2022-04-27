// ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  FILE:       ExtensionMethods.cs
// ┃  PROJECT:    Utility.NET
// ┃  SOLUTION:   Utility.NET
// ┃  CREATED:    2015-12-26 @ 12:03 AM
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  AUTHOR:     Jonathan Ruisi
// ┃  EMAIL:      JonathanRuisi@gmail.com
// ┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// ┃  GIT REPO:   https://github.com/jonathanruisi/Utility.NET
// ┃  LICENSE:    https://github.com/jonathanruisi/Utility.NET/blob/master/LICENSE
// ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

using System;
using System.ComponentModel;
using System.Linq.Expressions;

using JLR.Utility.NETFramework.ChangeNotification.EventArgs;

namespace JLR.Utility.NETFramework.ChangeNotification
{
	public static class ExtensionMethods
	{
		#region PropertyChangedEventHandler
		public static T SetPropertyAndNotify<T>(this PropertyChangedEventHandler handler,
												T newValue,
												Expression<Func<T>> oldValueExpression,
												Action<T> setter)
		{
			return SetPropertyAndNotify(handler, null, newValue, oldValueExpression, setter);
		}

		public static T SetPropertyAndNotify<T>(this PropertyChangedEventHandler postHandler,
												PropertyChangingEventHandler preHandler,
												T newValue,
												Expression<Func<T>> oldValueExpression,
												Action<T> setter)
		{
			var oldValue = oldValueExpression.Compile()();

			if (!(Equals(oldValue, null) ? Equals(newValue, null) : oldValue.Equals(newValue)))
			{
				// Obtain and verify all information needed about the property from the specified arguments
				var body = oldValueExpression.Body as MemberExpression;
				if (body == null)
					throw new ArgumentException("Invalid expression", nameof(oldValueExpression));

				var targetExpression = body.Expression as ConstantExpression;
				if (targetExpression == null)
					throw new ArgumentException("Invalid expression", nameof(oldValueExpression));
				var target = targetExpression.Value;

				// Raise the PropertyChanging event
				preHandler?.Invoke(target, new PropertyChangingAdvancedEventArgs(body.Member.Name, oldValue, newValue));

				// Update the property value
				setter(newValue);

				// Raise the PropertyChanged event
				postHandler?.Invoke(target, new PropertyChangedAdvancedEventArgs(body.Member.Name, oldValue, newValue));
			}

			return newValue;
		}
		#endregion
	}
}