using NUnit.Framework;
using Supremes.Helper;

namespace Supremes.Test.Nodes
{
    [TestFixture]
    public class StringUtilTest
    {
        //[Test]
        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: API removed; test case is also removed.
        /// </remarks>
        public void Join()
        {
            // API removed; test case is also removed.
        }

        [Test]
        public void Padding()
        {
            Assert.AreEqual("", StringUtil.Padding(0));
            Assert.AreEqual(" ", StringUtil.Padding(1));
            Assert.AreEqual("  ", StringUtil.Padding(2));
            Assert.AreEqual("               ", StringUtil.Padding(15));
        }

        //[Test]
        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: API removed; test case is also removed.
        /// </remarks>
        public void IsBlank()
        {
            // API removed; test case is also removed.
        }

        [Test]
        public void IsNumeric()
        {
            Assert.IsFalse(StringUtil.IsNumeric(null));
            Assert.IsFalse(StringUtil.IsNumeric(" "));
            Assert.IsFalse(StringUtil.IsNumeric("123 546"));
            Assert.IsFalse(StringUtil.IsNumeric("hello"));
            Assert.IsFalse(StringUtil.IsNumeric("123.334"));

            Assert.IsTrue(StringUtil.IsNumeric("1"));
            Assert.IsTrue(StringUtil.IsNumeric("1234"));
        }

        [Test]
        public void IsWhitespace()
        {
            Assert.IsTrue(StringUtil.IsWhitespace('\t'));
            Assert.IsTrue(StringUtil.IsWhitespace('\n'));
            Assert.IsTrue(StringUtil.IsWhitespace('\r'));
            Assert.IsTrue(StringUtil.IsWhitespace('\f'));
            Assert.IsTrue(StringUtil.IsWhitespace(' '));

            Assert.IsFalse(StringUtil.IsWhitespace('\u00a0'));
            Assert.IsFalse(StringUtil.IsWhitespace('\u2000'));
            Assert.IsFalse(StringUtil.IsWhitespace('\u3000'));
        }

        [Test]
        public void NormaliseWhiteSpace()
        {
            Assert.AreEqual(" ", StringUtil.NormaliseWhitespace("    \r \n \r\n"));
            Assert.AreEqual(" hello there ", StringUtil.NormaliseWhitespace("   hello   \r \n  there    \n"));
            Assert.AreEqual("hello", StringUtil.NormaliseWhitespace("hello"));
            Assert.AreEqual("hello there", StringUtil.NormaliseWhitespace("hello\nthere"));
        }

        [Test]
        public void NormaliseWhiteSpaceHandlesHighSurrogates()
        {
            string test71540chars = "\ud869\udeb2\u304b\u309a  1";
            string test71540charsExpectedSingleWhitespace = "\ud869\udeb2\u304b\u309a 1";

            Assert.AreEqual(test71540charsExpectedSingleWhitespace, StringUtil.NormaliseWhitespace(test71540chars));
            string extractedText = Dcsoup.Parse(test71540chars).Text;
            Assert.AreEqual(test71540charsExpectedSingleWhitespace, extractedText);
        }
    }
}
