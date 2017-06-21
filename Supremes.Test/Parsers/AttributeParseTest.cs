using NUnit.Framework;
using Supremes.Nodes;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Parsers
#else
namespace Supremes.Test.net45.Parsers
#endif
{
    [TestFixture]
    public class AttributeParseTest
    {
        [Test]
        public void ParsesRoughAttributeString()
        {
            string html = "<a id=\"123\" class=\"baz = 'bar'\" style = 'border: 2px'qux zim foo = 12 mux=18 />";
            // should be: <id=123>, <class=baz = 'bar'>, <qux=>, <zim=>, <foo=12>, <mux.=18>

            Element el = Dcsoup.Parse(html).GetElementsByTag("a")[0];
            Attributes attr = el.Attributes;
            Assert.AreEqual(7, attr.Count);
            Assert.AreEqual("123", attr["id"]);
            Assert.AreEqual("baz = 'bar'", attr["class"]);
            Assert.AreEqual("border: 2px", attr["style"]);
            Assert.AreEqual("", attr["qux"]);
            Assert.AreEqual("", attr["zim"]);
            Assert.AreEqual("12", attr["foo"]);
            Assert.AreEqual("18", attr["mux"]);
        }

        [Test]
        public void HandlesNewLinesAndReturns()
        {
            string html = "<a\r\nfoo='bar\r\nqux'\r\nbar\r\n=\r\ntwo>One</a>";
            Element el = Dcsoup.Parse(html).Select("a").First;
            Assert.AreEqual(2, el.Attributes.Count);
            Assert.AreEqual("bar\r\nqux", el.Attr("foo")); // currently preserves newlines in quoted attributes. todo confirm if should.
            Assert.AreEqual("two", el.Attr("bar"));
        }

        [Test]
        public void ParsesEmptyString()
        {
            string html = "<a />";
            Element el = Dcsoup.Parse(html).GetElementsByTag("a")[0];
            Attributes attr = el.Attributes;
            Assert.AreEqual(0, attr.Count);
        }

        [Test]
        public void CanStartWithEq()
        {
            string html = "<a =empty />";
            Element el = Dcsoup.Parse(html).GetElementsByTag("a")[0];
            Attributes attr = el.Attributes;
            Assert.AreEqual(1, attr.Count);
            Assert.IsTrue(attr.ContainsKey("=empty"));
            Assert.AreEqual("", attr["=empty"]);
        }

        [Test]
        public void StrictAttributeUnescapes()
        {
            string html = "<a id=1 href='?foo=bar&mid&lt=true'>One</a> <a id=2 href='?foo=bar&lt;qux&lg=1'>Two</a>";
            Elements els = Dcsoup.Parse(html).Select("a");
            Assert.AreEqual("?foo=bar&mid&lt=true", els.First.Attr("href"));
            Assert.AreEqual("?foo=bar<qux&lg=1", els.Last.Attr("href"));
        }

        [Test]
        public void MreAttributeUnescapes()
        {
            string html = "<a href='&wr_id=123&mid-size=true&ok=&wr'>Check</a>";
            Elements els = Dcsoup.Parse(html).Select("a");
            Assert.AreEqual("&wr_id=123&mid-size=true&ok=&wr", els.First.Attr("href"));
        }
    }
}
