using Supremes.Helper;
using System;
using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// A single key + value attribute.
    /// </summary>
    /// <remarks>
    /// Keys are trimmed and normalised to lower-case.
    /// </remarks>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class Attribute
    {
        private static readonly string[] booleanAttributes = new string[] { "allowfullscreen"
            , "async", "autofocus", "checked", "compact", "declare", "default", "defer", "disabled"
            , "formnovalidate", "hidden", "inert", "ismap", "itemscope", "multiple", "muted"
            , "nohref", "noresize", "noshade", "novalidate", "nowrap", "open", "readonly", "required"
            , "reversed", "seamless", "selected", "sortable", "truespeed", "typemustmatch" };

        private string key;

        private string value;

        /// <summary>
        /// Create a new attribute from unencoded (raw) key and value.
        /// </summary>
        /// <param name="key">attribute key</param>
        /// <param name="value">attribute value</param>
        /// <seealso cref="CreateFromEncoded(string, string)">
        /// CreateFromEncoded(string, string)
        /// </seealso>
        internal Attribute(string key, string value)
        {
            Validate.NotEmpty(key);
            Validate.NotNull(value);
            this.key = key.Trim().ToLower();
            this.value = value;
        }

        /// <summary>
        /// Get or set the attribute key.
        /// </summary>
        /// <returns>the attribute key</returns>
        /// <value>the new key; must not be null when set</value>
        public string Key
        {
            get { return key; }
            set
            {
                Validate.NotEmpty(value);
                this.key = value.Trim().ToLower();
            }
        }

        /// <summary>
        /// Get or set the attribute value.
        /// </summary>
        /// <returns>the attribute value</returns>
        /// <value>the new attribute value; must not be null when set</value>
        public string Value
        {
            get { return value; }
            set
            {
                Validate.NotNull(value);
                this.value = value;
            }
        }

        /// <summary>
        /// Get the HTML representation of this attribute.
        /// </summary>
        /// <remarks>
        /// e.g. <c>href="index.html"</c>.
        /// </remarks>
        /// <returns>HTML</returns>
        public string Html
        {
            get
            {
                StringBuilder accum = new StringBuilder();
                AppendHtmlTo(accum, (new Document(string.Empty)).OutputSettings);
                return accum.ToString();
            }
        }

        internal void AppendHtmlTo(StringBuilder accum, DocumentOutputSettings @out)
        {
            accum.Append(key);
            if (!ShouldCollapseAttribute(@out))
            {
                accum.Append("=\"");
                Entities.Escape(accum, value, Convert(@out.EscapeMode), @out.Charset, true, false, false);
                accum.Append('"');
            }
        }
        
        private static Entities.EscapeMode Convert(DocumentEscapeMode escapeMode)
        {
            switch (escapeMode)
            {
                case DocumentEscapeMode.Base:
                    return Entities.EscapeMode.Base;
                case DocumentEscapeMode.Extended:
                    return Entities.EscapeMode.Extended;
                case DocumentEscapeMode.Xhtml:
                    return Entities.EscapeMode.Xhtml;
                default:
                    return Entities.EscapeMode.Base;
            }
        }

        /// <summary>
        /// Get the string representation of this attribute, implemented as
        /// <see cref="Html">Html</see>
        /// .
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return Html;
        }

        /// <summary>
        /// Create a new Attribute from an unencoded key and a HTML attribute encoded value.
        /// </summary>
        /// <param name="unencodedKey">
        /// assumes the key is not encoded, as can be only run of simple \w chars.
        /// </param>
        /// <param name="encodedValue">HTML attribute encoded value</param>
        /// <returns>attribute</returns>
        internal static Attribute CreateFromEncoded(string unencodedKey, string encodedValue)
        {
            string value = Entities.Unescape(encodedValue, true);
            return new Supremes.Nodes.Attribute(unencodedKey, value);
        }

        internal bool IsDataAttribute()
        {
            return key.StartsWith(Attributes.dataPrefix, StringComparison.Ordinal)
                && key.Length > Attributes.dataPrefix.Length;
        }

        /// <summary>
        /// Collapsible if it's a boolean attribute and value is empty or same as name
        /// </summary>
        internal bool ShouldCollapseAttribute(DocumentOutputSettings @out)
        {
            return (string.Empty.Equals(value) || string.Equals(value, key, StringComparison.OrdinalIgnoreCase))
                && @out.Syntax == DocumentSyntax.Html
                && Array.BinarySearch(booleanAttributes, key) >= 0;
        }

        /// <summary>
        /// Compares two <see cref="Attribute"/> instances for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            Attribute attribute = obj as Attribute;
            if (attribute == null)
            {
                return false;
            }
            if (key != null ? !key.Equals(attribute.Key) : attribute.Key != null)
            {
                return false;
            }
            if (value != null ? !value.Equals(attribute.Value) : attribute.Value != null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int result = key != null ? key.GetHashCode() : 0;
            result = 31 * result + (value != null ? value.GetHashCode() : 0);
            return result;
        }

        internal Attribute Clone()
        {
            return (Attribute)this.MemberwiseClone();
            // only fields are immutable strings key and value, so no more deep copy required
        }
    }
}
