using System;
using Supremes.Helper;
using Supremes.Nodes;
using System.Collections.Generic;

namespace Supremes.Safety
{
    /// <summary>
    /// Whitelists define what HTML (elements and attributes) to allow through the cleaner.
    /// </summary>
    /// <remarks>
    /// Everything else is removed.
    /// <p/>
    /// Start with one of the defaults:
    /// <ul>
    /// <li>
    /// <see cref="None">None</see>
    /// </li>
    /// <li>
    /// <see cref="SimpleText">SimpleText</see>
    /// </li>
    /// <li>
    /// <see cref="Basic">Basic</see>
    /// </li>
    /// <li>
    /// <see cref="BasicWithImages">BasicWithImages</see>
    /// </li>
    /// <li>
    /// <see cref="Relaxed">Relaxed</see>
    /// </li>
    /// </ul>
    /// <p/>
    /// If you need to allow more through (please be careful!), tweak a base whitelist with:
    /// <ul>
    /// <li>
    /// <see cref="AddTags(string[])">AddTags(string[])</see>
    /// </li>
    /// <li>
    /// <see cref="AddAttributes(string, string[])">AddAttributes(string, string[])</see>
    /// </li>
    /// <li>
    /// <see cref="AddEnforcedAttribute(string, string, string)">AddEnforcedAttribute(string, string, string)
    /// </see>
    /// </li>
    /// <li>
    /// <see cref="AddProtocols(string, string, string[])">AddProtocols(string, string, string[])
    /// </see>
    /// </li>
    /// </ul>
    /// <p/>
    /// The cleaner and these whitelists assume that you want to clean a <c>body</c> fragment of HTML (to add user
    /// supplied HTML into a templated page), and not to clean a full HTML document. If the latter is the case, either wrap the
    /// document HTML around the cleaned body HTML, or create a whitelist that allows <c>html</c> and <c>head</c>
    /// elements as appropriate.
    /// <p/>
    /// If you are going to extend a whitelist, please be very careful. Make sure you understand what attributes may lead to
    /// XSS attack vectors. URL attributes are particularly vulnerable and require careful validation. See
    /// http://ha.ckers.org/xss.html for some XSS attack examples.
    /// </remarks>
    /// <author>Jonathan Hedley</author>
    public sealed class Whitelist
    {
        private ICollection<Whitelist.TagName> tagNames;

        private IDictionary<Whitelist.TagName, ICollection<Whitelist.AttributeKey>> attributes;

        private IDictionary<Whitelist.TagName, IDictionary<Whitelist.AttributeKey, Whitelist.AttributeValue>> enforcedAttributes;

        private IDictionary<Whitelist.TagName, IDictionary<Whitelist.AttributeKey, ICollection<Whitelist.Protocol>>> protocols;

        private bool preserveRelativeLinks;

        // tags allowed, lower case. e.g. [p, br, span]
        // tag -> attribute[]. allowed attributes [href] for a tag.
        // always set these attribute values
        // allowed URL protocols for attributes
        // option to preserve relative links

        /// <summary>
        /// This whitelist allows only text nodes: all HTML will be stripped.
        /// </summary>
        /// <returns>whitelist</returns>
        public static Whitelist None
        {
            get { return new Supremes.Safety.Whitelist(); }
        }

        /// <summary>
        /// This whitelist allows only simple text formatting: <c>b, em, i, strong, u</c>.
        /// </summary>
        /// <remarks>
        /// All other HTML (tags and attributes) will be removed.
        /// </remarks>
        /// <returns>whitelist</returns>
        public static Whitelist SimpleText
        {
            get { return new Supremes.Safety.Whitelist().AddTags("b", "em", "i", "strong", "u"); }
        }

