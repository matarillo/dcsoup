using Supremes.Helper;
using Supremes.Nodes;
using System.Collections.Generic;
using System.Diagnostics;

namespace Supremes.Parsers
{
    /// <summary>
    /// HTML Tree Builder; creates a DOM from Tokens.
    /// </summary>
    internal class HtmlTreeBuilder : TreeBuilder
    {
        public static readonly string[] TagsSearchInScope = new string[] { "applet", "caption"
            , "html", "table", "td", "th", "marquee", "object" };

        private static readonly string[] TagSearchList = new string[] { "ol", "ul" };

        private static readonly string[] TagSearchButton = new string[] { "button" };

        private static readonly string[] TagSearchTableScope = new string[] { "html", "table"
             };

        private static readonly string[] TagSearchSelectScope = new string[] { "optgroup"
            , "option" };

        private static readonly string[] TagSearchEndTags = new string[] { "dd", "dt", "li"
            , "option", "optgroup", "p", "rp", "rt" };

        private static readonly string[] TagSearchSpecial = new string[] { "address", "applet"
            , "area", "article", "aside", "base", "basefont", "bgsound", "blockquote", "body"
            , "br", "button", "caption", "center", "col", "colgroup", "command", "dd", "details"
            , "dir", "div", "dl", "dt", "embed", "fieldset", "figcaption", "figure", "footer"
            , "form", "frame", "frameset", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header"
            , "hgroup", "hr", "html", "iframe", "img", "input", "isindex", "li", "link", "listing"
            , "marquee", "menu", "meta", "nav", "noembed", "noframes", "noscript", "object", 
            "ol", "p", "param", "plaintext", "pre", "script", "section", "select", "style", 
            "summary", "table", "tbody", "td", "textarea", "tfoot", "th", "thead", "title", 
            "tr", "ul", "wbr", "xmp" };

        private HtmlTreeBuilderState state;

        private HtmlTreeBuilderState originalState;

        private bool baseUriSetFromDoc = false;

        private Element headElement;

        private FormElement formElement;

        private Element contextElement;

        private DescendableLinkedList<Element> formattingElements = new DescendableLinkedList<Element>();

        private IList<Token.Character> pendingTableCharacters = new List<Token.Character>();

        private bool framesetOk = true;

        private bool fosterInserts = false;

        private bool fragmentParsing = false;

        public HtmlTreeBuilder()
        {
        }

        // tag searches
        //private static final String[] TagsScriptStyle = new String[]{"script", "style"};
        // the current state
        // original / marked state
        // the current head element
        // the current form element
        // fragment parse context -- could be null even if fragment parsing
        // active (open) formatting elements
        // chars in table to be shifted out
        // if ok to go into frameset
        // if next inserts should be fostered
        // if parsing a fragment of html
        internal override Document Parse(string input, string baseUri, ParseErrorList errors)
        {
            state = HtmlTreeBuilderState.Initial;
            baseUriSetFromDoc = false;
            return base.Parse(input, baseUri, errors);
        }

