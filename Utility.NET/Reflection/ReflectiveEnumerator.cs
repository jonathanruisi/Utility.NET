using System.Reflection;

namespace JLR.Utility.NET.Reflection
{
    public static class ReflectiveEnumerator
    {
        /// <summary>
        /// Uses reflection to build a list of all classes that derive from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// Base class type
        /// </typeparam>
        /// <returns>A list of <see cref="Type"/>s derived from <typeparamref name="T"/>.</returns>
        public static IEnumerable<Type> GetSubclassTypes<T>() where T : class
        {
            var assembly = Assembly.GetAssembly(typeof(T));

            if (assembly == null)
                throw new Exception("Unable to get assembly for specified type");

            return assembly.GetTypes().Where(type =>
                type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T))
            );
        }
    }
}