        /// <summary>
        /// This whitelist allows a fuller range of text nodes: <c>a, b, blockquote, br, cite, code, dd, dl, dt, em, i, li,
        /// ol, p, pre, q, small, span, strike, strong, sub, sup, u, ul</c>, and appropriate attributes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Links (<c>a</c> elements) can point to <c>http, https, ftp, mailto</c>, and have an enforced
        /// <c>rel=nofollow</c> attribute.
        /// </para>
        /// <para>
        /// Does not allow images.
        /// </para>
        /// </remarks>
        /// <returns>whitelist</returns>
        public static Whitelist Basic
        {
            get
            {
                return new Supremes.Safety.Whitelist()
                    .AddTags("a", "b", "blockquote", "br", "cite", "code", "dd", "dl", "dt", "em", "i",
                        "li", "ol", "p", "pre", "q", "small", "span", "strike", "strong", "sub", "sup", "u", "ul")
                    .AddAttributes("a", "href")
                    .AddAttributes("blockquote", "cite")
                    .AddAttributes("q", "cite")
                    .AddProtocols("a", "href", "ftp", "http", "https", "mailto")
                    .AddProtocols("blockquote", "cite", "http", "https")
                    .AddProtocols("cite", "cite", "http", "https")
                    .AddEnforcedAttribute("a", "rel", "nofollow");
            }
        }

        /// <summary>
        /// This whitelist allows the same text tags as
        /// <see cref="Basic">Basic</see>
        /// , and also allows <c>img</c> tags, with appropriate
        /// attributes, with <c>src</c> pointing to <c>http</c> or <c>https</c>.
        /// </summary>
        /// <returns>whitelist</returns>
        public static Whitelist BasicWithImages
        {
            get
            {
                return Basic
                    .AddTags("img")
                    .AddAttributes("img", "align", "alt", "height", "src", "title", "width")
                    .AddProtocols("img", "src", "http", "https");
            }
        }

        /// <summary>
        /// This whitelist allows a full range of text and structural body HTML: <c>a, b, blockquote, br, caption, cite,
        /// code, col, colgroup, dd, div, dl, dt, em, h1, h2, h3, h4, h5, h6, i, img, li, ol, p, pre, q, small, span, strike, strong, sub,
        /// sup, table, tbody, td, tfoot, th, thead, tr, u, ul</c>
        /// <p/>
        /// Links do not have an enforced <c>rel=nofollow</c> attribute, but you can add that if desired.
        /// </summary>
        /// <remarks>
        /// This whitelist allows a full range of text and structural body HTML: <c>a, b, blockquote, br, caption, cite,
        /// code, col, colgroup, dd, div, dl, dt, em, h1, h2, h3, h4, h5, h6, i, img, li, ol, p, pre, q, small, span, strike, strong, sub,
        /// sup, table, tbody, td, tfoot, th, thead, tr, u, ul</c>
        /// <p/>
        /// Links do not have an enforced <c>rel=nofollow</c> attribute, but you can add that if desired.
        /// </remarks>
        /// <returns>whitelist</returns>
        public static Whitelist Relaxed
        {
            get
            {
                return new Supremes.Safety.Whitelist()
                    .AddTags("a", "b", "blockquote", "br", "caption", "cite", "code", "col", "colgroup",
                        "dd", "div", "dl", "dt", "em", "h1", "h2", "h3", "h4", "h5", "h6", "i", "img",
                        "li", "ol", "p", "pre", "q", "small", "span", "strike", "strong", "sub", "sup",
                        "table", "tbody", "td", "tfoot", "th", "thead", "tr", "u", "ul")
                    .AddAttributes("a", "href", "title")
                    .AddAttributes("blockquote", "cite")
                    .AddAttributes("col", "span", "width")
                    .AddAttributes("colgroup", "span", "width")
                    .AddAttributes("img", "align", "alt", "height", "src", "title", "width")
                    .AddAttributes("ol", "start", "type")
                    .AddAttributes("q", "cite")
                    .AddAttributes("table", "summary", "width")
                    .AddAttributes("td", "abbr", "axis", "colspan", "rowspan", "width")
                    .AddAttributes("th", "abbr", "axis", "colspan", "rowspan", "scope", "width")
                    .AddAttributes("ul", "type")
                    .AddProtocols("a", "href", "ftp", "http", "https", "mailto")
                    .AddProtocols("blockquote", "cite", "http", "https")
                    .AddProtocols("cite", "cite", "http", "https")
                    .AddProtocols("img", "src", "http", "https")
                    .AddProtocols("q", "cite", "http", "https");
            }
        }

