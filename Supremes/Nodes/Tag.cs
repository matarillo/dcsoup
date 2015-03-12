using Supremes.Helper;
using System.Collections.Generic;

namespace Supremes.Nodes
{
    /// <summary>
    /// HTML Tag capabilities.
    /// </summary>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class Tag
    {
        private static readonly IDictionary<string, Tag> tags = new Dictionary<string, Tag>();

        private string tagName;

        private bool isBlock = true;

        private bool formatAsBlock = true;

        private bool canContainBlock = true;

        private bool canContainInline = true;

        private bool empty = false;

        private bool selfClosing = false;

        private bool preserveWhitespace = false;

        private bool formList = false;

        private bool formSubmit = false;

        private Tag(string tagName)
        {
            // map of known tags
            // block or inline
            // should be formatted as a block
            // Can this tag hold block level tags?
            // only pcdata if not
            // can hold nothing; e.g. img
            // can self close (<foo />). used for unknown tags that self close, without forcing them as empty.
            // for pre, textarea, script etc
            // a control that appears in forms: input, textarea, output etc
            // a control that can be submitted in a form: input etc
            this.tagName = tagName.ToLower();
        }

        /// <summary>
        /// Get this tag's name.
        /// </summary>
        /// <returns>the tag's name</returns>
        public string GetName()
        {
            return tagName;
        }

        /// <summary>
        /// Get a Tag by name.
        /// </summary>
        /// <remarks>
        /// If not previously defined (unknown), returns a new generic tag, that can do anything.
        /// <p/>
        /// Pre-defined tags (P, DIV etc) will be ==, but unknown tags are not registered and will only .equals().
        /// </remarks>
        /// <param name="tagName">Name of tag, e.g. "p". Case insensitive.</param>
        /// <returns>The tag, either defined or new generic.</returns>
        public static Supremes.Nodes.Tag ValueOf(string tagName)
        {
            Validate.NotNull(tagName);
            Supremes.Nodes.Tag tag;
            if (!tags.TryGetValue(tagName, out tag))
            {
                tagName = tagName.Trim().ToLower();
                Validate.NotEmpty(tagName);
                if (!tags.TryGetValue(tagName, out tag))
                {
                    // not defined: create default; go anywhere, do anything! (incl be inside a <p>)
                    tag = new Supremes.Nodes.Tag(tagName);
                    tag.isBlock = false;
                    tag.canContainBlock = true;
                }
            }
            return tag;
        }

        /// <summary>
        /// Gets if this is a block tag.
        /// </summary>
        /// <returns>if block tag</returns>
        public bool IsBlock()
        {
            return isBlock;
        }

        /// <summary>
        /// Gets if this tag should be formatted as a block (or as inline)
        /// </summary>
        /// <returns>if should be formatted as block or inline</returns>
        public bool FormatAsBlock()
        {
            return formatAsBlock;
        }

        /// <summary>
        /// Gets if this tag can contain block tags.
        /// </summary>
        /// <returns>if tag can contain block tags</returns>
        public bool CanContainBlock()
        {
            return canContainBlock;
        }

        /// <summary>
        /// Gets if this tag is an inline tag.
        /// </summary>
        /// <returns>if this tag is an inline tag.</returns>
        public bool IsInline()
        {
            return !isBlock;
        }

        /// <summary>
        /// Gets if this tag is a data only tag.
        /// </summary>
        /// <returns>if this tag is a data only tag</returns>
        public bool IsData()
        {
            return !canContainInline && !IsEmpty();
        }

        /// <summary>
        /// Get if this is an empty tag
        /// </summary>
        /// <returns>if this is an empty tag</returns>
        public bool IsEmpty()
        {
            return empty;
        }

        /// <summary>
        /// Get if this tag is self closing.
        /// </summary>
        /// <returns>if this tag should be output as self closing.</returns>
        public bool IsSelfClosing()
        {
            return empty || selfClosing;
        }

        /// <summary>
        /// Get if this is a pre-defined tag, or was auto created on parsing.
        /// </summary>
        /// <returns>if a known tag</returns>
        public bool IsKnownTag()
        {
            return tags.ContainsKey(tagName);
        }

        /// <summary>
        /// Check if this tagname is a known tag.
        /// </summary>
        /// <param name="tagName">name of tag</param>
        /// <returns>if known HTML tag</returns>
        public static bool IsKnownTag(string tagName)
        {
            return tags.ContainsKey(tagName);
        }

        /// <summary>
        /// Get if this tag should preserve whitespace within child text nodes.
        /// </summary>
        /// <returns>if preserve whitepace</returns>
        public bool PreserveWhitespace()
        {
            return preserveWhitespace;
        }

        /// <summary>
        /// Get if this tag represents a control associated with a form.
        /// </summary>
        /// <remarks>
        /// E.g. input, textarea, output
        /// </remarks>
        /// <returns>if associated with a form</returns>
        public bool IsFormListed()
        {
            return formList;
        }

        /// <summary>
        /// Get if this tag represents an element that should be submitted with a form.
        /// </summary>
        /// <remarks>
        /// E.g. input, option
        /// </remarks>
        /// <returns>if submittable with a form</returns>
        public bool IsFormSubmittable()
        {
            return formSubmit;
        }

        internal Tag SetSelfClosing()
        {
            selfClosing = true;
            return this;
        }

        /// <summary>
        /// Compares two <see cref="Tag"/> instances for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
			Supremes.Nodes.Tag tag = obj as Supremes.Nodes.Tag;
            if (tag == null)
            {
                return false;
            }
            if (canContainBlock != tag.canContainBlock)
            {
                return false;
            }
            if (canContainInline != tag.canContainInline)
            {
                return false;
            }
            if (empty != tag.empty)
            {
                return false;
            }
            if (formatAsBlock != tag.formatAsBlock)
            {
                return false;
            }
            if (isBlock != tag.isBlock)
            {
                return false;
            }
            if (preserveWhitespace != tag.preserveWhitespace)
            {
                return false;
            }
            if (selfClosing != tag.selfClosing)
            {
                return false;
            }
            if (formList != tag.formList)
            {
                return false;
            }
            if (formSubmit != tag.formSubmit)
            {
                return false;
            }
            if (!tagName.Equals(tag.tagName))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int result = tagName.GetHashCode();
            result = 31 * result + (isBlock ? 1 : 0);
            result = 31 * result + (formatAsBlock ? 1 : 0);
            result = 31 * result + (canContainBlock ? 1 : 0);
            result = 31 * result + (canContainInline ? 1 : 0);
            result = 31 * result + (empty ? 1 : 0);
            result = 31 * result + (selfClosing ? 1 : 0);
            result = 31 * result + (preserveWhitespace ? 1 : 0);
            result = 31 * result + (formList ? 1 : 0);
            result = 31 * result + (formSubmit ? 1 : 0);
            return result;
        }

