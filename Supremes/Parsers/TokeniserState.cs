namespace Supremes.Parsers
{
    /// <summary>
    /// States and transition activations for the Tokeniser.
    /// </summary>
    internal abstract class TokeniserState
    {
        private sealed class _TokeniserState_7 : TokeniserState
        {
            public _TokeniserState_7(string baseArg1)
                : base(baseArg1)
            {
            }

            // in data state, gather characters until a character reference or tag is found
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                switch (r.Current())
                {
                    case '&':
                        t.AdvanceTransition(TokeniserState.CharacterReferenceInData);
                        break;

                    case '<':
                        t.AdvanceTransition(TokeniserState.TagOpen);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        // NOT replacement character (oddly?)
                        t.Emit(r.Consume());
                        break;

                    case TokeniserState.eof:
                        t.Emit(new Token.EOF());
                        break;

                    default:
                        string data = r.ConsumeToAny('&', '<', TokeniserState.nullChar);
                        t.Emit(data);
                        break;
                }
            }
        }

        public static readonly TokeniserState Data = new _TokeniserState_7("Data");

        private sealed class _TokeniserState_31 : TokeniserState
        {
            public _TokeniserState_31(string baseArg1)
                : base(baseArg1)
            {
            }

            // from & in data
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char[] c = t.ConsumeCharacterReference(null, false);
                if (c == null)
                {
                    t.Emit('&');
                }
                else
                {
                    t.Emit(c);
                }
                t.Transition(TokeniserState.Data);
            }
        }

        public static readonly TokeniserState CharacterReferenceInData = new _TokeniserState_31("CharacterReferenceInData");

        private sealed class _TokeniserState_42 : TokeniserState
        {
            public _TokeniserState_42(string baseArg1)
                : base(baseArg1)
            {
            }

            /// handles data in title, textarea etc
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                switch (r.Current())
                {
                    case '&':
                        t.AdvanceTransition(TokeniserState.CharacterReferenceInRcdata);
                        break;

                    case '<':
                        t.AdvanceTransition(TokeniserState.RcdataLessthanSign);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        r.Advance();
                        t.Emit(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.Emit(new Token.EOF());
                        break;

                    default:
                        string data = r.ConsumeToAny('&', '<', TokeniserState.nullChar);
                        t.Emit(data);
                        break;
                }
            }
        }

        public static readonly TokeniserState Rcdata = new _TokeniserState_42("Rcdata");

        private sealed class _TokeniserState_67 : TokeniserState
        {
            public _TokeniserState_67(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char[] c = t.ConsumeCharacterReference(null, false);
                if (c == null)
                {
                    t.Emit('&');
                }
                else
                {
                    t.Emit(c);
                }
                t.Transition(TokeniserState.Rcdata);
            }
        }

        public static readonly TokeniserState CharacterReferenceInRcdata = new _TokeniserState_67("CharacterReferenceInRcdata");

        private sealed class _TokeniserState_77 : TokeniserState
        {
            public _TokeniserState_77(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                switch (r.Current())
                {
                    case '<':
                        t.AdvanceTransition(TokeniserState.RawtextLessthanSign);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        r.Advance();
                        t.Emit(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.Emit(new Token.EOF());
                        break;

                    default:
                        string data = r.ConsumeToAny('<', TokeniserState.nullChar);
                        t.Emit(data);
                        break;
                }
            }
        }

        public static readonly TokeniserState Rawtext = new _TokeniserState_77("Rawtext");

        private sealed class _TokeniserState_98 : TokeniserState
        {
            public _TokeniserState_98(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                switch (r.Current())
                {
                    case '<':
                        t.AdvanceTransition(TokeniserState.ScriptDataLessthanSign);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        r.Advance();
                        t.Emit(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.Emit(new Token.EOF());
                        break;

                    default:
                        string data = r.ConsumeToAny('<', TokeniserState.nullChar);
                        t.Emit(data);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptData = new _TokeniserState_98("ScriptData");

        private sealed class _TokeniserState_119 : TokeniserState
        {
            public _TokeniserState_119(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                switch (r.Current())
                {
                    case TokeniserState.nullChar:
                        t.Error(this);
                        r.Advance();
                        t.Emit(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.Emit(new Token.EOF());
                        break;

                    default:
                        string data = r.ConsumeTo(TokeniserState.nullChar);
                        t.Emit(data);
                        break;
                }
            }
        }

        public static readonly TokeniserState PLAINTEXT = new _TokeniserState_119("PLAINTEXT");

        private sealed class _TokeniserState_137 : TokeniserState
        {
            public _TokeniserState_137(string baseArg1)
                : base(baseArg1)
            {
            }

            // from < in data
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                switch (r.Current())
                {
                    case '!':
                        t.AdvanceTransition(TokeniserState.MarkupDeclarationOpen);
                        break;

                    case '/':
                        t.AdvanceTransition(TokeniserState.EndTagOpen);
                        break;

                    case '?':
                        t.AdvanceTransition(TokeniserState.BogusComment);
                        break;

                    default:
                        if (r.MatchesLetter())
                        {
                            t.CreateTagPending(true);
                            t.Transition(TokeniserState.TagName);
                        }
                        else
                        {
                            t.Error(this);
                            t.Emit('<');
                            // char that got us here
                            t.Transition(TokeniserState.Data);
                        }
                        break;
                }
            }
        }

        public static readonly TokeniserState TagOpen = new _TokeniserState_137("TagOpen");

        private sealed class _TokeniserState_163 : TokeniserState
        {
            public _TokeniserState_163(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.IsEmpty())
                {
                    t.EofError(this);
                    t.Emit("</");
                    t.Transition(TokeniserState.Data);
                }
                else if (r.MatchesLetter())
                {
                    t.CreateTagPending(false);
                    t.Transition(TokeniserState.TagName);
                }
                else if (r.Matches('>'))
                {
                    t.Error(this);
                    t.AdvanceTransition(TokeniserState.Data);
                }
                else
                {
                    t.Error(this);
                    t.AdvanceTransition(TokeniserState.BogusComment);
                }
            }
        }

        public static readonly TokeniserState EndTagOpen = new _TokeniserState_163("EndTagOpen");

        private sealed class _TokeniserState_181 : TokeniserState
        {
            public _TokeniserState_181(string baseArg1)
                : base(baseArg1)
            {
            }

            // from < or </ in data, will have start or end tag pending
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                // previous TagOpen state did NOT consume, will have a letter char in current
                string tagName = r.ConsumeToAny('\t', '\n', '\r', '\f', ' ', '/', '>', TokeniserState.nullChar).ToLower();
                t.tagPending.AppendTagName(tagName);
                switch (r.Consume())
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.BeforeAttributeName);
                        break;

                    case '/':
                        t.Transition(TokeniserState.SelfClosingStartTag);
                        break;

                    case '>':
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.nullChar:
                        // replacement
                        t.tagPending.AppendTagName(TokeniserState.replacementStr);
                        break;

                    case TokeniserState.eof:
                        // should emit pending tag?
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;
                }
            }
        }

        public static readonly TokeniserState TagName = new _TokeniserState_181("TagName");

        private sealed class _TokeniserState_213 : TokeniserState
        {
            public _TokeniserState_213(string baseArg1)
                : base(baseArg1)
            {
            }

            // no default, as covered with above consumeToAny
            // from < in rcdata
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.Matches('/'))
                {
                    t.CreateTempBuffer();
                    t.AdvanceTransition(TokeniserState.RCDATAEndTagOpen);
                }
                else if (r.MatchesLetter() && t.AppropriateEndTagName() != null && !r.ContainsIgnoreCase("</" + t.AppropriateEndTagName()))
                {
                    // diverge from spec: got a start tag, but there's no appropriate end tag (</title>), so rather than
                    // consuming to EOF; break out here
                    t.tagPending = new Token.EndTag(t.AppropriateEndTagName());
                    t.EmitTagPending();
                    r.Unconsume();
                    // undo "<"
                    t.Transition(TokeniserState.Data);
                }
                else
                {
                    t.Emit("<");
                    t.Transition(TokeniserState.Rcdata);
                }
            }
        }

        public static readonly TokeniserState RcdataLessthanSign = new _TokeniserState_213("RcdataLessthanSign");

        private sealed class _TokeniserState_232 : TokeniserState
        {
            public _TokeniserState_232(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    t.CreateTagPending(false);
                    t.tagPending.AppendTagName(System.Char.ToLower(r.Current()));
                    t.dataBuffer.Append(System.Char.ToLower(r.Current()));
                    t.AdvanceTransition(TokeniserState.RCDATAEndTagName);
                }
                else
                {
                    t.Emit("</");
                    t.Transition(TokeniserState.Rcdata);
                }
            }
        }

        public static readonly TokeniserState RCDATAEndTagOpen = new _TokeniserState_232("RCDATAEndTagOpen");

        private sealed class _TokeniserState_245 : TokeniserState
        {
            public _TokeniserState_245(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    string name = r.ConsumeLetterSequence();
                    t.tagPending.AppendTagName(name.ToLower());
                    t.dataBuffer.Append(name);
                    return;
                }
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        if (t.IsAppropriateEndTagToken())
                        {
                            t.Transition(TokeniserState.BeforeAttributeName);
                        }
                        else
                        {
                            this.AnythingElse(t, r);
                        }
                        break;

                    case '/':
                        if (t.IsAppropriateEndTagToken())
                        {
                            t.Transition(TokeniserState.SelfClosingStartTag);
                        }
                        else
                        {
                            this.AnythingElse(t, r);
                        }
                        break;

                    case '>':
                        if (t.IsAppropriateEndTagToken())
                        {
                            t.EmitTagPending();
                            t.Transition(TokeniserState.Data);
                        }
                        else
                        {
                            this.AnythingElse(t, r);
                        }
                        break;

                    default:
                        this.AnythingElse(t, r);
                        break;
                }
            }

            private void AnythingElse(Tokeniser t, CharacterReader r)
            {
                t.Emit("</" + t.dataBuffer.ToString());
                r.Unconsume();
                t.Transition(TokeniserState.Rcdata);
            }
        }

        public static readonly TokeniserState RCDATAEndTagName = new _TokeniserState_245("RCDATAEndTagName");

        private sealed class _TokeniserState_291 : TokeniserState
        {
            public _TokeniserState_291(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.Matches('/'))
                {
                    t.CreateTempBuffer();
                    t.AdvanceTransition(TokeniserState.RawtextEndTagOpen);
                }
                else
                {
                    t.Emit('<');
                    t.Transition(TokeniserState.Rawtext);
                }
            }
        }

        public static readonly TokeniserState RawtextLessthanSign = new _TokeniserState_291("RawtextLessthanSign");

        private sealed class _TokeniserState_302 : TokeniserState
        {
            public _TokeniserState_302(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    t.CreateTagPending(false);
                    t.Transition(TokeniserState.RawtextEndTagName);
                }
                else
                {
                    t.Emit("</");
                    t.Transition(TokeniserState.Rawtext);
                }
            }
        }

        public static readonly TokeniserState RawtextEndTagOpen = new _TokeniserState_302("RawtextEndTagOpen");

        private sealed class _TokeniserState_313 : TokeniserState
        {
            public _TokeniserState_313(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                TokeniserState.HandleDataEndTag(t, r, TokeniserState.Rawtext);
            }
        }

        public static readonly TokeniserState RawtextEndTagName = new _TokeniserState_313("RawtextEndTagName");

        private sealed class _TokeniserState_318 : TokeniserState
        {
            public _TokeniserState_318(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                switch (r.Consume())
                {
                    case '/':
                        t.CreateTempBuffer();
                        t.Transition(TokeniserState.ScriptDataEndTagOpen);
                        break;

                    case '!':
                        t.Emit("<!");
                        t.Transition(TokeniserState.ScriptDataEscapeStart);
                        break;

                    default:
                        t.Emit("<");
                        r.Unconsume();
                        t.Transition(TokeniserState.ScriptData);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptDataLessthanSign = new _TokeniserState_318("ScriptDataLessthanSign");

        private sealed class _TokeniserState_336 : TokeniserState
        {
            public _TokeniserState_336(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    t.CreateTagPending(false);
                    t.Transition(TokeniserState.ScriptDataEndTagName);
                }
                else
                {
                    t.Emit("</");
                    t.Transition(TokeniserState.ScriptData);
                }
            }
        }

        public static readonly TokeniserState ScriptDataEndTagOpen = new _TokeniserState_336("ScriptDataEndTagOpen");

        private sealed class _TokeniserState_348 : TokeniserState
        {
            public _TokeniserState_348(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                TokeniserState.HandleDataEndTag(t, r, TokeniserState.ScriptData);
            }
        }

        public static readonly TokeniserState ScriptDataEndTagName = new _TokeniserState_348("ScriptDataEndTagName");

        private sealed class _TokeniserState_353 : TokeniserState
        {
            public _TokeniserState_353(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.Matches('-'))
                {
                    t.Emit('-');
                    t.AdvanceTransition(TokeniserState.ScriptDataEscapeStartDash);
                }
                else
                {
                    t.Transition(TokeniserState.ScriptData);
                }
            }
        }

        public static readonly TokeniserState ScriptDataEscapeStart = new _TokeniserState_353("ScriptDataEscapeStart");

        private sealed class _TokeniserState_363 : TokeniserState
        {
            public _TokeniserState_363(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.Matches('-'))
                {
                    t.Emit('-');
                    t.AdvanceTransition(TokeniserState.ScriptDataEscapedDashDash);
                }
                else
                {
                    t.Transition(TokeniserState.ScriptData);
                }
            }
        }

        public static readonly TokeniserState ScriptDataEscapeStartDash = new _TokeniserState_363("ScriptDataEscapeStartDash");

        private sealed class _TokeniserState_373 : TokeniserState
        {
            public _TokeniserState_373(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.IsEmpty())
                {
                    t.EofError(this);
                    t.Transition(TokeniserState.Data);
                    return;
                }
                switch (r.Current())
                {
                    case '-':
                        t.Emit('-');
                        t.AdvanceTransition(TokeniserState.ScriptDataEscapedDash);
                        break;

                    case '<':
                        t.AdvanceTransition(TokeniserState.ScriptDataEscapedLessthanSign);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        r.Advance();
                        t.Emit(TokeniserState.replacementChar);
                        break;

                    default:
                        string data = r.ConsumeToAny('-', '<', TokeniserState.nullChar);
                        t.Emit(data);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptDataEscaped = new _TokeniserState_373("ScriptDataEscaped");

        private sealed class _TokeniserState_400 : TokeniserState
        {
            public _TokeniserState_400(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.IsEmpty())
                {
                    t.EofError(this);
                    t.Transition(TokeniserState.Data);
                    return;
                }
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataEscapedDashDash);
                        break;

                    case '<':
                        t.Transition(TokeniserState.ScriptDataEscapedLessthanSign);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.Emit(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.ScriptDataEscaped);
                        break;

                    default:
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataEscaped);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptDataEscapedDash = new _TokeniserState_400("ScriptDataEscapedDash");

        private sealed class _TokeniserState_428 : TokeniserState
        {
            public _TokeniserState_428(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.IsEmpty())
                {
                    t.EofError(this);
                    t.Transition(TokeniserState.Data);
                    return;
                }
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.Emit(c);
                        break;

                    case '<':
                        t.Transition(TokeniserState.ScriptDataEscapedLessthanSign);
                        break;

                    case '>':
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptData);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.Emit(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.ScriptDataEscaped);
                        break;

                    default:
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataEscaped);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptDataEscapedDashDash = new _TokeniserState_428("ScriptDataEscapedDashDash");

        private sealed class _TokeniserState_459 : TokeniserState
        {
            public _TokeniserState_459(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    t.CreateTempBuffer();
                    t.dataBuffer.Append(System.Char.ToLower(r.Current()));
                    t.Emit("<" + r.Current());
                    t.AdvanceTransition(TokeniserState.ScriptDataDoubleEscapeStart);
                }
                else if (r.Matches('/'))
                {
                    t.CreateTempBuffer();
                    t.AdvanceTransition(TokeniserState.ScriptDataEscapedEndTagOpen);
                }
                else
                {
                    t.Emit('<');
                    t.Transition(TokeniserState.ScriptDataEscaped);
                }
            }
        }

        public static readonly TokeniserState ScriptDataEscapedLessthanSign = new _TokeniserState_459("ScriptDataEscapedLessthanSign");

        private sealed class _TokeniserState_475 : TokeniserState
        {
            public _TokeniserState_475(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    t.CreateTagPending(false);
                    t.tagPending.AppendTagName(System.Char.ToLower(r.Current()));
                    t.dataBuffer.Append(r.Current());
                    t.AdvanceTransition(TokeniserState.ScriptDataEscapedEndTagName);
                }
                else
                {
                    t.Emit("</");
                    t.Transition(TokeniserState.ScriptDataEscaped);
                }
            }
        }

        public static readonly TokeniserState ScriptDataEscapedEndTagOpen = new _TokeniserState_475("ScriptDataEscapedEndTagOpen");

        private sealed class _TokeniserState_488 : TokeniserState
        {
            public _TokeniserState_488(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                TokeniserState.HandleDataEndTag(t, r, TokeniserState.ScriptDataEscaped);
            }
        }

        public static readonly TokeniserState ScriptDataEscapedEndTagName = new _TokeniserState_488("ScriptDataEscapedEndTagName");

        private sealed class _TokeniserState_493 : TokeniserState
        {
            public _TokeniserState_493(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                TokeniserState.HandleDataDoubleEscapeTag(t, r, TokeniserState.ScriptDataDoubleEscaped, TokeniserState.ScriptDataEscaped);
            }
        }

        public static readonly TokeniserState ScriptDataDoubleEscapeStart = new _TokeniserState_493("ScriptDataDoubleEscapeStart");

        private sealed class _TokeniserState_498 : TokeniserState
        {
            public _TokeniserState_498(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Current();
                switch (c)
                {
                    case '-':
                        t.Emit(c);
                        t.AdvanceTransition(TokeniserState.ScriptDataDoubleEscapedDash);
                        break;

                    case '<':
                        t.Emit(c);
                        t.AdvanceTransition(TokeniserState.ScriptDataDoubleEscapedLessthanSign
                            );
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        r.Advance();
                        t.Emit(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        string data = r.ConsumeToAny('-', '<', TokeniserState.nullChar);
                        t.Emit(data);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptDataDoubleEscaped = new _TokeniserState_498("ScriptDataDoubleEscaped");

        private sealed class _TokeniserState_525 : TokeniserState
        {
            public _TokeniserState_525(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataDoubleEscapedDashDash);
                        break;

                    case '<':
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataDoubleEscapedLessthanSign);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.Emit(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.ScriptDataDoubleEscaped);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataDoubleEscaped);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptDataDoubleEscapedDash = new _TokeniserState_525("ScriptDataDoubleEscapedDash");

        private sealed class _TokeniserState_552 : TokeniserState
        {
            public _TokeniserState_552(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.Emit(c);
                        break;

                    case '<':
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataDoubleEscapedLessthanSign);
                        break;

                    case '>':
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptData);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.Emit(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.ScriptDataDoubleEscaped);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Emit(c);
                        t.Transition(TokeniserState.ScriptDataDoubleEscaped);
                        break;
                }
            }
        }

        public static readonly TokeniserState ScriptDataDoubleEscapedDashDash = new _TokeniserState_552("ScriptDataDoubleEscapedDashDash");

        private sealed class _TokeniserState_582 : TokeniserState
        {
            public _TokeniserState_582(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.Matches('/'))
                {
                    t.Emit('/');
                    t.CreateTempBuffer();
                    t.AdvanceTransition(TokeniserState.ScriptDataDoubleEscapeEnd);
                }
                else
                {
                    t.Transition(TokeniserState.ScriptDataDoubleEscaped);
                }
            }
        }

        public static readonly TokeniserState ScriptDataDoubleEscapedLessthanSign = new _TokeniserState_582("ScriptDataDoubleEscapedLessthanSign");

        private sealed class _TokeniserState_593 : TokeniserState
        {
            public _TokeniserState_593(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                TokeniserState.HandleDataDoubleEscapeTag(t, r, TokeniserState.ScriptDataEscaped, TokeniserState.ScriptDataDoubleEscaped);
            }
        }

        public static readonly TokeniserState ScriptDataDoubleEscapeEnd = new _TokeniserState_593("ScriptDataDoubleEscapeEnd");

        private sealed class _TokeniserState_598 : TokeniserState
        {
            public _TokeniserState_598(string baseArg1)
                : base(baseArg1)
            {
            }

            // from tagname <xxx
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        break;

                    case '/':
                        // ignore whitespace
                        t.Transition(TokeniserState.SelfClosingStartTag);
                        break;

                    case '>':
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.tagPending.NewAttribute();
                        r.Unconsume();
                        t.Transition(TokeniserState.AttributeName);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    case '"':
                    case '\'':
                    case '<':
                    case '=':
                        t.Error(this);
                        t.tagPending.NewAttribute();
                        t.tagPending.AppendAttributeName(c);
                        t.Transition(TokeniserState.AttributeName);
                        break;

                    default:
                        // A-Z, anything else
                        t.tagPending.NewAttribute();
                        r.Unconsume();
                        t.Transition(TokeniserState.AttributeName);
                        break;
                }
            }
        }

        public static readonly TokeniserState BeforeAttributeName = new _TokeniserState_598("BeforeAttributeName");

        private sealed class _TokeniserState_642 : TokeniserState
        {
            public _TokeniserState_642(string baseArg1)
                : base(baseArg1)
            {
            }

            // from before attribute name
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                string name = r.ConsumeToAny('\t', '\n', '\r', '\f', ' ', '/', '=', '>', TokeniserState.nullChar, '"', '\'', '<');
                t.tagPending.AppendAttributeName(name.ToLower());
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.AfterAttributeName);
                        break;

                    case '/':
                        t.Transition(TokeniserState.SelfClosingStartTag);
                        break;

                    case '=':
                        t.Transition(TokeniserState.BeforeAttributeValue);
                        break;

                    case '>':
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.tagPending.AppendAttributeName(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    case '"':
                    case '\'':
                    case '<':
                        t.Error(this);
                        t.tagPending.AppendAttributeName(c);
                        break;
                }
            }
        }

        public static readonly TokeniserState AttributeName = new _TokeniserState_642("AttributeName");

        private sealed class _TokeniserState_684 : TokeniserState
        {
            public _TokeniserState_684(string baseArg1)
                : base(baseArg1)
            {
            }

            // no default, as covered in consumeToAny
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        // ignore
                        break;

                    case '/':
                        t.Transition(TokeniserState.SelfClosingStartTag);
                        break;

                    case '=':
                        t.Transition(TokeniserState.BeforeAttributeValue);
                        break;

                    case '>':
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.tagPending.AppendAttributeName(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.AttributeName);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    case '"':
                    case '\'':
                    case '<':
                        t.Error(this);
                        t.tagPending.NewAttribute();
                        t.tagPending.AppendAttributeName(c);
                        t.Transition(TokeniserState.AttributeName);
                        break;

                    default:
                        // A-Z, anything else
                        t.tagPending.NewAttribute();
                        r.Unconsume();
                        t.Transition(TokeniserState.AttributeName);
                        break;
                }
            }
        }

        public static readonly TokeniserState AfterAttributeName = new _TokeniserState_684("AfterAttributeName");

        private sealed class _TokeniserState_729 : TokeniserState
        {
            public _TokeniserState_729(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        // ignore
                        break;

                    case '"':
                        t.Transition(TokeniserState.AttributeValue_doubleQuoted);
                        break;

                    case '&':
                        r.Unconsume();
                        t.Transition(TokeniserState.AttributeValue_unquoted);
                        break;

                    case '\'':
                        t.Transition(TokeniserState.AttributeValue_singleQuoted);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.tagPending.AppendAttributeValue(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.AttributeValue_unquoted);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    case '>':
                        t.Error(this);
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case '<':
                    case '=':
                    case '`':
                        t.Error(this);
                        t.tagPending.AppendAttributeValue(c);
                        t.Transition(TokeniserState.AttributeValue_unquoted);
                        break;

                    default:
                        r.Unconsume();
                        t.Transition(TokeniserState.AttributeValue_unquoted);
                        break;
                }
            }
        }

        public static readonly TokeniserState BeforeAttributeValue = new _TokeniserState_729("BeforeAttributeValue");

        private sealed class _TokeniserState_777 : TokeniserState
        {
            public _TokeniserState_777(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                string value = r.ConsumeToAny('"', '&', TokeniserState.nullChar);
                if (value.Length > 0)
                {
                    t.tagPending.AppendAttributeValue(value);
                }
                char c = r.Consume();
                switch (c)
                {
                    case '"':
                        t.Transition(TokeniserState.AfterAttributeValue_quoted);
                        break;

                    case '&':
                        char[] @ref = t.ConsumeCharacterReference('"', true);
                        if (@ref != null)
                        {
                            t.tagPending.AppendAttributeValue(@ref);
                        }
                        else
                        {
                            t.tagPending.AppendAttributeValue('&');
                        }
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.tagPending.AppendAttributeValue(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;
                }
            }
        }

        public static readonly TokeniserState AttributeValue_doubleQuoted = new _TokeniserState_777("AttributeValue_doubleQuoted");

        private sealed class _TokeniserState_807 : TokeniserState
        {
            public _TokeniserState_807(string baseArg1)
                : base(baseArg1)
            {
            }

            // no default, handled in consume to any above
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                string value = r.ConsumeToAny('\'', '&', TokeniserState.nullChar);
                if (value.Length > 0)
                {
                    t.tagPending.AppendAttributeValue(value);
                }
                char c = r.Consume();
                switch (c)
                {
                    case '\'':
                        t.Transition(TokeniserState.AfterAttributeValue_quoted);
                        break;

                    case '&':
                        char[] @ref = t.ConsumeCharacterReference('\'', true);
                        if (@ref != null)
                        {
                            t.tagPending.AppendAttributeValue(@ref);
                        }
                        else
                        {
                            t.tagPending.AppendAttributeValue('&');
                        }
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.tagPending.AppendAttributeValue(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;
                }
            }
        }

        public static readonly TokeniserState AttributeValue_singleQuoted = new _TokeniserState_807("AttributeValue_singleQuoted");

        private sealed class _TokeniserState_837 : TokeniserState
        {
            public _TokeniserState_837(string baseArg1)
                : base(baseArg1)
            {
            }

            // no default, handled in consume to any above
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                string value = r.ConsumeToAny('\t', '\n', '\r', '\f', ' ', '&', '>', TokeniserState.nullChar, '"', '\'', '<', '=', '`');
                if (value.Length > 0)
                {
                    t.tagPending.AppendAttributeValue(value);
                }
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.BeforeAttributeName);
                        break;

                    case '&':
                        char[] @ref = t.ConsumeCharacterReference('>', true);
                        if (@ref != null)
                        {
                            t.tagPending.AppendAttributeValue(@ref);
                        }
                        else
                        {
                            t.tagPending.AppendAttributeValue('&');
                        }
                        break;

                    case '>':
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.tagPending.AppendAttributeValue(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    case '"':
                    case '\'':
                    case '<':
                    case '=':
                    case '`':
                        t.Error(this);
                        t.tagPending.AppendAttributeValue(c);
                        break;
                }
            }
        }

        public static readonly TokeniserState AttributeValue_unquoted = new _TokeniserState_837("AttributeValue_unquoted");

        private sealed class _TokeniserState_885 : TokeniserState
        {
            public _TokeniserState_885(string baseArg1)
                : base(baseArg1)
            {
            }

            // no default, handled in consume to any above
            // CharacterReferenceInAttributeValue state handled inline
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.BeforeAttributeName);
                        break;

                    case '/':
                        t.Transition(TokeniserState.SelfClosingStartTag);
                        break;

                    case '>':
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        r.Unconsume();
                        t.Transition(TokeniserState.BeforeAttributeName);
                        break;
                }
            }
        }

        public static readonly TokeniserState AfterAttributeValue_quoted = new _TokeniserState_885("AfterAttributeValue_quoted");

        private sealed class _TokeniserState_915 : TokeniserState
        {
            public _TokeniserState_915(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '>':
                        t.tagPending.selfClosing = true;
                        t.EmitTagPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.Transition(TokeniserState.BeforeAttributeName);
                        break;
                }
            }
        }

        public static readonly TokeniserState SelfClosingStartTag = new _TokeniserState_915("SelfClosingStartTag");

        private sealed class _TokeniserState_934 : TokeniserState
        {
            public _TokeniserState_934(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                // todo: handle bogus comment starting from eof. when does that trigger?
                // rewind to capture character that lead us here
                r.Unconsume();
                Token.Comment comment = new Token.Comment();
                comment.bogus = true;
                comment.data.Append(r.ConsumeTo('>'));
                // todo: replace nullChar with replaceChar
                t.Emit(comment);
                t.AdvanceTransition(TokeniserState.Data);
            }
        }

        public static readonly TokeniserState BogusComment = new _TokeniserState_934("BogusComment");

        private sealed class _TokeniserState_947 : TokeniserState
        {
            public _TokeniserState_947(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchConsume("--"))
                {
                    t.CreateCommentPending();
                    t.Transition(TokeniserState.CommentStart);
                }
                else if (r.MatchConsumeIgnoreCase("DOCTYPE"))
                    {
                        t.Transition(TokeniserState.Doctype);
                    }
                else if (r.MatchConsume("[CDATA["))
                {
                    // todo: should actually check current namepspace, and only non-html allows cdata. until namespace
                    // is implemented properly, keep handling as cdata
                    //} else if (!t.currentNodeInHtmlNS() && r.matchConsume("[CDATA[")) {
                    t.Transition(TokeniserState.CdataSection);
                }
                else
                {
                    t.Error(this);
                    t.AdvanceTransition(TokeniserState.BogusComment);
                }
            }
        }

        public static readonly TokeniserState MarkupDeclarationOpen = new _TokeniserState_947("MarkupDeclarationOpen");

        private sealed class _TokeniserState_965 : TokeniserState
        {
            public _TokeniserState_965(string baseArg1)
                : base(baseArg1)
            {
            }

            // advance so this character gets in bogus comment data's rewind
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.Transition(TokeniserState.CommentStartDash);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.commentPending.data.Append(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.Comment);
                        break;

                    case '>':
                        t.Error(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.commentPending.data.Append(c);
                        t.Transition(TokeniserState.Comment);
                        break;
                }
            }
        }

        public static readonly TokeniserState CommentStart = new _TokeniserState_965("CommentStart");

        private sealed class _TokeniserState_993 : TokeniserState
        {
            public _TokeniserState_993(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.Transition(TokeniserState.CommentStartDash);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.commentPending.data.Append(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.Comment);
                        break;

                    case '>':
                        t.Error(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.commentPending.data.Append(c);
                        t.Transition(TokeniserState.Comment);
                        break;
                }
            }
        }

        public static readonly TokeniserState CommentStartDash = new _TokeniserState_993("CommentStartDash");

        private sealed class _TokeniserState_1021 : TokeniserState
        {
            public _TokeniserState_1021(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Current();
                switch (c)
                {
                    case '-':
                        t.AdvanceTransition(TokeniserState.CommentEndDash);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        r.Advance();
                        t.commentPending.data.Append(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.commentPending.data.Append(r.ConsumeToAny('-', TokeniserState.nullChar
                            ));
                        break;
                }
            }
        }

        public static readonly TokeniserState Comment = new _TokeniserState_1021("Comment");

        private sealed class _TokeniserState_1043 : TokeniserState
        {
            public _TokeniserState_1043(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.Transition(TokeniserState.CommentEnd);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.commentPending.data.Append('-').Append(TokeniserState.replacementChar
                            );
                        t.Transition(TokeniserState.Comment);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.commentPending.data.Append('-').Append(c);
                        t.Transition(TokeniserState.Comment);
                        break;
                }
            }
        }

        public static readonly TokeniserState CommentEndDash = new _TokeniserState_1043("CommentEndDash");

        private sealed class _TokeniserState_1066 : TokeniserState
        {
            public _TokeniserState_1066(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '>':
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.commentPending.data.Append("--").Append(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.Comment);
                        break;

                    case '!':
                        t.Error(this);
                        t.Transition(TokeniserState.CommentEndBang);
                        break;

                    case '-':
                        t.Error(this);
                        t.commentPending.data.Append('-');
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.commentPending.data.Append("--").Append(c);
                        t.Transition(TokeniserState.Comment);
                        break;
                }
            }
        }

        public static readonly TokeniserState CommentEnd = new _TokeniserState_1066("CommentEnd");

        private sealed class _TokeniserState_1099 : TokeniserState
        {
            public _TokeniserState_1099(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '-':
                        t.commentPending.data.Append("--!");
                        t.Transition(TokeniserState.CommentEndDash);
                        break;

                    case '>':
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.commentPending.data.Append("--!").Append(TokeniserState.replacementChar
                            );
                        t.Transition(TokeniserState.Comment);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.EmitCommentPending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.commentPending.data.Append("--!").Append(c);
                        t.Transition(TokeniserState.Comment);
                        break;
                }
            }
        }

        public static readonly TokeniserState CommentEndBang = new _TokeniserState_1099("CommentEndBang");

        private sealed class _TokeniserState_1127 : TokeniserState
        {
            public _TokeniserState_1127(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.BeforeDoctypeName);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        goto case '>';

                    case '>':
                        // note: fall through to > case
                        // catch invalid <!DOCTYPE>
                        t.Error(this);
                        t.CreateDoctypePending();
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.Transition(TokeniserState.BeforeDoctypeName);
                        break;
                }
            }
        }

        public static readonly TokeniserState Doctype = new _TokeniserState_1127("Doctype");

        private sealed class _TokeniserState_1154 : TokeniserState
        {
            public _TokeniserState_1154(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    t.CreateDoctypePending();
                    t.Transition(TokeniserState.DoctypeName);
                    return;
                }
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        break;

                    case TokeniserState.nullChar:
                        // ignore whitespace
                        t.Error(this);
                        t.CreateDoctypePending();
                        t.doctypePending.name.Append(TokeniserState.replacementChar);
                        t.Transition(TokeniserState.DoctypeName);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.CreateDoctypePending();
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.CreateDoctypePending();
                        t.doctypePending.name.Append(c);
                        t.Transition(TokeniserState.DoctypeName);
                        break;
                }
            }
        }

        public static readonly TokeniserState BeforeDoctypeName = new _TokeniserState_1154("BeforeDoctypeName");

        private sealed class _TokeniserState_1189 : TokeniserState
        {
            public _TokeniserState_1189(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.MatchesLetter())
                {
                    string name = r.ConsumeLetterSequence();
                    t.doctypePending.name.Append(name.ToLower());
                    return;
                }
                char c = r.Consume();
                switch (c)
                {
                    case '>':
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.AfterDoctypeName);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.doctypePending.name.Append(TokeniserState.replacementChar);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.doctypePending.name.Append(c);
                        break;
                }
            }
        }

        public static readonly TokeniserState DoctypeName = new _TokeniserState_1189("DoctypeName");

        private sealed class _TokeniserState_1224 : TokeniserState
        {
            public _TokeniserState_1224(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                if (r.IsEmpty())
                {
                    t.EofError(this);
                    t.doctypePending.forceQuirks = true;
                    t.EmitDoctypePending();
                    t.Transition(TokeniserState.Data);
                    return;
                }
                if (r.MatchesAny('\t', '\n', '\r', '\f', ' '))
                {
                    r.Advance();
                    // ignore whitespace
                }
                else if (r.Matches('>'))
                {
                    t.EmitDoctypePending();
                    t.AdvanceTransition(TokeniserState.Data);
                }
                else if (r.MatchConsumeIgnoreCase("PUBLIC"))
                {
                    t.Transition(TokeniserState.AfterDoctypePublicKeyword);
                }
                else if (r.MatchConsumeIgnoreCase("SYSTEM"))
                {
                    t.Transition(TokeniserState.AfterDoctypeSystemKeyword);
                }
                else
                {
                    t.Error(this);
                    t.doctypePending.forceQuirks = true;
                    t.AdvanceTransition(TokeniserState.BogusDoctype);
                }
            }
        }

        public static readonly TokeniserState AfterDoctypeName = new _TokeniserState_1224("AfterDoctypeName");

        private sealed class _TokeniserState_1250 : TokeniserState
        {
            public _TokeniserState_1250(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.BeforeDoctypePublicIdentifier);
                        break;

                    case '"':
                        t.Error(this);
                        // set public id to empty string
                        t.Transition(TokeniserState.DoctypePublicIdentifier_doubleQuoted);
                        break;

                    case '\'':
                        t.Error(this);
                        // set public id to empty string
                        t.Transition(TokeniserState.DoctypePublicIdentifier_singleQuoted);
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.Transition(TokeniserState.BogusDoctype);
                        break;
                }
            }
        }

        public static readonly TokeniserState AfterDoctypePublicKeyword = new _TokeniserState_1250("AfterDoctypePublicKeyword");

        private sealed class _TokeniserState_1290 : TokeniserState
        {
            public _TokeniserState_1290(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        break;

                    case '"':
                        // set public id to empty string
                        t.Transition(TokeniserState.DoctypePublicIdentifier_doubleQuoted);
                        break;

                    case '\'':
                        // set public id to empty string
                        t.Transition(TokeniserState.DoctypePublicIdentifier_singleQuoted);
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.Transition(TokeniserState.BogusDoctype);
                        break;
                }
            }
        }

        public static readonly TokeniserState BeforeDoctypePublicIdentifier = new _TokeniserState_1290("BeforeDoctypePublicIdentifier");

        private sealed class _TokeniserState_1327 : TokeniserState
        {
            public _TokeniserState_1327(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '"':
                        t.Transition(TokeniserState.AfterDoctypePublicIdentifier);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.doctypePending.publicIdentifier.Append(TokeniserState.replacementChar
                            );
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.doctypePending.publicIdentifier.Append(c);
                        break;
                }
            }
        }

        public static readonly TokeniserState DoctypePublicIdentifier_doubleQuoted = new _TokeniserState_1327("DoctypePublicIdentifier_doubleQuoted");

        private sealed class _TokeniserState_1355 : TokeniserState
        {
            public _TokeniserState_1355(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\'':
                        t.Transition(TokeniserState.AfterDoctypePublicIdentifier);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.doctypePending.publicIdentifier.Append(TokeniserState.replacementChar
                            );
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.doctypePending.publicIdentifier.Append(c);
                        break;
                }
            }
        }

        public static readonly TokeniserState DoctypePublicIdentifier_singleQuoted = new _TokeniserState_1355("DoctypePublicIdentifier_singleQuoted");

        private sealed class _TokeniserState_1383 : TokeniserState
        {
            public _TokeniserState_1383(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.BetweenDoctypePublicAndSystemIdentifiers);
                        break;

                    case '>':
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case '"':
                        t.Error(this);
                        // system id empty
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_doubleQuoted);
                        break;

                    case '\'':
                        t.Error(this);
                        // system id empty
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_singleQuoted);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.Transition(TokeniserState.BogusDoctype);
                        break;
                }
            }
        }

        public static readonly TokeniserState AfterDoctypePublicIdentifier = new _TokeniserState_1383("AfterDoctypePublicIdentifier");

        private sealed class _TokeniserState_1421 : TokeniserState
        {
            public _TokeniserState_1421(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        break;

                    case '>':
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case '"':
                        t.Error(this);
                        // system id empty
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_doubleQuoted);
                        break;

                    case '\'':
                        t.Error(this);
                        // system id empty
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_singleQuoted);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.Transition(TokeniserState.BogusDoctype);
                        break;
                }
            }
        }

        public static readonly TokeniserState BetweenDoctypePublicAndSystemIdentifiers = new _TokeniserState_1421("BetweenDoctypePublicAndSystemIdentifiers");

        private sealed class _TokeniserState_1458 : TokeniserState
        {
            public _TokeniserState_1458(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        t.Transition(TokeniserState.BeforeDoctypeSystemIdentifier);
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case '"':
                        t.Error(this);
                        // system id empty
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_doubleQuoted);
                        break;

                    case '\'':
                        t.Error(this);
                        // system id empty
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_singleQuoted);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        break;
                }
            }
        }

        public static readonly TokeniserState AfterDoctypeSystemKeyword = new _TokeniserState_1458("AfterDoctypeSystemKeyword");

        private sealed class _TokeniserState_1498 : TokeniserState
        {
            public _TokeniserState_1498(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        break;

                    case '"':
                        // set system id to empty string
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_doubleQuoted);
                        break;

                    case '\'':
                        // set public id to empty string
                        t.Transition(TokeniserState.DoctypeSystemIdentifier_singleQuoted);
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.Transition(TokeniserState.BogusDoctype);
                        break;
                }
            }
        }

        public static readonly TokeniserState BeforeDoctypeSystemIdentifier = new _TokeniserState_1498("BeforeDoctypeSystemIdentifier");

        private sealed class _TokeniserState_1535 : TokeniserState
        {
            public _TokeniserState_1535(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '"':
                        t.Transition(TokeniserState.AfterDoctypeSystemIdentifier);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.doctypePending.systemIdentifier.Append(TokeniserState.replacementChar
                            );
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.doctypePending.systemIdentifier.Append(c);
                        break;
                }
            }
        }

        public static readonly TokeniserState DoctypeSystemIdentifier_doubleQuoted = new _TokeniserState_1535("DoctypeSystemIdentifier_doubleQuoted");

        private sealed class _TokeniserState_1563 : TokeniserState
        {
            public _TokeniserState_1563(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\'':
                        t.Transition(TokeniserState.AfterDoctypeSystemIdentifier);
                        break;

                    case TokeniserState.nullChar:
                        t.Error(this);
                        t.doctypePending.systemIdentifier.Append(TokeniserState.replacementChar
                            );
                        break;

                    case '>':
                        t.Error(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.doctypePending.systemIdentifier.Append(c);
                        break;
                }
            }
        }

        public static readonly TokeniserState DoctypeSystemIdentifier_singleQuoted = new _TokeniserState_1563("DoctypeSystemIdentifier_singleQuoted");

        private sealed class _TokeniserState_1591 : TokeniserState
        {
            public _TokeniserState_1591(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                        break;

                    case '>':
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EofError(this);
                        t.doctypePending.forceQuirks = true;
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        t.Error(this);
                        t.Transition(TokeniserState.BogusDoctype);
                        break;
                }
            }
        }

        public static readonly TokeniserState AfterDoctypeSystemIdentifier = new _TokeniserState_1591("AfterDoctypeSystemIdentifier");

        private sealed class _TokeniserState_1618 : TokeniserState
        {
            public _TokeniserState_1618(string baseArg1)
                : base(baseArg1)
            {
            }

            // NOT force quirks
            internal override void Read(Tokeniser t, CharacterReader r)
            {
                char c = r.Consume();
                switch (c)
                {
                    case '>':
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    case TokeniserState.eof:
                        t.EmitDoctypePending();
                        t.Transition(TokeniserState.Data);
                        break;

                    default:
                        // ignore char
                        break;
                }
            }
        }

        public static readonly TokeniserState BogusDoctype = new _TokeniserState_1618("BogusDoctype");

        private sealed class _TokeniserState_1636 : TokeniserState
        {
            public _TokeniserState_1636(string baseArg1)
                : base(baseArg1)
            {
            }

            internal override void Read(Tokeniser t, CharacterReader r)
            {
                string data = r.ConsumeTo("]]>");
                t.Emit(data);
                r.MatchConsume("]]>");
                t.Transition(TokeniserState.Data);
            }
        }

        public static readonly TokeniserState CdataSection = new _TokeniserState_1636("CdataSection");
        
        //internal static readonly TokeniserState Data = new Dummy();
        //internal static readonly TokeniserState Rcdata = new Dummy();
        //internal static readonly TokeniserState Rawtext = new Dummy();
        //internal static readonly TokeniserState ScriptData = new Dummy();
        //internal static readonly TokeniserState PLAINTEXT = new Dummy();
        //internal static readonly TokeniserState BeforeAttributeName = new Dummy();
        //internal static readonly TokeniserState SelfClosingStartTag = new Dummy();

        private readonly string name;

        public TokeniserState(string name)
        {
            this.name = name;
        }

        public string Name()
        {
            return name;
        }

        internal abstract void Read(Tokeniser t, CharacterReader r);

        private const char nullChar = '\u0000';

        private const char replacementChar = Tokeniser.replacementChar;

        private static readonly string replacementStr = Tokeniser.replacementChar.ToString();

        private const char eof = CharacterReader.EOF;

        /// <summary>
        /// Handles RawtextEndTagName, ScriptDataEndTagName, and ScriptDataEscapedEndTagName.
        /// </summary>
        /// <remarks>
        /// Handles RawtextEndTagName, ScriptDataEndTagName, and ScriptDataEscapedEndTagName. Same body impl, just
        /// different else exit transitions.
        /// </remarks>
        private static void HandleDataEndTag(Tokeniser t, CharacterReader r, TokeniserState elseTransition)
        {
            if (r.MatchesLetter())
            {
                string name = r.ConsumeLetterSequence();
                t.tagPending.AppendTagName(name.ToLower());
                t.dataBuffer.Append(name);
                return;
            }
            bool needsExitTransition = false;
            if (t.IsAppropriateEndTagToken() && !r.IsEmpty())
            {
                char c = r.Consume();
                switch (c)
                {
                    case '\t':
                    case '\n':
                    case '\r':
                    case '\f':
                    case ' ':
                    {
                        t.Transition(BeforeAttributeName);
                        break;
                    }

                    case '/':
                    {
                        t.Transition(SelfClosingStartTag);
                        break;
                    }

                    case '>':
                    {
                        t.EmitTagPending();
                        t.Transition(Data);
                        break;
                    }

                    default:
                    {
                        t.dataBuffer.Append(c);
                        needsExitTransition = true;
                        break;
                    }
                }
            }
            else
            {
                needsExitTransition = true;
            }
            if (needsExitTransition)
            {
                t.Emit("</" + t.dataBuffer.ToString());
                t.Transition(elseTransition);
            }
        }

        private static void HandleDataDoubleEscapeTag(Tokeniser t, CharacterReader r, TokeniserState primary, TokeniserState fallback)
        {
            if (r.MatchesLetter())
            {
                string name = r.ConsumeLetterSequence();
                t.dataBuffer.Append(name.ToLower());
                t.Emit(name);
                return;
            }
            char c = r.Consume();
            switch (c)
            {
                case '\t':
                case '\n':
                case '\r':
                case '\f':
                case ' ':
                case '/':
                case '>':
                {
                    if (t.dataBuffer.ToString().Equals("script"))
                    {
                        t.Transition(primary);
                    }
                    else
                    {
                        t.Transition(fallback);
                    }
                    t.Emit(c);
                    break;
                }

                default:
                {
                    r.Unconsume();
                    t.Transition(fallback);
                    break;
                }
            }
        }
    }
}
