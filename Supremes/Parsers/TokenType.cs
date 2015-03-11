using System;
using System.Linq;

namespace Supremes.Parsers
{
    internal enum TokenType
    {
        Doctype,
        StartTag,
        EndTag,
        Comment,
        Character,
        EOF
    }
}
