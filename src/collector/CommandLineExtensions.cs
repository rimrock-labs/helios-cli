namespace Rimrock.Helios.Common
{
    using System.CommandLine;
    using System.CommandLine.Parsing;
    using System.Diagnostics.CodeAnalysis;

    internal static class CommandLineExtensions
    {
        public delegate bool TryParse<TValue>(string @in, out TValue @out);

        public static bool TryGetParsedOptionValue<TValue>(
            this ParseResult result,
            Option<string> option,
            TryParse<TValue> parser,
            [NotNullWhen(true)] out TValue? value)
        {
            bool result2 = false;
            value = default;
            string? optionValue = result.CommandResult.GetValueForOption(option);
            if (!string.IsNullOrEmpty(optionValue) &&
                parser(optionValue, out value))
            {
                result2 = true;
            }

            return result2;
        }
    }
}