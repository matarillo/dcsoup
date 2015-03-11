/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using Supremes.Nodes;

namespace Supremes.Select
{
    /// <summary>Depth-first node traversor.</summary>
    /// <remarks>
    /// Depth-first node traversor. Use to iterate through all nodes under and including the specified root node.
    /// <p/>
    /// This implementation does not use recursion, so a deep DOM does not risk blowing the stack.
    /// </remarks>
    internal class NodeTraversor
    {
        private readonly INodeVisitor visitor;

        /// <summary>
        /// Create a new traversor.
        /// </summary>
        /// <param name="visitor">
        /// a class implementing the
        /// <see cref="NodeVisitor">NodeVisitor</see>
        /// interface, to be called when visiting each node.
        /// </param>
        public NodeTraversor(INodeVisitor visitor)
        {
            this.visitor = visitor;
        }

        /// <summary>
        /// Start a depth-first traverse of the root and all of its descendants.
        /// </summary>
        /// <param name="root">the root node point to traverse.</param>
        public void Traverse(Node root)
        {
            Node node = root;
            int depth = 0;
            while (node != null)
            {
                visitor.Head(node, depth);
                if (node.ChildNodeSize() > 0)
                {
                    node = node.ChildNode(0);
                    depth++;
                }
                else
                {
                    while (node.NextSibling() == null && depth > 0)
                    {
                        visitor.Tail(node, depth);
                        node = node.ParentNode();
                        depth--;
                    }
                    visitor.Tail(node, depth);
                    if (node == root)
                    {
                        break;
                    }
                    node = node.NextSibling();
                }
            }
        }
    }
}
