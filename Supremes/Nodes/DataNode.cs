using Supremes.Helper;
using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// A data node, for contents of style, script tags etc, where contents should not show in text().
    /// </summary>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class DataNode : Node
    {
        private const string DATA_KEY = "data";

        /// <summary>
        /// Create a new DataNode.
        /// </summary>
        /// <param name="data">data contents</param>
        /// <param name="baseUri">base URI</param>
        internal DataNode(string data, string baseUri) : base(baseUri)
        {
            attributes[DATA_KEY] = data;
        }

        internal override string NodeName
        {
        	get { return "#data"; }
        }

        /// <summary>
        /// Get or Set the data contents of this node.
        /// </summary>
        /// <remarks>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </remarks>
        /// <value>unencoded data</value>
        /// <returns>data will be unescaped and with original new lines, space etc.</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public string WholeData
        {
            set
            {
                attributes[DATA_KEY] = value;
            }
            get
            {
                return attributes[DATA_KEY];
            }
        }

        internal override void AppendOuterHtmlHeadTo(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            accum.Append(WholeData);
            // data is not escaped in return from data nodes, so " in script, style is plain
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

        /// <summary>
        /// Create a new DataNode from HTML encoded data.
        /// </summary>
        /// <param name="encodedData">encoded data</param>
        /// <param name="baseUri">base URI</param>
        /// <returns>new DataNode</returns>
        internal static DataNode CreateFromEncoded(string encodedData, string baseUri)
        {
            string data = Entities.Unescape(encodedData);
            return new Supremes.Nodes.DataNode(data, baseUri);
        }
    }
}
