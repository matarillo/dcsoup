using Supremes.Helper;
using System;
using System.Text;

namespace Supremes.Parsers
{
    /// <summary>
    /// Readers the input stream into tokens.
    /// </summary>
    internal class Tokeniser
    {
        internal const char replacementChar = '\uFFFD';

        private readonly CharacterReader reader;

        private ParseErrorList errors;

        private TokeniserState state = TokeniserState.Data;

        private Token emitPending;

        private bool isEmitPending = false;

        private StringBuilder charBuffer = new StringBuilder();

        internal StringBuilder dataBuffer;

        internal Token.Tag tagPending;

        internal Token.Doctype doctypePending;

        internal Token.Comment commentPending;

        private Token.StartTag lastStartTag;

        private bool selfClosingFlagAcknowledged = true;

        internal Tokeniser(CharacterReader reader, ParseErrorList errors)
        {
            // replaces null character
            // html input
            // errors found while tokenising
            // current tokenisation state
            // the token we are about to emit on next read
            // buffers characters to output as one token
            // buffers data looking for </script>
            // tag we are building up
            // doctype building up
            // comment building up
            // the last start tag emitted, to test appropriate end tag
            this.reader = reader;
            this.errors = errors;
        }

        internal Token Read()
        {
            if (!selfClosingFlagAcknowledged)
            {
                Error("Self closing flag not acknowledged");
                selfClosingFlagAcknowledged = true;
            }
            while (!isEmitPending)
            {
                state.Read(this, reader);
            }
            // if emit is pending, a non-character token was found: return any chars in buffer, and leave token for next read:
            if (charBuffer.Length > 0)
            {
                string str = charBuffer.ToString();
                charBuffer.Remove(0, charBuffer.Length);
                return new Token.Character(str);
            }
            else
            {
                isEmitPending = false;
                return emitPending;
            }
        }

        internal void Emit(Token token)
        {
            Validate.IsFalse(isEmitPending, "There is an unread token pending!");
            emitPending = token;
            isEmitPending = true;
            if (token.type == TokenType.StartTag)
            {
                Token.StartTag startTag = (Token.StartTag)token;
                lastStartTag = startTag;
                if (startTag.selfClosing)
                {
                    selfClosingFlagAcknowledged = false;
                }
            }
            else
            {
                if (token.type == TokenType.EndTag)
                {
                    Token.EndTag endTag = (Token.EndTag)token;
                    if (endTag.attributes != null)
                    {
                        Error("Attributes incorrectly present on end tag");
                    }
                }
            }
        }

        internal void Emit(string str)
        {
            // buffer strings up until last string token found, to emit only one token for a run of character refs etc.
            // does not set isEmitPending; read checks that
            charBuffer.Append(str);
        }

        internal void Emit(char[] chars)
        {
            charBuffer.Append(chars);
        }

        internal void Emit(char c)
        {
            charBuffer.Append(c);
        }

        internal TokeniserState GetState()
        {
            return state;
        }

        internal void Transition(TokeniserState state)
        {
            this.state = state;
        }

        internal void AdvanceTransition(TokeniserState state)
        {
            reader.Advance();
            this.state = state;
        }

        internal void AcknowledgeSelfClosingFlag()
        {
            selfClosingFlagAcknowledged = true;
        }

