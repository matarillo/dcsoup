using Supremes.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// Specifies the document's current HTML escape mode.
    /// </summary>
    public enum DocumentEscapeMode
    {
        /// <summary>
        /// Restricted entities suitable for XHTML output: lt, gt, amp, and quot only.
        /// </summary>
        Xhtml,
        /// <summary>
        /// Default HTML output entities.
        /// </summary>
        Base,
        /// <summary>
        /// Complete HTML entities.
        /// </summary>
        Extended
    }

    /// <summary>
    /// Specifies the output serialization syntax.
    /// </summary>
    public enum DocumentSyntax
    {
        /// <summary>
        /// Serialize according to the HTML rules.
        /// </summary>
        Html,
        /// <summary>
        /// Serialize according to the XML rules.
        /// </summary>
        Xml
    }

    /// <summary>
    /// Specifies how browsers should display an HTML file.
    /// </summary>
    public enum DocumentQuirksMode
    {
        /// <summary>
        /// Follows Web standards.
        /// </summary>
        NoQuirks,
        /// <summary>
        /// Emulate non-standard behavior of older browsers.
        /// </summary>
        Quirks,
        /// <summary>
        /// Almost follows Web standards except few behaviors.
        /// </summary>
        LimitedQuirks
    }
    
    /// <summary>
    /// A Document's output settings control the form of the text() and html() methods.
    /// </summary>
    public sealed class DocumentOutputSettings
    {
        private DocumentEscapeMode escapeMode = DocumentEscapeMode.Base;

        private Encoding charset = Encoding.UTF8;

        //private CharsetEncoder charsetEncoder = charset.NewEncoder();

        private bool prettyPrint = true;

        private bool outline = false;

        private int indentAmount = 1;

        private DocumentSyntax syntax = DocumentSyntax.Html;

        internal DocumentOutputSettings()
        {
        }

        /// <summary>
        /// Get or Set the document's current HTML escape mode,
        /// which determines how characters are escaped
        /// when the output character set does not support a given character.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>Base</c>, which provides a limited set of named HTML entities
        /// and escapes other characters as numbered entities for maximum compatibility;
        /// or <c>Extended</c>, which uses the complete set of HTML named entities.
        /// </para>
        /// <para>
        /// The default escape mode is <c>Base</c>.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>the new escape mode to use</value>
        /// <returns>the document's current escape mode</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public DocumentEscapeMode EscapeMode
        {
            get { return escapeMode; }
            set { escapeMode = value; }
        }

        /// <summary>
        /// Get or Set the document's current output charset, 
        /// which is used to control which characters are escaped
        /// when generating HTML (via the <c>Html</c> properties),
        /// and which are kept intact.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Where possible (when parsing from a URL or File),
        /// the document's output charset is automatically set to the input charset.
        /// Otherwise, it defaults to utf-8.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>the new charset to use</value>
        /// <returns>the document's current charset</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public Encoding Charset
        {
            get { return charset; }
            set { charset = value; }
        }

        /// <summary>
        /// Get or Set the document's current output syntax.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Either <c>Html</c>, with empty tags and boolean attributes (etc),
        /// or <c>Xml</c>, with self-closing tags.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>serialization syntax</value>
        /// <returns>current syntax</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public DocumentSyntax Syntax
        {
            get { return syntax; }
            set { syntax = value; }
        }

        /// <summary>
        /// Get or Set if pretty printing is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default is true. If disabled, the HTML output methods will not re-format
        /// the output, and the output will generally look like the input.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>new pretty print setting</value>
        /// <returns>if pretty printing is enabled.</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public bool PrettyPrint
        {
            get { return prettyPrint; }
            set { prettyPrint = value; }
        }

        /// <summary>
        /// Get or Set if outline mode is enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default is false.
        /// If enabled, the HTML output methods will consider all tags as block.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>new outline setting</value>
        /// <returns>true if outline mode is enabled</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public bool Outline
        {
            get { return outline; }
            set { outline = value; }
        }

        /// <summary>
        /// Get or Set the current tag indent amount, used when pretty printing.
        /// </summary>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// <value>number of spaces to use for indenting each level. Must be &gt;= 0</value>
        /// <returns>the current indent amount</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public int IndentAmount
        {
            get { return indentAmount; }
            set
            {
                Validate.IsTrue(indentAmount >= 0);
                indentAmount = value;
            }
        }

        internal DocumentOutputSettings Clone()
        {
            DocumentOutputSettings clone;
            clone = (DocumentOutputSettings)this.MemberwiseClone();
            clone.Charset = charset;
            // new charset and charset encoder
            clone.escapeMode = escapeMode;
            // indentAmount, prettyPrint are primitives so object.clone() will handle
            return clone;
        }
    }

    /// <summary>
    /// A HTML Document.
    /// </summary>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class Document : Element
    {
        private DocumentOutputSettings outputSettings = new DocumentOutputSettings();

        private DocumentQuirksMode quirksMode = DocumentQuirksMode.NoQuirks;

        private string location;

        /// <summary>
        /// Create a new, empty Document.
        /// </summary>
        /// <param name="baseUri">base URI of document</param>
        /// <seealso cref="Supremes.Dcsoup.Parse(string)">Supremes.Dcsoup.Parse(string)</seealso>
        /// <seealso cref="CreateShell(string)">CreateShell(string)</seealso>
        internal Document(string baseUri) : base(Supremes.Nodes.Tag.ValueOf("#root"), baseUri)
        {
            this.location = baseUri;
        }

        /// <summary>
        /// Create a valid, empty shell of a document, suitable for adding more elements to.
        /// </summary>
        /// <param name="baseUri">baseUri of document</param>
        /// <returns>document with html, head, and body elements.</returns>
        public static Document CreateShell(string baseUri)
        {
            Validate.NotNull(baseUri);
            Document doc = new Supremes.Nodes.Document(baseUri);
            Element html = doc.AppendElement("html");
            html.AppendElement("head");
            html.AppendElement("body");
            return doc;
        }

        /// <summary>
        /// Get the URL this Document was parsed from.
        /// </summary>
        /// <remarks>
        /// If the starting URL is a redirect,
        /// this will return the final URL from which the document was served from.
        /// </remarks>
        /// <returns>location</returns>
        public string Location
        {
            get { return location; }
        }

        /// <summary>
        /// Accessor to the document's
        /// <c>head</c>
        /// element.
        /// </summary>
        /// <returns>
        /// <c>head</c>
        /// </returns>
        public Element Head
        {
            get { return FindFirstElementByTagName("head", this); }
        }

        /// <summary>
        /// Accessor to the document's <c>body</c> element.
        /// </summary>
        /// <returns><c>body</c></returns>
        public Element Body
        {
            get { return FindFirstElementByTagName("body", this); }
        }

        /// <summary>
        /// Get or Set the string contents of the document's <c>title</c> element.
        /// </summary>
        /// <remarks>
        /// when set, updates the existing element,
        /// or adds <c>title</c> to <c>head</c> if not present
        /// </remarks>
        /// <value>string to set as title</value>
        /// <returns>Trimmed title, or empty string if none set.</returns>
        public string Title
        {
            get
            {
                // title is a preserve whitespace tag (for document output), but normalised here
                Element titleEl = GetElementsByTag("title").First;
                return titleEl != null ? StringUtil.NormaliseWhitespace(titleEl.Text).Trim() :
                    string.Empty;
            }
            set
            {
                Validate.NotNull(value);
                Element titleEl = GetElementsByTag("title").First;
                if (titleEl == null)
                {
                    // add to head
                    Head.AppendElement("title").Text = value;
                }
                else
                {
                    titleEl.Text = value;
                }
            }
        }

        /// <summary>
        /// Create a new Element, with this document's base uri.
        /// </summary>
        /// <remarks>
        /// Does not make the new element a child of this document.
        /// </remarks>
        /// <param name="tagName">
        /// element tag name (e.g.
        /// <c>a</c>
        /// )
        /// </param>
        /// <returns>new element</returns>
        public Element CreateElement(string tagName)
        {
            Tag tag = Supremes.Nodes.Tag.ValueOf(tagName);
            return new Element(tag, this.BaseUri);
        }

        /// <summary>
        /// Normalise the document.
        /// </summary>
        /// <remarks>
        /// This happens after the parse phase so generally does not need to be called.
        /// Moves any text content that is not in the body element into the body.
        /// </remarks>
        /// <returns>this document after normalisation</returns>
        internal Document Normalise()
        {
            Element htmlEl = FindFirstElementByTagName("html", this);
            if (htmlEl == null)
            {
                htmlEl = AppendElement("html");
            }
            if (Head == null)
            {
                htmlEl.PrependElement("head");
            }
            if (Body == null)
            {
                htmlEl.AppendElement("body");
            }
            // pull text nodes out of root, html, and head els, and push into body. non-text nodes are already taken care
            // of. do in inverse order to maintain text order.
            NormaliseTextNodes(Head);
            NormaliseTextNodes(htmlEl);
            NormaliseTextNodes(this);
            NormaliseStructure("head", htmlEl);
            NormaliseStructure("body", htmlEl);
            return this;
        }

        // does not recurse.

        private void NormaliseTextNodes(Element element)
        {
            List<Node> toMove = element.childNodes
                .OfType<TextNode>()
                .Where(n => !n.IsBlank)
                .Cast<Node>()
                .ToList();
            for (int i = toMove.Count - 1; i >= 0; i--)
            {
                Node node_1 = toMove[i];
                element.RemoveChild(node_1);
                Body.PrependChild(new TextNode(" ", string.Empty));
                Body.PrependChild(node_1);
            }
        }

        // merge multiple <head> or <body> contents into one, delete the remainder, and ensure they are owned by <html>

        private void NormaliseStructure(string tag, Element htmlEl)
        {
            Elements elements = this.GetElementsByTag(tag);
            Element master = elements.First;
            // will always be available as created above if not existent
            if (elements.Count > 1)
            {
                // dupes, move contents to master
                List<Node> toMove = new List<Node>();
                for (int i = 1; i < elements.Count; i++)
                {
                    Node dupe = elements[i];
                    foreach (Node node in dupe.ChildNodes)
                    {
                        toMove.Add(node);
                    }
                    dupe.Remove();
                }
                foreach (Node dupe_1 in toMove)
                {
                    master.AppendChild(dupe_1);
                }
            }
            // ensure parented by <html>
            if (!master.Parent.Equals(htmlEl))
            {
                htmlEl.AppendChild(master);
            }
        }

        // includes remove()
        // fast method to get first by tag name, used for html, head, body finders

        private Element FindFirstElementByTagName(string tag, Node node)
        {
            if (node.NodeName.Equals(tag))
            {
                return (Element)node;
            }
            else
            {
                foreach (Node child in node.ChildNodes)
                {
                    Element found = FindFirstElementByTagName(tag, child);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get the outer HTML of this document.
        /// </summary>
        /// <returns></returns>
        public sealed override string OuterHtml
        {
            get { return base.Html; } // no outer wrapper tag
        }

        /// <summary>
        /// Get or Set the combined text of this element and all its children.
        /// </summary>
        /// <remarks>
        /// <para>
        /// when get, whitespace is normalized and trimmed.
        /// <p/>
        /// For example, given HTML
        /// <c>&lt;p&gt;Hello  &lt;b&gt;there&lt;/b&gt; now! &lt;/p&gt;</c>,
        /// <c>p.Text</c> returns <c>"Hello there now!"</c>
        /// </para>
        /// <para>
        /// when set, any existing contents (text or elements) will be cleared.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>unencoded text</value>
        /// <returns>unencoded text, or empty string if none.</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public override string Text
        {
            get { return base.Text; }
            set { Body.Text = value; } // overridden to not nuke doc structure
        }

        internal override string NodeName
        {
        	get { return "#document"; }
        }

        internal override sealed Node Clone()
        {
            return PrivateClone();
        }

        private Document PrivateClone()
        {
            Document clone = (Document)base.Clone();
            clone.outputSettings = this.outputSettings.Clone();
            return clone;
        }

        /// <summary>
        /// Get or Set the document's current output settings.
        /// </summary>
        /// <remarks>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </remarks>
        /// <value>new output settings</value>
        /// <returns>the document's current output settings</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public DocumentOutputSettings OutputSettings
        {
            get { return outputSettings; }
            set
            {
                Validate.NotNull(outputSettings);
                outputSettings = value;
            }
        }

        /// <summary>
        /// Get or Set the document's quirks mode.
        /// </summary>
        /// <remarks>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </remarks>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public DocumentQuirksMode QuirksMode
        {
            get { return quirksMode; }
            set { quirksMode = value; }
        }
    }
}
