using System.Reflection;

namespace JLR.Utility.NET.Reflection
{
    public static class ReflectiveEnumerator
    {
        /// <summary>
        /// Uses reflection to build a list of all classes that derive from <typeparamref name="T"/>.
        /// All assemblies within the current AppDomain are searched.
        /// </summary>
        /// <typeparam name="T">
        /// Base class type
        /// </typeparam>
        /// <returns>A list of <see cref="Type"/>s derived from <typeparamref name="T"/>.</returns>
        public static IEnumerable<Type> GetSubclassTypes<T>() where T : class
        {

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            if (assemblies == null)
                throw new Exception($"Unable to get assemblies for the current AppDomain: {AppDomain.CurrentDomain.FriendlyName}");

            var typeList = new List<Type>();
            foreach (var assembly in assemblies)
            {
                var typesToAdd = assembly.GetTypes().Where(type =>
                    type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)));

                if (typesToAdd != null)
                    typeList.AddRange(typesToAdd);
            }

            return typeList;
        }
    }
}