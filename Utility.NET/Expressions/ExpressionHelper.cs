using System.Linq.Expressions;

namespace JLR.Utility.NET.Expressions
{
    public static class ExpressionHelper
    {
        /// <summary>
        /// Creates an executable, pre-compiled lambda expression of
        /// the constructor for the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the object.
        /// <para><b><typeparamref name="T"/>
        /// must have a public parameterless constructor.
        /// </b></para>
        /// </typeparam>
        /// <returns>
        /// Delegate representing the pre-compiled lambda expression.
        /// </returns>
        public static Func<T> Constructor<T>() where T : new()
        {
            var expr = Expression.Lambda<Func<T>>(Expression.New(typeof(T)));
            return expr.Compile();
        }

        /// <summary>
        /// Creates an executable, pre-compiled lambda expression of
        /// the getter for the property specified by <paramref name="propertyName"/>
        /// of type <typeparamref name="TProperty"/>,
        /// in an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the desired property.</param>
        /// <returns>
        /// Delegate representing the pre-compiled lambda expression.
        /// </returns>
        public static Func<T, TProperty> PropertyGetter<T, TProperty>(string propertyName)
        {
            var arg = Expression.Parameter(typeof(T));
            var body = Expression.Property(arg, propertyName);
            var expr = Expression.Lambda<Func<T, TProperty>>(body, arg);
            return expr.Compile();
        }
    }
}