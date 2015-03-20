using Supremes.Helper;
using Supremes.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremes.Parsers
{
    /// <summary>
    /// The Tree Builder's current state.
    /// </summary>
    /// <remarks>
    /// Each state embodies the processing for the state, and transitions to other states.
    /// </remarks>
    internal abstract class HtmlTreeBuilderState
    {
        private sealed class _HtmlTreeBuilderState_16 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_16(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    // ignore whitespace
                    return true;
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (t.IsDoctype())
                {
                    // todo: parse error check on expected doctypes
                    // todo: quirk state check on doctype ids
                    Token.Doctype d = t.AsDoctype();
                    DocumentType doctype = new DocumentType(d.GetName(), d.GetPublicIdentifier(), d.GetSystemIdentifier(), tb.GetBaseUri());
                    tb.GetDocument().AppendChild(doctype);
                    if (d.IsForceQuirks())
                    {
                        tb.GetDocument().QuirksMode = DocumentQuirksMode.Quirks;
                    }
                    tb.Transition(HtmlTreeBuilderState.BeforeHtml);
                }
                else
                {
                    // todo: check not iframe srcdoc
                    tb.Transition(HtmlTreeBuilderState.BeforeHtml);
                    return tb.Process(t); // re-process token
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState Initial = new _HtmlTreeBuilderState_16("Initial");

        private sealed class _HtmlTreeBuilderState_40 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_40(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsDoctype())
                {
                    tb.Error(this);
                    return false;
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    return true;
                    // ignore whitespace
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("html"))
                {
                    tb.Insert(t.AsStartTag());
                    tb.Transition(HtmlTreeBuilderState.BeforeHead);
                }
                else if (t.IsEndTag() && (StringUtil.In(t.AsEndTag().Name(), "head", "body", "html", "br")))
                {
                    return this.AnythingElse(t, tb);
                }
                else if (t.IsEndTag())
                {
                    tb.Error(this);
                    return false;
                }
                else
                {
                    return this.AnythingElse(t, tb);
                }
                return true;
            }

            private bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                tb.Insert("html");
                tb.Transition(HtmlTreeBuilderState.BeforeHead);
                return tb.Process(t);
            }
        }

        public static readonly HtmlTreeBuilderState BeforeHtml = new _HtmlTreeBuilderState_40("BeforeHtml");

        private sealed class _HtmlTreeBuilderState_69 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_69(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    return true;
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (t.IsDoctype())
                {
                    tb.Error(this);
                    return false;
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("html"))
                {
                    return HtmlTreeBuilderState.InBody.Process(t, tb);
                    // does not transition
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("head"))
                {
                    Element head = tb.Insert(t.AsStartTag());
                    tb.SetHeadElement(head);
                    tb.Transition(HtmlTreeBuilderState.InHead);
                }
                else if (t.IsEndTag() && (StringUtil.In(t.AsEndTag().Name(), "head", "body", "html", "br")))
                {
                    tb.Process(new Token.StartTag("head"));
                    return tb.Process(t);
                }
                else if (t.IsEndTag())
                {
                    tb.Error(this);
                    return false;
                }
                else
                {
                    tb.Process(new Token.StartTag("head"));
                    return tb.Process(t);
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState BeforeHead = new _HtmlTreeBuilderState_69("BeforeHead");

        private sealed class _HtmlTreeBuilderState_97 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_97(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    tb.Insert(t.AsCharacter());
                    return true;
                }
                switch (t.type)
                {
                    case TokenType.Comment:
                    {
                        tb.Insert(t.AsComment());
                        break;
                    }

                    case TokenType.Doctype:
                    {
                        tb.Error(this);
                        return false;
                    }

                    case TokenType.StartTag:
                    {
                        Token.StartTag start = t.AsStartTag();
                        string name = start.Name();
                        if (name.Equals("html"))
                        {
                            return HtmlTreeBuilderState.InBody.Process(t, tb);
                        }
                        else if (StringUtil.In(name, "base", "basefont", "bgsound", "command", "link"))
                        {
                            Element el = tb.InsertEmpty(start);
                            // jsoup special: update base the frist time it is seen
                            if (name.Equals("base") && el.HasAttr("href"))
                            {
                                tb.MaybeSetBaseUri(el);
                            }
                        }
                        else if (name.Equals("meta"))
                        {
                            tb.InsertEmpty(start); //Element meta = tb.insertEmpty(start);
                            // todo: charset switches
                        }
                        else if (name.Equals("title"))
                        {
                            HtmlTreeBuilderState.HandleRcData(start, tb);
                        }
                        else if (StringUtil.In(name, "noframes", "style"))
                        {
                            HtmlTreeBuilderState.HandleRawtext(start, tb);
                        }
                        else if (name.Equals("noscript"))
                        {
                            // else if noscript && scripting flag = true: rawtext (jsoup doesn't run script, to handle as noscript)
                            tb.Insert(start);
                            tb.Transition(HtmlTreeBuilderState.InHeadNoscript);
                        }
                        else if (name.Equals("script"))
                        {
                            // skips some script rules as won't execute them
                            tb.tokeniser.Transition(TokeniserState.ScriptData);
                            tb.MarkInsertionMode();
                            tb.Transition(HtmlTreeBuilderState.Text);
                            tb.Insert(start);
                        }
                        else if (name.Equals("head"))
                        {
                            tb.Error(this);
                            return false;
                        }
                        else
                        {
                            return this.AnythingElse(t, tb);
                        }
                        break;
                    }

                    case TokenType.EndTag:
                    {
                        Token.EndTag end = t.AsEndTag();
                        string name = end.Name();
                        if (name.Equals("head"))
                        {
                            tb.Pop();
                            tb.Transition(HtmlTreeBuilderState.AfterHead);
                        }
                        else if (StringUtil.In(name, "body", "html", "br"))
                        {
                            return this.AnythingElse(t, tb);
                        }
                        else
                        {
                            tb.Error(this);
                            return false;
                        }
                        break;
                    }

                    default:
                    {
                        return this.AnythingElse(t, tb);
                    }
                }
                return true;
            }

            private bool AnythingElse(Token t, TreeBuilder tb)
            {
                tb.Process(new Token.EndTag("head"));
                return tb.Process(t);
            }
        }

        public static readonly HtmlTreeBuilderState InHead = new _HtmlTreeBuilderState_97("InHead");

        private sealed class _HtmlTreeBuilderState_169 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_169(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsDoctype())
                {
                    tb.Error(this);
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("html"))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                else if (t.IsEndTag() && t.AsEndTag().Name().Equals("noscript"))
                {
                    tb.Pop();
                    tb.Transition(HtmlTreeBuilderState.InHead);
                }
                else if (HtmlTreeBuilderState.IsWhitespace(t)
                    || t.IsComment()
                    || (t.IsStartTag() && StringUtil.In(t.AsStartTag().Name(), "basefont", "bgsound", "link", "meta", "noframes", "style")))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InHead);
                }
                else if (t.IsEndTag() && t.AsEndTag().Name().Equals("br"))
                {
                    return this.AnythingElse(t, tb);
                }
                else if ((t.IsStartTag() && StringUtil.In(t.AsStartTag().Name(), "head", "noscript"))
                    || t.IsEndTag())
                {
                    tb.Error(this);
                    return false;
                }
                else
                {
                    return this.AnythingElse(t, tb);
                }
                return true;
            }

            private bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                tb.Error(this);
                tb.Process(new Token.EndTag("noscript"));
                return tb.Process(t);
            }
        }

        public static readonly HtmlTreeBuilderState InHeadNoscript = new _HtmlTreeBuilderState_169("InHeadNoscript");

        private sealed class _HtmlTreeBuilderState_198 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_198(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    tb.Insert(t.AsCharacter());
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (t.IsDoctype())
                {
                    tb.Error(this);
                }
                else if (t.IsStartTag())
                {
                    Token.StartTag startTag = t.AsStartTag();
                    string name = startTag.Name();
                    if (name.Equals("html"))
                    {
                        return tb.Process(t, HtmlTreeBuilderState.InBody);
                    }
                    else if (name.Equals("body"))
                    {
                        tb.Insert(startTag);
                        tb.FramesetOk(false);
                        tb.Transition(HtmlTreeBuilderState.InBody);
                    }
                    else if (name.Equals("frameset"))
                    {
                        tb.Insert(startTag);
                        tb.Transition(HtmlTreeBuilderState.InFrameset);
                    }
                    else if (StringUtil.In(name, "base", "basefont", "bgsound", "link", "meta", "noframes", "script", "style", "title"))
                    {
                        tb.Error(this);
                        Element head = tb.GetHeadElement();
                        tb.Push(head);
                        tb.Process(t, HtmlTreeBuilderState.InHead);
                        tb.RemoveFromStack(head);
                    }
                    else if (name.Equals("head"))
                    {
                        tb.Error(this);
                        return false;
                    }
                    else
                    {
                        this.AnythingElse(t, tb);
                    }
                }
                else if (t.IsEndTag())
                {
                    if (StringUtil.In(t.AsEndTag().Name(), "body", "html"))
                    {
                        this.AnythingElse(t, tb);
                    }
                    else
                    {
                        tb.Error(this);
                        return false;
                    }
                }
                else
                {
                    this.AnythingElse(t, tb);
                }
                return true;
            }

            private bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                tb.Process(new Token.StartTag("body"));
                tb.FramesetOk(true);
                return tb.Process(t);
            }
        }

        public static readonly HtmlTreeBuilderState AfterHead = new _HtmlTreeBuilderState_198("AfterHead");

        private sealed class _HtmlTreeBuilderState_249 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_249(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                switch (t.type)
                {
                    case TokenType.Character:
                    {
                        Token.Character c = t.AsCharacter();
                        if (c.GetData().Equals(HtmlTreeBuilderState.nullString))
                        {
                            // todo confirm that check
                            tb.Error(this);
                            return false;
                        }
                        else if (tb.FramesetOk() && HtmlTreeBuilderState.IsWhitespace(c))
                        {
                            // don't check if whitespace if frames already closed
                            tb.ReconstructFormattingElements();
                            tb.Insert(c);
                        }
                        else
                        {
                            tb.ReconstructFormattingElements();
                            tb.Insert(c);
                            tb.FramesetOk(false);
                        }
                        break;
                    }

                    case TokenType.Comment:
                    {
                        tb.Insert(t.AsComment());
                        break;
                    }

                    case TokenType.Doctype:
                    {
                        tb.Error(this);
                        return false;
                    }

                    case TokenType.StartTag:
                    {
                        Token.StartTag startTag = t.AsStartTag();
                        string name = startTag.Name();
                        if (name.Equals("html"))
                        {
                            tb.Error(this);
                            // merge attributes onto real html
                            Element html = tb.GetStack().First.Value;
                            foreach (Supremes.Nodes.Attribute attribute in startTag.GetAttributes())
                            {
                                if (!html.HasAttr(attribute.Key))
                                {
                                    html.Attributes.Put(attribute);
                                }
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartToHead))
                        {
                            return tb.Process(t, HtmlTreeBuilderState.InHead);
                        }
                        else if (name.Equals("body"))
                        {
                            tb.Error(this);
                            IList<Element> stack = tb.GetStack();
                            if (stack.Count == 1 || (stack.Count > 2 && !stack[1].NodeName.Equals("body")))
                            {
                                // only in fragment case
                                return false;
                            }
                            else
                            {
                                // ignore
                                tb.FramesetOk(false);
                                Element body = stack[1];
                                foreach (Supremes.Nodes.Attribute attribute in startTag.GetAttributes())
                                {
                                    if (!body.HasAttr(attribute.Key))
                                    {
                                        body.Attributes.Put(attribute);
                                    }
                                }
                            }
                        }
                        else if (name.Equals("frameset"))
                        {
                            tb.Error(this);
                            var stack = tb.GetStack();
                            if (stack.Count == 1 || (stack.Count > 2 && !stack[1].NodeName.Equals("body")))
                            {
                                // only in fragment case
                                return false;
                                // ignore
                            }
                            else if (!tb.FramesetOk())
                            {
                                return false;
                                // ignore frameset
                            }
                            else
                            {
                                Element second = stack[1];
                                if (second.Parent != null)
                                {
                                    second.Remove();
                                }
                                // pop up to html element
                                while (stack.Count > 1)
                                {
                                    stack.RemoveLast();
                                }
                                tb.Insert(startTag);
                                tb.Transition(HtmlTreeBuilderState.InFrameset);
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartPClosers))
                        {
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.Insert(startTag);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.Headings))
                        {
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            if (StringUtil.In(tb.CurrentElement().NodeName, HtmlTreeBuilderState.Constants.Headings))
                            {
                                tb.Error(this);
                                tb.Pop();
                            }
                            tb.Insert(startTag);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartPreListing))
                        {
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.Insert(startTag);
                            // todo: ignore LF if next token
                            tb.FramesetOk(false);
                        }
                        else if (name.Equals("form"))
                        {
                            if (tb.GetFormElement() != null)
                            {
                                tb.Error(this);
                                return false;
                            }
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.InsertForm(startTag, true);
                        }
                        else if (name.Equals("li"))
                        {
                            tb.FramesetOk(false);
                            IList<Element> stack = tb.GetStack();
                            for (int i = stack.Count - 1; i > 0; i--)
                            {
                                Element el = stack[i];
                                if (el.NodeName.Equals("li"))
                                {
                                    tb.Process(new Token.EndTag("li"));
                                    break;
                                }
                                if (tb.IsSpecial(el) && !StringUtil.In(el.NodeName, HtmlTreeBuilderState.Constants.InBodyStartLiBreakers))
                                {
                                    break;
                                }
                            }
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.Insert(startTag);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.DdDt))
                        {
                            tb.FramesetOk(false);
                            IList<Element> stack = tb.GetStack();
                            for (int i = stack.Count - 1; i > 0; i--)
                            {
                                Element el = stack[i];
                                if (StringUtil.In(el.NodeName, HtmlTreeBuilderState.Constants.DdDt))
                                {
                                    tb.Process(new Token.EndTag(el.NodeName));
                                    break;
                                }
                                if (tb.IsSpecial(el) && !StringUtil.In(el.NodeName, HtmlTreeBuilderState.Constants
                                    .InBodyStartLiBreakers))
                                {
                                    break;
                                }
                            }
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.Insert(startTag);
                        }
                        else if (name.Equals("plaintext"))
                        {
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.Insert(startTag);
                            tb.tokeniser.Transition(TokeniserState.PLAINTEXT);
                            // once in, never gets out
                        }
                        else if (name.Equals("button"))
                        {
                            if (tb.InButtonScope("button"))
                            {
                                // close and reprocess
                                tb.Error(this);
                                tb.Process(new Token.EndTag("button"));
                                tb.Process(startTag);
                            }
                            else
                            {
                                tb.ReconstructFormattingElements();
                                tb.Insert(startTag);
                                tb.FramesetOk(false);
                            }
                        }
                        else if (name.Equals("a"))
                        {
                            if (tb.GetActiveFormattingElement("a") != null)
                            {
                                tb.Error(this);
                                tb.Process(new Token.EndTag("a"));
                                // still on stack?
                                Element remainingA = tb.GetFromStack("a");
                                if (remainingA != null)
                                {
                                    tb.RemoveFromActiveFormattingElements(remainingA);
                                    tb.RemoveFromStack(remainingA);
                                }
                            }
                            tb.ReconstructFormattingElements();
                            Element a = tb.Insert(startTag);
                            tb.PushActiveFormattingElements(a);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.Formatters))
                        {
                            tb.ReconstructFormattingElements();
                            Element el = tb.Insert(startTag);
                            tb.PushActiveFormattingElements(el);
                        }
                        else if (name.Equals("nobr"))
                        {
                            tb.ReconstructFormattingElements();
                            if (tb.InScope("nobr"))
                            {
                                tb.Error(this);
                                tb.Process(new Token.EndTag("nobr"));
                                tb.ReconstructFormattingElements();
                            }
                            Element el = tb.Insert(startTag);
                            tb.PushActiveFormattingElements(el);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartApplets))
                        {
                            tb.ReconstructFormattingElements();
                            tb.Insert(startTag);
                            tb.InsertMarkerToFormattingElements();
                            tb.FramesetOk(false);
                        }
                        else if (name.Equals("table"))
                        {
                            if (tb.GetDocument().QuirksMode != DocumentQuirksMode.Quirks && tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.Insert(startTag);
                            tb.FramesetOk(false);
                            tb.Transition(HtmlTreeBuilderState.InTable);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartEmptyFormatters))
                        {
                            tb.ReconstructFormattingElements();
                            tb.InsertEmpty(startTag);
                            tb.FramesetOk(false);
                        }
                        else if (name.Equals("input"))
                        {
                            tb.ReconstructFormattingElements();
                            Element el = tb.InsertEmpty(startTag);
                            if (!string.Equals(el.Attr("type"), "hidden", StringComparison.OrdinalIgnoreCase))
                            {
                                tb.FramesetOk(false);
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartMedia))
                        {
                            tb.InsertEmpty(startTag);
                        }
                        else if (name.Equals("hr"))
                        {
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.InsertEmpty(startTag);
                            tb.FramesetOk(false);
                        }
                        else if (name.Equals("image"))
                        {
                            if (tb.GetFromStack("svg") == null)
                            {
                                return tb.Process(startTag.Name("img"));
                            }
                            else
                            {
                                // change <image> to <img>, unless in svg
                                tb.Insert(startTag);
                            }
                        }
                        else if (name.Equals("isindex"))
                        {
                            // how much do we care about the early 90s?
                            tb.Error(this);
                            if (tb.GetFormElement() != null)
                            {
                                return false;
                            }
                            tb.tokeniser.AcknowledgeSelfClosingFlag();
                            tb.Process(new Token.StartTag("form"));
                            if (startTag.attributes.ContainsKey("action"))
                            {
                                Element form = tb.GetFormElement();
                                form.Attr("action", startTag.attributes["action"]);
                            }
                            tb.Process(new Token.StartTag("hr"));
                            tb.Process(new Token.StartTag("label"));
                            // hope you like english.
                            string prompt = startTag.attributes.ContainsKey("prompt") ? startTag.attributes["prompt"] : "This is a searchable index. Enter search keywords: ";
                            tb.Process(new Token.Character(prompt));
                            // input
                            Attributes inputAttribs = new Attributes();
                            foreach (Supremes.Nodes.Attribute attr in startTag.attributes)
                            {
                                if (!StringUtil.In(attr.Key, HtmlTreeBuilderState.Constants.InBodyStartInputAttribs))
                                {
                                    inputAttribs.Put(attr);
                                }
                            }
                            inputAttribs["name"] = "isindex";
                            tb.Process(new Token.StartTag("input", inputAttribs));
                            tb.Process(new Token.EndTag("label"));
                            tb.Process(new Token.StartTag("hr"));
                            tb.Process(new Token.EndTag("form"));
                        }
                        else if (name.Equals("textarea"))
                        {
                            tb.Insert(startTag);
                            // todo: If the next token is a U+000A LINE FEED (LF) character token, then ignore that token and move on to the next one. (Newlines at the start of textarea elements are ignored as an authoring convenience.)
                            tb.tokeniser.Transition(TokeniserState.Rcdata);
                            tb.MarkInsertionMode();
                            tb.FramesetOk(false);
                            tb.Transition(HtmlTreeBuilderState.Text);
                        }
                        else if (name.Equals("xmp"))
                        {
                            if (tb.InButtonScope("p"))
                            {
                                tb.Process(new Token.EndTag("p"));
                            }
                            tb.ReconstructFormattingElements();
                            tb.FramesetOk(false);
                            HtmlTreeBuilderState.HandleRawtext(startTag, tb);
                        }
                        else if (name.Equals("iframe"))
                        {
                            tb.FramesetOk(false);
                            HtmlTreeBuilderState.HandleRawtext(startTag, tb);
                        }
                        else if (name.Equals("noembed"))
                        {
                            // also handle noscript if script enabled
                            HtmlTreeBuilderState.HandleRawtext(startTag, tb);
                        }
                        else if (name.Equals("select"))
                        {
                            tb.ReconstructFormattingElements();
                            tb.Insert(startTag);
                            tb.FramesetOk(false);
                            HtmlTreeBuilderState state = tb.State();
                            if (state.Equals(HtmlTreeBuilderState.InTable)
                                || state.Equals(HtmlTreeBuilderState.InCaption)
                                || state.Equals(HtmlTreeBuilderState.InTableBody)
                                || state.Equals(HtmlTreeBuilderState.InRow)
                                || state.Equals(HtmlTreeBuilderState.InCell))
                            {
                                tb.Transition(HtmlTreeBuilderState.InSelectInTable);
                            }
                            else
                            {
                                tb.Transition(HtmlTreeBuilderState.InSelect);
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartOptions))
                        {
                            if (tb.CurrentElement().NodeName.Equals("option"))
                            {
                                tb.Process(new Token.EndTag("option"));
                            }
                            tb.ReconstructFormattingElements();
                            tb.Insert(startTag);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartRuby))
                        {
                            if (tb.InScope("ruby"))
                            {
                                tb.GenerateImpliedEndTags();
                                if (!tb.CurrentElement().NodeName.Equals("ruby"))
                                {
                                    tb.Error(this);
                                    tb.PopStackToBefore("ruby");
                                }
                                // i.e. close up to but not include name
                                tb.Insert(startTag);
                            }
                        }
                        else if (name.Equals("math"))
                        {
                            tb.ReconstructFormattingElements();
                            // todo: handle A start tag whose tag name is "math" (i.e. foreign, mathml)
                            tb.Insert(startTag);
                            tb.tokeniser.AcknowledgeSelfClosingFlag();
                        }
                        else if (name.Equals("svg"))
                        {
                            tb.ReconstructFormattingElements();
                            // todo: handle A start tag whose tag name is "svg" (xlink, svg)
                            tb.Insert(startTag);
                            tb.tokeniser.AcknowledgeSelfClosingFlag();
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartDrop))
                        {
                            tb.Error(this);
                            return false;
                        }
                        else
                        {
                            tb.ReconstructFormattingElements();
                            tb.Insert(startTag);

                        }
                        break;
                    }

                    case TokenType.EndTag:
                    {
                        Token.EndTag endTag = t.AsEndTag();
                        string name = endTag.Name();
                        if (name.Equals("body"))
                        {
                            if (!tb.InScope("body"))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                // todo: error if stack contains something not dd, dt, li, optgroup, option, p, rp, rt, tbody, td, tfoot, th, thead, tr, body, html
                                tb.Transition(HtmlTreeBuilderState.AfterBody);
                            }
                        }
                        else if (name.Equals("html"))
                        {
                            bool notIgnored = tb.Process(new Token.EndTag("body"));
                            if (notIgnored)
                            {
                                return tb.Process(endTag);
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyEndClosers))
                        {
                            if (!tb.InScope(name))
                            {
                                // nothing to close
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.GenerateImpliedEndTags();
                                if (!tb.CurrentElement().NodeName.Equals(name))
                                {
                                    tb.Error(this);
                                }
                                tb.PopStackToClose(name);
                            }
                        }
                        else if (name.Equals("form"))
                        {
                            Element currentForm = tb.GetFormElement();
                            tb.SetFormElement(null);
                            if (currentForm == null || !tb.InScope(name))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.GenerateImpliedEndTags();
                                if (!tb.CurrentElement().NodeName.Equals(name))
                                {
                                    tb.Error(this);
                                }
                                // remove currentForm from stack. will shift anything under up.
                                tb.RemoveFromStack(currentForm);
                            }
                        }
                        else if (name.Equals("p"))
                        {
                            if (!tb.InButtonScope(name))
                            {
                                tb.Error(this);
                                tb.Process(new Token.StartTag(name));
                                // if no p to close, creates an empty <p></p>
                                return tb.Process(endTag);
                            }
                            else
                            {
                                tb.GenerateImpliedEndTags(name);
                                if (!tb.CurrentElement().NodeName.Equals(name))
                                {
                                    tb.Error(this);
                                }
                                tb.PopStackToClose(name);
                            }
                        }
                        else if (name.Equals("li"))
                        {
                            if (!tb.InListItemScope(name))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.GenerateImpliedEndTags(name);
                                if (!tb.CurrentElement().NodeName.Equals(name))
                                {
                                    tb.Error(this);
                                }
                                tb.PopStackToClose(name);
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.DdDt))
                        {
                            if (!tb.InScope(name))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.GenerateImpliedEndTags(name);
                                if (!tb.CurrentElement().NodeName.Equals(name))
                                {
                                    tb.Error(this);
                                }
                                tb.PopStackToClose(name);
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.Headings))
                        {
                            if (!tb.InScope(HtmlTreeBuilderState.Constants.Headings))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.GenerateImpliedEndTags(name);
                                if (!tb.CurrentElement().NodeName.Equals(name))
                                {
                                    tb.Error(this);
                                }
                                tb.PopStackToClose(HtmlTreeBuilderState.Constants.Headings);
                            }
                        }
                        else if (name.Equals("sarcasm"))
                        {
                            // *sigh*
                            return this.AnyOtherEndTag(t, tb);
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyEndAdoptionFormatters))
                        {
                            // Adoption Agency Algorithm.
                            // OUTER:
                            for (int i = 0; i < 8; i++)
                            {
                                Element formatEl = tb.GetActiveFormattingElement(name);
                                if (formatEl == null)
                                {
                                    return this.AnyOtherEndTag(t, tb);
                                }
                                else if (!tb.OnStack(formatEl))
                                {
                                    tb.Error(this);
                                    tb.RemoveFromActiveFormattingElements(formatEl);
                                    return true;
                                }
                                else if (!tb.InScope(formatEl.NodeName))
                                {
                                    tb.Error(this);
                                    return false;
                                }
                                else if (tb.CurrentElement() != formatEl)
                                {
                                    tb.Error(this);
                                }
                                Element furthestBlock = null;
                                Element commonAncestor = null;
                                bool seenFormattingElement = false;
                                IList<Element> stack = tb.GetStack();
                                // the spec doesn't limit to < 64, but in degenerate cases (9000+ stack depth) this prevents
                                // run-aways
                                int stackSize = stack.Count;
                                for (int si = 0; si < stackSize && si < 64; si++)
                                {
                                    Element el = stack[si];
                                    if (el == formatEl)
                                    {
                                        commonAncestor = stack[si - 1];
                                        seenFormattingElement = true;
                                    }
                                    else if (seenFormattingElement && tb.IsSpecial(el))
                                    {
                                        furthestBlock = el;
                                        break;
                                    }
                                }
                                if (furthestBlock == null)
                                {
                                    tb.PopStackToClose(formatEl.NodeName);
                                    tb.RemoveFromActiveFormattingElements(formatEl);
                                    return true;
                                }
                                // todo: Let a bookmark note the position of the formatting element in the list of active formatting elements relative to the elements on either side of it in the list.
                                // does that mean: int pos of format el in list?
                                Element node = furthestBlock;
                                Element lastNode = furthestBlock;
                                for (int j = 0; j < 3; j++)
                                {
                                    if (tb.OnStack(node))
                                    {
                                        node = tb.AboveOnStack(node);
                                    }
                                    if (!tb.IsInActiveFormattingElements(node))
                                    {
                                        // note no bookmark check
                                        tb.RemoveFromStack(node);
                                        goto INNER_continue;
                                    }
                                    else
                                    {
                                        if (node == formatEl)
                                        {
                                            goto INNER_break;
                                        }
                                    }
                                    Element replacement = new Element(Tag.ValueOf(node.NodeName), tb.GetBaseUri());
                                    tb.ReplaceActiveFormattingElement(node, replacement);
                                    tb.ReplaceOnStack(node, replacement);
                                    node = replacement;
                                    if (lastNode == furthestBlock)
                                    {
                                    }
                                    // todo: move the aforementioned bookmark to be immediately after the new node in the list of active formatting elements.
                                    // not getting how this bookmark both straddles the element above, but is inbetween here...
                                    if (lastNode.Parent != null)
                                    {
                                        lastNode.Remove();
                                    }
                                    node.AppendChild(lastNode);
                                    lastNode = node;
                                INNER_continue: ;
                                }
                            INNER_break: ;
                                if (StringUtil.In(commonAncestor.NodeName, HtmlTreeBuilderState.Constants.InBodyEndTableFosters))
                                {
                                    if (lastNode.Parent!= null)
                                    {
                                        lastNode.Remove();
                                    }
                                    tb.InsertInFosterParent(lastNode);
                                }
                                else
                                {
                                    if (lastNode.Parent != null)
                                    {
                                        lastNode.Remove();
                                    }
                                    commonAncestor.AppendChild(lastNode);
                                }
                                Element adopter = new Element(formatEl.Tag, tb.GetBaseUri());
                                adopter.Attributes.SetAll(formatEl.Attributes);
                                Node[] childNodes = furthestBlock.ChildNodes.ToArray();
                                foreach (Node childNode in childNodes)
                                {
                                    adopter.AppendChild(childNode);
                                }
                                // append will reparent. thus the clone to avoid concurrent mod.
                                furthestBlock.AppendChild(adopter);
                                tb.RemoveFromActiveFormattingElements(formatEl);
                                // todo: insert the new element into the list of active formatting elements at the position of the aforementioned bookmark.
                                tb.RemoveFromStack(formatEl);
                                tb.InsertOnStackAfter(furthestBlock, adopter);
                            }
                        }
                        else if (StringUtil.In(name, HtmlTreeBuilderState.Constants.InBodyStartApplets))
                        {
                            if (!tb.InScope("name"))
                            {
                                if (!tb.InScope(name))
                                {
                                    tb.Error(this);
                                    return false;
                                }
                                tb.GenerateImpliedEndTags();
                                if (!tb.CurrentElement().NodeName.Equals(name))
                                {
                                    tb.Error(this);
                                }
                                tb.PopStackToClose(name);
                                tb.ClearFormattingElementsToLastMarker();
                            }
                        }
                        else if (name.Equals("br"))
                        {
                            tb.Error(this);
                            tb.Process(new Token.StartTag("br"));
                            return false;
                        }
                        else
                        {
                            return this.AnyOtherEndTag(t, tb);
                        }
                        break;
                    }

                    case TokenType.EOF:
                    {
                        // todo: error if stack contains something not dd, dt, li, p, tbody, td, tfoot, th, thead, tr, body, html
                        // stop parsing
                        break;
                    }
                }
                return true;
            }

            internal bool AnyOtherEndTag(Token t, HtmlTreeBuilder tb)
            {
                string name = t.AsEndTag().Name();
                DescendableLinkedList<Element> stack = tb.GetStack();
                var it = stack.GetDescendingEnumerator();
                while (it.MoveNext())
                {
                    Element node = it.Current;
                    if (node.NodeName.Equals(name))
                    {
                        tb.GenerateImpliedEndTags(name);
                        if (!name.Equals(tb.CurrentElement().NodeName))
                        {
                            tb.Error(this);
                        }
                        tb.PopStackToClose(name);
                        break;
                    }
                    else
                    {
                        if (tb.IsSpecial(node))
                        {
                            tb.Error(this);
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState InBody = new _HtmlTreeBuilderState_249("InBody");

        private sealed class _HtmlTreeBuilderState_786 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_786(string baseArg1) : base(baseArg1)
            {
            }

            // in script, style etc. normally treated as data tags
            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsCharacter())
                {
                    tb.Insert(t.AsCharacter());
                }
                else if (t.IsEOF())
                {
                    tb.Error(this);
                    // if current node is script: already started
                    tb.Pop();
                    tb.Transition(tb.OriginalState());
                    return tb.Process(t);
                }
                else if (t.IsEndTag())
                {
                    // if: An end tag whose tag name is "script" -- scripting nesting level, if evaluating scripts
                    tb.Pop();
                    tb.Transition(tb.OriginalState());
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState Text = new _HtmlTreeBuilderState_786("Text");

        private sealed class _HtmlTreeBuilderState_805 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_805(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsCharacter())
                {
                    tb.NewPendingTableCharacters();
                    tb.MarkInsertionMode();
                    tb.Transition(HtmlTreeBuilderState.InTableText);
                    return tb.Process(t);
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                    return true;
                }
                else if (t.IsDoctype())
                {
                    tb.Error(this);
                    return false;
                }
                else if (t.IsStartTag())
                {
                    Token.StartTag startTag = t.AsStartTag();
                    string name = startTag.Name();
                    if (name.Equals("caption"))
                    {
                        tb.ClearStackToTableContext();
                        tb.InsertMarkerToFormattingElements();
                        tb.Insert(startTag);
                        tb.Transition(HtmlTreeBuilderState.InCaption);
                    }
                    else if (name.Equals("colgroup"))
                    {
                        tb.ClearStackToTableContext();
                        tb.Insert(startTag);
                        tb.Transition(HtmlTreeBuilderState.InColumnGroup);
                    }
                    else if (name.Equals("col"))
                    {
                        tb.Process(new Token.StartTag("colgroup"));
                        return tb.Process(t);
                    }
                    else if (StringUtil.In(name, "tbody", "tfoot", "thead"))
                    {
                        tb.ClearStackToTableContext();
                        tb.Insert(startTag);
                        tb.Transition(HtmlTreeBuilderState.InTableBody);
                    }
                    else if (StringUtil.In(name, "td", "th", "tr"))
                    {
                        tb.Process(new Token.StartTag("tbody"));
                        return tb.Process(t);
                    }
                    else if (name.Equals("table"))
                    {
                        tb.Error(this);
                        bool processed = tb.Process(new Token.EndTag("table"));
                        if (processed)
                        {
                            // only ignored if in fragment
                            return tb.Process(t);
                        }
                    }
                    else if (StringUtil.In(name, "style", "script"))
                    {
                        return tb.Process(t, HtmlTreeBuilderState.InHead);
                    }
                    else if (name.Equals("input"))
                    {
                        if (!string.Equals(startTag.attributes["type"], "hidden", StringComparison.OrdinalIgnoreCase))
                        {
                            return this.AnythingElse(t, tb);
                        }
                        else
                        {
                            tb.InsertEmpty(startTag);
                        }
                    }
                    else if (name.Equals("form"))
                    {
                        tb.Error(this);
                        if (tb.GetFormElement() != null)
                        {
                            return false;
                        }
                        else
                        {
                            tb.InsertForm(startTag, false);
                        }
                    }
                    else
                    {
                        return this.AnythingElse(t, tb);
                    }
                    return true;
                    // todo: check if should return processed http://www.whatwg.org/specs/web-apps/current-work/multipage/tree-construction.html#parsing-main-intable
                }
                else if (t.IsEndTag())
                {
                    Token.EndTag endTag = t.AsEndTag();
                    string name = endTag.Name();
                    if (name.Equals("table"))
                    {
                        if (!tb.InTableScope(name))
                        {
                            tb.Error(this);
                            return false;
                        }
                        else
                        {
                            tb.PopStackToClose("table");
                        }
                        tb.ResetInsertionMode();
                    }
                    else if (StringUtil.In(name, "body", "caption", "col", "colgroup", "html", "tbody", "td", "tfoot", "th", "thead", "tr"))
                    {
                        tb.Error(this);
                        return false;
                    }
                    else
                    {
                        return this.AnythingElse(t, tb);
                    }
                    return true;
                    // todo: as above todo
                }
                else if (t.IsEOF())
                {
                    if (tb.CurrentElement().NodeName.Equals("html"))
                    {
                        tb.Error(this);
                    }
                    return true;
                }
                // stops parsing
                return this.AnythingElse(t, tb);
            }

            internal bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                tb.Error(this);
                bool processed = true;
                if (StringUtil.In(tb.CurrentElement().NodeName, "table", "tbody", "tfoot", "thead"
                    , "tr"))
                {
                    tb.SetFosterInserts(true);
                    processed = tb.Process(t, HtmlTreeBuilderState.InBody);
                    tb.SetFosterInserts(false);
                }
                else
                {
                    processed = tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                return processed;
            }
        }

        public static readonly HtmlTreeBuilderState InTable = new _HtmlTreeBuilderState_805("InTable");

        private sealed class _HtmlTreeBuilderState_905 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_905(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                switch (t.type)
                {
                    case TokenType.Character:
                    {
                        Token.Character c = t.AsCharacter();
                        if (c.GetData().Equals(HtmlTreeBuilderState.nullString))
                        {
                            tb.Error(this);
                            return false;
                        }
                        else
                        {
                            tb.GetPendingTableCharacters().Add(c);
                        }
                        break;
                    }

                    default:
                    {
                        if (tb.GetPendingTableCharacters().Count > 0)
                        {
                            foreach (Token.Character character in tb.GetPendingTableCharacters())
                            {
                                if (!HtmlTreeBuilderState.IsWhitespace(character))
                                {
                                    // InTable anything else section:
                                    tb.Error(this);
                                    if (StringUtil.In(tb.CurrentElement().NodeName, "table", "tbody", "tfoot", "thead", "tr"))
                                    {
                                        tb.SetFosterInserts(true);
                                        tb.Process(character, HtmlTreeBuilderState.InBody);
                                        tb.SetFosterInserts(false);
                                    }
                                    else
                                    {
                                        tb.Process(character, HtmlTreeBuilderState.InBody);
                                    }
                                }
                                else
                                {
                                    tb.Insert(character);
                                }
                            }
                            tb.NewPendingTableCharacters();
                        }
                        tb.Transition(tb.OriginalState());
                        return tb.Process(t);
                    }
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState InTableText = new _HtmlTreeBuilderState_905("InTableText");

        private sealed class _HtmlTreeBuilderState_941 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_941(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsEndTag() && t.AsEndTag().Name().Equals("caption"))
                {
                    Token.EndTag endTag = t.AsEndTag();
                    string name = endTag.Name();
                    if (!tb.InTableScope(name))
                    {
                        tb.Error(this);
                        return false;
                    }
                    else
                    {
                        tb.GenerateImpliedEndTags();
                        if (!tb.CurrentElement().NodeName.Equals("caption"))
                        {
                            tb.Error(this);
                        }
                        tb.PopStackToClose("caption");
                        tb.ClearFormattingElementsToLastMarker();
                        tb.Transition(HtmlTreeBuilderState.InTable);
                    }
                }
                else if ((t.IsStartTag() && StringUtil.In(t.AsStartTag().Name(), "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr")
                    || t.IsEndTag() && t.AsEndTag().Name().Equals("table")))
                {
                    tb.Error(this);
                    bool processed = tb.Process(new Token.EndTag("caption"));
                    if (processed)
                    {
                        return tb.Process(t);
                    }
                }
                else if (t.IsEndTag() && StringUtil.In(t.AsEndTag().Name(), "body", "col", "colgroup", "html", "tbody", "td", "tfoot", "th", "thead", "tr"))
                {
                    tb.Error(this);
                    return false;
                }
                else
                {
                    return tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState InCaption = new _HtmlTreeBuilderState_941("InCaption");

        private sealed class _HtmlTreeBuilderState_976 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_976(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    tb.Insert(t.AsCharacter());
                    return true;
                }
                switch (t.type)
                {
                    case TokenType.Comment:
                    {
                        tb.Insert(t.AsComment());
                        break;
                    }

                    case TokenType.Doctype:
                    {
                        tb.Error(this);
                        break;
                    }

                    case TokenType.StartTag:
                    {
                        Token.StartTag startTag = t.AsStartTag();
                        string name = startTag.Name();
                        if (name.Equals("html"))
                        {
                            return tb.Process(t, HtmlTreeBuilderState.InBody);
                        }
                        else if (name.Equals("col"))
                        {
                            tb.InsertEmpty(startTag);
                        }
                        else
                        {
                            return this.AnythingElse(t, tb);
                        }
                        break;
                    }

                    case TokenType.EndTag:
                    {
                        Token.EndTag endTag = t.AsEndTag();
                        string name = endTag.Name();
                        if (name.Equals("colgroup"))
                        {
                            if (tb.CurrentElement().NodeName.Equals("html"))
                            {
                                // frag case
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.Pop();
                                tb.Transition(HtmlTreeBuilderState.InTable);
                            }
                        }
                        else
                        {
                            return this.AnythingElse(t, tb);
                        }
                        break;
                    }

                    case TokenType.EOF:
                    {
                        if (tb.CurrentElement().NodeName.Equals("html"))
                        {
                            return true;
                        }
                        else
                        {
                            // stop parsing; frag case
                            return this.AnythingElse(t, tb);
                        }
                    }

                    default:
                    {
                        return this.AnythingElse(t, tb);
                    }
                }
                return true;
            }

            private bool AnythingElse(Token t, TreeBuilder tb)
            {
                bool processed = tb.Process(new Token.EndTag("colgroup"));
                if (processed)
                {
                    // only ignored in frag case
                    return tb.Process(t);
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState InColumnGroup = new _HtmlTreeBuilderState_976("InColumnGroup");

        private sealed class _HtmlTreeBuilderState_1031 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1031(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                switch (t.type)
                {
                    case TokenType.StartTag:
                    {
                        Token.StartTag startTag = t.AsStartTag();
                        string name = startTag.Name();
                        if (name.Equals("tr"))
                        {
                            tb.ClearStackToTableBodyContext();
                            tb.Insert(startTag);
                            tb.Transition(HtmlTreeBuilderState.InRow);
                        }
                        else if (StringUtil.In(name, "th", "td"))
                        {
                            tb.Error(this);
                            tb.Process(new Token.StartTag("tr"));
                            return tb.Process(startTag);
                        }
                        else if (StringUtil.In(name, "caption", "col", "colgroup", "tbody", "tfoot", "thead"))
                        {
                            return this.ExitTableBody(t, tb);
                        }
                        else
                        {
                            return this.AnythingElse(t, tb);
                        }
                        break;
                    }

                    case TokenType.EndTag:
                    {
                        Token.EndTag endTag = t.AsEndTag();
                        string name = endTag.Name();
                        if (StringUtil.In(name, "tbody", "tfoot", "thead"))
                        {
                            if (!tb.InTableScope(name))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.ClearStackToTableBodyContext();
                                tb.Pop();
                                tb.Transition(HtmlTreeBuilderState.InTable);
                            }
                        }
                        else
                        {
                            if (name.Equals("table"))
                            {
                                return this.ExitTableBody(t, tb);
                            }
                            else if (StringUtil.In(name, "body", "caption", "col", "colgroup", "html", "td", "th", "tr"))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                return this.AnythingElse(t, tb);
                            }
                        }
                        break;
                    }

                    default:
                    {
                        return this.AnythingElse(t, tb);
                    }
                }
                return true;
            }

            private bool ExitTableBody(Token t, HtmlTreeBuilder tb)
            {
                if (!(tb.InTableScope("tbody") || tb.InTableScope("thead") || tb.InScope("tfoot")
                    ))
                {
                    // frag case
                    tb.Error(this);
                    return false;
                }
                tb.ClearStackToTableBodyContext();
                tb.Process(new Token.EndTag(tb.CurrentElement().NodeName));
                // tbody, tfoot, thead
                return tb.Process(t);
            }

            private bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                return tb.Process(t, HtmlTreeBuilderState.InTable);
            }
        }

        public static readonly HtmlTreeBuilderState InTableBody = new _HtmlTreeBuilderState_1031("InTableBody");

        private sealed class _HtmlTreeBuilderState_1091 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1091(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsStartTag())
                {
                    Token.StartTag startTag = t.AsStartTag();
                    string name = startTag.Name();
                    if (StringUtil.In(name, "th", "td"))
                    {
                        tb.ClearStackToTableRowContext();
                        tb.Insert(startTag);
                        tb.Transition(HtmlTreeBuilderState.InCell);
                        tb.InsertMarkerToFormattingElements();
                    }
                    else if (StringUtil.In(name, "caption", "col", "colgroup", "tbody", "tfoot", "thead", "tr"))
                    {
                        return this.HandleMissingTr(t, tb);
                    }
                    else
                    {
                        return this.AnythingElse(t, tb);
                    }
                }
                else if (t.IsEndTag())
                {
                    Token.EndTag endTag = t.AsEndTag();
                    string name = endTag.Name();
                    if (name.Equals("tr"))
                    {
                        if (!tb.InTableScope(name))
                        {
                            tb.Error(this);
                            // frag
                            return false;
                        }
                        tb.ClearStackToTableRowContext();
                        tb.Pop();
                        // tr
                        tb.Transition(HtmlTreeBuilderState.InTableBody);
                    }
                    else if (name.Equals("table"))
                    {
                        return this.HandleMissingTr(t, tb);
                    }
                    else if (StringUtil.In(name, "tbody", "tfoot", "thead"))
                    {
                        if (!tb.InTableScope(name))
                        {
                            tb.Error(this);
                            return false;
                        }
                        tb.Process(new Token.EndTag("tr"));
                        return tb.Process(t);
                    }
                    else if (StringUtil.In(name, "body", "caption", "col", "colgroup", "html", "td", "th"))
                    {
                        tb.Error(this);
                        return false;
                    }
                    else
                    {
                        return this.AnythingElse(t, tb);
                    }
                }
                else
                {
                    return this.AnythingElse(t, tb);
                }
                return true;
            }

            private bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                return tb.Process(t, HtmlTreeBuilderState.InTable);
            }

            private bool HandleMissingTr(Token t, TreeBuilder tb)
            {
                bool processed = tb.Process(new Token.EndTag("tr"));
                if (processed)
                {
                    return tb.Process(t);
                }
                else
                {
                    return false;
                }
            }
        }

        public static readonly HtmlTreeBuilderState InRow = new _HtmlTreeBuilderState_1091("InRow");

        private sealed class _HtmlTreeBuilderState_1152 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1152(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsEndTag())
                {
                    Token.EndTag endTag = t.AsEndTag();
                    string name = endTag.Name();
                    if (StringUtil.In(name, "td", "th"))
                    {
                        if (!tb.InTableScope(name))
                        {
                            tb.Error(this);
                            tb.Transition(HtmlTreeBuilderState.InRow);
                            // might not be in scope if empty: <td /> and processing fake end tag
                            return false;
                        }
                        tb.GenerateImpliedEndTags();
                        if (!tb.CurrentElement().NodeName.Equals(name))
                        {
                            tb.Error(this);
                        }
                        tb.PopStackToClose(name);
                        tb.ClearFormattingElementsToLastMarker();
                        tb.Transition(HtmlTreeBuilderState.InRow);
                    }
                    else if (StringUtil.In(name, "body", "caption", "col", "colgroup", "html"))
                    {
                        tb.Error(this);
                        return false;
                    }
                    else if (StringUtil.In(name, "table", "tbody", "tfoot", "thead", "tr"))
                    {
                        if (!tb.InTableScope(name))
                        {
                            tb.Error(this);
                            return false;
                        }
                        this.CloseCell(tb);
                        return tb.Process(t);
                    }
                    else
                    {
                        return this.AnythingElse(t, tb);
                    }
                }
                else if (t.IsStartTag() && StringUtil.In(t.AsStartTag().Name(), "caption", "col", "colgroup", "tbody", "td", "tfoot", "th", "thead", "tr"))
                {
                    if (!(tb.InTableScope("td") || tb.InTableScope("th")))
                    {
                        tb.Error(this);
                        return false;
                    }
                    this.CloseCell(tb);
                    return tb.Process(t);
                }
                else
                {
                    return this.AnythingElse(t, tb);
                }
                return true;
            }

            private bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                return tb.Process(t, HtmlTreeBuilderState.InBody);
            }

            private void CloseCell(HtmlTreeBuilder tb)
            {
                if (tb.InTableScope("td"))
                {
                    tb.Process(new Token.EndTag("td"));
                }
                else
                {
                    tb.Process(new Token.EndTag("th"));
                }
            }
        }

        public static readonly HtmlTreeBuilderState InCell = new _HtmlTreeBuilderState_1152("InCell");

        private sealed class _HtmlTreeBuilderState_1209 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1209(string baseArg1) : base(baseArg1)
            {
            }

            // only here if th or td in scope
            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                switch (t.type)
                {
                    case TokenType.Character:
                    {
                        Token.Character c = t.AsCharacter();
                        if (c.GetData().Equals(HtmlTreeBuilderState.nullString))
                        {
                            tb.Error(this);
                            return false;
                        }
                        else
                        {
                            tb.Insert(c);
                        }
                        break;
                    }

                    case TokenType.Comment:
                    {
                        tb.Insert(t.AsComment());
                        break;
                    }

                    case TokenType.Doctype:
                    {
                        tb.Error(this);
                        return false;
                    }

                    case TokenType.StartTag:
                    {
                        Token.StartTag start = t.AsStartTag();
                        string name = start.Name();
                        if (name.Equals("html"))
                        {
                            return tb.Process(start, HtmlTreeBuilderState.InBody);
                        }
                        else if (name.Equals("option"))
                        {
                            tb.Process(new Token.EndTag("option"));
                            tb.Insert(start);
                        }
                        else if (name.Equals("optgroup"))
                        {
                            if (tb.CurrentElement().NodeName.Equals("option"))
                            {
                                tb.Process(new Token.EndTag("option"));
                            }
                            else if (tb.CurrentElement().NodeName.Equals("optgroup"))
                            {
                                tb.Process(new Token.EndTag("optgroup"));
                            }
                            tb.Insert(start);
                        }
                        else if (name.Equals("select"))
                        {
                            tb.Error(this);
                            return tb.Process(new Token.EndTag("select"));
                        }
                        else if (StringUtil.In(name, "input", "keygen", "textarea"))
                        {
                            tb.Error(this);
                            if (!tb.InSelectScope("select"))
                            {
                                return false;
                            }
                            // frag
                            tb.Process(new Token.EndTag("select"));
                            return tb.Process(start);
                        }
                        else if (name.Equals("script"))
                        {
                            return tb.Process(t, HtmlTreeBuilderState.InHead);
                        }
                        else
                        {
                            return this.AnythingElse(t, tb);
                        }
                        break;
                    }

                    case TokenType.EndTag:
                    {
                        Token.EndTag end = t.AsEndTag();
                        string name = end.Name();
                        if (name.Equals("optgroup"))
                        {
                            if (tb.CurrentElement().NodeName.Equals("option")
                                && tb.AboveOnStack(tb.CurrentElement()) != null
                                && tb.AboveOnStack(tb.CurrentElement()).NodeName.Equals("optgroup"))
                            {
                                tb.Process(new Token.EndTag("option"));
                            }
                            if (tb.CurrentElement().NodeName.Equals("optgroup"))
                            {
                                tb.Pop();
                            }
                            else
                            {
                                tb.Error(this);
                            }
                        }
                        else if (name.Equals("option"))
                        {
                            if (tb.CurrentElement().NodeName.Equals("option"))
                            {
                                tb.Pop();
                            }
                            else
                            {
                                tb.Error(this);
                            }
                        }
                        else if (name.Equals("select"))
                        {
                            if (!tb.InSelectScope(name))
                            {
                                tb.Error(this);
                                return false;
                            }
                            else
                            {
                                tb.PopStackToClose(name);
                                tb.ResetInsertionMode();
                            }
                        }
                        else
                        {
                            return this.AnythingElse(t, tb);
                        }
                        break;
                    }

                    case TokenType.EOF:
                    {
                        if (!tb.CurrentElement().NodeName.Equals("html"))
                        {
                            tb.Error(this);
                        }
                        break;
                    }

                    default:
                    {
                        return this.AnythingElse(t, tb);
                    }
                }
                return true;
            }

            private bool AnythingElse(Token t, HtmlTreeBuilder tb)
            {
                tb.Error(this);
                return false;
            }
        }

        public static readonly HtmlTreeBuilderState InSelect = new _HtmlTreeBuilderState_1209("InSelect");

        private sealed class _HtmlTreeBuilderState_1297 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1297(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsStartTag() && StringUtil.In(t.AsStartTag().Name(), "caption", "table", "tbody", "tfoot", "thead", "tr", "td", "th"))
                {
                    tb.Error(this);
                    tb.Process(new Token.EndTag("select"));
                    return tb.Process(t);
                }
                else if (t.IsEndTag() && StringUtil.In(t.AsEndTag().Name(), "caption", "table", "tbody", "tfoot", "thead", "tr", "td", "th"))
                {
                    tb.Error(this);
                    if (tb.InTableScope(t.AsEndTag().Name()))
                    {
                        tb.Process(new Token.EndTag("select"));
                        return (tb.Process(t));
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return tb.Process(t, HtmlTreeBuilderState.InSelect);
                }
            }
        }

        public static readonly HtmlTreeBuilderState InSelectInTable = new _HtmlTreeBuilderState_1297("InSelectInTable");

        private sealed class _HtmlTreeBuilderState_1315 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1315(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                    // into html node
                }
                else if (t.IsDoctype())
                {
                    tb.Error(this);
                    return false;
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("html"))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                else if (t.IsEndTag() && t.AsEndTag().Name().Equals("html"))
                {
                    if (tb.IsFragmentParsing())
                    {
                        tb.Error(this);
                        return false;
                    }
                    else
                    {
                        tb.Transition(HtmlTreeBuilderState.AfterAfterBody);
                    }
                }
                else if (t.IsEOF())
                {
                    // chillax! we're done
                }
                else
                {
                    tb.Error(this);
                    tb.Transition(HtmlTreeBuilderState.InBody);
                    return tb.Process(t);
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState AfterBody = new _HtmlTreeBuilderState_1315("AfterBody");

        private sealed class _HtmlTreeBuilderState_1343 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1343(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    tb.Insert(t.AsCharacter());
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (t.IsDoctype())
                {
                    tb.Error(this);
                    return false;
                }
                else if (t.IsStartTag())
                {
                    Token.StartTag start = t.AsStartTag();
                    string name = start.Name();
                    if (name.Equals("html"))
                    {
                        return tb.Process(start, HtmlTreeBuilderState.InBody);
                    }
                    else if (name.Equals("frameset"))
                    {
                        tb.Insert(start);
                    }
                    else if (name.Equals("frame"))
                    {
                        tb.InsertEmpty(start);
                    }
                    else if (name.Equals("noframes"))
                    {
                        return tb.Process(start, HtmlTreeBuilderState.InHead);
                    }
                    else
                    {
                        tb.Error(this);
                        return false;
                    }
                }
                else if (t.IsEndTag() && t.AsEndTag().Name().Equals("frameset"))
                {
                    if (tb.CurrentElement().NodeName.Equals("html"))
                    {
                        // frag
                        tb.Error(this);
                        return false;
                    }
                    else
                    {
                        tb.Pop();
                        if (!tb.IsFragmentParsing() && !tb.CurrentElement().NodeName.Equals("frameset"))
                        {
                            tb.Transition(HtmlTreeBuilderState.AfterFrameset);
                        }
                    }
                }
                else if (t.IsEOF())
                {
                    if (!tb.CurrentElement().NodeName.Equals("html"))
                    {
                        tb.Error(this);
                        return true;
                    }
                }
                else
                {
                    tb.Error(this);
                    return false;
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState InFrameset = new _HtmlTreeBuilderState_1343("InFrameset");

        private sealed class _HtmlTreeBuilderState_1389 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1389(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (HtmlTreeBuilderState.IsWhitespace(t))
                {
                    tb.Insert(t.AsCharacter());
                }
                else if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (t.IsDoctype())
                {
                    tb.Error(this);
                    return false;
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("html"))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                else if (t.IsEndTag() && t.AsEndTag().Name().Equals("html"))
                {
                    tb.Transition(HtmlTreeBuilderState.AfterAfterFrameset);
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("noframes"))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InHead);
                }
                else if (t.IsEOF())
                {
                }
                else
                {
                    // cool your heels, we're complete
                    tb.Error(this);
                    return false;
                } return true;
            }
        }

        public static readonly HtmlTreeBuilderState AfterFrameset = new _HtmlTreeBuilderState_1389("AfterFrameset");

        private sealed class _HtmlTreeBuilderState_1413 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1413(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (t.IsDoctype()
                    || HtmlTreeBuilderState.IsWhitespace(t)
                    || (t.IsStartTag() && t.AsStartTag().Name().Equals("html")))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                else if (t.IsEOF())
                {
                    // nice work chuck
                }
                else
                {
                    tb.Error(this);
                    tb.Transition(HtmlTreeBuilderState.InBody);
                    return tb.Process(t);
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState AfterAfterBody = new _HtmlTreeBuilderState_1413("AfterAfterBody");

        private sealed class _HtmlTreeBuilderState_1429 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1429(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                if (t.IsComment())
                {
                    tb.Insert(t.AsComment());
                }
                else if (t.IsDoctype()
                    || HtmlTreeBuilderState.IsWhitespace(t)
                    || (t.IsStartTag() && t.AsStartTag().Name().Equals("html")))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InBody);
                }
                else if (t.IsEOF())
                {
                    // nice work chuck
                }
                else if (t.IsStartTag() && t.AsStartTag().Name().Equals("noframes"))
                {
                    return tb.Process(t, HtmlTreeBuilderState.InHead);
                }
                else
                {
                    tb.Error(this);
                    return false;
                }
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState AfterAfterFrameset = new _HtmlTreeBuilderState_1429("AfterAfterFrameset");

        private sealed class _HtmlTreeBuilderState_1446 : HtmlTreeBuilderState
        {
            public _HtmlTreeBuilderState_1446(string baseArg1) : base(baseArg1)
            {
            }

            internal override bool Process(Token t, HtmlTreeBuilder tb)
            {
                return true;
            }
        }

        public static readonly HtmlTreeBuilderState ForeignContent = new _HtmlTreeBuilderState_1446("ForeignContent");

        private readonly string name;

        public HtmlTreeBuilderState(string name)
        {
            // todo: implement. Also; how do we get here?
            this.name = name;
        }

        public string Name()
        {
            return this.name;
        }

        private static string nullString = '\u0000'.ToString();

        internal abstract bool Process(Token t, HtmlTreeBuilder tb);

        private static bool IsWhitespace(Token t)
        {
            if (t.IsCharacter())
            {
                string data = t.AsCharacter().GetData();
                // todo: this checks more than spec - "\t", "\n", "\f", "\r", " "
                for (int i = 0; i < data.Length; i++)
                {
                    char c = data[i];
                    if (!StringUtil.IsWhitespace(c))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static void HandleRcData(Token.StartTag startTag, HtmlTreeBuilder tb)
        {
            tb.Insert(startTag);
            tb.tokeniser.Transition(TokeniserState.Rcdata);
            tb.MarkInsertionMode();
            tb.Transition(Text);
        }

        private static void HandleRawtext(Token.StartTag startTag, HtmlTreeBuilder tb)
        {
            tb.Insert(startTag);
            tb.tokeniser.Transition(TokeniserState.Rawtext);
            tb.MarkInsertionMode();
            tb.Transition(Text);
        }

        private sealed class Constants
        {
            internal static readonly string[] InBodyStartToHead = new string[] { "base", "basefont"
                , "bgsound", "command", "link", "meta", "noframes", "script", "style", "title" };

            internal static readonly string[] InBodyStartPClosers = new string[] { "address", 
                "article", "aside", "blockquote", "center", "details", "dir", "div", "dl", "fieldset"
                , "figcaption", "figure", "footer", "header", "hgroup", "menu", "nav", "ol", "p"
                , "section", "summary", "ul" };

            internal static readonly string[] Headings = new string[] { "h1", "h2", "h3", "h4"
                , "h5", "h6" };

            internal static readonly string[] InBodyStartPreListing = new string[] { "pre", "listing"
                 };

            internal static readonly string[] InBodyStartLiBreakers = new string[] { "address"
                , "div", "p" };

            internal static readonly string[] DdDt = new string[] { "dd", "dt" };

            internal static readonly string[] Formatters = new string[] { "b", "big", "code", 
                "em", "font", "i", "s", "small", "strike", "strong", "tt", "u" };

            internal static readonly string[] InBodyStartApplets = new string[] { "applet", "marquee"
                , "object" };

            internal static readonly string[] InBodyStartEmptyFormatters = new string[] { "area"
                , "br", "embed", "img", "keygen", "wbr" };

            internal static readonly string[] InBodyStartMedia = new string[] { "param", "source"
                , "track" };

            internal static readonly string[] InBodyStartInputAttribs = new string[] { "name", 
                "action", "prompt" };

            internal static readonly string[] InBodyStartOptions = new string[] { "optgroup", 
                "option" };

            internal static readonly string[] InBodyStartRuby = new string[] { "rp", "rt" };

            internal static readonly string[] InBodyStartDrop = new string[] { "caption", "col"
                , "colgroup", "frame", "head", "tbody", "td", "tfoot", "th", "thead", "tr" };

            internal static readonly string[] InBodyEndClosers = new string[] { "address", "article"
                , "aside", "blockquote", "button", "center", "details", "dir", "div", "dl", "fieldset"
                , "figcaption", "figure", "footer", "header", "hgroup", "listing", "menu", "nav"
                , "ol", "pre", "section", "summary", "ul" };

            internal static readonly string[] InBodyEndAdoptionFormatters = new string[] { "a"
                , "b", "big", "code", "em", "font", "i", "nobr", "s", "small", "strike", "strong"
                , "tt", "u" };

            internal static readonly string[] InBodyEndTableFosters = new string[] { "table", 
                "tbody", "tfoot", "thead", "tr" };
            // lists of tags to search through. A little harder to read here, but causes less GC than dynamic varargs.
            // was contributing around 10% of parse GC load.
        }
    }
}
