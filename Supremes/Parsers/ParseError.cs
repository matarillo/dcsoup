using Supremes.Parsers;

namespace Supremes.Parsers
{
    /// <summary>
    /// A Parse Error records an error in the input HTML that occurs in either the tokenisation or the tree building phase.
    /// </summary>
    public sealed class ParseError
    {
        private readonly int pos;

        private readonly string errorMsg;

        internal ParseError(int pos, string errorMsg)
        {
            this.pos = pos;
            this.errorMsg = errorMsg;
        }

        internal ParseError(int pos, string errorFormat, params object[] args)
        {
            this.errorMsg = string.Format(errorFormat, args);
            this.pos = pos;
        }

        /// <summary>
        /// Retrieve the error message.
        /// </summary>
        /// <returns>the error message.</returns>
        public string ErrorMessage
        {
            get { return errorMsg; }
        }

        /// <summary>
        /// Retrieves the offset of the error.
        /// </summary>
        /// <returns>error offset within input</returns>
        public int Position
        {
            get { return pos; }
        }

        /// <summary>
        /// Converts the value of this instance to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return pos + ": " + errorMsg;
        }
    }
}
