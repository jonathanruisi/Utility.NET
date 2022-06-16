using System.Linq.Expressions;
using System.Reflection;

namespace JLR.Utility.NET.Expressions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Creates a delegate for an executable, pre-compiled lambda expression of
        /// the constructor for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type of the object.</param>
        /// <returns>
        /// Delegate representing the pre-compiled lambda expression.
        /// </returns>
        public static Func<object> Constructor(this Type type)
        {
            return (Func<object>)Expression.Lambda(Expression.New(type)).Compile();
        }

        /// <summary>
        /// Creates an executable, pre-compiled lambda expression of
        /// the getter for the property of type <typeparamref name="T"/>
        /// specified by <paramref name="propertyInfo"/>.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyInfo">
        /// <see cref="PropertyInfo"/> object calling this extension method.
        /// </param>
        /// <returns>
        /// Delegate representing the pre-compiled lambda expression.
        /// </returns>
        public static Delegate PropertyGetter<T>(this PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(object));
#pragma warning disable CS8604 // Possible null reference argument.
            var instanceConv = Expression.Convert(instance, propertyInfo.DeclaringType);
#pragma warning restore CS8604 // Possible null reference argument.
            var body = Expression.Property(instanceConv, propertyInfo.Name);
            var bodyConv = Expression.Convert(body, typeof(T));
            return Expression.Lambda(bodyConv, instance).Compile();
        }

        /// <summary>
        /// Creates an executable, pre-compiled lambda expression of the setter
        /// for the property specified by <paramref name="propertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo">
        /// <see cref="PropertyInfo"/> object calling this extension method.
        /// </param>
        /// <returns>
        /// Delegate representing the pre-compiled lambda expression.
        /// </returns>
        public static Delegate PropertySetter(this PropertyInfo propertyInfo)
        {
            var instance = Expression.Parameter(typeof(object));
#pragma warning disable CS8604 // Possible null reference argument.
            var instanceConv = Expression.Convert(instance, propertyInfo.DeclaringType);
#pragma warning restore CS8604 // Possible null reference argument.
            var arg = Expression.Parameter(typeof(object));
            var argConv = Expression.Convert(arg, propertyInfo.PropertyType);
#pragma warning disable CS8604 // Possible null reference argument.
            var call = Expression.Call(instanceConv, propertyInfo.GetSetMethod(), argConv);
#pragma warning restore CS8604 // Possible null reference argument.
            return Expression.Lambda(call, instance, arg).Compile();
        }
    }
}