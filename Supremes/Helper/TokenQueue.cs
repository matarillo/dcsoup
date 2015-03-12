/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using System;
using System.Text;

namespace Supremes.Helper
{
    /// <summary>
    /// A character queue with parsing helpers.
    /// </summary>
    /// <author>Jonathan Hedley</author>
    internal class TokenQueue
    {
        private string queue;

        private int pos = 0;

        private const char ESC = '\\';

        /// <summary>
        /// Create a new TokenQueue.
        /// </summary>
        /// <param name="data">string of data to back queue.</param>
        public TokenQueue(string data)
        {
            // escape char for chomp balanced.
            Validate.NotNull(data);
            queue = data;
        }

        /// <summary>
        /// Is the queue empty?
        /// </summary>
        /// <returns>true if no data left in queue.</returns>
        public bool IsEmpty()
        {
            return RemainingLength() == 0;
        }

        private int RemainingLength()
        {
            return queue.Length - pos;
        }

        /// <summary>
        /// Retrieves but does not remove the first character from the queue.
        /// </summary>
        /// <returns>First character, or 0 if empty.</returns>
        public char Peek()
        {
            return IsEmpty() ? '\u0000' : queue[pos];
        }

        /// <summary>
        /// Add a character to the start of the queue
        /// (will be the next character retrieved).
        /// </summary>
        /// <param name="c">character to add</param>
        public void AddFirst(char c)
        {
            AddFirst(c.ToString());
        }

        /// <summary>
        /// Add a string to the start of the queue.
        /// </summary>
        /// <param name="seq">string to add.</param>
        public void AddFirst(string seq)
        {
            // not very performant, but an edge case
            queue = seq + queue.Substring(pos); /*substring*/
            pos = 0;
        }

        /// <summary>
        /// Tests if the next characters on the queue match the sequence.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="seq">String to check queue for.</param>
        /// <returns>true if the next characters match.</returns>
        public bool Matches(string seq)
        {
            //return queue.RegionMatches(ignoreCase: true, toffset: pos, other:seq, ooffset:0, len:seq.Length);
            return queue.IndexOf(seq, pos, StringComparison.OrdinalIgnoreCase) == pos;
        }

        /// <summary>
        /// Case sensitive match test.
        /// </summary>
        /// <param name="seq">string to case sensitively check for</param>
        /// <returns>true if matched, false if not</returns>
        public bool MatchesCS(string seq)
        {
            //return queue.StartsWith(seq, pos);
            return queue.IndexOf(seq, pos, StringComparison.Ordinal) == pos;
        }

        /// <summary>
        /// Tests if the next characters match any of the sequences.
        /// </summary>
        /// <remarks>
        /// Case insensitive.
        /// </remarks>
        /// <param name="seq">list of strings to case insensitively check for</param>
        /// <returns>true of any matched, false if none did</returns>
        public bool MatchesAny(params string[] seq)
        {
            foreach (string s in seq)
            {
                if (Matches(s))
                {
                    return true;
                }
            }
            return false;
        }

        public bool MatchesAny(params char[] seq)
        {
            if (IsEmpty())
            {
                return false;
            }
            foreach (char c in seq)
            {
                if (queue[pos] == c)
                {
                    return true;
                }
            }
            return false;
        }

        public bool MatchesStartTag()
        {
            // micro opt for matching "<x"
            return (RemainingLength() >= 2 && queue[pos] == '<' && char.IsLetter(queue[pos + 1]));
        }

