/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using Supremes.Helper;
using Supremes.Nodes;

namespace Supremes.Parsers
{
    /// <author>Jonathan Hedley</author>
    internal abstract class TreeBuilder
    {
        internal CharacterReader reader;

        internal Tokeniser tokeniser;

        internal Document doc;

        internal DescendableLinkedList<Element> stack;

        internal string baseUri;

        internal Token currentToken;

        internal ParseErrorList errors;

        // current doc we are building into
        // the stack of open elements
        // current base uri, for creating new elements
        // currentToken is used only for error tracking.
        // null when not tracking errors
        internal virtual void InitialiseParse(string input, string baseUri, ParseErrorList errors)
        {
            Validate.NotNull(input, "String input must not be null");
            Validate.NotNull(baseUri, "BaseURI must not be null");
            doc = new Document(baseUri);
            reader = new CharacterReader(input);
            this.errors = errors;
            tokeniser = new Tokeniser(reader, errors);
            stack = new DescendableLinkedList<Element>();
            this.baseUri = baseUri;
        }

        internal Document Parse(string input, string baseUri)
        {
            return Parse(input, baseUri, ParseErrorList.NoTracking());
        }

        internal virtual Document Parse(string input, string baseUri, ParseErrorList errors)
        {
            InitialiseParse(input, baseUri, errors);
            RunParser();
            return doc;
        }

        internal void RunParser()
        {
            while (true)
            {
                Token token = tokeniser.Read();
                Process(token);
                if (token.type == TokenType.EOF)
                {
                    break;
                }
            }
        }

        internal abstract bool Process(Token token);

        internal Element CurrentElement()
        {
            return stack.Last.Value;
        }
    }
}
