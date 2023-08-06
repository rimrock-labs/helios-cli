namespace Rimrock.Helios.Common
{
    using System.IO;

    /// <summary>
    /// File system wrapper class.
    /// </summary>
    public class FileSystem
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly FileSystem Instance = new();

        /// <summary>
        /// <see href="System.IO.FileStream" /> wrapper.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="access">The access.</param>
        /// <returns>The stream.</returns>
        public virtual Stream FileOpen(string path, FileMode mode, FileAccess access) =>
            new FileStream(path, mode, access);
    }
}