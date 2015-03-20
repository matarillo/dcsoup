using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// A comment node.
    /// </summary>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class Comment : Node
    {
        private const string COMMENT_KEY = "comment";

        /// <summary>
        /// Create a new comment node.
        /// </summary>
        /// <param name="data">The contents of the comment</param>
        /// <param name="baseUri">base URI</param>
        internal Comment(string data, string baseUri)
            : base(baseUri)
        {
            attributes[COMMENT_KEY] = data;
        }

        internal override string NodeName
        {
        	get { return "#comment"; }
        }

        /// <summary>
        /// Get the contents of the comment.
        /// </summary>
        /// <returns>comment content</returns>
        public string Data
        {
            get { return attributes[COMMENT_KEY]; }
        }

        internal override void AppendOuterHtmlHeadTo(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            if (@out.PrettyPrint)
            {
                Indent(accum, depth, @out);
            }
            accum.Append("<!--").Append(Data).Append("-->");
        }

        internal override void AppendOuterHtmlTailTo(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
        }

        /// <summary>
        /// Converts the value of this instance to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return OuterHtml;
        }
    }
}
