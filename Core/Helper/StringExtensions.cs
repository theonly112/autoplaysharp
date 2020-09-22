using F23.StringSimilarity;
using System.Text.RegularExpressions;

namespace autoplaysharp.Core.Helper
{
    internal static class StringExtensions
    {
        internal static Regex StatusRegex = new Regex(@"(?<Current>\d*)\s*/\s*(?<Max>\d*)");

        /// <summary>
        /// Tries to parse the content of a string like this 'X/Y'
        /// </summary>
        public static (bool Success, int Current, int Max) TryParseStatus(this string str)
        {
            var match = StatusRegex.Match(str);
            if (!match.Success)
            {
                return (false,-1,-1);
            }

            var success = int.TryParse(match.Groups["Current"].Value, out var current);
            success &= int.TryParse(match.Groups["Max"].Value, out var max);
            return (success, current, max);
        }

        public static double Similarity(this string expected, string actual)
        {
            var nl = new NormalizedLevenshtein();
            return nl.Similarity(expected, actual);
        }
    }
}
