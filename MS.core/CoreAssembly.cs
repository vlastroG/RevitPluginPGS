using System.Reflection;

namespace MS.core
{
    /// <summary>
    /// The core assembly helper methods.
    /// </summary>
    public static class CoreAssembly
    {
        /// <summary>
        /// Gets the core assembly location.
        /// </summary>
        /// <returns></returns>
        public static string GetAssemblyLocation()
        {
            return Assembly.GetExecutingAssembly().Location;
        }
    }
}