        internal char[] ConsumeCharacterReference(char? additionalAllowedCharacter, bool inAttribute)
        {
            if (reader.IsEmpty())
            {
                return null;
            }
            if (additionalAllowedCharacter != null && additionalAllowedCharacter == reader.Current())
            {
                return null;
            }
            if (reader.MatchesAny('\t', '\n', '\r', '\f', ' ', '<', '&'))
            {
                return null;
            }
            reader.Mark();
            if (reader.MatchConsume("#"))
            {
                // numbered
                bool isHexMode = reader.MatchConsumeIgnoreCase("X");
                string numRef = isHexMode ? reader.ConsumeHexSequence() : reader.ConsumeDigitSequence();
                if (numRef.Length == 0)
                {
                    // didn't match anything
                    CharacterReferenceError("numeric reference with no numerals");
                    reader.RewindToMark();
                    return null;
                }
                if (!reader.MatchConsume(";"))
                {
                    CharacterReferenceError("missing semicolon");
                }
                // missing semi
                int charval = -1;
                try
                {
                    int @base = isHexMode ? 16 : 10;
                    charval = Convert.ToInt32(numRef, @base);
                }
                catch (FormatException)
                {
                }
                // skip
                if (charval == -1 || (charval >= 0xD800 && charval <= 0xDFFF) || charval > 0x10FFFF)
                {
                    CharacterReferenceError("character outside of valid range");
                    return new char[] { replacementChar };
                }
                else
                {
                    // todo: implement number replacement table
                    // todo: check for extra illegal unicode points as parse errors
                    return char.ConvertFromUtf32(charval).ToCharArray();
                }
            }
            else
            {
                // named
                // get as many letters as possible, and look for matching entities.
                string nameRef = reader.ConsumeLetterThenDigitSequence();
                bool looksLegit = reader.Matches(';');
                // found if a base named entity without a ;, or an extended entity with the ;.
                bool found = (Entities.IsBaseNamedEntity(nameRef) || (Entities.IsNamedEntity(nameRef) && looksLegit));
                if (!found)
                {
                    reader.RewindToMark();
                    if (looksLegit)
                    {
                        // named with semicolon
                        CharacterReferenceError(string.Format("invalid named referenece '{0}'", nameRef));
                    }
                    return null;
                }
                if (inAttribute && (reader.MatchesLetter() || reader.MatchesDigit() || reader.MatchesAny('=', '-', '_')))
                {
                    // don't want that to match
                    reader.RewindToMark();
                    return null;
                }
                if (!reader.MatchConsume(";"))
                {
                    CharacterReferenceError("missing semicolon");
                }
                // missing semi
                int charval = Entities.GetCharacterByName(nameRef);
                return char.ConvertFromUtf32(charval).ToCharArray();
            }
        }

        internal Token.Tag CreateTagPending(bool start)
        {
            tagPending = start ? (Token.Tag)new Token.StartTag() : (Token.Tag)new Token.EndTag();
            return tagPending;
        }

        internal void EmitTagPending()
        {
            tagPending.FinaliseTag();
            Emit(tagPending);
        }

        internal void CreateCommentPending()
        {
            commentPending = new Token.Comment();
        }

        internal void EmitCommentPending()
        {
            Emit(commentPending);
        }

        internal void CreateDoctypePending()
        {
            doctypePending = new Token.Doctype();
        }

        internal void EmitDoctypePending()
        {
            Emit(doctypePending);
        }

        internal void CreateTempBuffer()
        {
            dataBuffer = new StringBuilder();
        }

        internal bool IsAppropriateEndTagToken()
        {
            if (lastStartTag == null)
            {
                return false;
            }
            return tagPending.tagName.Equals(lastStartTag.tagName);
        }

        internal string AppropriateEndTagName()
        {
            if (lastStartTag == null)
            {
                return null;
            }
            return lastStartTag.tagName;
        }

        internal void Error(TokeniserState state)
        {
            if (errors.CanAddError())
            {
                errors.Add(new ParseError(reader.Pos(), "Unexpected character '{0}' in input state [{1}]", reader.Current(), state.Name()));
            }
        }

        internal void EofError(TokeniserState state)
        {
            if (errors.CanAddError())
            {
                errors.Add(new ParseError(reader.Pos(), "Unexpectedly reached end of file (EOF) in input state [{0}]", state.Name()));
            }
        }

        private void CharacterReferenceError(string message)
        {
            if (errors.CanAddError())
            {
                errors.Add(new ParseError(reader.Pos(), "Invalid character reference: {0}", message));
            }
        }

        private void Error(string errorMsg)
        {
            if (errors.CanAddError())
            {
                errors.Add(new ParseError(reader.Pos(), errorMsg));
            }
        }

        internal bool CurrentNodeInHtmlNS()
        {
            // todo: implement namespaces correctly
            return true;
            // Element currentNode = currentNode();
            // return currentNode != null && currentNode.namespace().equals("HTML");
        }

        /// <summary>
        /// Utility method to consume reader and unescape entities found within.
        /// </summary>
        /// <param name="inAttribute"></param>
        /// <returns>unescaped string from reader</returns>
        internal string UnescapeEntities(bool inAttribute)
        {
            StringBuilder builder = new StringBuilder();
            while (!reader.IsEmpty())
            {
                builder.Append(reader.ConsumeTo('&'));
                if (reader.Matches('&'))
                {
                    reader.Consume();
                    char[] c = ConsumeCharacterReference(null, inAttribute);
                    if (c == null || c.Length == 0)
                    {
                        builder.Append('&');
                    }
                    else
                    {
                        builder.Append(c);
                    }
                }
            }
            return builder.ToString();
        }
    }
}
