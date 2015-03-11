/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using System;
using System.Text;

namespace Supremes.Helper
{
    /// <summary>
    /// A minimal String utility class.
    /// </summary>
    /// <remarks>
    /// Designed for internal jsoup use only.
    /// </remarks>
    internal static class StringUtil
    {
        private static readonly string[] padding = new string[] {
            string.Empty,
            " ",
            "  ",
            "   ",
            "    ",
            "     ",
            "      ",
            "       ",
            "        ",
            "         ",
            "          "
        }; // memoised padding up to 10

        /// <summary>
        /// Returns space padding
        /// </summary>
        /// <param name="width">amount of padding desired</param>
        /// <returns>string of spaces * width</returns>
        public static string Padding(int width)
        {
            if (width < 0)
            {
                throw new ArgumentException("width must be > 0");
            }
            if (width < padding.Length)
            {
                return padding[width];
            }
            char[] @out = new char[width];
            for (int i = 0; i < width; i++)
            {
                @out[i] = ' ';
            }
            return @out.ToString();
        }

        /// <summary>
        /// Tests if a string is numeric
        /// </summary>
        /// <remarks>
        /// i.e. contains only digit characters
        /// </remarks>
        /// <param name="string">string to test</param>
        /// <returns>
        /// true if only digit chars, false if empty or null or contains non-digit chrs
        /// </returns>
        public static bool IsNumeric(string @string)
        {
			if (string.IsNullOrEmpty(@string)) {
				return false;
			}
            int l = @string.Length;
            for (int i = 0; i < l; i++)
            {
                if (!char.IsDigit(@string[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests if a code point is "whitespace" as defined in the HTML spec.
        /// </summary>
        /// <param name="c">code point to test</param>
        /// <returns>true if code point is whitespace, false otherwise</returns>
        public static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\f' || c == '\r';
        }

        public static bool IsWhitespace(int codePoint)
        {
            return (codePoint < 0x10000 && IsWhitespace((char)codePoint));
        }

        /// <summary>
        /// Normalise the whitespace within this string
        /// </summary>
        /// <remarks>
        /// multiple spaces collapse to a single, and all whitespace characters
        /// (e.g. newline, tab) convert to a simple space
        /// </remarks>
        /// <param name="string">content to normalise</param>
        /// <returns>normalised string</returns>
        public static string NormaliseWhitespace(string @string)
        {
            StringBuilder sb = new StringBuilder(@string.Length);
            AppendNormalisedWhitespace(sb, @string, false);
            return sb.ToString();
        }

        /// <summary>
        /// After normalizing the whitespace within a string, appends it to a string builder.
        /// </summary>
        /// <param name="accum">builder to append to</param>
        /// <param name="string">string to normalize whitespace within</param>
        /// <param name="stripLeading">
        /// set to true if you wish to remove any leading whitespace
        /// </param>
        /// <returns></returns>
        public static void AppendNormalisedWhitespace(StringBuilder accum, string @string, bool stripLeading)
        {
            bool lastWasWhite = false;
            bool reachedNonWhite = false;
            int len = @string.Length;
            char c;
            for (int i = 0; i < len; i++)
            {
                c = @string[i];
                if (IsWhitespace(c))
                {
                    if ((stripLeading && !reachedNonWhite) || lastWasWhite)
                    {
                        continue;
                    }
                    accum.Append(' ');
                    lastWasWhite = true;
                }
                else
                {
                    accum.Append(c);
                    lastWasWhite = false;
                    reachedNonWhite = true;
                }
            }
        }

        public static bool In(string needle, params string[] haystack)
        {
            foreach (string hay in haystack)
            {
                if (hay.Equals(needle))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
