namespace Rimrock.Helios.Analysis
{
    using System;
    using System.IO;

    /// <summary>
    /// <see cref="Frame" /> formatter interface.
    /// </summary>
    internal interface IFrameFormatter
    {
        /// <summary>
        /// Writes the frame to the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="frame">The frame.</param>
        /// <param name="escaper">The option string escaper.</param>
        void Write(StreamWriter writer, Frame frame, Func<string, string>? escaper = null);
    }
}