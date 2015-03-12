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
        /// Get the data contents of this node.
        /// </summary>
        /// <remarks>
        /// Will be unescaped and with original new lines, space etc.
        /// </remarks>
        /// <returns>data</returns>
        public string GetWholeData()
        {
            return attributes[DATA_KEY];
        }

        /// <summary>
        /// Set the data contents of this node.
        /// </summary>
        /// <param name="data">unencoded data</param>
        /// <returns>this node, for chaining</returns>
        public DataNode SetWholeData(string data)
        {
            attributes[DATA_KEY] = data;
            return this;
        }

        internal override void OuterHtmlHead(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            accum.Append(GetWholeData());
            // data is not escaped in return from data nodes, so " in script, style is plain
        }

        internal override void OuterHtmlTail(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
        }

        public override string ToString()
        {
            return OuterHtml();
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
