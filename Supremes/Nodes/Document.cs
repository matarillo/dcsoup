using Supremes.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Supremes.Nodes
{
    public enum DocumentEscapeMode
    {
        Xhtml,
        Base,
        Extended
    }

    public enum DocumentSyntax
    {
        Html,
        Xml
    }

    public enum DocumentQuirksMode
    {
        NoQuirks,
        Quirks,
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
        /// Get the document's current HTML escape mode: <code>base</code>, which provides a limited set of named HTML
        /// entities and escapes other characters as numbered entities for maximum compatibility; or <code>extended</code>,
        /// which uses the complete set of HTML named entities.
        /// </summary>
        /// <remarks>
        /// The default escape mode is <code>base</code>.
        /// </remarks>
        /// <returns>the document's current escape mode</returns>
        public DocumentEscapeMode EscapeMode()
        {
            return escapeMode;
        }

        /// <summary>
        /// Set the document's escape mode, which determines how characters are escaped when the output character set
        /// does not support a given character:- using either a named or a numbered escape.
        /// </summary>
        /// <param name="escapeMode">the new escape mode to use</param>
        /// <returns>the document's output settings, for chaining</returns>
        public DocumentOutputSettings EscapeMode(DocumentEscapeMode escapeMode)
        {
            this.escapeMode = escapeMode;
            return this;
        }

        /// <summary>
        /// Get the document's current output charset, which is used to control which characters are escaped when
        /// generating HTML (via the <code>html()</code> methods), and which are kept intact.
        /// </summary>
        /// <remarks>
        /// Where possible (when parsing from a URL or File), the document's output charset is automatically set to the
        /// input charset. Otherwise, it defaults to UTF-8.
        /// </remarks>
        /// <returns>the document's current charset.</returns>
        public Encoding Charset()
        {
            return charset;
        }

        /// <summary>
        /// Update the document's output charset.
        /// </summary>
        /// <param name="charset">the new charset to use.</param>
        /// <returns>the document's output settings, for chaining</returns>
        public DocumentOutputSettings Charset(Encoding charset)
        {
            // todo: this should probably update the doc's meta charset
            this.charset = charset;
            //charsetEncoder = charset.NewEncoder();
            return this;
        }

        /// <summary>
        /// Update the document's output charset.
        /// </summary>
        /// <param name="charset">the new charset (by name) to use.</param>
        /// <returns>the document's output settings, for chaining</returns>
        public DocumentOutputSettings Charset(string charset)
        {
            Charset(Encoding.GetEncoding(charset)); // may throw an exception
            return this;
        }

        /// <summary>
        /// Get the document's current output syntax.
        /// </summary>
        /// <returns>current syntax</returns>
        public DocumentSyntax Syntax()
        {
            return syntax;
        }

        /// <summary>
        /// Set the document's output syntax.
        /// </summary>
        /// <remarks>
        /// Either
        /// <code>html</code>
        /// , with empty tags and boolean attributes (etc), or
        /// <code>xml</code>
        /// , with self-closing tags.
        /// </remarks>
        /// <param name="syntax">serialization syntax</param>
        /// <returns>the document's output settings, for chaining</returns>
        public DocumentOutputSettings Syntax(DocumentSyntax syntax)
        {
            this.syntax = syntax;
            return this;
        }

        /// <summary>
        /// Get if pretty printing is enabled.
        /// </summary>
        /// <remarks>
        /// Default is true. If disabled, the HTML output methods will not re-format
        /// the output, and the output will generally look like the input.
        /// </remarks>
        /// <returns>if pretty printing is enabled.</returns>
        public bool PrettyPrint()
        {
            return prettyPrint;
        }

        /// <summary>
        /// Enable or disable pretty printing.
        /// </summary>
        /// <param name="pretty">new pretty print setting</param>
        /// <returns>this, for chaining</returns>
        public DocumentOutputSettings PrettyPrint(bool pretty)
        {
            prettyPrint = pretty;
            return this;
        }

        /// <summary>
        /// Get if outline mode is enabled.
        /// </summary>
        /// <remarks>
        /// Default is false. If enabled, the HTML output methods will consider
        /// all tags as block.
        /// </remarks>
        /// <returns>if outline mode is enabled.</returns>
        public bool Outline()
        {
            return outline;
        }

        /// <summary>
        /// Enable or disable HTML outline mode.
        /// </summary>
        /// <param name="outlineMode">new outline setting</param>
        /// <returns>this, for chaining</returns>
        public DocumentOutputSettings Outline(bool outlineMode)
        {
            outline = outlineMode;
            return this;
        }

        /// <summary>
        /// Get the current tag indent amount, used when pretty printing.
        /// </summary>
        /// <returns>the current indent amount</returns>
        public int IndentAmount()
        {
            return indentAmount;
        }

        /// <summary>
        /// Set the indent amount for pretty printing
        /// </summary>
        /// <param name="indentAmount">number of spaces to use for indenting each level. Must be &gt;= 0.
        /// </param>
        /// <returns>this, for chaining</returns>
        public DocumentOutputSettings IndentAmount(int indentAmount)
        {
            Validate.IsTrue(indentAmount >= 0);
            this.indentAmount = indentAmount;
            return this;
        }

        internal DocumentOutputSettings Clone()
        {
            DocumentOutputSettings clone;
            clone = (DocumentOutputSettings)this.MemberwiseClone();
            clone.Charset(charset);
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
        public string Location()
        {
            return location;
        }

        /// <summary>
        /// Accessor to the document's
        /// <code>head</code>
        /// element.
        /// </summary>
        /// <returns>
        /// <code>head</code>
        /// </returns>
        public Element Head()
        {
            return FindFirstElementByTagName("head", this);
        }

        /// <summary>
        /// Accessor to the document's
        /// <code>body</code>
        /// element.
        /// </summary>
        /// <returns>
        /// 
        /// <code>body</code>
        /// </returns>
        public Element Body()
        {
            return FindFirstElementByTagName("body", this);
        }

        /// <summary>
        /// Get the string contents of the document's
        /// <code>title</code>
        /// element.
        /// </summary>
        /// <returns>Trimmed title, or empty string if none set.</returns>
        public string Title()
        {
            // title is a preserve whitespace tag (for document output), but normalised here
            Element titleEl = GetElementsByTag("title").First();
            return titleEl != null ? StringUtil.NormaliseWhitespace(titleEl.Text()).Trim() :
                string.Empty;
        }

        /// <summary>
        /// Set the document's
        /// <code>title</code>
        /// element. Updates the existing element, or adds
        /// <code>title</code>
        /// to
        /// <code>head</code>
        /// if
        /// not present
        /// </summary>
        /// <param name="title">string to set as title</param>
        public void Title(string title)
        {
            Validate.NotNull(title);
            Element titleEl = GetElementsByTag("title").First();
            if (titleEl == null)
            {
                // add to head
                Head().AppendElement("title").Text(title);
            }
            else
            {
                titleEl.Text(title);
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
        /// <code>a</code>
        /// )
        /// </param>
        /// <returns>new element</returns>
        public Element CreateElement(string tagName)
        {
            Tag tag = Supremes.Nodes.Tag.ValueOf(tagName);
            return new Element(tag, this.BaseUri());
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
            if (Head() == null)
            {
                htmlEl.PrependElement("head");
            }
            if (Body() == null)
            {
                htmlEl.AppendElement("body");
            }
            // pull text nodes out of root, html, and head els, and push into body. non-text nodes are already taken care
            // of. do in inverse order to maintain text order.
            NormaliseTextNodes((Element)Head());
            NormaliseTextNodes((Element)htmlEl);
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
                .Where(n => !n.IsBlank())
                .Cast<Node>()
                .ToList();
            for (int i = toMove.Count - 1; i >= 0; i--)
            {
                Node node_1 = toMove[i];
                element.RemoveChild(node_1);
                Body().PrependChild(new TextNode(" ", string.Empty));
                Body().PrependChild(node_1);
            }
        }

        // merge multiple <head> or <body> contents into one, delete the remainder, and ensure they are owned by <html>

        private void NormaliseStructure(string tag, Element htmlEl)
        {
            Elements elements = this.GetElementsByTag(tag);
            Element master = elements.First();
            // will always be available as created above if not existent
            if (elements.Count > 1)
            {
                // dupes, move contents to master
                List<Node> toMove = new List<Node>();
                for (int i = 1; i < elements.Count; i++)
                {
                    Node dupe = elements[i];
                    foreach (Node node in dupe.ChildNodes())
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
            if (!master.Parent().Equals(htmlEl))
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
                foreach (Node child in node.ChildNodes())
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

        public override string OuterHtml()
        {
            return base.Html();
            // no outer wrapper tag
        }

        /// <summary>
        /// Set the text of the
        /// <code>body</code>
        /// of this document. Any existing nodes within the body will be cleared.
        /// </summary>
        /// <param name="text">unencoded text</param>
        /// <returns>this document</returns>
        public override Element Text(string text)
        {
            Body().Text(text);
            // overridden to not nuke doc structure
            return this;
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
        /// Get the document's current output settings.
        /// </summary>
        /// <returns>the document's current output settings.</returns>
        public DocumentOutputSettings OutputSettings()
        {
            return outputSettings;
        }

        /// <summary>
        /// Set the document's output settings.
        /// </summary>
        /// <param name="outputSettings">new output settings.</param>
        /// <returns>this document, for chaining.</returns>
        public Document OutputSettings(DocumentOutputSettings outputSettings)
        {
            Validate.NotNull(outputSettings);
            this.outputSettings = outputSettings;
            return this;
        }

        public DocumentQuirksMode QuirksMode()
        {
            return quirksMode;
        }

        public Document QuirksMode(DocumentQuirksMode quirksMode)
        {
            this.quirksMode = quirksMode;
            return this;
        }
    }
}
