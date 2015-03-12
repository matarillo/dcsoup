using System;
using Supremes.Helper;
using Supremes.Nodes;
using System.Collections.Generic;

namespace Supremes.Parsers
{
    /// <summary>
    /// Use the
    /// <code>XmlTreeBuilder</code>
    /// when you want to parse XML without any of the HTML DOM rules being applied to the
    /// document.
    /// <p>Usage example:
    /// <code>Document xmlDoc = Nsoup.Parse(html, baseUrl, Parser.XmlParser());</code>
    /// </p>
    /// </summary>
    /// <author>Jonathan Hedley</author>
    internal class XmlTreeBuilder : TreeBuilder
    {
        internal override void InitialiseParse(string input, string baseUri, ParseErrorList errors)
        {
            base.InitialiseParse(input, baseUri, errors);
            stack.AddLast(doc);
            // place the document onto the stack. differs from HtmlTreeBuilder (not on stack)
            doc.OutputSettings().Syntax(DocumentSyntax.Xml);
        }

        internal override bool Process(Token token)
        {
            switch (token.type)
            {
                case TokenType.StartTag:
                {
                    // start tag, end tag, doctype, comment, character, eof
                    Insert(token.AsStartTag());
                    break;
                }

                case TokenType.EndTag:
                {
                    PopStackToClose(token.AsEndTag());
                    break;
                }

                case TokenType.Comment:
                {
                    Insert(token.AsComment());
                    break;
                }

                case TokenType.Character:
                {
                    Insert(token.AsCharacter());
                    break;
                }

                case TokenType.Doctype:
                {
                    Insert(token.AsDoctype());
                    break;
                }

                case TokenType.EOF:
                {
                    // could put some normalisation here if desired
                    break;
                }

                default:
                {
                    Validate.Fail("Unexpected token type: " + token.type);
                    break;
                }
            }
            return true;
        }

        private void InsertNode(Node node)
        {
            CurrentElement().AppendChild(node);
        }

        internal Element Insert(Token.StartTag startTag)
        {
            Tag tag = Tag.ValueOf(startTag.Name());
            // todo: wonder if for xml parsing, should treat all tags as unknown? because it's not html.
            Element el = new Element(tag, baseUri, startTag.attributes);
            InsertNode(el);
            if (startTag.IsSelfClosing())
            {
                tokeniser.AcknowledgeSelfClosingFlag();
                if (!tag.IsKnownTag())
                {
                    // unknown tag, remember this is self closing for output. see above.
                    tag.SetSelfClosing();
                }
            }
            else
            {
                stack.AddLast(el);
            }
            return el;
        }

        internal void Insert(Token.Comment commentToken)
        {
            Comment comment = new Comment(commentToken.GetData(), baseUri);
            Node insert = comment;
            if (commentToken.bogus)
            {
                // xml declarations are emitted as bogus comments (which is right for html, but not xml)
                string data = comment.GetData();
                if (data.Length > 1 && (data.StartsWith("!", StringComparison.Ordinal) || data.StartsWith("?", StringComparison.Ordinal)))
                {
                    string declaration = data.Substring(1); /*substring*/
                    insert = new XmlDeclaration(declaration, comment.BaseUri(), data.StartsWith("!", StringComparison.Ordinal));
                }
            }
            InsertNode(insert);
        }

        internal void Insert(Token.Character characterToken)
        {
            Node node = new TextNode(characterToken.GetData(), baseUri);
            InsertNode(node);
        }

        internal void Insert(Token.Doctype d)
        {
            DocumentType doctypeNode = new DocumentType(d.GetName(), d.GetPublicIdentifier(), d.GetSystemIdentifier(), baseUri);
            InsertNode(doctypeNode);
        }

        /// <summary>
        /// If the stack contains an element with this tag's name, pop up the stack to remove the first occurrence.
        /// </summary>
        /// <remarks>
        /// If not found, skips.
        /// </remarks>
        /// <param name="endTag"></param>
        private void PopStackToClose(Token.EndTag endTag)
        {
            string elName = endTag.Name();
            Element firstFound = null;
            var it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next.NodeName.Equals(elName))
                {
                    firstFound = next;
                    break;
                }
            }
            if (firstFound == null)
            {
                // not found, skip
                return;
            }
            it = stack.GetDescendingEnumerator();
            while (it.MoveNext())
            {
                Element next = it.Current;
                if (next == firstFound)
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

        internal IReadOnlyList<Node> ParseFragment(string inputFragment, string baseUri, ParseErrorList errors)
        {
            InitialiseParse(inputFragment, baseUri, errors);
            RunParser();
            return doc.ChildNodes();
        }
    }
}
