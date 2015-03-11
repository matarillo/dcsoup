/*
 * This code is derived from jsoup 1.8.1 (http://jsoup.org/news/release-1.8.1)
 */

using System;
using System.Collections.Generic;
using System.Text;
using NSoup.Helper;
using NSoup.Nodes;
using NSoup.Select;
using Sharpen;

namespace NSoup.Select
{
	/// <summary>
	/// A list of
	/// <see cref="NSoup.Nodes.Element">NSoup.Nodes.Element</see>
	/// s, with methods that act on every element in the list.
	/// <p/>
	/// To get an
	/// <code>Elements</code>
	/// object, use the
	/// <see cref="NSoup.Nodes.Element.Select(string)">NSoup.Nodes.Element.Select(string)
	/// 	</see>
	/// method.
	/// </summary>
	/// <author>Jonathan Hedley, jonathan@hedley.net</author>
	public class Elements : IList<Element>, ICloneable
	{
		private IList<Element> contents;

		public Elements()
		{
			contents = new AList<Element>();
		}

		public Elements(int initialCapacity)
		{
			contents = new AList<Element>(initialCapacity);
		}

		public Elements(ICollection<Element> elements)
		{
			contents = new AList<Element>(elements);
		}

		public Elements(IList<Element> elements)
		{
			contents = elements;
		}

		public Elements(params Element[] elements) : this(Arrays.AsList(elements))
		{
		}

		/// <summary>Creates a deep copy of these elements.</summary>
		/// <remarks>Creates a deep copy of these elements.</remarks>
		/// <returns>a deep copy</returns>
		public virtual NSoup.Select.Elements Clone()
		{
			NSoup.Select.Elements clone;
			clone = (NSoup.Select.Elements)base.Clone();
			IList<Element> elements = new AList<Element>();
			clone.contents = elements;
			foreach (Element e in contents)
			{
				elements.AddItem(((Element)e.Clone()));
			}
			return clone;
		}

		// attribute methods
		/// <summary>Get an attribute value from the first matched element that has the attribute.
		/// 	</summary>
		/// <remarks>Get an attribute value from the first matched element that has the attribute.
		/// 	</remarks>
		/// <param name="attributeKey">The attribute key.</param>
		/// <returns>
		/// The attribute value from the first matched element that has the attribute.. If no elements were matched (isEmpty() == true),
		/// or if the no elements have the attribute, returns empty string.
		/// </returns>
		/// <seealso cref="HasAttr(string)">HasAttr(string)</seealso>
		public virtual string Attr(string attributeKey)
		{
			foreach (Element element in contents)
			{
				if (element.HasAttr(attributeKey))
				{
					return element.Attr(attributeKey);
				}
			}
			return string.Empty;
		}

