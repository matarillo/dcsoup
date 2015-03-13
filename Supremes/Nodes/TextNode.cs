using Supremes.Helper;
using System.Text;
using System.Text.RegularExpressions;

namespace Supremes.Nodes
{
    /// <summary>
    /// A text node.
    /// </summary>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class TextNode : Node
    {
        private const string TEXT_KEY = "text";

        internal string text;

        /// <summary>
        /// Create a new TextNode representing the supplied (unencoded) text).
        /// </summary>
        /// <param name="text">raw text</param>
        /// <param name="baseUri">base uri</param>
        /// <seealso cref="CreateFromEncoded(string, string)">CreateFromEncoded(string, string)
        /// </seealso>
        internal TextNode(string text, string baseUri)
        {
            this.baseUri = baseUri;
            this.text = text;
        }

        internal override string NodeName
        {
        	get { return "#text"; }
        }

        /// <summary>
        /// Get the text content of this text node.
        /// </summary>
        /// <returns>Unencoded, normalised text.</returns>
        /// <seealso cref="TextNode.GetWholeText()">TextNode.GetWholeText()</seealso>
        public string Text()
        {
            return NormaliseWhitespace(GetWholeText());
        }

        /// <summary>
        /// Set the text content of this text node.
        /// </summary>
        /// <param name="text">unencoded text</param>
        /// <returns>this, for chaining</returns>
        public TextNode Text(string text)
        {
            this.text = text;
            if (attributes != null)
            {
                attributes[TEXT_KEY] = text;
            }
            return this;
        }

        /// <summary>
        /// Get the (unencoded) text of this text node, including any newlines and spaces present in the original.
        /// </summary>
        /// <returns>text</returns>
        public string GetWholeText()
        {
            return attributes == null ? text : attributes[TEXT_KEY];
        }

        /// <summary>
        /// Test if this text node is blank -- that is, empty or only whitespace (including newlines).
        /// </summary>
        /// <returns>true if this document is empty or only whitespace, false if it contains any text content.
        /// </returns>
        public bool IsBlank()
        {
            return string.IsNullOrWhiteSpace(GetWholeText());
        }

        /// <summary>
        /// Split this text node into two nodes at the specified string offset.
        /// </summary>
        /// <remarks>
        /// After splitting, this node will contain the
        /// original text up to the offset, and will have a new text node sibling containing the text after the offset.
        /// </remarks>
        /// <param name="offset">string offset point to split node at.</param>
        /// <returns>the newly created text node containing the text after the offset.</returns>
        internal TextNode SplitText(int offset)
        {
            Validate.IsTrue(offset >= 0, "Split offset must be not be negative");
            Validate.IsTrue(offset < text.Length, "Split offset must not be greater than current text length");
            string head = GetWholeText().Substring(0, offset); /*substring*/
            string tail = GetWholeText().Substring(offset); /*substring*/
            Text(head);
            TextNode tailNode = new Supremes.Nodes.TextNode(tail, this.BaseUri());
            if (Parent() != null)
            {
                ((Node)Parent()).AddChildren(SiblingIndex() + 1, tailNode);
            }
            return tailNode;
        }

        internal override void OuterHtmlHead(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            if (@out.PrettyPrint()
                && ((SiblingIndex() == 0
                        && parentNode is Element
                        && ((Element)parentNode).Tag().FormatAsBlock()
                        && !IsBlank())
                    || (@out.Outline()
                        && SiblingNodes().Count > 0
                        && !IsBlank())))
            {
                Indent(accum, depth, @out);
            }
            bool normaliseWhite = @out.PrettyPrint()
                && Parent() is Element
                && !Element.PreserveWhitespace((Element)Parent());
            Entities.Escape(accum, GetWholeText(), Convert(@out.EscapeMode()), @out.Charset(), false, normaliseWhite, false);
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
 
        internal override void OuterHtmlTail(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
        }

        /// <summary>
        /// Converts the value of this instance to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return OuterHtml();
        }

        /// <summary>
        /// Create a new TextNode from HTML encoded (aka escaped) data.
        /// </summary>
        /// <param name="encodedText">Text containing encoded HTML (e.g. &amp;lt;)</param>
        /// <param name="baseUri">base URI</param>
        /// <returns>TextNode containing unencoded data (e.g. &lt;)</returns>
        internal static TextNode CreateFromEncoded(string encodedText, string baseUri)
        {
            string text = Entities.Unescape(encodedText);
            return new Supremes.Nodes.TextNode(text, baseUri);
        }

        internal static string NormaliseWhitespace(string text)
        {
            text = StringUtil.NormaliseWhitespace(text);
            return text;
        }

        internal static string StripLeadingWhitespace(string text)
        {
            return Regex.Replace(text, "^\\s+", string.Empty); //text.ReplaceFirst("^\\s+", string.Empty);
        }

        internal static bool LastCharIsWhitespace(StringBuilder sb)
        {
            return sb.Length != 0 && sb[sb.Length - 1] == ' ';
        }

        private void EnsureAttributes()
        {
            // attribute fiddling. create on first access.
            if (attributes == null)
            {
                attributes = new Attributes();
                attributes[TEXT_KEY] = text;
            }
        }

        /// <summary>
        /// Get an attribute's value by its key.
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        public override string Attr(string attributeKey)
        {
            EnsureAttributes();
            return base.Attr(attributeKey);
        }

        /// <summary>
        /// Get all of the element's attributes.
        /// </summary>
        /// <returns></returns>
        public override Supremes.Nodes.Attributes Attributes()
        {
            EnsureAttributes();
            return base.Attributes();
        }

        /// <summary>
        /// Set an attribute (key=value).
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public override Node Attr(string attributeKey, string attributeValue)
        {
            EnsureAttributes();
            return base.Attr(attributeKey, attributeValue);
        }

        /// <summary>
        /// Test if this element has an attribute.
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        public override bool HasAttr(string attributeKey)
        {
            EnsureAttributes();
            return base.HasAttr(attributeKey);
        }

        /// <summary>
        /// Remove an attribute from this element.
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        public override Node RemoveAttr(string attributeKey)
        {
            EnsureAttributes();
            return base.RemoveAttr(attributeKey);
        }

        /// <summary>
        /// Get an absolute URL from a URL attribute that may be relative.
        /// </summary>
        /// <param name="attributeKey">The attribute key</param>
        /// <returns></returns>
        public override string AbsUrl(string attributeKey)
        {
            EnsureAttributes();
            return base.AbsUrl(attributeKey);
        }
    }
}
