using Supremes.Helper;
using Supremes.Parsers;
using Supremes.Select;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Supremes.Nodes
{
    /// <summary>
    /// A HTML element consists of a tag name, attributes, and child nodes
    /// (including text nodes and other elements).
    /// </summary>
    /// <remarks>
    /// From an Element, you can extract data, traverse the node graph, and manipulate the HTML.
    /// </remarks>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public class Element : Node
    {
        private Tag tag;

        private ICollection<string> classNames;

        /// <summary>
        /// Create a new, standalone Element.
        /// </summary>
        /// <remarks>
        /// (Standalone in that is has no parent.)
        /// </remarks>
        /// <param name="tag">tag of this element</param>
        /// <param name="baseUri">the base URI</param>
        /// <param name="attributes">initial attributes</param>
        /// <seealso cref="AppendChild(Node)">AppendChild(Node)</seealso>
        /// <seealso cref="AppendElement(string)">AppendElement(string)</seealso>
        internal Element(Tag tag, string baseUri, Attributes attributes)
            : base(baseUri, attributes)
        {
            Validate.NotNull(tag);
            this.tag = tag;
        }

        /// <summary>
        /// Create a new Element from a tag and a base URI.
        /// </summary>
        /// <param name="tag">element tag</param>
        /// <param name="baseUri">
        /// the base URI of this element. It is acceptable for the base URI to be an empty
        /// string, but not null.
        /// </param>
        /// <seealso cref="Supremes.Nodes.Tag.ValueOf(string)">Tag.ValueOf(string)</seealso>
        internal Element(Tag tag, string baseUri)
            : this(tag, baseUri, new Attributes())
        {
        }

        internal override string NodeName
        {
        	get { return tag.Name; }
        }

        /// <summary>
        /// Get or Set the name of the tag for this element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// E.g. <c>div</c>
        /// </para>
        /// <para>
        /// For example, convert a <c>&lt;span&gt;</c> to a <c>&lt;div&gt;</c>
        /// with <c>el.TagName = "div";</c> .
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>the new tag name</value>
        /// <returns>the tag name</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public string TagName
        {
            get { return tag.Name; }
            set
            {
                Validate.NotEmpty(value, "Tag name must not be empty.");
                tag = Supremes.Nodes.Tag.ValueOf(value);
            }
        }

        /// <summary>
        /// Get the Tag for this element.
        /// </summary>
        /// <returns>the tag object</returns>
        public Tag Tag
        {
            get { return tag; }
        }

        /// <summary>
        /// Test if this element is a block-level element.
        /// </summary>
        /// <remarks>
        /// (E.g.
        /// <c>&lt;div&gt; == true</c>
        /// or an inline element
        /// <c>&lt;p&gt; == false</c>
        /// ).
        /// </remarks>
        /// <returns>true if block, false if not (and thus inline)</returns>
        public bool IsBlock
        {
            get { return tag.IsBlock; }
        }

        /// <summary>
        /// Get the <c>id</c> attribute of this element.
        /// </summary>
        /// <returns>The id attribute, if present, or an empty string if not.</returns>
        public string Id
        {
            get { return Attr("id") ?? string.Empty; }
        }

        /// <summary>
        /// Set an attribute value on this element.
        /// </summary>
        /// <remarks>
        /// If this element already has an attribute with the
        /// key, its value is updated; otherwise, a new attribute is added.
        /// </remarks>
        /// <returns>this element</returns>
        internal new Element Attr(string attributeKey, string attributeValue)
        {
            base.Attr(attributeKey, attributeValue);
            return this;
        }

        /// <summary>
        /// Get this element's HTML5 custom data attributes.
        /// </summary>
        /// <remarks>
        /// Each attribute in the element that has a key
        /// starting with "data-" is included the dataset.
        /// <p/>
        /// E.g., the element
        /// <c>&lt;div data-package="jsoup" data-language="Java" class="group"&gt;...</c>
        /// has the dataset
        /// <c>package=jsoup, language=java</c>
        /// .
        /// <p/>
        /// This map is a filtered view of the element's attribute map. Changes to one map (add, remove, update) are reflected
        /// in the other map.
        /// <p/>
        /// You can find elements that have data attributes using the
        /// <c>[^data-]</c>
        /// attribute key prefix selector.
        /// </remarks>
        /// <returns>
        /// a map of
        /// <c>key=value</c>
        /// custom data attributes.
        /// </returns>
        public IDictionary<string, string> Dataset
        {
            get { return attributes.Dataset; }
        }

        /// <summary>
        /// Gets this element's parent element.
        /// </summary>
        /// <returns></returns>
        public new Element Parent
        {
            get { return (Element)parentNode; }
        }
        
        /// <summary>
        /// Get this element's parent and ancestors, up to the document root.
        /// </summary>
        /// <returns>this element's stack of parents, closest first.</returns>
        public Elements Parents
        {
            get
            {
                Elements parents = new Elements();
                AccumulateParents(this, parents);
                return parents;
            }
        }

        private static void AccumulateParents(Element el, Elements parents)
        {
        	Element parent = el.Parent;
            if (parent != null && !parent.TagName.Equals("#root"))
            {
                parents.Add(parent);
                AccumulateParents(parent, parents);
            }
        }

        /// <summary>
        /// Get a child element of this element, by its 0-based index number.
        /// </summary>
        /// <remarks>
        /// Note that an element can have both mixed Nodes and Elements as children. This method inspects
        /// a filtered list of children that are elements, and the index is based on that filtered list.
        /// </remarks>
        /// <param name="index">the index number of the element to retrieve</param>
        /// <returns>
        /// the child element, if it exists, otherwise throws an
        /// <c>IndexOutOfBoundsException</c>
        /// </returns>
        /// <seealso cref="Node.ChildNode(int)">Node.ChildNode(int)</seealso>
        public Element Child(int index)
        {
            return Children[index];
        }

        /// <summary>
        /// Get this element's child elements.
        /// </summary>
        /// <remarks>
        /// This is effectively a filter on
        /// <see cref="Node.ChildNodes">Node.ChildNodes</see>
        /// to get Element nodes.
        /// </remarks>
        /// <returns>
        /// child elements. If this element has no children, returns an
        /// empty list.
        /// </returns>
        /// <seealso cref="Node.ChildNodes">Node.ChildNodes</seealso>
        public Elements Children
        {
            get
            {
                // create on the fly rather than maintaining two lists. if gets slow, memoize, and mark dirty on change
                IList<Element> elements = childNodes.OfType<Element>().ToList();
                return new Elements(elements);
            }
        }

        /// <summary>
        /// Get this element's child text nodes.
        /// </summary>
        /// <remarks>
        /// The list is unmodifiable but the text nodes may be manipulated.
        /// <p/>
        /// This is effectively a filter on
        /// <see cref="Node.ChildNodes">Node.ChildNodes</see>
        /// to get Text nodes.
        /// </remarks>
        /// <returns>
        /// child text nodes. If this element has no text nodes, returns an
        /// empty list.
        /// <p/>
        /// For example, with the input HTML:
        /// <c><![CDATA[<p>One <span>Two</span> Three <br /> Four</p>]]></c>
        /// with the
        /// <c>p</c>
        /// element selected:
        /// <ul>
        /// <li>
        /// <c>p.Text</c>
        /// =
        /// <c>"One Two Three Four"</c>
        /// </li>
        /// <li>
        /// <c>p.OwnText</c>
        /// =
        /// <c>"One Three Four"</c>
        /// </li>
        /// <li>
        /// <c>p.Children</c>
        /// =
        /// <c>Elements[&lt;span&gt;, &lt;br /&gt;]</c>
        /// </li>
        /// <li>
        /// <c>p.ChildNodes</c>
        /// =
        /// <c>List&lt;Node&gt;["One ", &lt;span&gt;, " Three ", &lt;br /&gt;, " Four"]</c>
        /// </li>
        /// <li>
        /// <c>p.TextNodes</c>
        /// =
        /// <c>List&lt;TextNode&gt;["One ", " Three ", " Four"]</c>
        /// </li>
        /// </ul>
        /// </returns>
        public IReadOnlyList<TextNode> TextNodes
        {
            get
            {
                List<TextNode> textNodes = childNodes.OfType<TextNode>().ToList();
                return new ReadOnlyCollection<TextNode>(textNodes);
            }
        }

        /// <summary>
        /// Get this element's child data nodes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The list is unmodifiable but the data nodes may be manipulated.
        /// </para>
        /// <para>
        /// This is effectively a filter on
        /// <see cref="Node.ChildNodes">Node.ChildNodes</see>
        /// to get Data nodes.
        /// </para>
        /// </remarks>
        /// <returns>
        /// child data nodes. If this element has no data nodes, returns an
        /// empty list.
        /// </returns>
        /// <seealso cref="Data">Data</seealso>
        public IReadOnlyList<DataNode> DataNodes
        {
            get
            {
                List<DataNode> dataNodes = childNodes.OfType<DataNode>().ToList();
                return new ReadOnlyCollection<DataNode>(dataNodes);
            }
        }

        /// <summary>
        /// Find elements that match the
        /// <see cref="Supremes.Select.Selector">Supremes.Select.Selector</see>
        /// CSS query, with this element as the starting context. Matched elements
        /// may include this element, or any of its children.
        /// </summary>
        /// <remarks>
        /// This method is generally more powerful to use than the DOM-type
        /// <c>GetElementBy*</c>
        /// methods, because
        /// multiple filters can be combined, e.g.:
        /// <ul>
        /// <li>
        /// <c>el.Select("a[href]")</c>
        /// - finds links (
        /// <c>a</c>
        /// tags with
        /// <c>href</c>
        /// attributes)
        /// </li>
        /// <li>
        /// <c>el.Select("a[href*=example.com]")</c>
        /// - finds links pointing to example.com (loosely)
        /// </li>
        /// </ul>
        /// <p/>
        /// See the query syntax documentation in
        /// <see cref="Supremes.Select.Selector">Supremes.Select.Selector</see>
        /// .
        /// </remarks>
        /// <param name="cssQuery">
        /// a
        /// <see cref="Supremes.Select.Selector">Supremes.Select.Selector</see>
        /// CSS-like query
        /// </param>
        /// <returns>elements that match the query (empty if none match)</returns>
        /// <seealso cref="Supremes.Select.Selector">Supremes.Select.Selector</seealso>
        public Elements Select(string cssQuery)
        {
            return Selector.Select(cssQuery, this);
        }

        /// <summary>
        /// Add a node child node to this element.
        /// </summary>
        /// <param name="child">node to add.</param>
        /// <returns>this element, so that you can add more child nodes or elements.</returns>
        public Element AppendChild(Node child)
        {
            Validate.NotNull(child);
            AddChildren(child);
            return this;
        }

        /// <summary>
        /// Add a node to the start of this element's children.
        /// </summary>
        /// <param name="child">node to add.</param>
        /// <returns>this element, so that you can add more child nodes or elements.</returns>
        public Element PrependChild(Node child)
        {
            Validate.NotNull(child);
            AddChildren(0, child);
            return this;
        }

        /// <summary>
        /// Inserts the given child nodes into this element at the specified index.
        /// </summary>
        /// <remarks>
        /// Current nodes will be shifted to the
        /// right. The inserted nodes will be moved from their current parent. To prevent moving, copy the nodes first.
        /// </remarks>
        /// <param name="index">
        /// 0-based index to insert children at. Specify
        /// <c>0</c>
        /// to insert at the start,
        /// <c>-1</c>
        /// at the
        /// end
        /// </param>
        /// <param name="children">child nodes to insert</param>
        /// <returns>this element, for chaining.</returns>
        public Element InsertChildren(int index, IEnumerable<Node> children)
        {
            Validate.NotNull(children, "Children collection to be inserted must not be null.");
            int currentSize = ChildNodeSize;
            if (index < 0)
            {
                index += currentSize + 1;
            }
            // roll around
            Validate.IsTrue(index >= 0 && index <= currentSize, "Insert position out of bounds.");
            AddChildren(index, children.ToArray());
            return this;
        }

        /// <summary>
        /// Create a new element by tag name, and add it as the last child.
        /// </summary>
        /// <param name="tagName">
        /// the name of the tag (e.g.
        /// <c>div</c>
        /// ).
        /// </param>
        /// <returns>
        /// the new element, to allow you to add content to it, e.g.:
        /// <c>parent.AppendElement("h1").Attr("id", "header").Text("Welcome");</c>
        /// </returns>
        public Element AppendElement(string tagName)
        {
        	Tag tag = Nodes.Tag.ValueOf(tagName);
            Element child = new Element(tag, BaseUri);
            AppendChild(child);
            return child;
        }

        /// <summary>
        /// Create a new element by tag name, and add it as the first child.
        /// </summary>
        /// <param name="tagName">
        /// the name of the tag (e.g.
        /// <c>div</c>
        /// ).
        /// </param>
        /// <returns>
        /// the new element, to allow you to add content to it, e.g.:
        /// <c>parent.PrependElement("h1").Attr("id", "header").Text("Welcome");</c>
        /// </returns>
        public Element PrependElement(string tagName)
        {
        	Tag tag = Nodes.Tag.ValueOf(tagName);
            Element child = new Element(tag, BaseUri);
            PrependChild(child);
            return child;
        }

        /// <summary>
        /// Create and append a new TextNode to this element.
        /// </summary>
        /// <param name="text">the unencoded text to add</param>
        /// <returns>this element</returns>
        public Element AppendText(string text)
        {
            TextNode node = new TextNode(text, BaseUri);
            AppendChild(node);
            return this;
        }

        /// <summary>
        /// Create and prepend a new TextNode to this element.
        /// </summary>
        /// <param name="text">the unencoded text to add</param>
        /// <returns>this element</returns>
        public Element PrependText(string text)
        {
            TextNode node = new TextNode(text, BaseUri);
            PrependChild(node);
            return this;
        }

        /// <summary>
        /// Add inner HTML to this element.
        /// </summary>
        /// <remarks>
        /// The supplied HTML will be parsed, and each node appended to the end of the children.
        /// </remarks>
        /// <param name="html">HTML to add inside this element, after the existing HTML</param>
        /// <returns>this element</returns>
        /// <seealso cref="Html">Html</seealso>
        public Element Append(string html)
        {
            Validate.NotNull(html);
            IReadOnlyList<Node> nodes = Parser.ParseFragment(html, this, BaseUri);
            AddChildren(nodes.ToArray());
            return this;
        }

        /// <summary>
        /// Add inner HTML into this element.
        /// </summary>
        /// <remarks>
        /// The supplied HTML will be parsed, and each node prepended to the start of the element's children.
        /// </remarks>
        /// <param name="html">HTML to add inside this element, before the existing HTML</param>
        /// <returns>this element</returns>
        /// <seealso cref="Html">Html</seealso>
        public Element Prepend(string html)
        {
            Validate.NotNull(html);
            IReadOnlyList<Node> nodes = Parser.ParseFragment(html, this, BaseUri);
            AddChildren(0, nodes.ToArray());
            return this;
        }

        /// <summary>
        /// Insert the specified HTML into the DOM before this element (as a preceding sibling).
        /// </summary>
        /// <param name="html">HTML to add before this element</param>
        /// <returns>this element, for chaining</returns>
        /// <seealso cref="After(string)">After(string)</seealso>
        public override Node Before(string html)
        {
            return (Element)base.Before(html);
        }

        /// <summary>
        /// Insert the specified node into the DOM before this node (as a preceding sibling).
        /// </summary>
        /// <param name="node">to add before this element</param>
        /// <returns>this Element, for chaining</returns>
        /// <seealso cref="After(Node)">After(Node)</seealso>
        public override Node Before(Node node)
        {
            return (Element)base.Before(node);
        }

        /// <summary>
        /// Insert the specified HTML into the DOM after this element (as a following sibling).
        /// </summary>
        /// <param name="html">HTML to add after this element</param>
        /// <returns>this element, for chaining</returns>
        /// <seealso cref="Before(string)">Before(string)</seealso>
        public override Node After(string html)
        {
            return (Element)base.After(html);
        }

        /// <summary>
        /// Insert the specified node into the DOM after this node (as a following sibling).
        /// </summary>
        /// <param name="node">to add after this element</param>
        /// <returns>this element, for chaining</returns>
        /// <seealso cref="Before(Node)">Before(Node)</seealso>
        public override Node After(Node node)
        {
            return (Element)base.After(node);
        }

        /// <summary>
        /// Remove all of the element's child nodes.
        /// </summary>
        /// <remarks>
        /// Any attributes are left as-is.
        /// </remarks>
        /// <returns>this element</returns>
        public Element Empty()
        {
            childNodes.Clear();
            return this;
        }

        /// <summary>
        /// Wrap the supplied HTML around this element.
        /// </summary>
        /// <param name="html">
        /// HTML to wrap around this element, e.g.
        /// <c><![CDATA[<div class="head"></div>]]></c>
        /// . Can be arbitrarily deep.
        /// </param>
        /// <returns>this element, for chaining.</returns>
        public new Element Wrap(string html)
        {
            return (Element)base.Wrap(html);
        }

        /// <summary>
        /// Get a CSS selector that will uniquely select this element.
        /// </summary>
        /// <remarks>
        /// If the element has an ID, returns #id;
        /// otherwise returns the parent (if any) CSS selector, followed by '&gt;',
        /// followed by a unique selector for the element (tag.class.class:nth-child(n)).
        /// </remarks>
        /// <returns>the CSS Path that can be used to retrieve the element in a selector.</returns>
        public string CssSelector
        {
            get
            {
                if (Id.Length > 0)
                {
                    return "#" + Id;
                }
                StringBuilder selector = new StringBuilder(TagName);
                string classes = string.Join(".", ClassNames);
                if (classes.Length > 0)
                {
                    selector.Append('.').Append(classes);
                }
                if (Parent == null || Parent is Document)
                {
                    // don't add Document to selector, as will always have a html node
                    return selector.ToString();
                }
                selector.Insert(0, " > ");
                if (Parent.Select(selector.ToString()).Count > 1)
                {
                    selector.Append(string.Format(":nth-child({0})", ElementSiblingIndex + 1));
                }
                return Parent.CssSelector + selector.ToString();
            }
        }

        /// <summary>
        /// Get sibling elements.
        /// </summary>
        /// <remarks>
        /// If the element has no sibling elements, returns an empty list. An element is not a sibling
        /// of itself, so will not be included in the returned list.
        /// </remarks>
        /// <returns>sibling elements</returns>
        public Elements SiblingElements
        {
            get
            {
                if (parentNode == null)
                {
                    return new Elements(0);
                }
                IList<Element> elements = Parent.Children;
                Elements siblings = new Elements(elements.Count - 1);
                foreach (Element el in elements)
                {
                    if (el != this)
                    {
                        siblings.Add(el);
                    }
                }
                return siblings;
            }
        }

        /// <summary>
        /// Gets the next sibling element of this element.
        /// </summary>
        /// <remarks>
        /// E.g., if a
        /// <c>div</c>
        /// contains two
        /// <c>p</c>
        /// s,
        /// the
        /// <c>NextElementSibling</c>
        /// of the first
        /// <c>p</c>
        /// is the second
        /// <c>p</c>
        /// .
        /// <p/>
        /// This is similar to
        /// <see cref="Node.NextSibling">Node.NextSibling</see>
        /// , but specifically finds only Elements
        /// </remarks>
        /// <returns>the next element, or null if there is no next element</returns>
        /// <seealso cref="PreviousElementSibling">PreviousElementSibling</seealso>
        public Element NextElementSibling
        {
            get
            {
                if (parentNode == null)
                {
                    return null;
                }
                IList<Element> siblings = Parent.Children;
                int index = IndexInList(this, siblings);
                Validate.IsTrue(index >= 0);
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
        /// Gets the previous element sibling of this element.
        /// </summary>
        /// <returns>the previous element, or null if there is no previous element</returns>
        /// <seealso cref="NextElementSibling">NextElementSibling</seealso>
        public Element PreviousElementSibling
        {
            get
            {
                if (parentNode == null)
                {
                    return null;
                }
                IList<Element> siblings = Parent.Children;
                int index = IndexInList(this, siblings);
                Validate.IsTrue(index >= 0);
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
        /// Gets the first element sibling of this element.
        /// </summary>
        /// <returns>the first sibling that is an element (aka the parent's first element child)
        /// </returns>
        public Element FirstElementSibling
        {
            get
            {
                // todo: should firstSibling() exclude this?
                IList<Element> siblings = Parent.Children;
                return siblings.Count > 1 ? siblings[0] : null;
            }
        }

        /// <summary>
        /// Get the list index of this element in its element sibling list.
        /// </summary>
        /// <remarks>
        /// I.e. if this is the first element sibling, returns 0.
        /// </remarks>
        /// <returns>position in element sibling list</returns>
        public int ElementSiblingIndex
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }
                return IndexInList(this, Parent.Children);
            }
        }

        /// <summary>
        /// Gets the last element sibling of this element
        /// </summary>
        /// <returns>
        /// the last sibling that is an element (aka the parent's last element child)
        /// </returns>
        public Element LastElementSibling
        {
            get
            {
                IList<Element> siblings = Parent.Children;
                return siblings.Count > 1 ? siblings[siblings.Count - 1] : null;
            }
        }

        private static int IndexInList(Element search, IList<Element> elements)
        {
            Validate.NotNull(search);
            Validate.NotNull(elements);
            return elements.IndexOf(search); // compare using Equals() method
        }

        // DOM type methods
        
        /// <summary>
        /// Finds elements, including and recursively under this element,
        /// with the specified tag name.
        /// </summary>
        /// <param name="tagName">The tag name to search for (case insensitively).</param>
        /// <returns>
        /// a matching unmodifiable list of elements.
        /// Will be empty if this element and none of its children match.
        /// </returns>
        public Elements GetElementsByTag(string tagName)
        {
            Validate.NotEmpty(tagName);
            tagName = tagName.ToLower().Trim();
            return Collector.Collect(new Evaluator.Tag(tagName), this);
        }

        /// <summary>
        /// Find an element by ID, including or under this element.
        /// </summary>
        /// <remarks>
        /// Note that this finds the first matching ID, starting with this element. If you search down from a different
        /// starting point, it is possible to find a different element by ID. For unique element by ID within a Document,
        /// use
        /// <see cref="Element.GetElementById(string)">Element.GetElementById(string)</see>
        /// </remarks>
        /// <param name="id">The ID to search for.</param>
        /// <returns>The first matching element by ID, starting with this element, or null if none found.
        /// </returns>
        public Element GetElementById(string id)
        {
            Validate.NotEmpty(id);
            Elements elements = Collector.Collect(new Evaluator.ID(id), this);
            if (elements.Count > 0)
            {
                return elements[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Find elements that have this class, including or under this element.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// <p/>
        /// Elements can have multiple classes (e.g.
        /// <c>&lt;div class="header round first"&gt;</c>
        /// . This method
        /// checks each class, so you can find the above with
        /// <c>el.GetElementsByClass("header");</c>
        /// .
        /// </remarks>
        /// <param name="className">the name of the class to search for.</param>
        /// <returns>elements with the supplied class name, empty if none</returns>
        /// <seealso cref="HasClass(string)">HasClass(string)</seealso>
        /// <seealso cref="ClassNames">ClassNames</seealso>
        public Elements GetElementsByClass(string className)
        {
            Validate.NotEmpty(className);
            return Collector.Collect(new Evaluator.Class(className), this);
        }

        /// <summary>
        /// Find elements that have a named attribute set.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="key">
        /// name of the attribute, e.g.
        /// <c>href</c>
        /// </param>
        /// <returns>elements that have this attribute, empty if none</returns>
        public Elements GetElementsByAttribute(string key)
        {
            Validate.NotEmpty(key);
            key = key.Trim().ToLower();
            return Collector.Collect(new Evaluator.Attribute(key), this);
        }

        /// <summary>
        /// Find elements that have an attribute name starting with the supplied prefix.
        /// </summary>
        /// <remarks>
        /// Use
        /// <c>data-</c>
        /// to find elements
        /// that have HTML5 datasets.
        /// </remarks>
        /// <param name="keyPrefix">
        /// name prefix of the attribute e.g.
        /// <c>data-</c>
        /// </param>
        /// <returns>elements that have attribute names that start with with the prefix, empty if none.
        /// </returns>
        public Elements GetElementsByAttributeStarting(string keyPrefix)
        {
            Validate.NotEmpty(keyPrefix);
            keyPrefix = keyPrefix.Trim().ToLower();
            return Collector.Collect(new Evaluator.AttributeStarting(keyPrefix), this);
        }

        /// <summary>
        /// Find elements that have an attribute with the specific value.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="key">name of the attribute</param>
        /// <param name="value">value of the attribute</param>
        /// <returns>elements that have this attribute with this value, empty if none</returns>
        public Elements GetElementsByAttributeValue(string key, string value)
        {
            return Collector.Collect(new Evaluator.AttributeWithValue(key, value), this);
        }

        /// <summary>
        /// Find elements that either do not have this attribute,
        /// or have it with a different value.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="key">name of the attribute</param>
        /// <param name="value">value of the attribute</param>
        /// <returns>elements that do not have a matching attribute</returns>
        public Elements GetElementsByAttributeValueNot(string key, string value)
        {
            return Collector.Collect(new Evaluator.AttributeWithValueNot(key, value), this);
        }

        /// <summary>
        /// Find elements that have attributes that start with the value prefix.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="key">name of the attribute</param>
        /// <param name="valuePrefix">start of attribute value</param>
        /// <returns>elements that have attributes that start with the value prefix</returns>
        public Elements GetElementsByAttributeValueStarting(string key, string valuePrefix)
        {
            return Collector.Collect(new Evaluator.AttributeWithValueStarting(key, valuePrefix), this);
        }

        /// <summary>
        /// Find elements that have attributes that end with the value suffix.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="key">name of the attribute</param>
        /// <param name="valueSuffix">end of the attribute value</param>
        /// <returns>elements that have attributes that end with the value suffix</returns>
        public Elements GetElementsByAttributeValueEnding(string key, string valueSuffix)
        {
            return Collector.Collect(new Evaluator.AttributeWithValueEnding(key, valueSuffix), this);
        }

        /// <summary>
        /// Find elements that have attributes whose value contains the match string.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="key">name of the attribute</param>
        /// <param name="match">substring of value to search for</param>
        /// <returns>elements that have attributes containing this text</returns>
        public Elements GetElementsByAttributeValueContaining(string key, string match)
        {
            return Collector.Collect(new Evaluator.AttributeWithValueContaining(key, match), this);
        }

        /// <summary>
        /// Find elements that have attributes whose values match the supplied regular expression.
        /// </summary>
        /// <param name="key">name of the attribute</param>
        /// <param name="pattern">compiled regular expression to match against attribute values
        /// </param>
        /// <returns>elements that have attributes matching this regular expression</returns>
        public Elements GetElementsByAttributeValueMatching(string key, Regex pattern)
        {
            return Collector.Collect(new Evaluator.AttributeWithValueMatching(key, pattern), this);
        }
        //public Elements GetElementsByAttributeValueMatching(string key, Sharpen.Pattern pattern)

        /// <summary>
        /// Find elements that have attributes whose values match the supplied regular expression.
        /// </summary>
        /// <param name="key">name of the attribute</param>
        /// <param name="regex">regular expression to match against attribute values. You can use <a href="http://java.sun.com/docs/books/tutorial/essential/regex/pattern.html#embedded">embedded flags</a> (such as (?i) and (?m) to control regex options.
        /// </param>
        /// <returns>elements that have attributes matching this regular expression</returns>
        public Elements GetElementsByAttributeValueMatching(string key, string regex)
        {
            Regex pattern;
            pattern = CompatUtil.CreateCompiledRegex(regex); // may throw an exception
            return GetElementsByAttributeValueMatching(key, pattern);
        }

        /// <summary>
        /// Find elements whose sibling index is less than the supplied index.
        /// </summary>
        /// <param name="index">0-based index</param>
        /// <returns>elements less than index</returns>
        public Elements GetElementsByIndexLessThan(int index)
        {
            return Collector.Collect(new Evaluator.IndexLessThan(index), this);
        }

        /// <summary>
        /// Find elements whose sibling index is greater than the supplied index.
        /// </summary>
        /// <param name="index">0-based index</param>
        /// <returns>elements greater than index</returns>
        public Elements GetElementsByIndexGreaterThan(int index)
        {
            return Collector.Collect(new Evaluator.IndexGreaterThan(index), this);
        }

        /// <summary>
        /// Find elements whose sibling index is equal to the supplied index.
        /// </summary>
        /// <param name="index">0-based index</param>
        /// <returns>elements equal to index</returns>
        public Elements GetElementsByIndexEquals(int index)
        {
            return Collector.Collect(new Evaluator.IndexEquals(index), this);
        }

        /// <summary>
        /// Find elements that contain the specified string.
        /// </summary>
        /// <remarks>
        /// The search is case insensitive. The text may appear directly
        /// in the element, or in any of its descendants.
        /// </remarks>
        /// <param name="searchText">to look for in the element's text</param>
        /// <returns>elements that contain the string, case insensitive.</returns>
        /// <seealso cref="Element.Text">Element.Text</seealso>
        public Elements GetElementsContainingText(string searchText)
        {
            return Collector.Collect(new Evaluator.ContainsText(searchText), this);
        }

        /// <summary>
        /// Find elements that directly contain the specified string.
        /// </summary>
        /// <remarks>
        /// The search is case insensitive. The text must appear directly
        /// in the element, not in any of its descendants.
        /// </remarks>
        /// <param name="searchText">to look for in the element's own text</param>
        /// <returns>elements that contain the string, case insensitive.</returns>
        /// <seealso cref="Element.OwnText">Element.OwnText</seealso>
        public Elements GetElementsContainingOwnText(string searchText)
        {
            return Collector.Collect(new Evaluator.ContainsOwnText(searchText), this);
        }

        /// <summary>
        /// Find elements whose text matches the supplied regular expression.
        /// </summary>
        /// <param name="pattern">regular expression to match text against</param>
        /// <returns>elements matching the supplied regular expression.</returns>
        /// <seealso cref="Element.Text">Element.Text</seealso>
        public Elements GetElementsMatchingText(Regex pattern)
        {
            return Collector.Collect(new Evaluator.MatchesText(pattern), this);
        }

        /// <summary>
        /// Find elements whose text matches the supplied regular expression.
        /// </summary>
        /// <param name="regex">regular expression to match text against. You can use <a href="http://java.sun.com/docs/books/tutorial/essential/regex/pattern.html#embedded">embedded flags</a> (such as (?i) and (?m) to control regex options.
        /// </param>
        /// <returns>elements matching the supplied regular expression.</returns>
        /// <seealso cref="Element.Text">Element.Text</seealso>
        public Elements GetElementsMatchingText(string regex)
        {
            Regex pattern;
            pattern = CompatUtil.CreateCompiledRegex(regex); // may throw an exception
            return GetElementsMatchingText(pattern);
        }

        /// <summary>
        /// Find elements whose own text matches the supplied regular expression.
        /// </summary>
        /// <param name="pattern">regular expression to match text against</param>
        /// <returns>elements matching the supplied regular expression.</returns>
        /// <seealso cref="Element.OwnText">Element.OwnText</seealso>
        public Elements GetElementsMatchingOwnText(Regex pattern)
        {
            return Collector.Collect(new Evaluator.MatchesOwnText(pattern), this);
        }

        /// <summary>
        /// Find elements whose text matches the supplied regular expression.
        /// </summary>
        /// <param name="regex">regular expression to match text against. You can use <a href="http://java.sun.com/docs/books/tutorial/essential/regex/pattern.html#embedded">embedded flags</a> (such as (?i) and (?m) to control regex options.
        /// </param>
        /// <returns>elements matching the supplied regular expression.</returns>
        /// <seealso cref="Element.OwnText">Element.OwnText</seealso>
        public Elements GetElementsMatchingOwnText(string regex)
        {
            Regex pattern = CompatUtil.CreateCompiledRegex(regex); // may throw an exception
            return GetElementsMatchingOwnText(pattern);
        }

        /// <summary>
        /// Find all elements under this element (including self, and children of children).
        /// </summary>
        /// <returns>all elements</returns>
        public Elements GetAllElements()
        {
            return Collector.Collect(new Evaluator.AllElements(), this);
        }

        /// <summary>
        /// Get or Set the combined text of this element and all its children.
        /// </summary>
        /// <remarks>
        /// <para>
        /// when get, whitespace is normalized and trimmed.
        /// <p/>
        /// For example, given HTML
        /// <c>&lt;p&gt;Hello  &lt;b&gt;there&lt;/b&gt; now! &lt;/p&gt;</c>,
        /// <c>p.Text</c> returns <c>"Hello there now!"</c>
        /// </para>
        /// <para>
        /// when set, any existing contents (text or elements) will be cleared.
        /// </para>
        /// </remarks>
        /// <value>unencoded text</value>
        /// <returns>unencoded text, or empty string if none.</returns>
        /// <seealso cref="OwnText">OwnText</seealso>
        /// <seealso cref="TextNodes">TextNodes</seealso>
        public virtual string Text
        {
            get
            {
                StringBuilder accum = new StringBuilder();
                new NodeTraversor(new TextVisitor(accum)).Traverse(this);
                return accum.ToString().Trim();
            }
            set
            {
                Validate.NotNull(value);
                Empty();
                TextNode textNode = new TextNode(value, baseUri);
                AppendChild(textNode);
            }
        }

        private sealed class TextVisitor : INodeVisitor
        {
            internal TextVisitor(StringBuilder accum)
            {
                this.accum = accum;
            }

            public void Head(Node node, int depth)
            {
                if (node is TextNode)
                {
                    TextNode textNode = (TextNode)node;
                    Supremes.Nodes.Element.AppendNormalisedText(accum, textNode);
                }
                else
                {
                    if (node is Element)
                    {
                        Element element = (Element)node;
                        if (accum.Length > 0 && (element.IsBlock || element.TagName.Equals("br")) && 
                            !TextNode.LastCharIsWhitespace(accum))
                        {
                            accum.Append(" ");
                        }
                    }
                }
            }

            public void Tail(Node node, int depth)
            {
            }

            private readonly StringBuilder accum;
        }

        /// <summary>
        /// Gets the text owned by this element only;
        /// does not get the combined text of all children.
        /// </summary>
        /// <remarks>
        /// For example, given HTML
        /// <c>&lt;p&gt;Hello &lt;b&gt;there&lt;/b&gt; now!&lt;/p&gt;</c>
        /// ,
        /// <c>p.OwnText</c>
        /// returns
        /// <c>"Hello now!"</c>
        /// ,
        /// whereas
        /// <c>p.Text</c>
        /// returns
        /// <c>"Hello there now!"</c>
        /// .
        /// Note that the text within the
        /// <c>b</c>
        /// element is not returned, as it is not a direct child of the
        /// <c>p</c>
        /// element.
        /// </remarks>
        /// <returns>unencoded text, or empty string if none.</returns>
        /// <seealso cref="Text">Text</seealso>
        /// <seealso cref="TextNodes">TextNodes</seealso>
        public string OwnText
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                AppendOwnTextTo(sb);
                return sb.ToString().Trim();
            }
        }

        private void AppendOwnTextTo(StringBuilder accum)
        {
            foreach (Node child in childNodes)
            {
                if (child is TextNode)
                {
                    TextNode textNode = (TextNode)child;
                    AppendNormalisedText(accum, textNode);
                }
                else
                {
                    if (child is Element)
                    {
                        AppendWhitespaceIfBr((Element)child, accum);
                    }
                }
            }
        }

        private static void AppendNormalisedText(StringBuilder accum, TextNode textNode)
        {
            string text = textNode.WholeText;
            if (PreserveWhitespace(textNode.Parent))
            {
                accum.Append(text);
            }
            else
            {
                StringUtil.AppendNormalisedWhitespace(accum, text, TextNode.LastCharIsWhitespace(accum));
            }
        }

        private static void AppendWhitespaceIfBr(Element element, StringBuilder accum)
        {
            if (element.TagName.Equals("br") && !TextNode.LastCharIsWhitespace(accum))
            {
                accum.Append(" ");
            }
        }

        internal static bool PreserveWhitespace(Node node)
        {
            // looks only at this element and one level up, to prevent recursion & needless stack searches
            if (node != null && node is Element)
            {
                Element element = (Element)node;
                return element.Tag.PreservesWhitespace || element.Parent != null && element.Parent.Tag.PreservesWhitespace;
            }
            return false;
        }

        /// <summary>
        /// Test if this element has any text content (that is not just whitespace).
        /// </summary>
        /// <returns>true if element has non-blank text content.</returns>
        public bool HasText
        {
            get
            {
                foreach (Node child in childNodes)
                {
                    if (child is TextNode)
                    {
                        TextNode textNode = (TextNode)child;
                        if (!textNode.IsBlank)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (child is Element)
                        {
                            Element el = (Element)child;
                            if (el.HasText)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Get the combined data of this element.
        /// </summary>
        /// <remarks>
        /// Data is e.g. the inside of a
        /// <c>script</c>
        /// tag.
        /// </remarks>
        /// <returns>the data, or empty string if none</returns>
        /// <seealso cref="DataNodes">DataNodes</seealso>
        public string Data
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (Node childNode in childNodes)
                {
                    if (childNode is DataNode)
                    {
                        DataNode data = (DataNode)childNode;
                        sb.Append(data.WholeData);
                    }
                    else
                    {
                        if (childNode is Element)
                        {
                            Element element = (Element)childNode;
                            string elementData = element.Data;
                            sb.Append(elementData);
                        }
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the literal value of this element's "class" attribute,
        /// which may include multiple class names, space separated.
        /// </summary>
        /// <remarks>
        /// (E.g. on <c>&lt;div class="header gray"&gt;</c> returns,
        /// "<c>header gray</c>")
        /// </remarks>
        /// <returns>
        /// The literal class attribute, or <b>empty string</b>
        /// if no class attribute set.
        /// </returns>
        public string ClassName
        {
            get { return Attr("class"); }
        }

        /// <summary>
        /// Get or Set all of the element's class names.
        /// </summary>
        /// <remarks>
        /// <para>
        /// E.g. on element
        /// <c>&lt;div class="header gray"&gt;</c>, 
        /// this property returns a set of two elements
        /// <c>"header", "gray"</c>.
        /// Note that modifications to this set are not pushed to
        /// the backing <c>class</c> attribute;
        /// use the
        /// <see cref="ClassNames">ClassNames</see>
        /// method to persist them.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>the new set of classes</value>
        /// <returns>set of classnames, empty if no class attribute</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public ICollection<string> ClassNames
        {
            get
            {
                if (classNames == null)
                {
                    string[] names = Regex.Split(ClassName, "\\s+");
                    classNames = new LinkedHashSet<string>(names);
                }
                return classNames;
            }
            set
            {
                Validate.NotNull(value);
                attributes["class"] = string.Join(" ", value);
            }
        }

        /// <summary>
        /// Tests if this element has a class.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="className">name of class to check for</param>
        /// <returns>true if it does, false if not</returns>
        public bool HasClass(string className)
        {
            ICollection<string> classNames = ClassNames;
            foreach (string name in classNames)
            {
                if (string.Equals(className, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add a class name to this element's
        /// <c>class</c>
        /// attribute.
        /// </summary>
        /// <param name="className">class name to add</param>
        /// <returns>this element</returns>
        public Element AddClass(string className)
        {
            Validate.NotNull(className);
            ICollection<string> classes = ClassNames;
            classes.Add(className);
            ClassNames = classes;
            return this;
        }

        /// <summary>
        /// Remove a class name from this element's
        /// <c>class</c>
        /// attribute.
        /// </summary>
        /// <param name="className">class name to remove</param>
        /// <returns>this element</returns>
        public Element RemoveClass(string className)
        {
            Validate.NotNull(className);
            ICollection<string> classes = ClassNames;
            classes.Remove(className);
            ClassNames = classes;
            return this;
        }

        /// <summary>
        /// Toggle a class name on this element's
        /// <c>class</c>
        /// attribute: if present, remove it; otherwise add it.
        /// </summary>
        /// <param name="className">class name to toggle</param>
        /// <returns>this element</returns>
        public Element ToggleClass(string className)
        {
            Validate.NotNull(className);
            ICollection<string> classes = ClassNames;
            if (classes.Contains(className))
            {
                classes.Remove(className);
            }
            else
            {
                classes.Add(className);
            }
            ClassNames = classes;
            return this;
        }

        /// <summary>
        /// Get or Set the value of a form element (input, textarea, etc).
        /// </summary>
        /// <remarks>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>value to set</value>
        /// <returns>the value of the form element, or empty string if not set.</returns>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public string Val
        {
            get
            {
                if (TagName.Equals("textarea"))
                {
                    return Text;
                }
                else
                {
                    return Attr("value");
                }
            }
            set
            {
                if (TagName.Equals("textarea"))
                {
                    Text = value;
                }
                else
                {
                    Attr("value", value);
                }
            }
        }

        internal override void AppendOuterHtmlHeadTo(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            if (accum.Length > 0
                && @out.PrettyPrint
                && (tag.IsFormattedAsBlock
                    || (Parent != null && Parent.Tag.IsFormattedAsBlock)
                    || @out.Outline))
            {
                Indent(accum, depth, @out);
            }
            accum.Append("<").Append(TagName);
            attributes.AppendHtmlTo(accum, @out);
            // selfclosing includes unknown tags, isEmpty defines tags that are always empty
            if (childNodes.Count == 0 && tag.IsSelfClosing)
            {
                if (@out.Syntax == DocumentSyntax.Html && tag.IsEmpty)
                {
                    accum.Append('>');
                }
                else
                {
                    accum.Append(" />");
                }
            }
            else
            {
                // <img> in html, <img /> in xml
                accum.Append(">");
            }
        }

        internal override void AppendOuterHtmlTailTo(StringBuilder accum, int depth, DocumentOutputSettings @out)
        {
            if (!(childNodes.Count == 0 && tag.IsSelfClosing))
            {
                if (@out.PrettyPrint
                    && (childNodes.Count > 0
                        && (tag.IsFormattedAsBlock
                            || (@out.Outline
                                && (childNodes.Count > 1
                                    || (childNodes.Count == 1
                                        && !(childNodes[0] is TextNode)))))))
                {
                    Indent(accum, depth, @out);
                }
                accum.Append("</").Append(TagName).Append(">");
            }
        }

        /// <summary>
        /// Get Or Set the element's inner HTML.
        /// </summary>
        /// <remarks>
        /// <para>
        /// when get on a <c>&lt;div&gt;</c> with one empty <c>&lt;p&gt;</c>,
        /// would return <c>&lt;p&gt;&lt;/p&gt;</c>.
        /// (Whereas
        /// <see cref="Node.OuterHtml">Node.OuterHtml</see>
        /// would return
        /// <c>&lt;div&gt;&lt;p&gt;&lt;/p&gt;&lt;/div&gt;</c>.)
        /// </para>
        /// <para>
        /// when set, clears the existing HTML first.
        /// </para>
        /// <para>
        /// if you want to use fluent API, write <c>using Supremes.Fluent;</c>.
        /// </para>
        /// </remarks>
        /// <value>HTML to parse and set into this element</value>
        /// <returns>String of HTML.</returns>
        /// <seealso cref="Node.OuterHtml">Node.OuterHtml</seealso>
        /// <seealso cref="Append(string)">Append(string)</seealso>
        /// <seealso cref="Supremes.Fluent.FluentUtility">Supremes.Fluent.FluentUtility</seealso>
        public string Html
        {
            get
            {
                StringBuilder accum = new StringBuilder();
                AppendHtmlTo(accum);
                return GetOutputSettings().PrettyPrint
                    ? accum.ToString().Trim()
                    : accum.ToString();
            }
            set
            {
                Empty();
                Append(value);
            }
        }

        private void AppendHtmlTo(StringBuilder accum)
        {
            foreach (Node node in childNodes)
            {
                ((Node)node).AppendOuterHtmlTo(accum);
            }
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
        /// Compares two <see cref="Element"/> instances for equality.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            return this == o;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // todo: fixup, not very useful
            int result = base.GetHashCode();
            unchecked
            {
                result = 31 * result + (tag != null ? tag.GetHashCode() : 0);
            }
            return result;
        }

        internal override Node Clone()
        {
            return PrivateClone();
        }

        private Element PrivateClone()
        {
            Supremes.Nodes.Element clone = (Supremes.Nodes.Element)base.Clone();
            clone.classNames = null;
            // derived on first hit, otherwise gets a pointer to source classnames
            return clone;
        }
    }
}
