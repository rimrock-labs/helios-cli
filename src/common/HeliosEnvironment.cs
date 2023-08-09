namespace Rimrock.Helios.Common
{
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// Helios environment class.
    /// </summary>
    public class HeliosEnvironment
    {
        /// <summary>
        /// Gets the application directory.
        /// </summary>
        public virtual string ApplicationDirectory { get; } = GetApplicationDirectory();

        private static string GetApplicationDirectory() =>
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    }
}