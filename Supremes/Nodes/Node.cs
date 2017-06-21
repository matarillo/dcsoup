using Supremes.Helper;
using Supremes.Parsers;
using Supremes.Select;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// The base, abstract Node model.
    /// </summary>
    /// <remarks>
    /// Elements, Documents, Comments etc are all Node instances.
    /// </remarks>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public abstract class Node
    {
        internal Node parentNode;

        internal IList<Node> childNodes;

        internal Attributes attributes;

        internal string baseUri;

        internal int siblingIndex;

        /// <summary>
        /// Create a new Node.
        /// </summary>
        /// <param name="baseUri">base URI</param>
        /// <param name="attributes">attributes (not null, but may be empty)</param>
        internal Node(string baseUri, Attributes attributes)
        {
            Validate.NotNull(baseUri);
            Validate.NotNull(attributes);
            childNodes = new List<Node>(4);
            this.baseUri = baseUri.Trim();
            this.attributes = attributes;
        }

        internal Node(string baseUri)
            : this(baseUri, new Attributes())
        {
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// Doesn't setup base uri, children, or attributes; use with caution.
        /// </remarks>
        internal Node()
        {
            childNodes = new List<Node>();
            attributes = null;
        }

        /// <summary>
        /// Get the node name of this node.
        /// </summary>
        /// <remarks>
        /// Use for debugging purposes and not logic switching (for that, use instanceof).
        /// </remarks>
        /// <returns>node name</returns>
        internal abstract string NodeName { get; }

        /// <summary>
        /// Get an attribute's value by its key.
        /// </summary>
        /// <remarks>
        /// To get an absolute URL from an attribute that may be a relative URL, prefix the key with <c><b>abs</b></c>,
        /// which is a shortcut to the
        /// <see cref="AbsUrl(string)">AbsUrl(string)</see>
        /// method.
        /// E.g.: <blockquote><c>String url = a.attr("abs:href");</c></blockquote>
        /// </remarks>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns>The attribute, or empty string if not present (to avoid nulls).</returns>
        /// <seealso cref="Attributes">Attributes</seealso>
        /// <seealso cref="HasAttr(string)">HasAttr(string)</seealso>
        /// <seealso cref="AbsUrl(string)">AbsUrl(string)</seealso>
        public virtual string Attr(string attributeKey)
        {
            Validate.NotNull(attributeKey);
            if (attributes.ContainsKey(attributeKey))
            {
                return attributes[attributeKey];
            }
            else
            {
                if (attributeKey.ToLower().StartsWith("abs:", StringComparison.Ordinal))
                {
                    return AbsUrl(attributeKey.Substring("abs:".Length)); /*substring*/
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Get all of the element's attributes.
        /// </summary>
        /// <returns>
        /// attributes (which implements iterable, in same order as presented in original HTML).
        /// </returns>
        public virtual Attributes Attributes
        {
            get { return attributes; }
        }

        /// <summary>
        /// Set an attribute (key=value).
        /// </summary>
        /// <remarks>
        /// If the attribute already exists, it is replaced.
        /// </remarks>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>this (for chaining)</returns>
        public virtual Node Attr(string attributeKey, string attributeValue)
        {
            attributes[attributeKey] = attributeValue;
            return this;
        }

        /// <summary>
        /// Test if this element has an attribute.
        /// </summary>
        /// <param name="attributeKey">The attribute key to check.</param>
        /// <returns>true if the attribute exists, false if not.</returns>
        public virtual bool HasAttr(string attributeKey)
        {
            Validate.NotNull(attributeKey);
            if (attributeKey.StartsWith("abs:", StringComparison.Ordinal))
            {
                string key = attributeKey.Substring("abs:".Length); /*substring*/
                if (attributes.ContainsKey(key) && !AbsUrl(key).Equals(string.Empty))
                {
                    return true;
                }
            }
            return attributes.ContainsKey(attributeKey);
        }

        /// <summary>
        /// Remove an attribute from this element.
        /// </summary>
        /// <param name="attributeKey">The attribute to remove.</param>
        /// <returns>this (for chaining)</returns>
        public virtual Node RemoveAttr(string attributeKey)
        {
            Validate.NotNull(attributeKey);
            attributes.Remove(attributeKey);
            return this;
        }

        /// <summary>
        /// Get or Set the base URI of this node.
        /// When set, it updates the base URI of this node and all of its descendants.
        /// </summary>
        /// <value>base URI to set</value>
        /// <returns>base URI</returns>
        public string BaseUri
        {
            get { return baseUri; }
            set
            {
                Validate.NotNull(value);
                Traverse(new BaseUriVisitor(value));
            }
        }

        private sealed class BaseUriVisitor : INodeVisitor
        {
            internal BaseUriVisitor(string baseUri)
            {
                this.baseUri = baseUri;
            }

            public void Head(Node node, int depth)
            {
                ((Supremes.Nodes.Node)node).baseUri = baseUri;
            }

            public void Tail(Node node, int depth)
            {
            }

            private readonly string baseUri;
        }

        /// <summary>
        /// Get an absolute URL from a URL attribute that may be relative
        /// (i.e. an <c>&lt;a href&gt;</c> or <c>&lt;img src&gt;</c>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// E.g.: <c>string absUrl = linkEl.AbsUrl("href");</c>
        /// </para>
        /// <para>
        /// If the attribute value is already absolute (i.e. it starts with a protocol, like
        /// <c>http://</c> or <c>https://</c> etc), and it successfully parses as a URL, the attribute is
        /// returned directly. Otherwise, it is treated as a URL relative to the element's
        /// <see cref="BaseUri">BaseUri</see>
        /// , and made
        /// absolute using that.
        /// </para>
        /// <para>
        /// As an alternate, you can use the
        /// <see cref="Attr(string)">Attr(string)</see>
        /// method with the <c>abs:</c> prefix, e.g.:
        /// <c>string absUrl = linkEl.Attr("abs:href");</c>
        /// </para>
        /// <para>
        /// This method add trailing slash to domain name: i.e.
        /// from <c>&lt;a id=2 href='http://jsoup.org'&gt;</c>
        /// to <c>"http://jsoup.org/"</c>
        /// </para>
        /// </remarks>
        /// <param name="attributeKey">The attribute key</param>
        /// <returns>
        /// An absolute URL if one could be made, or an empty string (not null) if the attribute was missing or
        /// could not be made successfully into a URL.
        /// </returns>
        /// <seealso cref="Attr(string)">Attr(string)</seealso>
        /// <seealso cref="System.Uri.TryCreate(string,UriKind,out Uri)">System.Uri.TryCreate(string,UriKind,out Uri)</seealso>
        public virtual string AbsUrl(string attributeKey)
        {
            Validate.NotEmpty(attributeKey);
            string relUrl = Attr(attributeKey);
            if (!HasAttr(attributeKey))
            {
                // nothing to make absolute with
                return string.Empty;
            }
            else
            {
                Uri @base;
                Uri abs;
                if (!TryCreateAbsolute(baseUri, out @base))
                {
                    // the base is unsuitable, but the attribute may be abs on its own, so try that
                    return TryCreateAbsolute(relUrl, out abs)
                        ? abs.ToString()
                        : string.Empty;
                }
                // .NET resolves '//path/file + ?foo' to '//path/file?foo' as desired, so no workaround needed
                return TryCreateRelative(@base, relUrl, out abs)
                    ? abs.AbsoluteUri.ToString()
                    : string.Empty;
            }
        }

        private static bool TryCreateAbsolute(string absoluteUri, out Uri result)
        {
            if (Uri.TryCreate(absoluteUri, UriKind.Absolute, out result))
            {
#if (NETSTANDARD1_3)
                if (IsKnownScheme(result.Scheme))
#else
                if (UriParser.IsKnownScheme(result.Scheme))
#endif
                {
                    return true;
                }
            }
            result = default(Uri);
            return false;
        }

        private static bool TryCreateRelative(Uri baseUri, string relativeUri, out Uri result)
        {
            if (Uri.TryCreate(baseUri, relativeUri, out result))
            {
#if (NETSTANDARD1_3)
                if (IsKnownScheme(result.Scheme))
#else
                if (UriParser.IsKnownScheme(result.Scheme))
#endif
                {
                    return true;
                }
            }
            result = default(Uri);
            return false;
        }

#if (NETSTANDARD1_3)
        private static bool IsKnownScheme(string scheme)
        {
            switch (scheme)
            {
                case "http":
                case "https":
                case "ws":
                case "wss":
                case "ftp":
                case "file":
                case "gopher":
                case "nntp":
                case "news":
                case "mailto":
                case "uuid":
                case "telnet":
                case "ldap":
                case "net.tcp":
                case "net.pipe":
                case "vsmacros":
                    return true;
                default:
                    return false;
            }
        }
#endif

        /// <summary>
        /// Get a child node by its 0-based index.
        /// </summary>
        /// <param name="index">index of child node</param>
        /// <returns>
        /// the child node at this index. Throws a
        /// <c>IndexOutOfBoundsException</c>
        /// if the index is out of bounds.
        /// </returns>
        public Node ChildNode(int index)
        {
            return childNodes[index];
        }

        /// <summary>
        /// Get this node's children.
        /// </summary>
        /// <remarks>
        /// Presented as an unmodifiable list: new children can not be added, but the child nodes
        /// themselves can be manipulated.
        /// </remarks>
        /// <returns>list of children. If no children, returns an empty list.</returns>
        public IReadOnlyList<Node> ChildNodes
        {
            get { return new ReadOnlyCollection<Node>(childNodes); }
        }

        /// <summary>
        /// Returns a deep copy of this node's children.
        /// </summary>
        /// <remarks>
        /// Changes made to these nodes will not be reflected in the original nodes
        /// </remarks>
        /// <returns>a deep copy of this node's children</returns>
        public IList<Node> ChildNodesCopy()
        {
            IList<Node> children = new List<Node>(childNodes.Count);
            foreach (Node node in childNodes)
            {
                children.Add(node.Clone());
            }
            return children;
        }

        /// <summary>
        /// Get the number of child nodes that this node holds.
        /// </summary>
        /// <returns>the number of child nodes that this node holds.</returns>
        public int ChildNodeSize
        {
            get { return childNodes.Count; }
        }

        internal Node[] ChildNodesAsArray()
        {
            return childNodes.ToArray();
        }

        /// <summary>
        /// Gets this node's parent node.
        /// </summary>
        /// <returns>parent node; or null if no parent.</returns>
        public Node Parent
        {
            get { return parentNode; }
        }

        /// <summary>
        /// Gets the Document associated with this Node.
        /// </summary>
        /// <returns>
        /// the Document associated with this Node, or null if there is no such Document.
        /// </returns>
        public Document OwnerDocument
        {
            get
            {
                if (this is Document)
                {
                    return (Document)this;
                }
                else if (parentNode == null)
                {
                    return null;
                }
                else
                {
                    return parentNode.OwnerDocument;
                }
            }
        }

        /// <summary>
        /// Remove (delete) this node from the DOM tree.
        /// </summary>
        /// <remarks>
        /// If this node has children, they are also removed.
        /// </remarks>
        public void Remove()
        {
            Validate.NotNull(parentNode);
            parentNode.RemoveChild(this);
        }

        /// <summary>
        /// Insert the specified HTML into the DOM before this node (i.e. as a preceding sibling).
        /// </summary>
        /// <param name="html">HTML to add before this node</param>
        /// <returns>this node, for chaining</returns>
        /// <seealso cref="After(string)">After(string)</seealso>
        public virtual Node Before(string html)
        {
            AddSiblingHtml(SiblingIndex, html);
            return this;
        }

        /// <summary>
        /// Insert the specified node into the DOM before this node (i.e. as a preceding sibling).
        /// </summary>
        /// <param name="node">to add before this node</param>
        /// <returns>this node, for chaining</returns>
        /// <seealso cref="After(Node)">After(Node)</seealso>
        public virtual Node Before(Node node)
        {
            Validate.NotNull(node);
            Validate.NotNull(parentNode);
            parentNode.AddChildren(SiblingIndex, node);
            return this;
        }

        /// <summary>
        /// Insert the specified HTML into the DOM after this node (i.e. as a following sibling).
        /// </summary>
        /// <param name="html">HTML to add after this node</param>
        /// <returns>this node, for chaining</returns>
        /// <seealso cref="Before(string)">Before(string)</seealso>
        public virtual Node After(string html)
        {
            AddSiblingHtml(SiblingIndex + 1, html);
            return this;
        }

        /// <summary>
        /// Insert the specified node into the DOM after this node (i.e. as a following sibling).
        /// </summary>
        /// <param name="node">to add after this node</param>
        /// <returns>this node, for chaining</returns>
        /// <seealso cref="Before(Node)">Before(Node)</seealso>
        public virtual Node After(Node node)
        {
            Validate.NotNull(node);
            Validate.NotNull(parentNode);
            parentNode.AddChildren(SiblingIndex + 1, node);
            return this;
        }

        private void AddSiblingHtml(int index, string html)
        {
            Validate.NotNull(html);
            Validate.NotNull(parentNode);
            Element context = Parent as Element;
            IReadOnlyList<Node> nodes = Parser.ParseFragment(html, context, BaseUri);
            parentNode.AddChildren(index, nodes.ToArray());
        }

        /// <summary>
        /// Wrap the supplied HTML around this node.
        /// </summary>
        /// <param name="html">
        /// HTML to wrap around this element, e.g.
        /// <c><![CDATA[<div class="head"></div>]]></c>
        /// . Can be arbitrarily deep.
        /// </param>
        /// <returns>this node, for chaining.</returns>
        public Node Wrap(string html)
        {
            Validate.NotEmpty(html);
            Element context = Parent as Element;
            IReadOnlyList<Node> wrapChildren = Parser.ParseFragment(html, context, BaseUri);
            Node wrapNode = wrapChildren[0];
            if (wrapNode == null || !(wrapNode is Element))
            {
                // nothing to wrap with; noop
                return null;
            }
            Element wrap = (Element)wrapNode;
            Element deepest = GetDeepChild(wrap);
            parentNode.ReplaceChild(this, wrap);
            ((Supremes.Nodes.Node)deepest).AddChildren(this);
            // remainder (unbalanced wrap, like <div></div><p></p> -- The <p> is remainder
            if (wrapChildren.Count > 0)
            {
                for (int i = 0; i < wrapChildren.Count; i++)
                {
                    Node remainder = (Node)wrapChildren[i];
                    remainder.parentNode.RemoveChild(remainder);
                    wrap.AppendChild(remainder);
                }
            }
            return this;
        }

        /// <summary>
        /// Removes this node from the DOM, and moves its children up into the node's parent.
        /// </summary>
        /// <remarks>
        /// This has the effect of dropping the node but keeping its children.
        /// <p/>
        /// For example, with the input html:<br/>
        /// <c><![CDATA[<div>One <span>Two <b>Three</b></span></div>]]></c>
        /// <br/>
        /// Calling
        /// <c>element.Unwrap()</c>
        /// on the
        /// <c>span</c>
        /// element will result in the html:<br/>
        /// <c><![CDATA[<div>One Two <b>Three</b></div>]]></c>
        /// <br/>
        /// and the
        /// <c>"Two "</c>
        /// <see cref="TextNode">TextNode</see>
        /// being returned.
        /// </remarks>
        /// <returns>
        /// the first child of this node, after the node has been unwrapped. Null if the node had no children.
        /// </returns>
        /// <seealso cref="Remove()">Remove()</seealso>
        /// <seealso cref="Wrap(string)">Wrap(string)</seealso>
        public Node Unwrap()
        {
            Validate.NotNull(parentNode);
            int index = siblingIndex;
            Node firstChild = childNodes.Count > 0 ? childNodes[0] : null;
            parentNode.AddChildren(index, this.ChildNodesAsArray());
            this.Remove();
            return firstChild;
        }

        private Element GetDeepChild(Element el)
        {
            IList<Element> children = el.Children;
            if (children.Count > 0)
            {
                return GetDeepChild(children[0]);
            }
            else
            {
                return el;
            }
        }

        /// <summary>
        /// Replace this node in the DOM with the supplied node.
        /// </summary>
        /// <param name="in">the node that will will replace the existing node.</param>
        public void ReplaceWith(Node @in)
        {
            Validate.NotNull(@in);
            Validate.NotNull(parentNode);
            parentNode.ReplaceChild(this, (Supremes.Nodes.Node)@in);
        }

        internal void SetParentNode(Node parentNode)
        {
            if (this.parentNode != null)
            {
                this.parentNode.RemoveChild(this);
            }
            this.parentNode = parentNode;
        }

        internal void ReplaceChild(Supremes.Nodes.Node @out, Supremes.Nodes.Node @in)
        {
            Validate.IsTrue(@out.parentNode == this);
            Validate.NotNull(@in);
            if (@in.parentNode != null)
            {
                @in.parentNode.RemoveChild(@in);
            }
            int index = @out.SiblingIndex;
            childNodes[index] = @in;
            @in.parentNode = this;
            @in.SiblingIndex = index;
            @out.parentNode = null;
        }

        internal void RemoveChild(Supremes.Nodes.Node @out)
        {
            Validate.IsTrue(@out.parentNode == this);
            int index = @out.SiblingIndex;
            childNodes.RemoveAt(index);
            ReindexChildren();
            @out.parentNode = null;
        }

        internal void AddChildren(params Node[] children)
        {
            //most used. short circuit addChildren(int), which hits reindex children and array copy
            foreach (Node child in children)
            {
                Supremes.Nodes.Node childImpl = (Supremes.Nodes.Node)child;
                ReparentChild(childImpl);
                childNodes.Add(childImpl);
                childImpl.SiblingIndex = childNodes.Count - 1;
            }
        }

        internal void AddChildren(int index, params Node[] children)
        {
            Validate.NoNullElements(children);
            for (int i = children.Length - 1; i >= 0; i--)
            {
                Supremes.Nodes.Node @in = (Supremes.Nodes.Node)children[i];
                ReparentChild(@in);
                childNodes.Insert(index, @in);
            }
            ReindexChildren();
        }

        private void ReparentChild(Supremes.Nodes.Node child)
        {
            if (child.parentNode != null)
            {
                child.parentNode.RemoveChild(child);
            }
            child.SetParentNode(this);
        }

        private void ReindexChildren()
        {
            for (int i = 0; i < childNodes.Count; i++)
            {
                childNodes[i].SiblingIndex = i;
            }
        }

        /// <summary>
        /// Retrieves this node's sibling nodes.
        /// </summary>
        /// <remarks>
        /// Similar to
        /// <see cref="ChildNodes">node.parent.ChildNodes</see>
        /// , but does not
        /// include this node (a node is not a sibling of itself).
        /// </remarks>
        /// <returns>node siblings. If the node has no parent, returns an empty list.</returns>
        public IReadOnlyList<Node> SiblingNodes
        {
            get
            {
                if (parentNode == null)
                {
                    return new ReadOnlyCollection<Node>(new Node[0]);
                }
                IList<Node> nodes = parentNode.childNodes;
                List<Node> siblings = new List<Node>(nodes.Count - 1);
                foreach (Node node in nodes)
                {
                    if (node != this)
                    {
                        siblings.Add(node);
                    }
                }
                return siblings.AsReadOnly();
            }
        }

        /// <summary>
        /// Get this node's next sibling.
        /// </summary>
        /// <returns>next sibling, or null if this is the last sibling</returns>
        public Node NextSibling
        {
            get
            {
                if (parentNode == null)
                {
                    return null; // root
                }
                IList<Node> siblings = parentNode.childNodes;
                int index = SiblingIndex;
                Validate.NotNull(index);
                if (siblings.Count > index + 1)
                {
                    return siblings[index + 1];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get this node's previous sibling.
        /// </summary>
        /// <returns>the previous sibling, or null if this is the first sibling</returns>
        public Node PreviousSibling
        {
            get
            {
                if (parentNode == null)
                {
                    return null; // root
                }
                IList<Node> siblings = parentNode.childNodes;
                int index = SiblingIndex;
                Validate.NotNull(index);
                if (index > 0)
                {
                    return siblings[index - 1];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the list index of this node in its node sibling list.
        /// </summary>
        /// <remarks>
        /// I.e. if this is the first node sibling, returns 0.
        /// </remarks>
        /// <returns>position in node sibling list</returns>
        /// <seealso cref="Element.ElementSiblingIndex">Element.ElementSiblingIndex</seealso>
        public int SiblingIndex
        {
            get { return siblingIndex; }
            internal set { siblingIndex = value; }
        }

        /// <summary>
        /// Perform a depth-first traversal through this node and its descendants.
        /// </summary>
        /// <param name="nodeVisitor">the visitor callbacks to perform on each node</param>
        /// <returns>this node, for chaining</returns>
        internal Node Traverse(INodeVisitor nodeVisitor)
        {
            Validate.NotNull(nodeVisitor);
            NodeTraversor traversor = new NodeTraversor(nodeVisitor);
            traversor.Traverse(this);
            return this;
        }

        /// <summary>
        /// Get the outer HTML of this node.
        /// </summary>
        /// <returns>HTML</returns>
        public virtual string OuterHtml
        {
            get
            {
                StringBuilder accum = new StringBuilder(128);
                AppendOuterHtmlTo(accum);
                return accum.ToString();
            }
        }

        internal void AppendOuterHtmlTo(StringBuilder accum)
        {
            new NodeTraversor(new Node.OuterHtmlVisitor(accum, GetOutputSettings())).Traverse(this);
        }

        internal DocumentOutputSettings GetOutputSettings()
        {
            // if this node has no document (or parent), retrieve the default output settings
            return OwnerDocument != null
                ? OwnerDocument.OutputSettings
                : (new Document(string.Empty)).OutputSettings;
        }

        /// <summary>
        /// Get the outer HTML of this node.
        /// </summary>
        /// <param name="accum">accumulator to place HTML into</param>
        /// <param name="depth"></param>
        /// <param name="out"></param>
        internal abstract void AppendOuterHtmlHeadTo(StringBuilder accum, int depth, DocumentOutputSettings @out);

        internal abstract void AppendOuterHtmlTailTo(StringBuilder accum, int depth, DocumentOutputSettings @out);

        /// <summary>
        /// Converts the value of this instance to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return OuterHtml;
        }

        internal void Indent(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            accum.Append("\n").Append(StringUtil.Padding(depth * @out.IndentAmount));
        }

        /// <summary>
        /// Compares two <see cref="Node"/> instances for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            // todo: have nodes hold a child index, compare against that and parent (not children)
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int result = parentNode != null ? parentNode.GetHashCode() : 0;
            // not children, or will block stack as they go back up to parent)
            unchecked
            {
                result = 31 * result + (attributes != null ? attributes.GetHashCode() : 0);
            }
            return result;
        }

        /// <summary>
        /// Create a stand-alone, deep copy of this node, and all of its children.
        /// </summary>
        /// <remarks>
        /// The cloned node will have no siblings or parent node.
        /// As a stand-alone object, any changes made to the clone or any of its children
        /// will not impact the original node.
        /// <p/>
        /// The cloned node may be adopted into another Document or node structure using
        /// <see cref="Element.AppendChild(Node)">Element.AppendChild(Node)</see>
        /// .
        /// </remarks>
        /// <returns>stand-alone cloned node</returns>
        internal virtual Node Clone()
        {
            Node thisClone = DoClone(null);
            // splits for orphan
            // Queue up nodes that need their children cloned (BFS).
            LinkedList<Node> nodesToProcess = new LinkedList<Node>();
            nodesToProcess.AddLast(thisClone);
            while (nodesToProcess.Count > 0)
            {
                Node currParent = nodesToProcess.First.Value;
                nodesToProcess.RemoveFirst();
                for (int i = 0; i < currParent.childNodes.Count; i++)
                {
                    Node childClone = ((Node)currParent.childNodes[i]).DoClone(currParent);
                    currParent.childNodes[i] = childClone;
                    nodesToProcess.AddLast(childClone);
                }
            }
            return thisClone;
        }

        internal Node DoClone(Node parent)
        {
            Node clone = (Node)this.MemberwiseClone();
            clone.parentNode = parent;
            // can be null, to create an orphan split
            clone.siblingIndex = parent == null ? 0 : siblingIndex;
            clone.attributes = attributes != null ? attributes.Clone() : null;
            clone.baseUri = baseUri;
            clone.childNodes = new List<Node>(childNodes.Count);
            foreach (Node child in childNodes)
            {
                clone.childNodes.Add(child);
            }
            return clone;
        }

        private class OuterHtmlVisitor : INodeVisitor
        {
            private StringBuilder accum;

            private DocumentOutputSettings @out;

            internal OuterHtmlVisitor(StringBuilder accum, DocumentOutputSettings @out)
            {
                this.accum = accum;
                this.@out = @out;
            }

            public void Head(Node node, int depth)
            {
                ((Node)node).AppendOuterHtmlHeadTo(accum, depth, @out);
            }

            public void Tail(Node node, int depth)
            {
                if (!node.NodeName.Equals("#text"))
                {
                    // saves a void hit.
                    ((Node)node).AppendOuterHtmlTailTo(accum, depth, @out);
                }
            }
        }
    }
}