        internal IReadOnlyList<Node> ParseFragment(string inputFragment, Element context, string baseUri, ParseErrorList errors)
        {
            // context may be null
            state = HtmlTreeBuilderState.Initial;
            InitialiseParse(inputFragment, baseUri, errors);
            contextElement = context;
            fragmentParsing = true;
            Element root = null;
            if (context != null)
            {
                if (context.OwnerDocument() != null)
                {
                    // quirks setup:
                    doc.QuirksMode(context.OwnerDocument().QuirksMode());
                }
                // initialise the tokeniser state:
                string contextTag = context.TagName();
                if (StringUtil.In(contextTag, "title", "textarea"))
                {
                    tokeniser.Transition(TokeniserState.Rcdata);
                }
                else if (StringUtil.In(contextTag, "iframe", "noembed", "noframes", "style", "xmp"))
                {
                    tokeniser.Transition(TokeniserState.Rawtext);
                }
                else if (contextTag.Equals("script"))
                {
                    tokeniser.Transition(TokeniserState.ScriptData);
                }
                else if (contextTag.Equals(("noscript")))
                {
                    tokeniser.Transition(TokeniserState.Data); // if scripting enabled, rawtext
                }
                else if (contextTag.Equals("plaintext"))
                {
                    tokeniser.Transition(TokeniserState.Data);
                }
                else
                {
                    tokeniser.Transition(TokeniserState.Data);
                }

                // default
                root = new Element(Tag.ValueOf("html"), baseUri);
                doc.AppendChild(root);
                stack.Push(root);
                ResetInsertionMode();
                // setup form element to nearest form on context (up ancestor chain). ensures form controls are associated
                // with form correctly
                Elements contextChain = context.Parents();
                contextChain.Insert(0, context);
                foreach (Element parent in contextChain)
                {
                    if (parent is FormElement)
                    {
                        formElement = (FormElement)parent;
                        break;
                    }
                }
            }
            RunParser();
            if (context != null)
            {
                return root.ChildNodes();
            }
            else
            {
                return doc.ChildNodes();
            }
        }

        internal override bool Process(Token token)
        {
            currentToken = token;
            return this.state.Process(token, this);
        }

        internal bool Process(Token token, HtmlTreeBuilderState state)
        {
            currentToken = token;
            return state.Process(token, this);
        }

        internal void Transition(HtmlTreeBuilderState state)
        {
            this.state = state;
        }

        internal HtmlTreeBuilderState State()
        {
            return state;
        }

        internal void MarkInsertionMode()
        {
            originalState = state;
        }

        internal HtmlTreeBuilderState OriginalState()
        {
            return originalState;
        }

        internal void FramesetOk(bool framesetOk)
        {
            this.framesetOk = framesetOk;
        }

        internal bool FramesetOk()
        {
            return framesetOk;
        }

        internal Document GetDocument()
        {
            return doc;
        }

        internal string GetBaseUri()
        {
            return baseUri;
        }

        internal void MaybeSetBaseUri(Element @base)
        {
            if (baseUriSetFromDoc)
            {
                // only listen to the first <base href> in parse
                return;
            }
            string href = @base.AbsUrl("href");
            if (href.Length != 0)
            {
                // ignore <base target> etc
                baseUri = href;
                baseUriSetFromDoc = true;
                doc.SetBaseUri(href);
            }
        }

        // set on the doc so doc.createElement(Tag) will get updated base, and to update all descendants
        internal bool IsFragmentParsing()
        {
            return fragmentParsing;
        }

        internal void Error(HtmlTreeBuilderState state)
        {
            if (errors.CanAddError())
            {
                errors.Add(new ParseError(reader.Pos(), "Unexpected token [{0}] when in state [{1}]", currentToken.Type(), state.Name()));
            }
        }

        internal Element Insert(Token.StartTag startTag)
        {
            // handle empty unknown tags
            // when the spec expects an empty tag, will directly hit insertEmpty, so won't generate this fake end tag.
            if (startTag.IsSelfClosing())
            {
                Element el = InsertEmpty(startTag);
                stack.AddLast(el);
                tokeniser.Transition(TokeniserState.Data);
                // handles <script />, otherwise needs breakout steps from script data
                tokeniser.Emit(new Token.EndTag(el.TagName()));
                // ensure we get out of whatever state we are in. emitted for yielded processing
                return el;
            }
            Tag tag = Tag.ValueOf(startTag.Name());
            Element el_1 = new Element(tag, baseUri, startTag.attributes);
            Insert(el_1);
            return el_1;
        }

        internal Element Insert(string startTagName)
        {
            Element el = new Element(Tag.ValueOf(startTagName), baseUri);
            Insert(el);
            return el;
        }

