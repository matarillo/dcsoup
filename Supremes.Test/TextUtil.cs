
using System;
using System.Text.RegularExpressions;

namespace Supremes.Test
{
    /// <summary>
    /// </summary>
    public static class TextUtil
    {
        public static string StripNewlines(string text)
        {
            text = Regex.Replace(text, "\\r?\\n\\s*", "");
            return text;
        }
    }
}
