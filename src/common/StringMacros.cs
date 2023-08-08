namespace Rimrock.Helios.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// String macros class.
    /// </summary>
    public class StringMacros : Dictionary<string, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringMacros"/> class.
        /// </summary>
        /// <param name="defaults">The optional defaults.</param>
        public StringMacros(DefaultValues? defaults = null)
            : base()
        {
            defaults ??= new DefaultValues();
            DateTimeOffset dateTime = defaults.TimeStamp;
            this["date:year"] = dateTime.ToString("yyyy");
            this["date:month"] = dateTime.ToString("MM");
            this["date:day"] = dateTime.ToString("dd");
            this["time:hours"] = dateTime.ToString("HH");
            this["time:minutes"] = dateTime.ToString("mm");
            this["time:seconds"] = dateTime.ToString("ss");

            this["guid"] = defaults.Guid.ToString();

            this["pwd"] = defaults.ApplicationDirectory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMacros"/> class.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        public StringMacros(IDictionary<string, string> dictionary)
            : base(dictionary)
        {
        }

        /// <summary>
        /// Expands the macros in the string.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The expanded string.
        /// </returns>
        public string Expand(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                foreach (KeyValuePair<string, string> pair in this)
                {
                    value = value.Replace($"{{{pair.Key}}}", pair.Value);
                }
            }

            return value;
        }

        /// <summary>
        /// Default macro values.
        /// </summary>
        public class DefaultValues
        {
            /// <summary>
            /// Gets or sets the application directory.
            /// </summary>
            public string ApplicationDirectory { get; set; } = HeliosEnvironment.Instance.ApplicationDirectory;

            /// <summary>
            /// Gets or sets the time stamp.
            /// </summary>
            public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;

            /// <summary>
            /// Gets or sets the guid.
            /// </summary>
            public Guid Guid { get; set; } = Guid.NewGuid();
        }
    }
}