        /// <summary>
        /// Create a new, empty whitelist.</summary>
        /// <remarks>
        /// Generally it will be better to start with a default prepared whitelist instead.
        /// </remarks>
        /// <seealso cref="Basic">Basic</seealso>
        /// <seealso cref="BasicWithImages">BasicWithImages</seealso>
        /// <seealso cref="SimpleText">SimpleText</seealso>
        /// <seealso cref="Relaxed">Relaxed</seealso>
        public Whitelist()
        {
            tagNames = new HashSet<Whitelist.TagName>();
            attributes = new Dictionary<Whitelist.TagName, ICollection<Whitelist.AttributeKey>>();
            enforcedAttributes = new Dictionary<Whitelist.TagName, IDictionary<Whitelist.AttributeKey, Whitelist.AttributeValue>>();
            protocols = new Dictionary<Whitelist.TagName, IDictionary<Whitelist.AttributeKey, ICollection<Whitelist.Protocol>>>();
            preserveRelativeLinks = false;
        }

        /// <summary>
        /// Add a list of allowed elements to a whitelist.</summary>
        /// <remarks>
        /// (If a tag is not allowed, it will be removed from the HTML.)
        /// </remarks>
        /// <param name="tags">tag names to allow</param>
        /// <returns>this (for chaining)</returns>
        public Whitelist AddTags(params string[] tags)
        {
            Validate.NotNull(tags);
            foreach (string tagName in tags)
            {
                Validate.NotEmpty(tagName);
                tagNames.Add(Whitelist.TagName.ValueOf(tagName));
            }
            return this;
        }

        /// <summary>
        /// Add a list of allowed attributes to a tag.
        /// </summary>
        /// <remarks>
        /// (If an attribute is not allowed on an element, it will be removed.)
        /// <p/>
        /// E.g.: <c>addAttributes("a", "href", "class")</c> allows <c>href</c> and <c>class</c> attributes
        /// on <c>a</c> tags.
        /// <p/>
        /// To make an attribute valid for <b>all tags</b>, use the pseudo tag <c>:all</c>, e.g.
        /// <c>addAttributes(":all", "class")</c>.
        /// </remarks>
        /// <param name="tag">The tag the attributes are for. The tag will be added to the allowed tag list if necessary.
        /// </param>
        /// <param name="keys">List of valid attributes for the tag</param>
        /// <returns>this (for chaining)</returns>
        public Whitelist AddAttributes(string tag, params string[] keys)
        {
            Validate.NotEmpty(tag);
            Validate.NotNull(keys);
            Validate.IsTrue(keys.Length > 0, "No attributes supplied.");
            Whitelist.TagName tagName = Whitelist.TagName.ValueOf(tag);
            if (!tagNames.Contains(tagName))
            {
                tagNames.Add(tagName);
            }
            ICollection<Whitelist.AttributeKey> attributeSet = new HashSet<Whitelist.AttributeKey>();
            foreach (string key in keys)
            {
                Validate.NotEmpty(key);
                attributeSet.Add(Whitelist.AttributeKey.ValueOf(key));
            }
            if (attributes.ContainsKey(tagName))
            {
                ICollection<Whitelist.AttributeKey> currentSet = attributes[tagName];
                //currentSet.AddAll(attributeSet);
                foreach (var attr in attributeSet) currentSet.Add(attr);
            }
            else
            {
                attributes[tagName] = attributeSet;
            }
            return this;
        }

        /// <summary>
        /// Add an enforced attribute to a tag.
        /// </summary>
        /// <remarks>
        /// An enforced attribute will always be added to the element. If the element
        /// already has the attribute set, it will be overridden.
        /// <p/>
        /// E.g.: <c>addEnforcedAttribute("a", "rel", "nofollow")</c> will make all <c>a</c> tags output as
        /// <c>&lt;a href="..." rel="nofollow"&gt;</c>
        /// </remarks>
        /// <param name="tag">The tag the enforced attribute is for. The tag will be added to the allowed tag list if necessary.
        /// </param>
        /// <param name="key">The attribute key</param>
        /// <param name="value">The enforced attribute value</param>
        /// <returns>this (for chaining)</returns>
        public Whitelist AddEnforcedAttribute(string tag, string key, string value)
        {
            Validate.NotEmpty(tag);
            Validate.NotEmpty(key);
            Validate.NotEmpty(value);
            Whitelist.TagName tagName = Whitelist.TagName.ValueOf(tag);
            if (!tagNames.Contains(tagName))
            {
                tagNames.Add(tagName);
            }
            Whitelist.AttributeKey attrKey = Whitelist.AttributeKey.ValueOf(key);
            Whitelist.AttributeValue attrVal = Whitelist.AttributeValue.ValueOf(value);
            if (enforcedAttributes.ContainsKey(tagName))
            {
                enforcedAttributes[tagName][attrKey] = attrVal;
            }
            else
            {
                IDictionary<Whitelist.AttributeKey, Whitelist.AttributeValue> attrMap = new Dictionary<Whitelist.AttributeKey, Whitelist.AttributeValue>();
                attrMap[attrKey] = attrVal;
                enforcedAttributes[tagName] = attrMap;
            }
            return this;
        }

