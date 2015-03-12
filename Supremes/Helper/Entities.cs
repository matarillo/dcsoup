using Supremes.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Text;
using Utf32 = System.Int32;

namespace Supremes.Helper
{
    /// <summary>
    /// HTML entities, and escape routines.
    /// </summary>
    /// <remarks>
    /// Source: <a href="http://www.w3.org/TR/html5/named-character-references.html#named-character-references">W3C HTML named character references</a>.
    /// </remarks>
    internal static class Entities
    {
        internal enum EscapeMode
        {
            Xhtml,
            Base,
            Extended
        }
        
        private static readonly IDictionary<string, Utf32> full;

        private static readonly IDictionary<Utf32, string> xhtmlByVal;

        private static readonly IDictionary<string, Utf32> @base;

        private static readonly IDictionary<Utf32, string> baseByVal;

        private static readonly IDictionary<Utf32, string> fullByVal;

        private static IDictionary<Utf32, string> GetMap(EscapeMode escapeMode)
        {
            switch (escapeMode)
            {
                case EscapeMode.Base:
                    return baseByVal;

                case EscapeMode.Extended:
                    return fullByVal;

                case EscapeMode.Xhtml:
                    return xhtmlByVal;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Check if the input is a known named entity
        /// </summary>
        /// <param name="name">the possible entity name (e.g. "lt" or "amp")</param>
        /// <returns>true if a known named entity</returns>
        public static bool IsNamedEntity(string name)
        {
            return full.ContainsKey(name);
        }

        /// <summary>
        /// Check if the input is a known named entity in the base entity set.
        /// </summary>
        /// <param name="name">the possible entity name (e.g. "lt" or "amp")</param>
        /// <returns>true if a known named entity in the base set</returns>
        /// <seealso cref="IsNamedEntity(string)">IsNamedEntity(string)</seealso>
        public static bool IsBaseNamedEntity(string name)
        {
            return @base.ContainsKey(name);
        }

        /// <summary>
        /// Get the Character value of the named entity
        /// </summary>
        /// <param name="name">named entity (e.g. "lt" or "amp")</param>
        /// <returns>the Character value of the named entity (e.g. '&lt;' or '&amp;')</returns>
        public static Utf32 GetCharacterByName(string name)
        {
            return full[name];
        }

        internal static string Escape(string @string, EscapeMode escapeMode, Encoding charset)
        {
            StringBuilder accum = new StringBuilder(@string.Length * 2);
            Escape(accum, @string, escapeMode, charset, false, false, false);
            return accum.ToString();
        }

        // this method is ugly, and does a lot. but other breakups cause rescanning and stringbuilder generations
        internal static void Escape(StringBuilder accum, string @string, EscapeMode escapeMode, Encoding charset, bool inAttribute, bool normaliseWhite, bool stripLeadingWhite)
        {
            const int MIN_SUPPLEMENTARY_CODE_POINT = 0x10000;
            bool lastWasWhite = false;
            bool reachedNonWhite = false;
            CharsetEncoder encoder = new CharsetEncoder(charset);
            IDictionary<Utf32, string> map = GetMap(escapeMode);
            int length = @string.Length;
            Utf32 codePoint;
            for (int offset = 0; offset < length; offset += (codePoint < MIN_SUPPLEMENTARY_CODE_POINT ? 1 : 2))
            {
                codePoint = char.ConvertToUtf32(@string, offset);
                if (normaliseWhite)
                {
                    if (StringUtil.IsWhitespace(codePoint))
                    {
                        if ((stripLeadingWhite && !reachedNonWhite) || lastWasWhite)
                        {
                            continue;
                        }
                        accum.Append(' ');
                        lastWasWhite = true;
                        continue;
                    }
                    else
                    {
                        lastWasWhite = false;
                        reachedNonWhite = true;
                    }
                }
                // surrogate pairs, split implementation for efficiency on single char common case (saves creating strings, char[]):
                if (codePoint < MIN_SUPPLEMENTARY_CODE_POINT)
                {
                    char c = (char)codePoint;
                    switch (c)
                    {
                        case '&':
                            // html specific and required escapes:
                            accum.Append("&amp;");
                            break;

                        case '\u00A0':
                            if (escapeMode != EscapeMode.Xhtml)
                            {
                                accum.Append("&nbsp;");
                            }
                            else
                            {
                                accum.Append(c);
                            }
                            break;

                        case '<':
                            if (!inAttribute)
                            {
                                accum.Append("&lt;");
                            }
                            else
                            {
                                accum.Append(c);
                            }
                            break;

                        case '>':
                            if (!inAttribute)
                            {
                                accum.Append("&gt;");
                            }
                            else
                            {
                                accum.Append(c);
                            }
                            break;

                        case '"':
                            if (inAttribute)
                            {
                                accum.Append("&quot;");
                            }
                            else
                            {
                                accum.Append(c);
                            }
                            break;

                        default:
                            if (encoder.CanEncode(c))
                            {
                                accum.Append(c);
                            }
                            else if (map.ContainsKey(codePoint))
                            {
                                accum.Append('&').Append(map[codePoint]).Append(';');
                            }
                            else
                            {
                                accum.Append("&#x").AppendFormat("{0:x}", codePoint).Append(';');
                            }
                            break;
                    }
                }
                else
                {
                    string c = char.ConvertFromUtf32(codePoint);
                    if (encoder.CanEncode(c))
                    {
                        accum.Append(c);
                    }
                    else
                    {
                        accum.Append("&#x").AppendFormat("{0:x}", codePoint).Append(';');
                    }
                }
            }
        }

        internal static string Unescape(string @string)
        {
            return Unescape(@string, false);
        }

        /// <summary>
        /// Unescape the input string.
        /// </summary>
        /// <param name="string"></param>
        /// <param name="strict">if "strict" (that is, requires trailing ';' char, otherwise that's optional)
        /// </param>
        /// <returns></returns>
        internal static string Unescape(string @string, bool strict)
        {
            return Parser.UnescapeEntities(@string, strict);
        }

        static Entities()
        {
            // xhtml has restricted entities
            xhtmlByVal = new Dictionary<Utf32, string>()
            {
                { 0x0022, "quat" },
                { 0x0026, "amp" },
                { 0x003C, "lt" },
                { 0x003E, "gt" },
            };
            @base = LoadEntities("entities-base.properties"); // most common / default
            baseByVal = ToCharacterKey(@base);
            full = LoadEntities("entities-full.properties"); // extended and overblown.
            fullByVal = ToCharacterKey(full);
        }

        private static IDictionary<string, Utf32> LoadEntities(string filename)
        {
            Type thisType = typeof(Supremes.Helper.Entities);
            string resoucePath = thisType.Namespace + "." + filename;
            Stream @in = thisType.Assembly.GetManifestResourceStream(resoucePath);
            if (@in == null)
            {
                throw new MissingManifestResourceException("Error loading entities resource: " + filename);
            }
            Dictionary<string, Utf32> entities = new Dictionary<string, Utf32>();
            using (var reader = new StreamReader(@in, Encoding.UTF8))
            {
                for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                {
                    var pos = line.IndexOf('=');
                    if (pos >= 0)
                    {
                        var name = line.Substring(0, pos); /*substring*/
                        var value = line.Substring(pos + 1); /*substring*/
                        Utf32 codePoint = Convert.ToInt32(value, 16);
                        entities[name] = codePoint;
                    }
                }
            }
            return entities;
        }

        private static IDictionary<Utf32, string> ToCharacterKey(IDictionary<string, Utf32> inMap)
        {
            IDictionary<Utf32, string> outMap = new Dictionary<Utf32, string>();
            foreach (KeyValuePair<string, Utf32> entry in inMap)
            {
                Utf32 character = entry.Value;
                string name = entry.Key;
                if (outMap.ContainsKey(character))
                {
                    // dupe, prefer the lower case version
                    if (name.ToLower().Equals(name))
                    {
                        outMap[character] = name;
                    }
                }
                else
                {
                    outMap[character] = name;
                }
            }
            return outMap;
        }
    }
}