        /// <summary>
        /// Converts the value of this instance to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return tagName;
        }

        private static readonly string[] blockTags = new string[] { "html", "head", "body"
            , "frameset", "script", "noscript", "style", "meta", "link", "title", "frame", "noframes"
            , "section", "nav", "aside", "hgroup", "header", "footer", "p", "h1", "h2", "h3"
            , "h4", "h5", "h6", "ul", "ol", "pre", "div", "blockquote", "hr", "address", "figure"
            , "figcaption", "form", "fieldset", "ins", "del", "s", "dl", "dt", "dd", "li", "table"
            , "caption", "thead", "tfoot", "tbody", "colgroup", "col", "tr", "th", "td", "video"
            , "audio", "canvas", "details", "menu", "plaintext" };

        private static readonly string[] inlineTags = new string[] { "object", "base", "font"
            , "tt", "i", "b", "u", "big", "small", "em", "strong", "dfn", "code", "samp", "kbd"
            , "var", "cite", "abbr", "time", "acronym", "mark", "ruby", "rt", "rp", "a", "img"
            , "br", "wbr", "map", "q", "sub", "sup", "bdo", "iframe", "embed", "span", "input"
            , "select", "textarea", "label", "button", "optgroup", "option", "legend", "datalist"
            , "keygen", "output", "progress", "meter", "area", "param", "source", "track", "summary"
            , "command", "device", "area", "basefont", "bgsound", "menuitem", "param", "source"
            , "track" };

        private static readonly string[] emptyTags = new string[] { "meta", "link", "base"
            , "frame", "img", "br", "wbr", "embed", "hr", "input", "keygen", "col", "command"
            , "device", "area", "basefont", "bgsound", "menuitem", "param", "source", "track"
             };

        private static readonly string[] formatAsInlineTags = new string[] { "title", "a"
            , "p", "h1", "h2", "h3", "h4", "h5", "h6", "pre", "address", "li", "th", "td", "script"
            , "style", "ins", "del", "s" };

        private static readonly string[] preserveWhitespaceTags = new string[] { "pre", "plaintext"
            , "title", "textarea" };

        private static readonly string[] formListedTags = new string[] { "button", "fieldset"
            , "input", "keygen", "object", "output", "select", "textarea" };

        private static readonly string[] formSubmitTags = new string[] { "input", "keygen"
            , "object", "select", "textarea" };

        static Tag()
        {
            // internal static initialisers:
            // prepped from http://www.w3.org/TR/REC-html40/sgml/dtd.html and other sources
            // script is not here as it is a data node, which always preserve whitespace
            // todo: I think we just need submit tags, and can scrub listed
            // creates
            foreach (string tagName in blockTags)
            {
                Supremes.Nodes.Tag tag = new Supremes.Nodes.Tag(tagName);
                Register(tag);
            }
            foreach (string tagName_1 in inlineTags)
            {
                Supremes.Nodes.Tag tag = new Supremes.Nodes.Tag(tagName_1);
                tag.isBlock = false;
                tag.canContainBlock = false;
                tag.formatAsBlock = false;
                Register(tag);
            }
            // mods:
            foreach (string tagName_2 in emptyTags)
            {
                Supremes.Nodes.Tag tag = tags[tagName_2];
                Validate.NotNull(tag);
                tag.canContainBlock = false;
                tag.canContainInline = false;
                tag.empty = true;
            }
            foreach (string tagName_3 in formatAsInlineTags)
            {
                Supremes.Nodes.Tag tag = tags[tagName_3];
                Validate.NotNull(tag);
                tag.formatAsBlock = false;
            }
            foreach (string tagName_4 in preserveWhitespaceTags)
            {
                Supremes.Nodes.Tag tag = tags[tagName_4];
                Validate.NotNull(tag);
                tag.preserveWhitespace = true;
            }
            foreach (string tagName_5 in formListedTags)
            {
                Supremes.Nodes.Tag tag = tags[tagName_5];
                Validate.NotNull(tag);
                tag.formList = true;
            }
            foreach (string tagName_6 in formSubmitTags)
            {
                Supremes.Nodes.Tag tag = tags[tagName_6];
                Validate.NotNull(tag);
                tag.formSubmit = true;
            }
        }

        private static void Register(Supremes.Nodes.Tag tag)
        {
            tags[tag.tagName] = tag;
        }
    }
}