        /// <summary>
        /// Configure this Whitelist to preserve relative links in an element's URL attribute, or convert them to absolute
        /// links.
        /// </summary>
        /// <remarks>
        /// Configure this Whitelist to preserve relative links in an element's URL attribute, or convert them to absolute
        /// links. By default, this is <b>false</b>: URLs will be  made absolute (e.g. start with an allowed protocol, like
        /// e.g.
        /// <c>http://</c>
        /// .
        /// <p />
        /// Note that when handling relative links, the input document must have an appropriate
        /// <c>base URI</c>
        /// set when
        /// parsing, so that the link's protocol can be confirmed. Regardless of the setting of the
        /// <c>
        /// preserve relative
        /// links
        /// </c>
        /// option, the link must be resolvable against the base URI to an allowed protocol; otherwise the attribute
        /// will be removed.
        /// </remarks>
        /// <param name="preserve">
        /// 
        /// <c>true</c>
        /// to allow relative links,
        /// <c>false</c>
        /// (default) to deny
        /// </param>
        /// <returns>this Whitelist, for chaining.</returns>
        /// <seealso cref="AddProtocols(string, string, string[])">AddProtocols(string, string, string[])
        /// </seealso>
        public Whitelist PreserveRelativeLinks(bool preserve)
        {
            preserveRelativeLinks = preserve;
            return this;
        }

        /// <summary>
        /// Add allowed URL protocols for an element's URL attribute.
        /// </summary>
        /// <remarks>
        /// This restricts the possible values of the attribute to
        /// URLs with the defined protocol.
        /// <p/>
        /// E.g.: <c>addProtocols("a", "href", "ftp", "http", "https")</c>
        /// </remarks>
        /// <param name="tag">Tag the URL protocol is for</param>
        /// <param name="key">Attribute key</param>
        /// <param name="protocols">List of valid protocols</param>
        /// <returns>this, for chaining</returns>
        public Whitelist AddProtocols(string tag, string key, params string[] protocols)
        {
            Validate.NotEmpty(tag);
            Validate.NotEmpty(key);
            Validate.NotNull(protocols);
            Whitelist.TagName tagName = Whitelist.TagName.ValueOf(tag);
            Whitelist.AttributeKey attrKey = Whitelist.AttributeKey.ValueOf(key);
            IDictionary<Whitelist.AttributeKey, ICollection<Whitelist.Protocol>> attrMap;
            ICollection<Whitelist.Protocol> protSet;
            if (this.protocols.ContainsKey(tagName))
            {
                attrMap = this.protocols[tagName];
            }
            else
            {
                attrMap = new Dictionary<Whitelist.AttributeKey, ICollection<Whitelist.Protocol>>();
                this.protocols[tagName] = attrMap;
            }
            if (attrMap.ContainsKey(attrKey))
            {
                protSet = attrMap[attrKey];
            }
            else
            {
                protSet = new HashSet<Whitelist.Protocol>();
                attrMap[attrKey] = protSet;
            }
            foreach (string protocol in protocols)
            {
                Validate.NotEmpty(protocol);
                Whitelist.Protocol prot = Whitelist.Protocol.ValueOf(protocol);
                protSet.Add(prot);
            }
            return this;
        }

        /// <summary>
        /// Test if the supplied tag is allowed by this whitelist
        /// </summary>
        /// <param name="tag">test tag</param>
        /// <returns>true if allowed</returns>
        internal bool IsSafeTag(string tag)
        {
            return tagNames.Contains(Whitelist.TagName.ValueOf(tag));
        }

