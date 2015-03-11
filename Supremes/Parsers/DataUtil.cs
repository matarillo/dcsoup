/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using Supremes.Helper;
using Supremes.Nodes;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Supremes.Parsers
{
    /// <summary>
    /// Internal static utilities for handling data.
    /// </summary>
    internal static class DataUtil
    {
        private static readonly Regex charsetPattern = new Regex("(?i)\\bcharset=\\s*(?:\"|')?([^\\s,;\"']*)", RegexOptions.Compiled);

        internal static readonly Encoding defaultCharset = Encoding.UTF8; // string defaultCharset = "UTF-8";

        private const int bufferSize = 0x20000;

        // used if not found in header or meta charset
        // ~130K.

        /// <summary>
        /// Loads a file to a Document.
        /// </summary>
        /// <param name="in">file to load</param>
        /// <param name="charsetName">character set of input</param>
        /// <param name="baseUri">base URI of document, to resolve relative links against</param>
        /// <returns>Document</returns>
        /// <exception cref="System.IO.IOException">on IO error</exception>
        public static Document Load(string @in, string charsetName, string baseUri)
        {
            byte[] byteData = ReadFileToByteBuffer(@in);
            Parser parser = Parser.HtmlParser();
            return ParseByteData(byteData, charsetName, baseUri, parser);
        }

        /// <summary>
        /// Parses a Document from an input steam.
        /// </summary>
        /// <param name="in">input stream to parse. You will need to close it.</param>
        /// <param name="charsetName">character set of input</param>
        /// <param name="baseUri">base URI of document, to resolve relative links against</param>
        /// <returns>Document</returns>
        /// <exception cref="System.IO.IOException">on IO error</exception>
        public static Document Load(Stream @in, string charsetName, string baseUri)
        {
            byte[] byteData = ReadToByteBuffer(@in);
            Parser parser = Parser.HtmlParser();
            return ParseByteData(byteData, charsetName, baseUri, parser);
        }

        /// <summary>
        /// Parses a Document from an input steam, using the provided Parser.
        /// </summary>
        /// <param name="in">input stream to parse. You will need to close it.</param>
        /// <param name="charsetName">character set of input</param>
        /// <param name="baseUri">base URI of document, to resolve relative links against</param>
        /// <param name="parser">
        /// alternate
        /// <see cref="Parser.XmlParser()">parser</see>
        /// to use.
        /// </param>
        /// <returns>Document</returns>
        /// <exception cref="System.IO.IOException">on IO error</exception>
        public static Document Load(Stream @in, string charsetName, string baseUri, Parser parser)
        {
            byte[] byteData = ReadToByteBuffer(@in);
            return ParseByteData(byteData, charsetName, baseUri, parser);
        }

        // reads bytes first into a buffer, then decodes with the appropriate charset. done this way to support
        // switching the chartset midstream when a meta http-equiv tag defines the charset.
        // todo - this is getting gnarly. needs a rewrite.

        internal static Document ParseByteData(byte[] byteData, string charsetName, string baseUri, Parser parser)
        {
            string docData;
            Document doc = null;
            if (charsetName == null)
            {
                // determine from meta. safe parse as UTF-8
                // look for <meta http-equiv="Content-Type" content="text/html;charset=gb2312"> or HTML5 <meta charset="gb2312">
                docData = defaultCharset.GetString(byteData);
                doc = parser.ParseInput(docData, baseUri);
                Element meta = doc.Select("meta[http-equiv=content-type], meta[charset]").First();
                if (meta != null)
                {
                    // if not found, will keep utf-8 as best attempt
                    string foundCharset;
                    if (meta.HasAttr("http-equiv"))
                    {
                        foundCharset = GetCharsetFromContentType(meta.Attr("content"));
                        if (foundCharset == null && meta.HasAttr("charset"))
                        {
                            foundCharset = meta.Attr("charset");
                        }
                    }
                    else
                    {
                        foundCharset = meta.Attr("charset");
                    }
                    if (!string.IsNullOrEmpty(foundCharset) && !foundCharset.Equals(defaultCharset))
                    {
                        // need to re-decode
                        var trimmed = foundCharset
                            .Trim()
                            .Where(c => c != '[' && c != '\"' && c != '\'' && c != ']')
                            .ToArray();
                        charsetName = new string(trimmed);
                        Encoding supportedEncoding = null;
                        try
                        {
                            supportedEncoding = Encoding.GetEncoding(charsetName);
                        }
                        catch(ArgumentException)
                        {
                            // supportedEncoding is null. fallback to default encoding
                        }
                        if (supportedEncoding != null)
                        {
                            // removed when converting
                            // byteData.Rewind();
                            docData = supportedEncoding.GetString(byteData);
                            doc = null;
                        }
                    }
                }
            }
            else
            {
                // specified by content type header (or by user on file load)
                Validate.NotEmpty(charsetName, "Must set charset arg to character set of file to parse. Set to null to attempt to detect from HTML");
                docData = Encoding.GetEncoding(charsetName).GetString(byteData);
            }
            // UTF-8 BOM indicator. takes precedence over everything else. rarely used. re-decodes incase above decoded incorrectly
            if (docData.Length > 0 && docData[0] == 65279)
            {
                // removed when converting
                // byteData.Rewind();
                docData = defaultCharset.GetString(byteData);
                docData = docData.Substring(1); /*substring*/
                charsetName = defaultCharset.WebName;
                doc = null;
            }
            if (doc == null)
            {
                doc = parser.ParseInput(docData, baseUri);
                doc.OutputSettings().Charset(charsetName);
            }
            return doc;
        }

        /// <summary>
        /// Read the input stream into a byte buffer.
        /// </summary>
        /// <param name="inStream">the input stream to read from</param>
        /// <param name="maxSize">
        /// the maximum size in bytes to read from the stream. Set to 0 to be unlimited.
        /// </param>
        /// <returns>the filled byte buffer</returns>
        /// <exception cref="System.IO.IOException">if an exception occurs whilst reading from the input stream.
        /// </exception>
        internal static byte[] ReadToByteBuffer(Stream inStream, int maxSize)
        {
            Validate.IsTrue(maxSize >= 0, "maxSize must be 0 (unlimited) or larger");
            bool capped = maxSize > 0;
            byte[] buffer = new byte[bufferSize];
            MemoryStream outStream = new MemoryStream(bufferSize);
            int read;
            int remaining = maxSize;
            while (true)
            {
                read = inStream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    break;
                }
                if (capped)
                {
                    if (read > remaining)
                    {
                        outStream.Write(buffer, 0, remaining);
                        break;
                    }
                    remaining -= read;
                }
                outStream.Write(buffer, 0, read);
            }
            //ByteBuffer byteData = ByteBuffer.Wrap(outStream.ToByteArray());
            byte[] byteData = outStream.ToArray();
            return byteData;
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal static byte[] ReadToByteBuffer(Stream inStream)
        {
            return ReadToByteBuffer(inStream, 0);
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal static byte[] ReadFileToByteBuffer(string file)
        {
            return File.ReadAllBytes(file);
        }

        /// <summary>
        /// Parse out a charset from a content type header.
        /// </summary>
        /// <remarks>
        /// regardless of whether the charset is not supported or not.
        /// </remarks>
        /// <param name="contentType">e.g. "text/html; charset=EUC-JP"</param>
        /// <returns>"EUC-JP", or null if not found. Charset is trimmed and uppercased.</returns>
        internal static string GetCharsetFromContentType(string contentType)
        {
            if (contentType == null)
            {
                return null;
            }
            Match m = charsetPattern.Match(contentType);
            if (m.Success) /*find*/
            {
                string charset = m.Groups[1].Value.Trim();
                charset = charset.Replace("charset=", string.Empty);
                if (charset.Length == 0)
                {
                    return null;
                }
                charset = charset.ToUpper(CultureInfo.InvariantCulture);
                return charset;
            }
            return null;
        }
    }
}
