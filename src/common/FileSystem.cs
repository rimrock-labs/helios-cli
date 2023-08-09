namespace Rimrock.Helios.Common
{
    using System.IO;

    /// <summary>
    /// File system wrapper class.
    /// </summary>
    public class FileSystem
    {
        /// <summary>
        /// <see cref="System.IO.Directory.CreateDirectory(string)"/> wrapper.
        /// </summary>
        /// <param name="path">The path.</param>
        public virtual void CreateDirectory(string path) =>
            Directory.CreateDirectory(path);

        /// <summary>
        /// <see cref="System.IO.File.Exists(string?)"/> wrapper.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>true if file exists, false otherwise.</returns>
        public virtual bool FileExists(string path) =>
            File.Exists(path);

        /// <summary>
        /// <see cref="System.IO.FileStream.FileStream(string, FileMode, FileAccess)" /> wrapper.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="access">The access.</param>
        /// <returns>The stream.</returns>
        public virtual Stream FileOpen(string path, FileMode mode, FileAccess access) =>
            new FileStream(path, mode, access);

        /// <summary>
        /// <see cref="System.IO.File.ReadAllText(string)"/> wrapper.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The file contents.</returns>
        public virtual string FileReadAllText(string path) =>
            File.ReadAllText(path);
    }
}