        /// <summary>
        /// Test if the supplied attribute is allowed by this whitelist for this tag
        /// </summary>
        /// <param name="tagName">tag to consider allowing the attribute in</param>
        /// <param name="el">element under test, to confirm protocol</param>
        /// <param name="attr">attribute under test</param>
        /// <returns>true if allowed</returns>
        internal bool IsSafeAttribute(string tagName, Element el, Nodes.Attribute attr)
        {
            Whitelist.TagName tag = Whitelist.TagName.ValueOf(tagName);
            Whitelist.AttributeKey key = Whitelist.AttributeKey.ValueOf(attr.Key);
            if (attributes.ContainsKey(tag))
            {
                if (attributes[tag].Contains(key))
                {
                    if (protocols.ContainsKey(tag))
                    {
                        IDictionary<Whitelist.AttributeKey, ICollection<Whitelist.Protocol>> attrProts = protocols[tag];
                        // ok if not defined protocol; otherwise test
                        return !attrProts.ContainsKey(key) || TestValidProtocol(el, attr, attrProts[key]);
                    }
                    else
                    {
                        // attribute found, no protocols defined, so OK
                        return true;
                    }
                }
            }
            // no attributes defined for tag, try :all tag
            return !tagName.Equals(":all") && IsSafeAttribute(":all", el, attr);
        }

        private bool TestValidProtocol(Element el, Nodes.Attribute attr, ICollection<Whitelist.Protocol> protocols)
        {
            // try to resolve relative urls to abs, and optionally update the attribute so output html has abs.
            // rels without a baseuri get removed
            string value = el.AbsUrl(attr.Key);
            if (value.Length == 0)
            {
                value = attr.Value;
            }
            // if it could not be made abs, run as-is to allow custom unknown protocols
            if (!preserveRelativeLinks)
            {
                attr.Value = value;
            }
            foreach (Whitelist.Protocol protocol in protocols)
            {
                string prot = protocol.ToString() + ":";
                if (value.ToLower().StartsWith(prot, StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        internal Attributes GetEnforcedAttributes(string tagName)
        {
            Attributes attrs = new Attributes();
            Whitelist.TagName tag = Whitelist.TagName.ValueOf(tagName);
            if (enforcedAttributes.ContainsKey(tag))
            {
                IDictionary<Whitelist.AttributeKey, Whitelist.AttributeValue> keyVals = enforcedAttributes[tag];
                foreach (KeyValuePair<Whitelist.AttributeKey, Whitelist.AttributeValue> entry in keyVals)
                {
                    attrs[entry.Key.ToString()] = entry.Value.ToString();
                }
            }
            return attrs;
        }

        internal class TagName : Whitelist.TypedValue
        {
            internal TagName(string value) : base(value)
            {
            }

            // named types for config. All just hold strings, but here for my sanity.
            internal static Whitelist.TagName ValueOf(string value)
            {
                return new Whitelist.TagName(value);
            }
        }

        internal class AttributeKey : Whitelist.TypedValue
        {
            internal AttributeKey(string value) : base(value)
            {
            }

            internal static Whitelist.AttributeKey ValueOf(string value)
            {
                return new Whitelist.AttributeKey(value);
            }
        }

        internal class AttributeValue : Whitelist.TypedValue
        {
            internal AttributeValue(string value) : base(value)
            {
            }

            internal static Whitelist.AttributeValue ValueOf(string value)
            {
                return new Whitelist.AttributeValue(value);
            }
        }

        internal class Protocol : Whitelist.TypedValue
        {
            internal Protocol(string value) : base(value)
            {
            }

            internal static Whitelist.Protocol ValueOf(string value)
            {
                return new Whitelist.Protocol(value);
            }
        }

        internal abstract class TypedValue
        {
            private readonly string value;

            internal TypedValue(string value)
            {
                Validate.NotNull(value);
                this.value = value;
            }

            public override int GetHashCode()
            {
                int prime = 31;
                int result = 1;
                result = prime * result + ((value == null) ? 0 : value.GetHashCode());
                return result;
            }

            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }
                if (obj == null)
                {
                    return false;
                }
                if (GetType() != obj.GetType())
                {
                    return false;
                }
                Whitelist.TypedValue other = (Whitelist.TypedValue)obj;
                if (value == null)
                {
                    if (other.value != null)
                    {
                        return false;
                    }
                }
                else if (!value.Equals(other.value))
                {
                    return false;
                }
                return true;
            }

            public override string ToString()
            {
                return value;
            }
        }
    }
}
