using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// An XML Declaration.
    /// </summary>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class XmlDeclaration : Node
    {
        private const string DECL_KEY = "declaration";

        private readonly bool isProcessingInstruction;

        /// <summary>
        /// Create a new XML declaration
        /// </summary>
        /// <param name="data">data</param>
        /// <param name="baseUri">base uri</param>
        /// <param name="isProcessingInstruction">is processing instruction</param>
        internal XmlDeclaration(string data, string baseUri, bool isProcessingInstruction)
            : base(baseUri)
        {
            // <! if true, <? if false, declaration (and last data char should be ?)
            attributes[DECL_KEY] = data;
            this.isProcessingInstruction = isProcessingInstruction;
        }

        internal override string NodeName
        {
        	get { return "#declaration"; }
        }

        /// <summary>
        /// Get the unencoded XML declaration.
        /// </summary>
        /// <returns>XML declaration</returns>
        public string WholeDeclaration
        {
            get { return attributes[DECL_KEY]; }
        }

        internal override void AppendOuterHtmlHeadTo(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            accum.Append("<").Append(isProcessingInstruction ? "!" : "?").Append(WholeDeclaration).Append(">");
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