        /// <summary>
        /// Tests if the queue matches the sequence (as with match), 
        /// and if they do, removes the matched string from the queue.
        /// </summary>
        /// <param name="seq">String to search for, and if found, remove from queue.</param>
        /// <returns>true if found and removed, false if not found.</returns>
        public bool MatchChomp(string seq)
        {
            if (Matches(seq))
            {
                pos += seq.Length;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tests if queue starts with a whitespace character.
        /// </summary>
        /// <returns>if starts with whitespace</returns>
        public bool MatchesWhitespace()
        {
            return !IsEmpty() && StringUtil.IsWhitespace(queue[pos]);
        }

        /// <summary>
        /// Test if the queue matches a word character (letter or digit).
        /// </summary>
        /// <returns>if matches a word character</returns>
        public bool MatchesWord()
        {
            return !IsEmpty() && char.IsLetterOrDigit(queue[pos]);
        }

        /// <summary>
        /// Drops the next character off the queue.
        /// </summary>
        public void Advance()
        {
            if (!IsEmpty())
            {
                pos++;
            }
        }

        /// <summary>
        /// Consume one character off queue.
        /// </summary>
        /// <returns>first character on queue.</returns>
        public char Consume()
        {
            return queue[pos++];
        }

        /// <summary>
        /// Consumes the supplied sequence of the queue.
        /// </summary>
        /// <remarks>
        /// If the queue does not start with the supplied sequence, will
        /// throw an illegal state exception
        /// -- but you should be running match() against that condition.
        /// <p/>
        /// Case insensitive.
        /// </remarks>
        /// <param name="seq">sequence to remove from head of queue.</param>
        public void Consume(string seq)
        {
            if (!Matches(seq))
            {
                throw new InvalidOperationException("Queue did not match expected sequence");
            }
            int len = seq.Length;
            if (len > RemainingLength())
            {
                throw new InvalidOperationException("Queue not long enough to consume sequence");
            }
            pos += len;
        }

        /// <summary>
        /// Pulls a string off the queue, up to but exclusive of the match sequence,
        /// or to the queue running out.
        /// </summary>
        /// <param name="seq">
        /// String to end on (and not include in return, but leave on queue).
        /// <b>Case sensitive.</b>
        /// </param>
        /// <returns>The matched data consumed from queue.</returns>
        public string ConsumeTo(string seq)
        {
            int offset = queue.IndexOf(seq, pos, StringComparison.Ordinal);
            if (offset != -1)
            {
                string consumed = queue.Substring(pos, offset - pos); /*substring*/
                pos += consumed.Length;
                return consumed;
            }
            else
            {
                return Remainder();
            }
        }

        public string ConsumeToIgnoreCase(string seq)
        {
            int start = pos;
            string first = seq.Substring(0, 1); /*substring*/
            bool canScan = first.ToLower().Equals(first.ToUpper());
            // if first is not cased, use index of
            while (!IsEmpty())
            {
                if (Matches(seq))
                {
                    break;
                }
                if (canScan)
                {
                    int skip = queue.IndexOf(first, pos, StringComparison.Ordinal) - pos;
                    if (skip == 0)
                    {
                        // this char is the skip char, but not match, so force advance of pos
                        pos++;
                    }
                    else if (skip < 0)
                    {
                        // no chance of finding, grab to end
                        pos = queue.Length;
                    }
                    else
                    {
                        pos += skip;
                    }
                }
                else
                {
                    pos++;
                }
            }
            string data = queue.Substring(start, pos - start); /*substring*/
            return data;
        }

        /// <summary>
        /// Consumes to the first sequence provided, or to the end of the queue.
        /// </summary>
        /// <remarks>
        /// Leaves the terminator on the queue.
        /// </remarks>
        /// <param name="seq">
        /// any number of terminators to consume to.
        /// <b>Case insensitive.</b>
        /// </param>
        /// <returns>consumed string</returns>
        public string ConsumeToAny(params string[] seq)
        {
            // todo: method name. not good that consumeTo cares for case, and consume to any doesn't. And the only use for this
            // is is a case sensitive time...
            int start = pos;
            while (!IsEmpty() && !MatchesAny(seq))
            {
                pos++;
            }
            string data = queue.Substring(start, pos - start); /*substring*/
            return data;
        }

        /// <summary>
        /// Pulls a string off the queue (like consumeTo),
        /// and then pulls off the matched string (but does not return it).
        /// </summary>
        /// <remarks>
        /// If the queue runs out of characters before finding the seq,
        /// will return as much as it can
        /// (and queue will go isEmpty() == true).
        /// </remarks>
        /// <param name="seq">
        /// String to match up to, and not include in return, and to pull off queue.
        /// <b>Case sensitive.</b>
        /// </param>
        /// <returns>Data matched from queue.</returns>
        public string ChompTo(string seq)
        {
            string data = ConsumeTo(seq);
            MatchChomp(seq);
            return data;
        }

        public string ChompToIgnoreCase(string seq)
        {
            string data = ConsumeToIgnoreCase(seq);
            // case insensitive scan
            MatchChomp(seq);
            return data;
        }

        /// <summary>
        /// Pulls a balanced string off the queue.
        /// </summary>
        /// <remarks>
        /// E.g. if queue is "(one (two) three) four", (,) will return "one (two) three",
        /// and leave " four" on the queue. Unbalanced openers and closers can be escaped (with \).
        /// Those escapes will be left in the returned string,
        /// which is suitable for regexes (where we need to preserve the escape),
        /// but unsuitable for contains text strings; use unescape for that.
        /// </remarks>
        /// <param name="open">opener</param>
        /// <param name="close">closer</param>
        /// <returns>data matched from the queue</returns>
        public string ChompBalanced(char open, char close)
        {
            int start = -1;
            int end = -1;
            int depth = 0;
            char last = '\u0000';
            do
            {
                if (IsEmpty())
                {
                    break;
                }
                char c = Consume();
                if (last == '\u0000' || last != ESC)
                {
                    if (c.Equals(open))
                    {
                        depth++;
                        if (start == -1)
                        {
                            start = pos;
                        }
                    }
                    else
                    {
                        if (c.Equals(close))
                        {
                            depth--;
                        }
                    }
                }
                if (depth > 0 && last != 0)
                {
                    end = pos;
                }
                // don't include the outer match pair in the return
                last = c;
            }
            while (depth > 0);
            return (end >= 0) ? queue.Substring(start, end - start) /*substring*/ : string.Empty;
        }

        /// <summary>
        /// Unescaped a \ escaped string.
        /// </summary>
        /// <param name="in">backslash escaped string</param>
        /// <returns>unescaped string</returns>
        public static string Unescape(string @in)
        {
            StringBuilder @out = new StringBuilder();
            char last = '\u0000';
            foreach (char c in @in.ToCharArray())
            {
                if (c == ESC)
                {
                    if (last != '\u0000' && last == ESC)
                    {
                        @out.Append(c);
                    }
                }
                else
                {
                    @out.Append(c);
                }
                last = c;
            }
            return @out.ToString();
        }

        /// <summary>
        /// Pulls the next run of whitespace characters of the queue.
        /// </summary>
        public bool ConsumeWhitespace()
        {
            bool seen = false;
            while (MatchesWhitespace())
            {
                pos++;
                seen = true;
            }
            return seen;
        }

        /// <summary>
        /// Retrieves the next run of word type (letter or digit) off the queue.
        /// </summary>
        /// <returns>String of word characters from queue, or empty string if none.</returns>
        public string ConsumeWord()
        {
            int start = pos;
            while (MatchesWord())
            {
                pos++;
            }
            return queue.Substring(start, pos - start); /*substring*/
        }

        /// <summary>
        /// Consume an tag name off the queue (word or :, _, -)
        /// </summary>
        /// <returns>tag name</returns>
        public string ConsumeTagName()
        {
            int start = pos;
            while (!IsEmpty() && (MatchesWord() || MatchesAny(':', '_', '-')))
            {
                pos++;
            }
            return queue.Substring(start, pos - start); /*substring*/
        }

        /// <summary>
        /// Consume a CSS element selector
        /// (tag name, but | instead of : for namespaces, to not conflict with :pseudo selects).
        /// </summary>
        /// <returns>tag name</returns>
        public string ConsumeElementSelector()
        {
            int start = pos;
            while (!IsEmpty() && (MatchesWord() || MatchesAny('|', '_', '-')))
            {
                pos++;
            }
            return queue.Substring(start, pos - start); /*substring*/
        }

        /// <summary>
        /// Consume a CSS identifier (ID or class) off the queue (letter, digit, -, _)
        /// http://www.w3.org/TR/CSS2/syndata.html#value-def-identifier
        /// </summary>
        /// <returns>identifier</returns>
        public string ConsumeCssIdentifier()
        {
            int start = pos;
            while (!IsEmpty() && (MatchesWord() || MatchesAny('-', '_')))
            {
                pos++;
            }
            return queue.Substring(start, pos - start); /*substring*/
        }

        /// <summary>
        /// Consume an attribute key off the queue (letter, digit, -, _, :")
        /// </summary>
        /// <returns>attribute key</returns>
        public string ConsumeAttributeKey()
        {
            int start = pos;
            while (!IsEmpty() && (MatchesWord() || MatchesAny('-', '_', ':')))
            {
                pos++;
            }
            return queue.Substring(start, pos - start); /*substring*/
        }

        /// <summary>
        /// Consume and return whatever is left on the queue.
        /// </summary>
        /// <returns>remained of queue.</returns>
        public string Remainder()
        {
            string remainder = queue.Substring(pos); /*substring*/
            pos = queue.Length;
            return remainder;
        }

        public override string ToString()
        {
            return queue.Substring(pos); /*substring*/
        }
    }
}
