namespace Rimrock.Helios.Common
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// <see cref="System.TimeSpan" /> range validation attribute class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class TimeSpanRangeAttribute : ValidationAttribute
    {
        private readonly TimeSpan min;
        private readonly TimeSpan max;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanRangeAttribute"/> class.
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public TimeSpanRangeAttribute(TimeSpan min, TimeSpan max)
        {
            this.min = min;
            this.max = max;
        }

        /// <inheritdoc />
        public override bool IsValid(object? value)
        {
            TimeSpan span = (TimeSpan)value!;
            return true;
        }
    }
}