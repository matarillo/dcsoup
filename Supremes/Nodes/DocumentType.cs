/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// A
    /// <code><!DOCTYPE></code>
    /// node.
    /// </summary>
    internal sealed class DocumentType : Node
    {
        /// <summary>Create a new doctype element.</summary>
        /// <param name="name">the doctype's name</param>
        /// <param name="publicId">the doctype's public ID</param>
        /// <param name="systemId">the doctype's system ID</param>
        /// <param name="baseUri">the doctype's base URI</param>
        internal DocumentType(string name, string publicId, string systemId, string baseUri)
            : base(baseUri)
        {
            // todo: quirk mode from publicId and systemId
            Attr("name", name);
            Attr("publicId", publicId);
            Attr("systemId", systemId);
        }

        internal override string NodeName
        {
        	get { return "#doctype"; }
        }

        internal override void OuterHtmlHead(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            accum.Append("<!DOCTYPE");
            if (!string.IsNullOrEmpty(Attr("name")))
            {
                accum.Append(" ").Append(Attr("name"));
            }
            if (!string.IsNullOrEmpty(Attr("publicId")))
            {
                accum.Append(" PUBLIC \"").Append(Attr("publicId")).Append('"');
            }
            if (!string.IsNullOrEmpty(Attr("systemId")))
            {
                accum.Append(" \"").Append(Attr("systemId")).Append('"');
            }
            accum.Append('>');
        }

        internal override void OuterHtmlTail(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
        }
    }
}
