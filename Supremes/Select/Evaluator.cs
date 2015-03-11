/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using Supremes.Helper;
using Supremes.Nodes;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Supremes.Select
{
    /// <summary>
    /// Evaluates that an element matches the selector.
    /// </summary>
    internal abstract class Evaluator
    {
        public Evaluator()
        {
        }

        /// <summary>Test if the element meets the evaluator's requirements.</summary>
        /// <remarks>Test if the element meets the evaluator's requirements.</remarks>
        /// <param name="root">Root of the matching subtree</param>
        /// <param name="element">tested element</param>
        public abstract bool Matches(Element root, Element element);

        /// <summary>Evaluator for tag name</summary>
        public sealed class Tag : Evaluator
        {
            private readonly string tagName;

            public Tag(string tagName)
            {
                this.tagName = tagName;
            }

            public override bool Matches(Element root, Element element)
            {
                return (element.TagName().Equals(tagName));
            }

            public override string ToString()
            {
                return tagName;
            }
        }

        /// <summary>Evaluator for element id</summary>
        public sealed class ID : Evaluator
        {
            private string id;

            public ID(string id)
            {
                this.id = id;
            }

            public override bool Matches(Element root, Element element)
            {
                return (id.Equals(element.Id()));
            }

            public override string ToString()
            {
                return string.Format("#{0}", id);
            }
        }

        /// <summary>Evaluator for element class</summary>
        public sealed class Class : Evaluator
        {
            private string className;

            public Class(string className)
            {
                this.className = className;
            }

            public override bool Matches(Element root, Element element)
            {
                return (element.HasClass(className));
            }

            public override string ToString()
            {
                return string.Format(".{0}", className);
            }
        }

        /// <summary>Evaluator for attribute name matching</summary>
        public sealed class Attribute : Evaluator
        {
            private string key;

            public Attribute(string key)
            {
                this.key = key;
            }

            public override bool Matches(Element root, Element element)
            {
                return element.HasAttr(key);
            }

            public override string ToString()
            {
                return string.Format("[{0}]", key);
            }
        }

        /// <summary>Evaluator for attribute name prefix matching</summary>
        public sealed class AttributeStarting : Evaluator
        {
            private string keyPrefix;

            public AttributeStarting(string keyPrefix)
            {
                this.keyPrefix = keyPrefix;
            }

            public override bool Matches(Element root, Element element)
            {
                IReadOnlyList<Nodes.Attribute> values = element.Attributes().AsList();
                foreach (Nodes.Attribute attribute in values)
                {
                    if (attribute.Key.StartsWith(keyPrefix, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return string.Format("[^{0}]", keyPrefix);
            }
        }

        /// <summary>Evaluator for attribute name/value matching</summary>
        public sealed class AttributeWithValue : Evaluator.AttributeKeyPair
        {
            public AttributeWithValue(string key, string value) : base(key, value)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return element.HasAttr(key) && string.Equals(value, element.Attr(key), StringComparison.OrdinalIgnoreCase);
            }

            public override string ToString()
            {
                return string.Format("[{0}={1}]", key, value);
            }
        }

        /// <summary>Evaluator for attribute name != value matching</summary>
        public sealed class AttributeWithValueNot : Evaluator.AttributeKeyPair
        {
            public AttributeWithValueNot(string key, string value) : base(key, value)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return !string.Equals(value, element.Attr(key), StringComparison.OrdinalIgnoreCase);
            }

            public override string ToString()
            {
                return string.Format("[{0}!={1}]", key, value);
            }
        }

        /// <summary>Evaluator for attribute name/value matching (value prefix)</summary>
        public sealed class AttributeWithValueStarting : Evaluator.AttributeKeyPair
        {
            public AttributeWithValueStarting(string key, string value) : base(key, value)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return element.HasAttr(key) && element.Attr(key).ToLower().StartsWith(value, StringComparison.Ordinal);
            }

            // value is lower case already
            public override string ToString()
            {
                return string.Format("[{0}^={1}]", key, value);
            }
        }

        /// <summary>Evaluator for attribute name/value matching (value ending)</summary>
        public sealed class AttributeWithValueEnding : Evaluator.AttributeKeyPair
        {
            public AttributeWithValueEnding(string key, string value) : base(key, value)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return element.HasAttr(key) && element.Attr(key).ToLower().EndsWith(value, StringComparison.Ordinal);
            }

            // value is lower case
            public override string ToString()
            {
                return string.Format("[{0}$={1}]", key, value);
            }
        }

        /// <summary>Evaluator for attribute name/value matching (value containing)</summary>
        public sealed class AttributeWithValueContaining : Evaluator.AttributeKeyPair
        {
            public AttributeWithValueContaining(string key, string value) : base(key, value)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return element.HasAttr(key) && element.Attr(key).ToLower().Contains(value);
            }

            // value is lower case
            public override string ToString()
            {
                return string.Format("[{0}*={1}]", key, value);
            }
        }

        /// <summary>Evaluator for attribute name/value matching (value regex matching)</summary>
        public sealed class AttributeWithValueMatching : Evaluator
        {
            internal string key;

            internal Regex pattern;

            public AttributeWithValueMatching(string key, Regex pattern)
            {
                this.key = key.Trim().ToLower();
                this.pattern = pattern;
            }

            public override bool Matches(Element root, Element element)
            {
                return element.HasAttr(key) && pattern.Match(element.Attr(key)).Success; /*find*/
            }

            public override string ToString()
            {
                return string.Format("[{0}~={1}]", key, pattern.ToString());
            }
        }

        /// <summary>Abstract evaluator for attribute name/value matching</summary>
        public abstract class AttributeKeyPair : Evaluator
        {
            internal string key;

            internal string value;

            public AttributeKeyPair(string key, string value)
            {
                Validate.NotEmpty(key);
                Validate.NotEmpty(value);
                this.key = key.Trim().ToLower();
                if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
                {
                    value = value.Substring(1, value.Length - 2); /*substring*/
                }
                this.value = value.Trim().ToLower();
            }
        }

        /// <summary>Evaluator for any / all element matching</summary>
        public sealed class AllElements : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                return true;
            }

            public override string ToString()
            {
                return "*";
            }
        }

        /// <summary>Evaluator for matching by sibling index number (e &lt; idx)</summary>
        public sealed class IndexLessThan : Evaluator.IndexEvaluator
        {
            public IndexLessThan(int index) : base(index)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return element.ElementSiblingIndex() < index;
            }

            public override string ToString()
            {
                return string.Format(":lt({0})", index);
            }
        }

        /// <summary>Evaluator for matching by sibling index number (e &gt; idx)</summary>
        public sealed class IndexGreaterThan : Evaluator.IndexEvaluator
        {
            public IndexGreaterThan(int index) : base(index)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return element.ElementSiblingIndex() > index;
            }

            public override string ToString()
            {
                return string.Format(":gt({0})", index);
            }
        }

        /// <summary>Evaluator for matching by sibling index number (e = idx)</summary>
        public sealed class IndexEquals : Evaluator.IndexEvaluator
        {
            public IndexEquals(int index) : base(index)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                return element.ElementSiblingIndex() == index;
            }

            public override string ToString()
            {
                return string.Format(":eq({0})", index);
            }
        }

        /// <summary>Evaluator for matching the last sibling (css :last-child)</summary>
        public sealed class IsLastChild : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                Element p = element.ParentElement();
                return p != null && !(p is Document) && element.ElementSiblingIndex() == p.Children
                    ().Count - 1;
            }

            public override string ToString()
            {
                return ":last-child";
            }
        }

        public sealed class IsFirstOfType : Evaluator.IsNthOfType
        {
            public IsFirstOfType() : base(0, 1)
            {
            }

            public override string ToString()
            {
                return ":first-of-type";
            }
        }

        public sealed class IsLastOfType : Evaluator.IsNthLastOfType
        {
            public IsLastOfType() : base(0, 1)
            {
            }

            public override string ToString()
            {
                return ":last-of-type";
            }
        }

        public abstract class CssNthEvaluator : Evaluator
        {
            internal readonly int a;

            internal readonly int b;

            public CssNthEvaluator(int a, int b)
            {
                this.a = a;
                this.b = b;
            }

            public CssNthEvaluator(int b) : this(0, b)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                Element p = element.ParentElement();
                if (p == null || (p is Document))
                {
                    return false;
                }
                int pos = CalculatePosition(root, element);
                if (a == 0)
                {
                    return pos == b;
                }
                return (pos - b) * a >= 0 && (pos - b) % a == 0;
            }

            public override string ToString()
            {
                if (a == 0)
                {
                    return string.Format(":{0}({1})", GetPseudoClass(), b);
                }
                if (b == 0)
                {
                    return string.Format(":{0}({1}n)", GetPseudoClass(), a);
                }
                return string.Format(":{0}({1}n{2})", GetPseudoClass(), a, b);
            }

            internal abstract string GetPseudoClass();

            internal abstract int CalculatePosition(Element root, Element element);
        }

        /// <summary>css-compatible Evaluator for :eq (css :nth-child)</summary>
        /// <seealso cref="IndexEquals">IndexEquals</seealso>
        public sealed class IsNthChild : Evaluator.CssNthEvaluator
        {
            public IsNthChild(int a, int b) : base(a, b)
            {
            }

            internal override int CalculatePosition(Element root, Element element)
            {
                return element.ElementSiblingIndex() + 1;
            }

            internal override string GetPseudoClass()
            {
                return "nth-child";
            }
        }

        /// <summary>css pseudo class :nth-last-child)</summary>
        /// <seealso cref="IndexEquals">IndexEquals</seealso>
        public sealed class IsNthLastChild : Evaluator.CssNthEvaluator
        {
            public IsNthLastChild(int a, int b) : base(a, b)
            {
            }

            internal override int CalculatePosition(Element root, Element element)
            {
                return element.ParentElement().Children().Count - element.ElementSiblingIndex();
            }

            internal override string GetPseudoClass()
            {
                return "nth-last-child";
            }
        }

        /// <summary>css pseudo class nth-of-type</summary>
        public class IsNthOfType : Evaluator.CssNthEvaluator
        {
            public IsNthOfType(int a, int b) : base(a, b)
            {
            }

            internal override int CalculatePosition(Element root, Element element)
            {
                int pos = 0;
                Elements family = element.ParentElement().Children();
                for (int i = 0; i < family.Count; i++)
                {
                    if (family[i].Tag().Equals(element.Tag()))
                    {
                        pos++;
                    }
                    if (family[i] == element)
                    {
                        break;
                    }
                }
                return pos;
            }

            internal override string GetPseudoClass()
            {
                return "nth-of-type";
            }
        }

        public class IsNthLastOfType : Evaluator.CssNthEvaluator
        {
            public IsNthLastOfType(int a, int b) : base(a, b)
            {
            }

            internal override int CalculatePosition(Element root, Element element)
            {
                int pos = 0;
                Elements family = element.ParentElement().Children();
                for (int i = element.ElementSiblingIndex(); i < family.Count; i++)
                {
                    if (family[i].Tag().Equals(element.Tag()))
                    {
                        pos++;
                    }
                }
                return pos;
            }

            internal override string GetPseudoClass()
            {
                return "nth-last-of-type";
            }
        }

        /// <summary>Evaluator for matching the first sibling (css :first-child)</summary>
        public sealed class IsFirstChild : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                Element p = element.ParentElement();
                return p != null && !(p is Document) && element.ElementSiblingIndex() == 0;
            }

            public override string ToString()
            {
                return ":first-child";
            }
        }

        /// <summary>css3 pseudo-class :root</summary>
        /// <seealso><a href="http://www.w3.org/TR/selectors/#root-pseudo">:root selector</a></seealso>
        public sealed class IsRoot : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                Element r = root is Document ? root.Child(0) : root;
                return element == r;
            }

            public override string ToString()
            {
                return ":root";
            }
        }

        public sealed class IsOnlyChild : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                Element p = element.ParentElement();
                return p != null && !(p is Document) && element.SiblingElements().Count == 0;
            }

            public override string ToString()
            {
                return ":only-child";
            }
        }

        public sealed class IsOnlyOfType : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                Element p = element.ParentElement();
                if (p == null || p is Document)
                {
                    return false;
                }
                int pos = 0;
                Elements family = p.Children();
                for (int i = 0; i < family.Count; i++)
                {
                    if (family[i].Tag().Equals(element.Tag()))
                    {
                        pos++;
                    }
                }
                return pos == 1;
            }

            public override string ToString()
            {
                return ":only-of-type";
            }
        }

        public sealed class IsEmpty : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                IReadOnlyList<Node> family = element.ChildNodes();
                for (int i = 0; i < family.Count; i++)
                {
                    Node n = family[i];
                    if (!(n is Comment || n is XmlDeclaration || n is DocumentType))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                return ":empty";
            }
        }

        /// <summary>Abstract evaluator for sibling index matching</summary>
        /// <author>ant</author>
        public abstract class IndexEvaluator : Evaluator
        {
            internal int index;

            public IndexEvaluator(int index)
            {
                this.index = index;
            }
        }

        /// <summary>Evaluator for matching Element (and its descendants) text</summary>
        public sealed class ContainsText : Evaluator
        {
            private string searchText;

            public ContainsText(string searchText)
            {
                this.searchText = searchText.ToLower();
            }

            public override bool Matches(Element root, Element element)
            {
                return (element.Text().ToLower().Contains(searchText));
            }

            public override string ToString()
            {
                return string.Format(":contains({0}", searchText);
            }
        }

        /// <summary>Evaluator for matching Element's own text</summary>
        public sealed class ContainsOwnText : Evaluator
        {
            private readonly string searchText;

            public ContainsOwnText(string searchText)
            {
                this.searchText = searchText.ToLower();
            }

            public override bool Matches(Element root, Element element)
            {
                return (element.OwnText().ToLower().Contains(searchText));
            }

            public override string ToString()
            {
                return string.Format(":containsOwn({0}", searchText);
            }
        }

        /// <summary>Evaluator for matching Element (and its descendants) text with regex</summary>
        public sealed class MatchesText : Evaluator
        {
            private readonly Regex pattern;

            public MatchesText(Regex pattern)
            {
                this.pattern = pattern;
            }

            public override bool Matches(Element root, Element element)
            {
                Match m = pattern.Match(element.Text());
                return m.Success; /*find*/
            }

            public override string ToString()
            {
                return string.Format(":matches({0}", pattern);
            }
        }

        /// <summary>Evaluator for matching Element's own text with regex</summary>
        public sealed class MatchesOwnText : Evaluator
        {
            private readonly Regex pattern;

            public MatchesOwnText(Regex pattern)
            {
                this.pattern = pattern;
            }

            public override bool Matches(Element root, Element element)
            {
                Match m = pattern.Match(element.OwnText());
                return m.Success; /*find*/
            }

            public override string ToString()
            {
                return string.Format(":matchesOwn({0}", pattern);
            }
        }
    }
}
