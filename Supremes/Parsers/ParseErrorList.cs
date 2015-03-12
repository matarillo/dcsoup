using Supremes.Parsers;
using System;
using System.Collections.Generic;

namespace Supremes.Parsers
{
    /// <summary>
    /// A container for ParseErrors.
    /// </summary>
    /// <author>Jonathan Hedley</author>
    [Serializable]
    internal class ParseErrorList : List<ParseError>
    {
        private const long serialVersionUID = 1L;

        private const int INITIAL_CAPACITY = 16;

        private readonly int maxSize;

        internal ParseErrorList(int initialCapacity, int maxSize)
        	: base(initialCapacity)
        {
            this.maxSize = maxSize;
        }

        internal bool CanAddError()
        {
            return Count < maxSize;
        }

        internal int GetMaxSize()
        {
            return maxSize;
        }

        internal static ParseErrorList NoTracking()
        {
            return new ParseErrorList(0, 0);
        }

        internal static ParseErrorList Tracking(int maxSize)
        {
            return new ParseErrorList(INITIAL_CAPACITY, maxSize);
        }
    }
}
