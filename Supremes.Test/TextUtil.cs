
using System;

namespace Supremes.Test
{
    /// <summary>
    /// </summary>
    public static class TextUtil
    {
        public static string StripNewlines(string text)
        {
            text = text.Replace("\\r?\\n\\s*", "");
            return text;
        }
    }
}