		/// <summary>Checks if any of the matched elements have this attribute set.</summary>
		/// <remarks>Checks if any of the matched elements have this attribute set.</remarks>
		/// <param name="attributeKey">attribute key</param>
		/// <returns>true if any of the elements have the attribute; false if none do.</returns>
		public virtual bool HasAttr(string attributeKey)
		{
			foreach (Element element in contents)
			{
				if (element.HasAttr(attributeKey))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Set an attribute on all matched elements.</summary>
		/// <remarks>Set an attribute on all matched elements.</remarks>
		/// <param name="attributeKey">attribute key</param>
		/// <param name="attributeValue">attribute value</param>
		/// <returns>this</returns>
		public virtual NSoup.Select.Elements Attr(string attributeKey, string attributeValue
			)
		{
			foreach (Element element in contents)
			{
				element.Attr(attributeKey, attributeValue);
			}
			return this;
		}

		/// <summary>Remove an attribute from every matched element.</summary>
		/// <remarks>Remove an attribute from every matched element.</remarks>
		/// <param name="attributeKey">The attribute to remove.</param>
		/// <returns>this (for chaining)</returns>
		public virtual NSoup.Select.Elements RemoveAttr(string attributeKey)
		{
			foreach (Element element in contents)
			{
				element.RemoveAttr(attributeKey);
			}
			return this;
		}

		/// <summary>
		/// Add the class name to every matched element's
		/// <code>class</code>
		/// attribute.
		/// </summary>
		/// <param name="className">class name to add</param>
		/// <returns>this</returns>
		public virtual NSoup.Select.Elements AddClass(string className)
		{
			foreach (Element element in contents)
			{
				element.AddClass(className);
			}
			return this;
		}

		/// <summary>
		/// Remove the class name from every matched element's
		/// <code>class</code>
		/// attribute, if present.
		/// </summary>
		/// <param name="className">class name to remove</param>
		/// <returns>this</returns>
		public virtual NSoup.Select.Elements RemoveClass(string className)
		{
			foreach (Element element in contents)
			{
				element.RemoveClass(className);
			}
			return this;
		}

		/// <summary>
		/// Toggle the class name on every matched element's
		/// <code>class</code>
		/// attribute.
		/// </summary>
		/// <param name="className">class name to add if missing, or remove if present, from every element.
		/// 	</param>
		/// <returns>this</returns>
		public virtual NSoup.Select.Elements ToggleClass(string className)
		{
			foreach (Element element in contents)
			{
				element.ToggleClass(className);
			}
			return this;
		}

		/// <summary>
		/// Determine if any of the matched elements have this class name set in their
		/// <code>class</code>
		/// attribute.
		/// </summary>
		/// <param name="className">class name to check for</param>
		/// <returns>true if any do, false if none do</returns>
		public virtual bool HasClass(string className)
		{
			foreach (Element element in contents)
			{
				if (element.HasClass(className))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Get the form element's value of the first matched element.</summary>
		/// <remarks>Get the form element's value of the first matched element.</remarks>
		/// <returns>The form element's value, or empty if not set.</returns>
		/// <seealso cref="NSoup.Nodes.Element.Val()">NSoup.Nodes.Element.Val()</seealso>
		public virtual string Val()
		{
			if (Count > 0)
			{
				return First().Val();
			}
			else
			{
				return string.Empty;
			}
		}

		/// <summary>Set the form element's value in each of the matched elements.</summary>
		/// <remarks>Set the form element's value in each of the matched elements.</remarks>
		/// <param name="value">The value to set into each matched element</param>
		/// <returns>this (for chaining)</returns>
		public virtual NSoup.Select.Elements Val(string value)
		{
			foreach (Element element in contents)
			{
				element.Val(value);
			}
			return this;
		}

		/// <summary>Get the combined text of all the matched elements.</summary>
		/// <remarks>
		/// Get the combined text of all the matched elements.
		/// <p>
		/// Note that it is possible to get repeats if the matched elements contain both parent elements and their own
		/// children, as the Element.text() method returns the combined text of a parent and all its children.
		/// </remarks>
		/// <returns>string of all text: unescaped and no HTML.</returns>
		/// <seealso cref="NSoup.Nodes.Element.Text()">NSoup.Nodes.Element.Text()</seealso>
		public virtual string Text()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Element element in contents)
			{
				if (sb.Length != 0)
				{
					sb.Append(" ");
				}
				sb.Append(element.Text());
			}
			return sb.ToString();
		}

		public virtual bool HasText()
		{
			foreach (Element element in contents)
			{
				if (element.HasText())
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Get the combined inner HTML of all matched elements.</summary>
		/// <remarks>Get the combined inner HTML of all matched elements.</remarks>
		/// <returns>string of all element's inner HTML.</returns>
		/// <seealso cref="Text()">Text()</seealso>
		/// <seealso cref="OuterHtml()">OuterHtml()</seealso>
		public virtual string Html()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Element element in contents)
			{
				if (sb.Length != 0)
				{
					sb.Append("\n");
				}
				sb.Append(element.Html());
			}
			return sb.ToString();
		}

		/// <summary>Get the combined outer HTML of all matched elements.</summary>
		/// <remarks>Get the combined outer HTML of all matched elements.</remarks>
		/// <returns>string of all element's outer HTML.</returns>
		/// <seealso cref="Text()">Text()</seealso>
		/// <seealso cref="Html()">Html()</seealso>
		public virtual string OuterHtml()
		{
			StringBuilder sb = new StringBuilder();
			foreach (Element element in contents)
			{
				if (sb.Length != 0)
				{
					sb.Append("\n");
				}
				sb.Append(element.OuterHtml());
			}
			return sb.ToString();
		}

		/// <summary>Get the combined outer HTML of all matched elements.</summary>
		/// <remarks>
		/// Get the combined outer HTML of all matched elements. Alias of
		/// <see cref="OuterHtml()">OuterHtml()</see>
		/// .
		/// </remarks>
		/// <returns>string of all element's outer HTML.</returns>
		/// <seealso cref="Text()">Text()</seealso>
		/// <seealso cref="Html()">Html()</seealso>
		public override string ToString()
		{
			return OuterHtml();
		}

		/// <summary>Update the tag name of each matched element.</summary>
		/// <remarks>
		/// Update the tag name of each matched element. For example, to change each
		/// <code><i></code>
		/// to a
		/// <code><em></code>
		/// , do
		/// <code>doc.select("i").tagName("em");</code>
		/// </remarks>
		/// <param name="tagName">the new tag name</param>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.TagName(string)">NSoup.Nodes.Element.TagName(string)
		/// 	</seealso>
		public virtual NSoup.Select.Elements TagName(string tagName)
		{
			foreach (Element element in contents)
			{
				element.TagName(tagName);
			}
			return this;
		}

		/// <summary>Set the inner HTML of each matched element.</summary>
		/// <remarks>Set the inner HTML of each matched element.</remarks>
		/// <param name="html">HTML to parse and set into each matched element.</param>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.Html(string)">NSoup.Nodes.Element.Html(string)
		/// 	</seealso>
		public virtual NSoup.Select.Elements Html(string html)
		{
			foreach (Element element in contents)
			{
				element.Html(html);
			}
			return this;
		}

		/// <summary>Add the supplied HTML to the start of each matched element's inner HTML.
		/// 	</summary>
		/// <remarks>Add the supplied HTML to the start of each matched element's inner HTML.
		/// 	</remarks>
		/// <param name="html">HTML to add inside each element, before the existing HTML</param>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.Prepend(string)">NSoup.Nodes.Element.Prepend(string)
		/// 	</seealso>
		public virtual NSoup.Select.Elements Prepend(string html)
		{
			foreach (Element element in contents)
			{
				element.Prepend(html);
			}
			return this;
		}

		/// <summary>Add the supplied HTML to the end of each matched element's inner HTML.</summary>
		/// <remarks>Add the supplied HTML to the end of each matched element's inner HTML.</remarks>
		/// <param name="html">HTML to add inside each element, after the existing HTML</param>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.Append(string)">NSoup.Nodes.Element.Append(string)
		/// 	</seealso>
		public virtual NSoup.Select.Elements Append(string html)
		{
			foreach (Element element in contents)
			{
				element.Append(html);
			}
			return this;
		}

		/// <summary>Insert the supplied HTML before each matched element's outer HTML.</summary>
		/// <remarks>Insert the supplied HTML before each matched element's outer HTML.</remarks>
		/// <param name="html">HTML to insert before each element</param>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.Before(string)">NSoup.Nodes.Element.Before(string)
		/// 	</seealso>
		public virtual NSoup.Select.Elements Before(string html)
		{
			foreach (Element element in contents)
			{
				element.Before(html);
			}
			return this;
		}

		/// <summary>Insert the supplied HTML after each matched element's outer HTML.</summary>
		/// <remarks>Insert the supplied HTML after each matched element's outer HTML.</remarks>
		/// <param name="html">HTML to insert after each element</param>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.After(string)">NSoup.Nodes.Element.After(string)
		/// 	</seealso>
		public virtual NSoup.Select.Elements After(string html)
		{
			foreach (Element element in contents)
			{
				element.After(html);
			}
			return this;
		}

		/// <summary>Wrap the supplied HTML around each matched elements.</summary>
		/// <remarks>
		/// Wrap the supplied HTML around each matched elements. For example, with HTML
		/// <code><p><b>This</b> is <b>Jsoup</b></p></code>
		/// ,
		/// <code>doc.select("b").wrap("&lt;i&gt;&lt;/i&gt;");</code>
		/// becomes
		/// <code><p><i><b>This</b></i> is <i><b>jsoup</b></i></p></code>
		/// </remarks>
		/// <param name="html">
		/// HTML to wrap around each element, e.g.
		/// <code><div class="head"></div></code>
		/// . Can be arbitrarily deep.
		/// </param>
		/// <returns>this (for chaining)</returns>
		/// <seealso cref="NSoup.Nodes.Element.Wrap(string)">NSoup.Nodes.Element.Wrap(string)
		/// 	</seealso>
		public virtual NSoup.Select.Elements Wrap(string html)
		{
			Validate.NotEmpty(html);
			foreach (Element element in contents)
			{
				element.Wrap(html);
			}
			return this;
		}

		/// <summary>Removes the matched elements from the DOM, and moves their children up into their parents.
		/// 	</summary>
		/// <remarks>
		/// Removes the matched elements from the DOM, and moves their children up into their parents. This has the effect of
		/// dropping the elements but keeping their children.
		/// <p/>
		/// This is useful for e.g removing unwanted formatting elements but keeping their contents.
		/// <p/>
		/// E.g. with HTML:
		/// <code><div><font>One</font> <font><a href="/">Two</a></font></div></code>
		/// <br/>
		/// <code>doc.select("font").unwrap();</code>
		/// <br/>
		/// HTML =
		/// <code><div>One <a href="/">Two</a></div></code>
		/// </remarks>
		/// <returns>this (for chaining)</returns>
		/// <seealso cref="NSoup.Nodes.Node.Unwrap()">NSoup.Nodes.Node.Unwrap()</seealso>
		public virtual NSoup.Select.Elements Unwrap()
		{
			foreach (Element element in contents)
			{
				element.Unwrap();
			}
			return this;
		}

		/// <summary>Empty (remove all child nodes from) each matched element.</summary>
		/// <remarks>
		/// Empty (remove all child nodes from) each matched element. This is similar to setting the inner HTML of each
		/// element to nothing.
		/// <p>
		/// E.g. HTML:
		/// <code><div><p>Hello <b>there</b></p> <p>now</p></div></code>
		/// <br />
		/// <code>doc.select("p").empty();</code><br />
		/// HTML =
		/// <code><div><p></p> <p></p></div></code>
		/// </remarks>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.Empty()">NSoup.Nodes.Element.Empty()</seealso>
		/// <seealso cref="Remove()">Remove()</seealso>
		public virtual NSoup.Select.Elements Empty()
		{
			foreach (Element element in contents)
			{
				element.Empty();
			}
			return this;
		}

		/// <summary>Remove each matched element from the DOM.</summary>
		/// <remarks>
		/// Remove each matched element from the DOM. This is similar to setting the outer HTML of each element to nothing.
		/// <p>
		/// E.g. HTML:
		/// <code><div><p>Hello</p> <p>there</p> <img /></div></code>
		/// <br />
		/// <code>doc.select("p").remove();</code><br />
		/// HTML =
		/// <code><div> <img /></div></code>
		/// <p>
		/// Note that this method should not be used to clean user-submitted HTML; rather, use
		/// <see cref="NSoup.Safety.Cleaner">NSoup.Safety.Cleaner</see>
		/// to clean HTML.
		/// </remarks>
		/// <returns>this, for chaining</returns>
		/// <seealso cref="NSoup.Nodes.Element.Empty()">NSoup.Nodes.Element.Empty()</seealso>
		/// <seealso cref="Empty()">Empty()</seealso>
		public virtual NSoup.Select.Elements Remove()
		{
			foreach (Element element in contents)
			{
				element.Remove();
			}
			return this;
		}

		// filters
		/// <summary>Find matching elements within this element list.</summary>
		/// <remarks>Find matching elements within this element list.</remarks>
		/// <param name="query">
		/// A
		/// <see cref="Selector">Selector</see>
		/// query
		/// </param>
		/// <returns>the filtered list of elements, or an empty list if none match.</returns>
		public virtual NSoup.Select.Elements Select(string query)
		{
			return Selector.Select(query, this);
		}

		/// <summary>
		/// Remove elements from this list that match the
		/// <see cref="Selector">Selector</see>
		/// query.
		/// <p>
		/// E.g. HTML:
		/// <code><div class=logo>One</div> <div>Two</div></code>
		/// <br />
		/// <code>Elements divs = doc.select("div").not("#logo");</code><br />
		/// Result:
		/// <code>divs: [<div>Two</div>]</code>
		/// <p>
		/// </summary>
		/// <param name="query">the selector query whose results should be removed from these elements
		/// 	</param>
		/// <returns>a new elements list that contains only the filtered results</returns>
		public virtual NSoup.Select.Elements Not(string query)
		{
			NSoup.Select.Elements @out = Selector.Select(query, this);
			return Selector.FilterOut(this, @out);
		}

		/// <summary>Get the <i>nth</i> matched element as an Elements object.</summary>
		/// <remarks>
		/// Get the <i>nth</i> matched element as an Elements object.
		/// <p>
		/// See also
		/// <see cref="Get(int)">Get(int)</see>
		/// to retrieve an Element.
		/// </remarks>
		/// <param name="index">the (zero-based) index of the element in the list to retain</param>
		/// <returns>Elements containing only the specified element, or, if that element did not exist, an empty list.
		/// 	</returns>
		public virtual NSoup.Select.Elements Eq(int index)
		{
			return contents.Count > index ? new NSoup.Select.Elements(this[index]) : new NSoup.Select.Elements
				();
		}

		/// <summary>Test if any of the matched elements match the supplied query.</summary>
		/// <remarks>Test if any of the matched elements match the supplied query.</remarks>
		/// <param name="query">A selector</param>
		/// <returns>true if at least one element in the list matches the query.</returns>
		public virtual bool Is(string query)
		{
			NSoup.Select.Elements children = Select(query);
			return !children.IsEmpty();
		}

		/// <summary>Get all of the parents and ancestor elements of the matched elements.</summary>
		/// <remarks>Get all of the parents and ancestor elements of the matched elements.</remarks>
		/// <returns>all of the parents and ancestor elements of the matched elements</returns>
		public virtual NSoup.Select.Elements Parents()
		{
			HashSet<Element> combo = new LinkedHashSet<Element>();
			foreach (Element e in contents)
			{
				Sharpen.Collections.AddAll(combo, e.Parents());
			}
			return new NSoup.Select.Elements(combo);
		}

		// list-like methods
		/// <summary>Get the first matched element.</summary>
		/// <remarks>Get the first matched element.</remarks>
		/// <returns>The first matched element, or <code>null</code> if contents is empty.</returns>
		public virtual Element First()
		{
			return contents.IsEmpty() ? null : contents[0];
		}

		/// <summary>Get the last matched element.</summary>
		/// <remarks>Get the last matched element.</remarks>
		/// <returns>The last matched element, or <code>null</code> if contents is empty.</returns>
		public virtual Element Last()
		{
			return contents.IsEmpty() ? null : contents[contents.Count - 1];
		}

		/// <summary>Perform a depth-first traversal on each of the selected elements.</summary>
		/// <remarks>Perform a depth-first traversal on each of the selected elements.</remarks>
		/// <param name="nodeVisitor">the visitor callbacks to perform on each node</param>
		/// <returns>this, for chaining</returns>
		public virtual NSoup.Select.Elements Traverse(NodeVisitor nodeVisitor)
		{
			Validate.NotNull(nodeVisitor);
			NodeTraversor traversor = new NodeTraversor(nodeVisitor);
			foreach (Element el in contents)
			{
				traversor.Traverse(el);
			}
			return this;
		}

		/// <summary>
		/// Get the
		/// <see cref="NSoup.Nodes.FormElement">NSoup.Nodes.FormElement</see>
		/// forms from the selected elements, if any.
		/// </summary>
		/// <returns>
		/// a list of
		/// <see cref="NSoup.Nodes.FormElement">NSoup.Nodes.FormElement</see>
		/// s pulled from the matched elements. The list will be empty if the elements contain
		/// no forms.
		/// </returns>
		public virtual IList<FormElement> Forms()
		{
			AList<FormElement> forms = new AList<FormElement>();
			foreach (Element el in contents)
			{
				if (el is FormElement)
				{
					forms.AddItem((FormElement)el);
				}
			}
			return forms;
		}

		public virtual int Count
		{
			get
			{
				// implements List<Element> delegates:
				return contents.Count;
			}
		}

		public virtual bool IsEmpty()
		{
			return contents.IsEmpty();
		}

		public virtual bool Contains(object o)
		{
			return contents.Contains(o);
		}

		public virtual Sharpen.Iterator<Element> Iterator()
		{
			return contents.Iterator();
		}

		public virtual object[] ToArray()
		{
			return Sharpen.Collections.ToArray(contents);
		}

		public virtual T[] ToArray<T>(T[] a)
		{
			return Sharpen.Collections.ToArray(contents, a);
		}

		public virtual bool AddItem(Element element)
		{
			return contents.AddItem(element);
		}

		public virtual bool Remove(object o)
		{
			return contents.Remove(o);
		}

		public virtual bool ContainsAll<_T0>(ICollection<_T0> c)
		{
			return contents.ContainsAll(c);
		}

		public virtual bool AddAll<_T0>(ICollection<_T0> c) where _T0:Element
		{
			return Sharpen.Collections.AddAll(contents, c);
		}

		public virtual bool AddRange<_T0>(int index, ICollection<_T0> c) where _T0:Element
		{
			return contents.AddRange(index, c);
		}

		public virtual bool RemoveAll<_T0>(ICollection<_T0> c)
		{
			return contents.RemoveAll(c);
		}

		public virtual bool RetainAll<_T0>(ICollection<_T0> c)
		{
			return contents.RetainAll(c);
		}

		public virtual void Clear()
		{
			contents.Clear();
		}

		public override bool Equals(object o)
		{
			return contents.Equals(o);
		}

		public override int GetHashCode()
		{
			return contents.GetHashCode();
		}

		public virtual Element Get(int index)
		{
			return contents[index];
		}

		public virtual Element Set(int index, Element element)
		{
			return contents.Set(index, element);
		}

		public virtual void Add(int index, Element element)
		{
			contents.Add(index, element);
		}

		public virtual Element Remove(int index)
		{
			return contents.Remove(index);
		}

		public virtual int IndexOf(object o)
		{
			return contents.IndexOf(o);
		}

		public virtual int LastIndexOf(object o)
		{
			return contents.LastIndexOf(o);
		}

		public virtual Sharpen.ListIterator<Element> ListIterator()
		{
			return contents.ListIterator();
		}

		public virtual Sharpen.ListIterator<Element> ListIterator(int index)
		{
			return contents.ListIterator(index);
		}

		public virtual IList<Element> SubList(int fromIndex, int toIndex)
		{
			return contents.SubList(fromIndex, toIndex);
		}
	}
}