        internal void Insert(Element el)
        {
            InsertNode(el);
            stack.AddLast(el);
        }

        internal Element InsertEmpty(Token.StartTag startTag)
        {
            Tag tag = Tag.ValueOf(startTag.Name());
            Element el = new Element(tag, baseUri, startTag.attributes);
            InsertNode(el);
            if (startTag.IsSelfClosing())
            {
                if (tag.IsKnownTag())
                {
                    if (tag.IsSelfClosing())
                    {
                        tokeniser.AcknowledgeSelfClosingFlag();
                    }
                }
                else
                {
                    // if not acked, promulagates error
                    // unknown tag, remember this is self closing for output
                    tag.SetSelfClosing();
                    tokeniser.AcknowledgeSelfClosingFlag();
                }
            }
            // not an distinct error
            return el;
        }

        internal FormElement InsertForm(Token.StartTag startTag, bool onStack)
        {
            Tag tag = Tag.ValueOf(startTag.Name());
            FormElement el = new FormElement(tag, baseUri, startTag.attributes);
            SetFormElement(el);
            InsertNode(el);
            if (onStack)
            {
                stack.AddLast(el);
            }
            return el;
        }

        internal void Insert(Token.Comment commentToken)
        {
            Comment comment = new Comment(commentToken.GetData(), baseUri);
            InsertNode(comment);
        }

        internal void Insert(Token.Character characterToken)
        {
            Node node;
            // characters in script and style go in as datanodes, not text nodes
            string tagName = CurrentElement().TagName();
            if (tagName.Equals("script") || tagName.Equals("style"))
            {
                node = new DataNode(characterToken.GetData(), baseUri);
            }
            else
            {
                node = new TextNode(characterToken.GetData(), baseUri);
            }
            CurrentElement().AppendChild(node);
        }

        // doesn't use insertNode, because we don't foster these; and will always have a stack.
        private void InsertNode(Node node)
        {
            // if the stack hasn't been set up yet, elements (doctype, comments) go into the doc
            if (stack.Count == 0)
            {
                doc.AppendChild(node);
            }
            else
            {
                if (IsFosterInserts())
                {
                    InsertInFosterParent(node);
                }
                else
                {
                    CurrentElement().AppendChild(node);
                }
            }
            // connect form controls to their form element
            if (node is Element && ((Element)node).Tag().IsFormListed())
            {
                if (formElement != null)
                {
                    formElement.AddElement((Element)node);
                }
            }
        }

        internal Element Pop()
        {
            // todo - dev, remove validation check
            if (stack.PeekLast().NodeName.Equals("td") && !state.Name().Equals("InCell"))
            {
                Validate.IsFalse(true, "pop td not in cell");
            }
            if (stack.PeekLast().NodeName.Equals("html"))
            {
                Validate.IsFalse(true, "popping html!");
            }
            return stack.PollLast();
        }

        internal void Push(Element element)
        {
            stack.AddLast(element);
        }

        internal DescendableLinkedList<Element> GetStack()
        {
            return stack;
        }

        internal bool OnStack(Element el)
        {
            return IsElementInQueue(stack, el);
        }

