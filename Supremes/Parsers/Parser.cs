using Supremes.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremes.Parsers
{
    /// <summary>
    /// Parses HTML into a
    /// <see cref="Supremes.Nodes.Document">Supremes.Nodes.Document</see>
    /// . Generally best to use one of the  more convenient parse methods
    /// in
    /// <see cref="Supremes.Dcsoup">Supremes.Dcsoup</see>
    /// .
    /// </summary>
    public class Parser
    {
        private const int DEFAULT_MAX_ERRORS = 0;

        private TreeBuilder treeBuilder;

        private int maxErrors = DEFAULT_MAX_ERRORS;

        private ParseErrorList errors;

        /// <summary>
        /// Create a new Parser, using the specified TreeBuilder
        /// </summary>
        /// <param name="treeBuilder">TreeBuilder to use to parse input into Documents.</param>
        internal Parser(TreeBuilder treeBuilder)
        {
            // by default, error tracking is disabled.
            this.treeBuilder = treeBuilder;
        }

        /// <summary>
        /// Parse HTML into a Document
        /// </summary>
        /// <param name="html"></param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        public Document ParseInput(string html, string baseUri)
        {
            errors = CanTrackErrors ? ParseErrorList.Tracking(maxErrors) : ParseErrorList.NoTracking();
            Document doc = treeBuilder.Parse(html, baseUri, errors);
            return doc;
        }

        // gets & sets

        /// <summary>
        /// Get the TreeBuilder currently in use.
        /// </summary>
        /// <returns>current TreeBuilder.</returns>
        internal TreeBuilder TreeBuilder
        {
            get { return treeBuilder; }
        }

        /// <summary>
        /// Update the TreeBuilder used when parsing content.
        /// </summary>
        /// <param name="treeBuilder">current TreeBuilder</param>
        /// <returns>this, for chaining</returns>
        internal Parser SetTreeBuilder(TreeBuilder treeBuilder)
        {
            this.treeBuilder = treeBuilder;
            return this;
        }

        /// <summary>
        /// Check if parse error tracking is enabled.
        /// </summary>
        /// <returns>current track error state.</returns>
        public bool CanTrackErrors
        {
            get { return maxErrors > 0; }
        }

        /// <summary>
        /// Enable or disable parse error tracking for the next parse.
        /// </summary>
        /// <param name="maxErrors">
        /// the maximum number of errors to track. Set to 0 to disable.
        /// </param>
        /// <returns>this, for chaining</returns>
        public Parser SetTrackErrors(int maxErrors)
        {
            this.maxErrors = maxErrors;
            return this;
        }

        /// <remarks>
        /// Retrieve the parse errors, if any, from the last parse.
        /// </remarks>
        /// <returns>list of parse errors, up to the size of the maximum errors tracked.</returns>
        public IList<ParseError> Errors
        {
            get { return errors; }
        }

        // builders

        /// <summary>
        /// Create a new HTML parser.
        /// </summary>
        /// <remarks>
        /// This parser treats input as HTML5, and enforces the creation of a normalised document,
        /// based on a knowledge of the semantics of the incoming tags.
        /// </remarks>
        /// <returns>a new HTML parser.</returns>
        public static Parser HtmlParser
        {
            get { return new Parser(new HtmlTreeBuilder()); }
        }

        /// <summary>
        /// Create a new XML parser.
        /// </summary>
        /// <remarks>
        /// This parser assumes no knowledge of the incoming tags and does not treat it as HTML,
        /// rather creates a simple tree directly from the input.
        /// </remarks>
        /// <returns>a new simple XML parser.</returns>
        public static Parser XmlParser
        {
            get { return new Parser(new XmlTreeBuilder()); }
        }
        
        // utility methods
        
        /// <summary>
        /// Parse HTML into a Document.
        /// </summary>
        /// <param name="html">HTML to parse</param>
        /// <param name="baseUri">base URI of document (i.e. original fetch location), for resolving relative URLs.</param>
        /// <returns>parsed Document</returns>
        public static Document Parse(string html, string baseUri)
        {
            TreeBuilder treeBuilder = new HtmlTreeBuilder();
            return treeBuilder.Parse(html, baseUri, ParseErrorList.NoTracking());
        }

        /// <summary>
        /// Parse a fragment of HTML into a list of nodes.
        /// </summary>
        /// <remarks>
        /// The context element, if supplied, supplies parsing context.
        /// </remarks>
        /// <param name="fragmentHtml">the fragment of HTML to parse</param>
        /// <param name="context">
        /// (optional) the element that this HTML fragment is being parsed for (i.e. for inner HTML). This
        /// provides stack context (for implicit element creation).
        /// </param>
        /// <param name="baseUri">
        /// base URI of document (i.e. original fetch location), for resolving relative URLs.
        /// </param>
        /// <returns>
        /// list of nodes parsed from the input HTML. Note that the context element, if supplied, is not modified.
        /// </returns>
        public static IReadOnlyList<Node> ParseFragment(string fragmentHtml, Element context, string baseUri)
        {
            HtmlTreeBuilder treeBuilder = new HtmlTreeBuilder();
            return treeBuilder.ParseFragment(fragmentHtml, context, baseUri, ParseErrorList.NoTracking());
        }

        /// <summary>
        /// Parse a fragment of XML into a list of nodes.
        /// </summary>
        /// <param name="fragmentXml">the fragment of XML to parse</param>
        /// <param name="baseUri">base URI of document (i.e. original fetch location), for resolving relative URLs.</param>
        /// <returns>list of nodes parsed from the input XML.</returns>
        public static IReadOnlyList<Node> ParseXmlFragment(string fragmentXml, string baseUri)
        {
            XmlTreeBuilder treeBuilder = new XmlTreeBuilder();
            return treeBuilder.ParseFragment(fragmentXml, baseUri, ParseErrorList.NoTracking());
        }

        /// <summary>
        /// Parse a fragment of HTML into the
        /// <c>body</c>
        /// of a Document.
        /// </summary>
        /// <param name="bodyHtml">fragment of HTML</param>
        /// <param name="baseUri">base URI of document (i.e. original fetch location), for resolving relative URLs.</param>
        /// <returns>Document, with empty head, and HTML parsed into body</returns>
        public static Document ParseBodyFragment(string bodyHtml, string baseUri)
        {
            Document doc = Document.CreateShell(baseUri);
            Element body = doc.Body;
            IReadOnlyList<Node> nodeList = ParseFragment(bodyHtml, body, baseUri);
            Node[] nodes = nodeList.ToArray();
            // the node list gets modified when re-parented
            foreach (Node node in nodes)
            {
                body.AppendChild(node);
            }
            return doc;
        }

        /// <summary>
        /// Utility method to unescape HTML entities from a string
        /// </summary>
        /// <param name="string">HTML escaped string</param>
        /// <param name="inAttribute">if the string is to be escaped in strict mode (as attributes are)</param>
        /// <returns>an unescaped string</returns>
        public static string UnescapeEntities(string @string, bool inAttribute)
        {
            Tokeniser tokeniser = new Tokeniser(new CharacterReader(@string), ParseErrorList.NoTracking());
            return tokeniser.UnescapeEntities(inAttribute);
        }

//        /// <param name="bodyHtml">HTML to parse</param>
//        /// <param name="baseUri">baseUri base URI of document (i.e. original fetch location), for resolving relative URLs.</param>
//        /// <returns>parsed Document</returns>
//        [Obsolete(@"Use ParseBodyFragment(string, string) or ParseFragment(string, Supremes.Nodes.Element, string) instead.")]
//        public static Document ParseBodyFragmentRelaxed(string bodyHtml, string baseUri)
//        {
//            return Parse(bodyHtml, baseUri);
//        }
    }
}
