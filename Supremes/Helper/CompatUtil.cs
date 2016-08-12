using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Supremes.Helper
{
    internal static class CompatUtil
    {
        internal static Regex CreateCompiledRegex(string pattern)
        {
#if NET45
            return new Regex(pattern, RegexOptions.Compiled);
#else
            return new Regex(pattern);
#endif
        }

        internal static Regex CreateCompiledRegex(string pattern, RegexOptions options)
        {
#if NET45
            return new Regex(pattern, RegexOptions.Compiled | options);
#else
            return new Regex(pattern, options);
#endif
        }

        internal static string ToUpperInvariantCulture(string text)
        {
#if NET45
            return text.ToUpper(CultureInfo.InvariantCulture);
#else
            return text.ToUpper();
#endif
        }

        internal static string ToLowerInvariantCulture(string text)
        {
#if NET45
            return text.ToLower(CultureInfo.InvariantCulture);
#else
            return text.ToLower();
#endif
        }

        internal static bool IsKnownScheme(string scheme)
        {
#if NET45
            return UriParser.IsKnownScheme(scheme);
#else
            return string.Equals(scheme, "http", StringComparison.OrdinalIgnoreCase)
                || string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase);
#endif
        }
    }
}