        private bool IsElementInQueue(DescendableLinkedList<Element> queue, Element element
            )
        {
            var it = queue.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next == element)
                {
                    return true;
                }
            }
            return false;
        }

        internal Element GetFromStack(string elName)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next.NodeName.Equals(elName))
                {
                    return next;
                }
            }
            return null;
        }

        internal bool RemoveFromStack(Element el)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next == el)
                {
                    it.Remove();
                    return true;
                }
            }
            return false;
        }

        internal void PopStackToClose(string elName)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next.NodeName.Equals(elName))
                {
                    it.Remove();
                    break;
                }
                else
                {
                    it.Remove();
                }
            }
        }

        internal void PopStackToClose(params string[] elNames)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (StringUtil.In(next.NodeName, elNames))
                {
                    it.Remove();
                    break;
                }
                else
                {
                    it.Remove();
                }
            }
        }

        internal void PopStackToBefore(string elName)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next.NodeName.Equals(elName))
                {
                    break;
                }
                else
                {
                    it.Remove();
                }
            }
        }

        internal void ClearStackToTableContext()
        {
            ClearStackToContext("table");
        }

        internal void ClearStackToTableBodyContext()
        {
            ClearStackToContext("tbody", "tfoot", "thead");
        }

        internal void ClearStackToTableRowContext()
        {
            ClearStackToContext("tr");
        }

        private void ClearStackToContext(params string[] nodeNames)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (StringUtil.In(next.NodeName, nodeNames) || next.NodeName.Equals("html"))
                {
                    break;
                }
                else
                {
                    it.Remove();
                }
            }
        }

        internal Element AboveOnStack(Element el)
        {
            Debug.Assert(OnStack(el));
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next == el)
                {
                    it.MoveNext();
                    return it.Current;
                }
            }
            return null;
        }

        internal void InsertOnStackAfter(Element after, Element @in)
        {
            var n = stack.FindLast(after);
            Validate.IsTrue(n != null);
            stack.AddAfter(n, @in);
        }

        internal void ReplaceOnStack(Element @out, Element @in)
        {
            ReplaceInQueue(stack, @out, @in);
        }

        private void ReplaceInQueue(LinkedList<Element>/*IList<Element>*/ queue, Element @out, Element @in)
        {
            var n = queue.FindLast(@out);
            Validate.IsTrue(n != null);
            queue.AddAfter(n, @in);
            queue.Remove(n);
        }

        internal void ResetInsertionMode()
        {
            bool last = false;
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element node = it.Current;
                if (!it.HasNext())
                {
                    last = true;
                    node = contextElement;
                }
                string name = node.NodeName;
                if ("select".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InSelect);
                    break; // frag
                }
                else if (("td".Equals(name) || "td".Equals(name) && !last))
                {
                    Transition(HtmlTreeBuilderState.InCell);
                    break;
                }
                else if ("tr".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InRow);
                    break;
                }
                else if ("tbody".Equals(name) || "thead".Equals(name) || "tfoot".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InTableBody);
                    break;
                }
                else if ("caption".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InCaption);
                    break;
                }
                else if ("colgroup".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InColumnGroup);
                    break; // frag
                }
                else if ("table".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InTable);
                    break;
                }
                else if ("head".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InBody);
                    break; // frag
                }
                else if ("body".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InBody);
                    break;
                }
                else if ("frameset".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.InFrameset);
                    break; // frag
                }
                else if ("html".Equals(name))
                {
                    Transition(HtmlTreeBuilderState.BeforeHead);
                    break; // frag
                }
                else if (last)
                {
                    Transition(HtmlTreeBuilderState.InBody);
                    break; // frag
                }
            }
        }
        
        // todo: tidy up in specific scope methods
        private bool InSpecificScope(string targetName, string[] baseTypes, string[] extraTypes)
        {
            return InSpecificScope(new string[] { targetName }, baseTypes, extraTypes);
        }

        private bool InSpecificScope(string[] targetNames, string[] baseTypes, string[] extraTypes)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element el = it.Current;
                string elName = el.NodeName;
                if (StringUtil.In(elName, targetNames))
                {
                    return true;
                }
                if (StringUtil.In(elName, baseTypes))
                {
                    return false;
                }
                if (extraTypes != null && StringUtil.In(elName, extraTypes))
                {
                    return false;
                }
            }
            Validate.Fail("Should not be reachable");
            return false;
        }

        internal bool InScope(string[] targetNames)
        {
            return InSpecificScope(targetNames, TagsSearchInScope, null);
        }

        internal bool InScope(string targetName)
        {
            return InScope(targetName, null);
        }

        internal bool InScope(string targetName, string[] extras)
        {
            return InSpecificScope(targetName, TagsSearchInScope, extras);
        }

        // todo: in mathml namespace: mi, mo, mn, ms, mtext annotation-xml
        // todo: in svg namespace: forignOjbect, desc, title
        internal bool InListItemScope(string targetName)
        {
            return InScope(targetName, TagSearchList);
        }

        internal bool InButtonScope(string targetName)
        {
            return InScope(targetName, TagSearchButton);
        }

        internal bool InTableScope(string targetName)
        {
            return InSpecificScope(targetName, TagSearchTableScope, null);
        }

        internal bool InSelectScope(string targetName)
        {
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element el = it.Current;
                string elName = el.NodeName;
                if (elName.Equals(targetName))
                {
                    return true;
                }
                if (!StringUtil.In(elName, TagSearchSelectScope))
                {
                    // all elements except
                    return false;
                }
            }
            Validate.Fail("Should not be reachable");
            return false;
        }

        internal void SetHeadElement(Element headElement)
        {
            this.headElement = headElement;
        }

        internal Element GetHeadElement()
        {
            return headElement;
        }

        internal bool IsFosterInserts()
        {
            return fosterInserts;
        }

        internal void SetFosterInserts(bool fosterInserts)
        {
            this.fosterInserts = fosterInserts;
        }

        internal FormElement GetFormElement()
        {
            return formElement;
        }

        internal void SetFormElement(FormElement formElement)
        {
            this.formElement = formElement;
        }

        internal void NewPendingTableCharacters()
        {
            pendingTableCharacters = new List<Token.Character>();
        }

        internal IList<Token.Character> GetPendingTableCharacters()
        {
            return pendingTableCharacters;
        }

        internal void SetPendingTableCharacters(IList<Token.Character> pendingTableCharacters
            )
        {
            this.pendingTableCharacters = pendingTableCharacters;
        }

        /// <summary>
        /// 11.2.5.2 Closing elements that have implied end tags<p/>
        /// When the steps below require the UA to generate implied end tags, then, while the current node is a dd element, a
        /// dt element, an li element, an option element, an optgroup element, a p element, an rp element, or an rt element,
        /// the UA must pop the current node off the stack of open elements.
        /// </summary>
        /// <remarks>
        /// 11.2.5.2 Closing elements that have implied end tags<p/>
        /// When the steps below require the UA to generate implied end tags, then, while the current node is a dd element, a
        /// dt element, an li element, an option element, an optgroup element, a p element, an rp element, or an rt element,
        /// the UA must pop the current node off the stack of open elements.
        /// </remarks>
        /// <param name="excludeTag">
        /// If a step requires the UA to generate implied end tags but lists an element to exclude from the
        /// process, then the UA must perform the above steps as if that element was not in the above list.
        /// </param>
        internal void GenerateImpliedEndTags(string excludeTag)
        {
            while ((excludeTag != null && !CurrentElement().NodeName.Equals(excludeTag))
                && StringUtil.In(CurrentElement().NodeName, TagSearchEndTags))
            {
                Pop();
            }
        }

        internal void GenerateImpliedEndTags()
        {
            GenerateImpliedEndTags(null);
        }

        internal bool IsSpecial(Element el)
        {
            // todo: mathml's mi, mo, mn
            // todo: svg's foreigObject, desc, title
            string name = el.NodeName;
            return StringUtil.In(name, TagSearchSpecial);
        }

        // active formatting elements
        internal void PushActiveFormattingElements(Element @in)
        {
            int numSeen = 0;
            var iter = formattingElements.GetDescendingEnumerator();
            while (iter.MoveNext())
            {
                Element el = iter.Current;
                if (el == null)
                {
                    // marker
                    break;
                }
                if (IsSameFormattingElement(@in, el))
                {
                    numSeen++;
                }
                if (numSeen == 3)
                {
                    iter.Remove();
                    break;
                }
            }
            formattingElements.AddLast(@in);
        }

        private bool IsSameFormattingElement(Element a, Element b)
        {
            // same if: same namespace, tag, and attributes. Element.equals only checks tag, might in future check children
            return a.NodeName.Equals(b.NodeName) && a.Attributes().Equals(b.Attributes());
        }

        // a.namespace().equals(b.namespace()) &&
        // todo: namespaces
        internal void ReconstructFormattingElements()
        {
            int size = formattingElements.Count;
            if (size == 0 || formattingElements.Last.Value == null || OnStack(formattingElements.Last.Value))
            {
                return;
            }
            Element entry = formattingElements.Last.Value;
            int pos = size - 1;
            bool skip = false;
            while (true)
            {
                if (pos == 0)
                {
                    // step 4. if none before, skip to 8
                    skip = true;
                    break;
                }
                entry = formattingElements[--pos];
                // step 5. one earlier than entry
                if (entry == null || OnStack(entry))
                {
                    // step 6 - neither marker nor on stack
                    break;
                }
            }
            // jump to 8, else continue back to 4
            while (true)
            {
                if (!skip)
                {
                    // step 7: on later than entry
                    entry = formattingElements[++pos];
                }
                Validate.NotNull(entry);
                // should not occur, as we break at last element
                // 8. create new element from element, 9 insert into current node, onto stack
                skip = false;
                // can only skip increment from 4.
                Element newEl = Insert(entry.NodeName);
                // todo: avoid fostering here?
                // newEl.namespace(entry.namespace()); // todo: namespaces
                newEl.Attributes().SetAll(entry.Attributes());
                // 10. replace entry with new entry
                formattingElements[pos] = newEl;
                // 11
                if (pos == size - 1)
                {
                    // if not last entry in list, jump to 7
                    break;
                }
            }
        }

        internal void ClearFormattingElementsToLastMarker()
        {
            while (formattingElements.Count > 0)
            {
                Element el = formattingElements.PeekLast();
                formattingElements.RemoveLast();
                if (el == null)
                {
                    break;
                }
            }
        }

        internal void RemoveFromActiveFormattingElements(Element el)
        {
            var it = formattingElements.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next == el)
                {
                    it.Remove();
                    break;
                }
            }
        }

        internal bool IsInActiveFormattingElements(Element el)
        {
            return IsElementInQueue(formattingElements, el);
        }

        internal Element GetActiveFormattingElement(string nodeName)
        {
            var it = formattingElements.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next == null)
                {
                    // scope marker
                    break;
                }
                else
                {
                    if (next.NodeName.Equals(nodeName))
                    {
                        return next;
                    }
                }
            }
            return null;
        }

        internal void ReplaceActiveFormattingElement(Element @out, Element @in)
        {
            ReplaceInQueue(formattingElements, @out, @in);
        }

        internal void InsertMarkerToFormattingElements()
        {
            formattingElements.AddLast((Element)null);
        }

        internal void InsertInFosterParent(Node @in)
        {
            Element fosterParent = null;
            Element lastTable = GetFromStack("table");
            bool isLastTableParent = false;
            if (lastTable != null)
            {
                if (lastTable.Parent() != null)
                {
                    fosterParent = lastTable.ParentElement();
                    isLastTableParent = true;
                }
                else
                {
                    fosterParent = AboveOnStack(lastTable);
                }
            }
            else
            {
                // no table == frag
                fosterParent = stack.First.Value;
            }
            if (isLastTableParent)
            {
                Validate.NotNull(lastTable);
                // last table cannot be null by this point.
                lastTable.Before(@in);
            }
            else
            {
                fosterParent.AppendChild(@in);
            }
        }

        public override string ToString()
        {
            return "TreeBuilder{" + "currentToken=" + currentToken + ", state=" + state + ", currentElement=" + CurrentElement() + '}';
        }
    }
}
