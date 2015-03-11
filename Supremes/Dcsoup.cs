/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using Supremes.Nodes;
using Supremes.Parsers;
using Supremes.Safety;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Supremes
{
    /// <summary>
    /// The core public access point to the Dcsoup functionality.
    /// </summary>
    /// <author>Jonathan Hedley</author>
    public static class Dcsoup
    {
        public static Document Parse(this HttpContent self)
        {
            HttpContentHeaders headers = self.Headers;
            string charsetName = (headers.ContentType != null) ? headers.ContentType.CharSet : null;
            string baseUri = (headers.ContentLocation != null) ? headers.ContentLocation.ToString() : null;
            byte[] byteData = self.ReadAsByteArrayAsync().Result;
            Parser parser = Parser.HtmlParser();
            return DataUtil.ParseByteData(byteData, charsetName, baseUri, parser);
        }

        /// <summary>
        /// Parse HTML into a Document.
        /// </summary>
        /// <remarks>
        /// The parser will make a sensible, balanced document tree out of any HTML.
        /// </remarks>
        /// <param name="html">HTML to parse</param>
        /// <param name="baseUri">
        /// The URL where the HTML was retrieved from. Used to resolve relative URLs to absolute URLs, that occur
        /// before the HTML declares a
        /// <code>&lt;base href&gt;</code>
        /// tag.
        /// </param>
        /// <returns>sane HTML</returns>
        public static Document Parse(string html, string baseUri)
        {
            return Parser.Parse(html, baseUri);
        }

        /// <summary>
        /// Parse HTML into a Document, using the provided Parser.
        /// </summary>
        /// <remarks>
        /// You can provide an alternate parser, such as a simple XML
        /// (non-HTML) parser.
        /// </remarks>
        /// <param name="html">HTML to parse</param>
        /// <param name="baseUri">
        /// The URL where the HTML was retrieved from. Used to resolve relative URLs to absolute URLs, that occur
        /// before the HTML declares a
        /// <code>&lt;base href&gt;</code>
        /// tag.
        /// </param>
        /// <param name="parser">
        /// alternate
        /// <see cref="Parser.XmlParser()">parser</see>
        /// to use.
        /// </param>
        /// <returns>sane HTML</returns>
        public static Document Parse(string html, string baseUri, Parser parser)
        {
            return parser.ParseInput(html, baseUri);
        }

        /// <summary>
        /// Parse HTML into a Document.
        /// </summary>
        /// <remarks>
        /// As no base URI is specified, absolute URL detection relies on the HTML including a
        /// <code>&lt;base href&gt;</code>
        /// tag.
        /// </remarks>
        /// <param name="html">HTML to parse</param>
        /// <returns>sane HTML</returns>
        /// <seealso cref="Parse(string, string)">Parse(string, string)</seealso>
        public static Document Parse(string html)
        {
            return Parser.Parse(html, string.Empty);
        }

        /// <summary>
        /// Parse the contents of a file as HTML.
        /// </summary>
        /// <param name="in">file to load HTML from</param>
        /// <param name="charsetName">
        /// (optional) character set of file contents. Set to
        /// <code>null</code>
        /// to determine from
        /// <code>http-equiv</code>
        /// meta tag, if present, or fall back to
        /// <code>UTF-8</code>
        /// (which is often safe to do).
        /// </param>
        /// <param name="baseUri">
        /// The URL where the HTML was retrieved from, to resolve relative links against.
        /// </param>
        /// <returns>sane HTML</returns>
        /// <exception cref="System.IO.IOException">
        /// if the file could not be found, or read, or if the charsetName is invalid.
        /// </exception>
        public static Document ParseFile(/*FilePath*/string @in, string charsetName, string baseUri)
        {
            return DataUtil.Load(@in, charsetName, baseUri);
        }

        /// <summary>
        /// Parse the contents of a file as HTML.
        /// </summary>
        /// <remarks>
        /// The location of the file is used as the base URI to qualify relative URLs.
        /// </remarks>
        /// <param name="in">file to load HTML from</param>
        /// <param name="charsetName">
        /// (optional) character set of file contents. Set to
        /// <code>null</code>
        /// to determine from
        /// <code>http-equiv</code>
        /// meta tag, if present, or fall back to
        /// <code>UTF-8</code>
        /// (which is often safe to do).
        /// </param>
        /// <returns>sane HTML</returns>
        /// <exception cref="System.IO.IOException">
        /// if the file could not be found, or read, or if the charsetName is invalid.
        /// </exception>
        /// <seealso cref="Parse(Sharpen.FilePath, string, string)">Parse(Sharpen.FilePath, string, string)</seealso>
        public static Document ParseFile(/*FilePath*/string @in, string charsetName)
        {
            return DataUtil.Load(@in, charsetName, Path.GetFullPath(@in));
        }

        /// <summary>
        /// Read an input stream, and parse it to a Document.
        /// </summary>
        /// <param name="in">input stream to read. Make sure to close it after parsing.</param>
        /// <param name="charsetName">
        /// (optional) character set of file contents. Set to
        /// <code>null</code>
        /// to determine from
        /// <code>http-equiv</code>
        /// meta tag, if present, or fall back to
        /// <code>UTF-8</code>
        /// (which is often safe to do).
        /// </param>
        /// <param name="baseUri">
        /// The URL where the HTML was retrieved from, to resolve relative links against.
        /// </param>
        /// <returns>sane HTML</returns>
        /// <exception cref="System.IO.IOException">
        /// if the file could not be found, or read, or if the charsetName is invalid.
        /// </exception>
        public static Document Parse(Stream @in, string charsetName, string baseUri)
        {
            return DataUtil.Load(@in, charsetName, baseUri);
        }

        /// <summary>
        /// Read an input stream, and parse it to a Document.
        /// </summary>
        /// <remarks>
        /// You can provide an alternate parser, such as a simple XML
        /// (non-HTML) parser.
        /// </remarks>
        /// <param name="in">input stream to read. Make sure to close it after parsing.</param>
        /// <param name="charsetName">
        /// (optional) character set of file contents. Set to
        /// <code>null</code>
        /// to determine from
        /// <code>http-equiv</code>
        /// meta tag, if present, or fall back to
        /// <code>UTF-8</code>
        /// (which is often safe to do).
        /// </param>
        /// <param name="baseUri">
        /// The URL where the HTML was retrieved from, to resolve relative links against.
        /// </param>
        /// <param name="parser">
        /// alternate
        /// <see cref="Parser.XmlParser()">parser</see>
        /// to use.
        /// </param>
        /// <returns>sane HTML</returns>
        /// <exception cref="System.IO.IOException">
        /// if the file could not be found, or read, or if the charsetName is invalid.
        /// </exception>
        public static Document Parse(Stream @in, string charsetName, string baseUri, Parser parser)
        {
            return DataUtil.Load(@in, charsetName, baseUri, parser);
        }

        /// <summary>
        /// Parse a fragment of HTML, with the assumption that it forms the
        /// <code>body</code>
        /// of the HTML.
        /// </summary>
        /// <param name="bodyHtml">body HTML fragment</param>
        /// <param name="baseUri">URL to resolve relative URLs against.</param>
        /// <returns>sane HTML document</returns>
        /// <seealso cref="Supremes.Nodes.Document.Body()">Supremes.Nodes.Document.Body()</seealso>
        public static Document ParseBodyFragment(string bodyHtml, string baseUri)
        {
            return Parser.ParseBodyFragment(bodyHtml, baseUri);
        }

        /// <summary>
        /// Parse a fragment of HTML, with the assumption that it forms the
        /// <code>body</code>
        /// of the HTML.
        /// </summary>
        /// <param name="bodyHtml">body HTML fragment</param>
        /// <returns>sane HTML document</returns>
        /// <seealso cref="Supremes.Nodes.Document.Body()">Supremes.Nodes.Document.Body()</seealso>
        public static Document ParseBodyFragment(string bodyHtml)
        {
            return Parser.ParseBodyFragment(bodyHtml, string.Empty);
        }

        /// <summary>
        /// Fetch a URL, and parse it as HTML.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Provided for compatibility.
        /// </para>
        /// <para>
        /// The encoding character set is determined by the content-type header or http-equiv meta tag, or falls back to
        /// <code>UTF-8</code>
        /// .
        /// </para>
        /// </remarks>
        /// <param name="url">
        /// URL to fetch (with a GET). The protocol must be
        /// <code>http</code>
        /// or
        /// <code>https</code>
        /// .
        /// </param>
        /// <param name="timeoutMillis">
        /// Connection and read timeout, in milliseconds. If exceeded, IOException is thrown.
        /// </param>
        /// <returns>The parsed HTML.</returns>
        /// <exception cref="System.UriFormatException">
        /// if the request URL is not a HTTP or HTTPS URL, or is otherwise malformed
        /// </exception>
        /// <exception cref="System.Exception">
        /// (HttpStatusException)if the response is not OK and HTTP response errors are not ignored
        /// (UnsupportedMimeTypeException)if the response mime type is not supported and those errors are not ignored
        /// </exception>
        /// <exception cref="System.TimeoutException">if the connection times out</exception>
        /// <exception cref="System.IO.IOException">if a connection or read error occurs</exception>
        public static Document Parse(Uri url, int timeoutMillis)
        {
            var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMillis) };
            var message = client.GetAsync(url).Result;
            return message.Content.Parse();
        }

        /// <summary>
        /// Get safe HTML from untrusted input HTML,
        /// by parsing input HTML and filtering it through a white-list of permitted
        /// tags and attributes.
        /// </summary>
        /// <param name="bodyHtml">input untrusted HTML (body fragment)</param>
        /// <param name="baseUri">URL to resolve relative URLs against</param>
        /// <param name="whitelist">white-list of permitted HTML elements</param>
        /// <returns>safe HTML (body fragment)</returns>
        /// <seealso cref="Supremes.Safety.Cleaner.Clean(Supremes.Nodes.IDocument)">
        /// Supremes.Safety.Cleaner.Clean(Supremes.Nodes.Document)
        /// </seealso>
        public static string Clean(string bodyHtml, string baseUri, Whitelist whitelist)
        {
            Document dirty = ParseBodyFragment(bodyHtml, baseUri);
            Cleaner cleaner = new Cleaner(whitelist);
            Document clean = cleaner.Clean(dirty);
            return clean.Body().Html();
        }

        /// <summary>
        /// Get safe HTML from untrusted input HTML, by parsing input HTML and filtering it through a white-list of permitted
        /// tags and attributes.
        /// </summary>
        /// <param name="bodyHtml">input untrusted HTML (body fragment)</param>
        /// <param name="whitelist">white-list of permitted HTML elements</param>
        /// <returns>safe HTML (body fragment)</returns>
        /// <seealso cref="Supremes.Safety.Cleaner.Clean(Supremes.Nodes.IDocument)">
        /// Supremes.Safety.Cleaner.Clean(Supremes.Nodes.Document)
        /// </seealso>
        public static string Clean(string bodyHtml, Whitelist whitelist)
        {
            return Clean(bodyHtml, string.Empty, whitelist);
        }

        /// <summary>
        /// Get safe HTML from untrusted input HTML,
        /// by parsing input HTML and filtering it through a white-list of permitted tags and attributes.
        /// </summary>
        /// <param name="bodyHtml">input untrusted HTML (body fragment)</param>
        /// <param name="baseUri">URL to resolve relative URLs against</param>
        /// <param name="whitelist">white-list of permitted HTML elements</param>
        /// <param name="outputSettings">document output settings; use to control pretty-printing and entity escape modes</param>
        /// <returns>safe HTML (body fragment)</returns>
        /// <seealso cref="Supremes.Safety.Cleaner.Clean(Supremes.Nodes.IDocument)">Supremes.Safety.Cleaner.Clean(Supremes.Nodes.Document)</seealso>
        public static string Clean(string bodyHtml, string baseUri, Whitelist whitelist, DocumentOutputSettings outputSettings)
        {
            Document dirty = ParseBodyFragment(bodyHtml, baseUri);
            Cleaner cleaner = new Cleaner(whitelist);
            Document clean = cleaner.Clean(dirty);
            clean.OutputSettings(outputSettings);
            return clean.Body().Html();
        }

        /// <summary>
        /// Test if the input HTML has only tags and attributes allowed by the Whitelist.
        /// </summary>
        /// <remarks>
        /// Useful for form validation. The input HTML should
        /// still be run through the cleaner to set up enforced attributes, and to tidy the output.
        /// </remarks>
        /// <param name="bodyHtml">HTML to test</param>
        /// <param name="whitelist">whitelist to test against</param>
        /// <returns>true if no tags or attributes were removed; false otherwise</returns>
        /// <seealso cref="Clean(string, Supremes.Safety.Whitelist)"></seealso>
        public static bool IsValid(string bodyHtml, Whitelist whitelist)
        {
            Document dirty = ParseBodyFragment(bodyHtml, string.Empty);
            Cleaner cleaner = new Cleaner(whitelist);
            return cleaner.IsValid(dirty);
        }
    }
}
