using Supremes.Helper;
using Supremes.Nodes;
using System.Text;

namespace Supremes.Parsers
{
    /// <summary>
    /// Parse tokens for the Tokeniser.
    /// </summary>
    internal abstract class Token
    {
        internal TokenType type;

        internal Token()
        {
        }

        internal string Type()
        {
            return this.GetType().Name;
        }

        internal class Doctype : Token
        {
            internal readonly StringBuilder name = new StringBuilder();

            internal readonly StringBuilder publicIdentifier = new StringBuilder();

            internal readonly StringBuilder systemIdentifier = new StringBuilder();

            internal bool forceQuirks = false;

            public Doctype()
            {
                type = TokenType.Doctype;
            }

            internal string GetName()
            {
                return name.ToString();
            }

            internal string GetPublicIdentifier()
            {
                return publicIdentifier.ToString();
            }

            public string GetSystemIdentifier()
            {
                return systemIdentifier.ToString();
            }

            public bool IsForceQuirks()
            {
                return forceQuirks;
            }
        }

        internal abstract class Tag : Token
        {
            internal string tagName;

            private string pendingAttributeName;

            private StringBuilder pendingAttributeValue;

            internal bool selfClosing = false;

            internal Attributes attributes;

            // attribute names are generally caught in one hop, not accumulated
            // but values are accumulated, from e.g. & in hrefs
            // start tags get attributes on construction. End tags get attributes on first new attribute (but only for parser convenience, not used).
            internal void NewAttribute()
            {
                if (attributes == null)
                {
                    attributes = new Attributes();
                }
                if (pendingAttributeName != null)
                {
                    Attribute attribute;
                    if (pendingAttributeValue == null)
                    {
                        attribute = new Attribute(pendingAttributeName, string.Empty);
                    }
                    else
                    {
                        attribute = new Attribute(pendingAttributeName, pendingAttributeValue.ToString());
                    }
                    attributes.Put(attribute);
                }
                pendingAttributeName = null;
                if (pendingAttributeValue != null)
                {
                    pendingAttributeValue.Remove(0, pendingAttributeValue.Length);
                }
            }

            internal void FinaliseTag()
            {
                // finalises for emit
                if (pendingAttributeName != null)
                {
                    // todo: check if attribute name exists; if so, drop and error
                    NewAttribute();
                }
            }

            internal string Name()
            {
                Validate.IsFalse(tagName == null || tagName.Length == 0);
                return tagName;
            }

            internal Token.Tag Name(string name)
            {
                tagName = name;
                return this;
            }

            internal bool IsSelfClosing()
            {
                return selfClosing;
            }

            internal Attributes GetAttributes()
            {
                return attributes;
            }

            // these appenders are rarely hit in not null state-- caused by null chars.
            internal void AppendTagName(string append)
            {
                tagName = tagName == null ? append : tagName + append;
            }

            internal void AppendTagName(char append)
            {
                AppendTagName(append.ToString());
            }

            internal void AppendAttributeName(string append)
            {
                pendingAttributeName = pendingAttributeName == null ? append : pendingAttributeName + append;
            }

            internal void AppendAttributeName(char append)
            {
                AppendAttributeName(append.ToString());
            }

            internal void AppendAttributeValue(string append)
            {
                EnsureAttributeValue();
                pendingAttributeValue.Append(append);
            }

            internal void AppendAttributeValue(char append)
            {
                EnsureAttributeValue();
                pendingAttributeValue.Append(append);
            }

            internal void AppendAttributeValue(char[] append)
            {
                EnsureAttributeValue();
                pendingAttributeValue.Append(append);
            }

            private void EnsureAttributeValue()
            {
                if (pendingAttributeValue == null)
                {
                    pendingAttributeValue = new StringBuilder();
                }
            }
        }

        internal class StartTag : Token.Tag
        {
            public StartTag() : base()
            {
                attributes = new Attributes();
                type = TokenType.StartTag;
            }

            internal StartTag(string name) : this()
            {
                this.tagName = name;
            }

            internal StartTag(string name, Attributes attributes) : this()
            {
                this.tagName = name;
                this.attributes = attributes;
            }

            public override string ToString()
            {
                if (attributes != null && attributes.Count > 0)
                {
                    return "<" + Name() + " " + attributes.ToString() + ">";
                }
                else
                {
                    return "<" + Name() + ">";
                }
            }
        }

        internal class EndTag : Token.Tag
        {
            public EndTag() : base()
            {
                type = TokenType.EndTag;
            }

            internal EndTag(string name) : this()
            {
                this.tagName = name;
            }

            public override string ToString()
            {
                return "</" + Name() + ">";
            }
        }

        internal class Comment : Token
        {
            internal readonly StringBuilder data = new StringBuilder();

            internal bool bogus = false;

            public Comment()
            {
                type = TokenType.Comment;
            }

            internal string GetData()
            {
                return data.ToString();
            }

            public override string ToString()
            {
                return "<!--" + GetData() + "-->";
            }
        }

        internal class Character : Token
        {
            private readonly string data;

            internal Character(string data)
            {
                type = TokenType.Character;
                this.data = data;
            }

            internal string GetData()
            {
                return data;
            }

            public override string ToString()
            {
                return GetData();
            }
        }

        internal class EOF : Token
        {
            public EOF()
            {
                type = TokenType.EOF;
            }
        }

        internal bool IsDoctype()
        {
            return type == TokenType.Doctype;
        }

        internal Token.Doctype AsDoctype()
        {
            return (Token.Doctype)this;
        }

        internal bool IsStartTag()
        {
            return type == TokenType.StartTag;
        }

        internal Token.StartTag AsStartTag()
        {
            return (Token.StartTag)this;
        }

        internal bool IsEndTag()
        {
            return type == TokenType.EndTag;
        }

        internal Token.EndTag AsEndTag()
        {
            return (Token.EndTag)this;
        }

        internal bool IsComment()
        {
            return type == TokenType.Comment;
        }

        internal Token.Comment AsComment()
        {
            return (Token.Comment)this;
        }

        internal bool IsCharacter()
        {
            return type == TokenType.Character;
        }

        internal Token.Character AsCharacter()
        {
            return (Token.Character)this;
        }

        internal bool IsEOF()
        {
            return type == TokenType.EOF;
        }

        
    }
}
