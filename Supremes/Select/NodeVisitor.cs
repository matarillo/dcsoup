/*
 * This code is derived from jsoup 1.8.1 (http://jsoup.org/news/release-1.8.1)
 */

using NSoup.Nodes;
using NSoup.Select;
using Sharpen;

namespace NSoup.Select
{
	/// <summary>Node visitor interface.</summary>
	/// <remarks>
	/// Node visitor interface. Provide an implementing class to
	/// <see cref="NodeTraversor">NodeTraversor</see>
	/// to iterate through nodes.
	/// <p/>
	/// This interface provides two methods,
	/// <code>head</code>
	/// and
	/// <code>tail</code>
	/// . The head method is called when the node is first
	/// seen, and the tail method when all of the node's children have been visited. As an example, head can be used to
	/// create a start tag for a node, and tail to create the end tag.
	/// </remarks>
	public interface NodeVisitor
	{
		/// <summary>Callback for when a node is first visited.</summary>
		/// <remarks>Callback for when a node is first visited.</remarks>
		/// <param name="node">the node being visited.</param>
		/// <param name="depth">
		/// the depth of the node, relative to the root node. E.g., the root node has depth 0, and a child node
		/// of that will have depth 1.
		/// </param>
		void Head(Node node, int depth);

		/// <summary>Callback for when a node is last visited, after all of its descendants have been visited.
		/// 	</summary>
		/// <remarks>Callback for when a node is last visited, after all of its descendants have been visited.
		/// 	</remarks>
		/// <param name="node">the node being visited.</param>
		/// <param name="depth">
		/// the depth of the node, relative to the root node. E.g., the root node has depth 0, and a child node
		/// of that will have depth 1.
		/// </param>
		void Tail(Node node, int depth);
	}
}
