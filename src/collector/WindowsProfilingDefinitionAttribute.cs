namespace Rimrock.Helios.Analysis
{
    using System;

    /// <summary>
    /// Windows profiling definition attribute class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WindowsProfilingDefinitionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsProfilingDefinitionAttribute"/> class.
        /// </summary>
        /// <param name="dataSetName">The data set name.</param>
        public WindowsProfilingDefinitionAttribute(string dataSetName)
        {
            this.DataSetName = dataSetName;
            this.KernelEvents = Array.Empty<string>();
            this.ClrEvents = Array.Empty<string>();
        }

        /// <summary>
        /// Gets the data set name.
        /// </summary>
        public string DataSetName { get; }

        /// <summary>
        /// Gets or sets the Windows ETW Kernel events required.
        /// </summary>
        public string[] KernelEvents { get; set; }

        /// <summary>
        /// Gets or sets the Windows CLR events required.
        /// </summary>
        public string[] ClrEvents { get; set; }
    }
}