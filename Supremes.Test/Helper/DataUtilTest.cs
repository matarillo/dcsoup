using NUnit.Framework;
using Supremes.Nodes;
using Supremes.Parsers;
using System;
using System.Text;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Helper
#else
namespace Supremes.Test.net45.Helper
#endif
{
    [TestFixture]
    public class DataUtilTest
    {
        [Test]
        public void TestCharset()
        {
            Assert.AreEqual("utf-8", DataUtil.GetCharsetFromContentType("text/html;charset=utf-8 "));
            Assert.AreEqual("UTF-8", DataUtil.GetCharsetFromContentType("text/html; charset=UTF-8"));
            Assert.AreEqual("ISO-8859-1", DataUtil.GetCharsetFromContentType("text/html; charset=ISO-8859-1"));
            Assert.AreEqual(null, DataUtil.GetCharsetFromContentType("text/html"));
            Assert.AreEqual(null, DataUtil.GetCharsetFromContentType(null));
            Assert.AreEqual(null, DataUtil.GetCharsetFromContentType("text/html;charset=Unknown"));
        }

        [Test]
        public void TestQuotedCharset()
        {
            Assert.AreEqual("utf-8", DataUtil.GetCharsetFromContentType("text/html; charset=\"utf-8\""));
            Assert.AreEqual("UTF-8", DataUtil.GetCharsetFromContentType("text/html;charset=\"UTF-8\""));
            Assert.AreEqual("ISO-8859-1", DataUtil.GetCharsetFromContentType("text/html; charset=\"ISO-8859-1\""));
            Assert.AreEqual(null, DataUtil.GetCharsetFromContentType("text/html; charset=\"Unsupported\""));
            Assert.AreEqual("UTF-8", DataUtil.GetCharsetFromContentType("text/html; charset='UTF-8'"));
        }

        [Test]
        public void DiscardsSpuriousByteOrderMark()
        {
            String html = "\uFEFF<html><head><title>One</title></head><body>Two</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            Document doc = DataUtil.ParseByteData(buffer, "UTF-8", "http://foo.com/", Parser.HtmlParser);
            Assert.AreEqual("One", doc.Head.Text);
        }

        [Test]
        public void DiscardsSpuriousByteOrderMarkWhenNoCharsetSet()
        {
            String html = "\uFEFF<html><head><title>One</title></head><body>Two</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            Document doc = DataUtil.ParseByteData(buffer, null, "http://foo.com/", Parser.HtmlParser);
            Assert.AreEqual("One", doc.Head.Text);
            Assert.AreEqual("utf-8", doc.OutputSettings.Charset.WebName);
        }

        [Test]
        public void ShouldNotThrowExceptionOnEmptyCharset()
        {
            Assert.AreEqual(null, DataUtil.GetCharsetFromContentType("text/html; charset="));
            Assert.AreEqual(null, DataUtil.GetCharsetFromContentType("text/html; charset=;"));
        }

        [Test]
        public void ShouldSelectFirstCharsetOnWeirdMultileCharsetsInMetaTags()
        {
            Assert.AreEqual("ISO-8859-1", DataUtil.GetCharsetFromContentType("text/html; charset=ISO-8859-1, charset=1251"));
        }

        [Test]
        public void ShouldCorrectCharsetForDuplicateCharsetString()
        {
            Assert.AreEqual("iso-8859-1", DataUtil.GetCharsetFromContentType("text/html; charset=charset=iso-8859-1"));
        }

        [Test]
        public void ShouldReturnNullForIllegalCharsetNames()
        {
            Assert.AreEqual(null, DataUtil.GetCharsetFromContentType("text/html; charset=$HJKDF§$/("));
        }
    }
}
