using NUnit.Framework;
using Supremes.Helper;
using Supremes.Nodes;
using Supremes.Parsers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Supremes.Test.Parsers
{
    [TestFixture]
    public class XmlTreeBuilderTest
    {
        [Test]
        public void TestSimpleXmlParse()
        {
            string xml = "<doc id=2 href='/bar'>Foo <br /><link>One</link><link>Two</link></doc>";
            XmlTreeBuilder tb = new XmlTreeBuilder();
            Document doc = tb.Parse(xml, "http://foo.com/");
            Assert.AreEqual("<doc id=\"2\" href=\"/bar\">Foo <br /><link>One</link><link>Two</link></doc>",
                    TextUtil.StripNewlines(doc.Html));
            Assert.AreEqual(doc.GetElementById("2").AbsUrl("href"), "http://foo.com/bar");
        }

        [Test]
        public void TestPopToClose()
        {
            // test: </val> closes Two, </bar> ignored
            string xml = "<doc><val>One<val>Two</val></bar>Three</doc>";
            XmlTreeBuilder tb = new XmlTreeBuilder();
            Document doc = tb.Parse(xml, "http://foo.com/");
            Assert.AreEqual("<doc><val>One<val>Two</val>Three</val></doc>",
                    TextUtil.StripNewlines(doc.Html));
        }

        [Test]
        public void TestCommentAndDocType()
        {
            string xml = "<!DOCTYPE html><!-- a comment -->One <qux />Two";
            XmlTreeBuilder tb = new XmlTreeBuilder();
            Document doc = tb.Parse(xml, "http://foo.com/");
            Assert.AreEqual("<!DOCTYPE html><!-- a comment -->One <qux />Two",
                    TextUtil.StripNewlines(doc.Html));
        }

        [Test]
        public void TestSupplyParserToDcsoupClass()
        {
            string xml = "<doc><val>One<val>Two</val></bar>Three</doc>";
            Document doc = Dcsoup.Parse(xml, "http://foo.com/", Parser.XmlParser);
            Assert.AreEqual("<doc><val>One<val>Two</val>Three</val></doc>",
                    TextUtil.StripNewlines(doc.Html));
        }

        //[Ignore]
        //[Test]
        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: API removed; test case is merged into TestSupplyParserToDataStream
        /// </remarks>
        public void TestSupplyParserToConnection()
        {
            // API removed; test case is merged into TestSupplyParserToDataStream
        }

        [Test]
        public void TestSupplyParserToDataStream()
        {
            // parse with both xml and html parser, ensure different
            string xml = "<doc><val>One<val>Two</val></bar>Three</doc>";
            byte[] utf8Xml = Encoding.UTF8.GetBytes(xml);
            Stream inStream = new MemoryStream(utf8Xml);
            Document xmlDoc = Dcsoup.Parse(inStream, null, "http://foo.com", Parser.XmlParser);
            inStream.Position = 0L;
            Document htmlDoc = Dcsoup.Parse(inStream, null, "http://foo.com", Parser.HtmlParser);

            Assert.AreEqual("<doc><val>One<val>Two</val>Three</val></doc>",
                TextUtil.StripNewlines(xmlDoc.Html));
            Assert.AreNotEqual(htmlDoc, xmlDoc);
            Assert.AreEqual(1, htmlDoc.Select("head").Count); // html parser normalises
            Assert.AreEqual(0, xmlDoc.Select("head").Count); // xml parser does not
        }

        [Test]
        public void TestDoesNotForceSelfClosingKnownTags()
        {
            // html will force "<br>one</br>" to logically "<br />One<br />". XML should be stay "<br>one</br> -- don't recognise tag.
            Document htmlDoc = Dcsoup.Parse("<br>one</br>");
            Assert.AreEqual("<br>one\n<br>", htmlDoc.Body.Html);

            Document xmlDoc = Dcsoup.Parse("<br>one</br>", "", Parser.XmlParser);
            Assert.AreEqual("<br>one</br>", xmlDoc.Html);
        }

        [Test]
        public void HandlesXmlDeclarationAsDeclaration()
        {
            string html = "<?xml encoding='UTF-8' ?><body>One</body><!-- comment -->";
            Document doc = Dcsoup.Parse(html, "", Parser.XmlParser);
            Assert.AreEqual("<?xml encoding='UTF-8' ?> <body> One </body> <!-- comment -->",
                    StringUtil.NormaliseWhitespace(doc.OuterHtml));
            Assert.AreEqual("#declaration", doc.ChildNode(0).NodeName);
            Assert.AreEqual("#comment", doc.ChildNode(2).NodeName);
        }

        [Test]
        public void XmlFragment()
        {
            string xml = "<one src='/foo/' />Two<three><four /></three>";
            IReadOnlyList<Node> nodes = Parser.ParseXmlFragment(xml, "http://example.com/");
            Assert.AreEqual(3, nodes.Count);

            Assert.AreEqual("http://example.com/foo/", nodes[0].AbsUrl("src"));
            Assert.AreEqual("one", nodes[0].NodeName);
            Assert.AreEqual("Two", ((TextNode)nodes[1]).Text);
        }

        [Test]
        public void XmlParseDefaultsToHtmlOutputSyntax()
        {
            Document doc = Dcsoup.Parse("x", "", Parser.XmlParser);
            Assert.AreEqual(DocumentSyntax.Xml, doc.OutputSettings.Syntax);
        }
    }
}