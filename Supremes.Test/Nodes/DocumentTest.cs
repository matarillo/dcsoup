using NUnit.Framework;
using Supremes.Nodes;
#if (NETSTANDARD1_3)
using Supremes.Test.Integration;
#else
using Supremes.Test.net45.Integration;
#endif
using System.Text;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Nodes
#else
namespace Supremes.Test.net45.Nodes
#endif
{
    [TestFixture]
    public class DocumentTest
    {
        [Test]
        public void SetTextPreservesDocumentStructure()
        {
            Document doc = Dcsoup.Parse("<p>Hello</p>");
            doc.Text = "Replaced";
            Assert.AreEqual("Replaced", doc.Text);
            Assert.AreEqual("Replaced", doc.Body.Text);
            Assert.AreEqual(1, doc.Select("head").Count);
        }

        [Test]
        public void TestTitles()
        {
            Document noTitle = Dcsoup.Parse("<p>Hello</p>");
            Document withTitle = Dcsoup.Parse("<title>First</title><title>Ignore</title><p>Hello</p>");

            Assert.AreEqual("", noTitle.Title);
            noTitle.Title = "Hello";
            Assert.AreEqual("Hello", noTitle.Title);
            Assert.AreEqual("Hello", noTitle.Select("title").First.Text);

            Assert.AreEqual("First", withTitle.Title);
            withTitle.Title = "Hello";
            Assert.AreEqual("Hello", withTitle.Title);
            Assert.AreEqual("Hello", withTitle.Select("title").First.Text);

            Document normaliseTitle = Dcsoup.Parse("<title>   Hello\nthere   \n   now   \n");
            Assert.AreEqual("Hello there now", normaliseTitle.Title);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: charset name is lower case
        /// </remarks>
        [Test]
        public void TestOutputEncoding()
        {
            Document doc = Dcsoup.Parse("<p title=π>π & < > </p>");
            // default is utf-8
            Assert.AreEqual("<p title=\"π\">π &amp; &lt; &gt; </p>", doc.Body.Html);
            Assert.AreEqual("utf-8", doc.OutputSettings.Charset.WebName); // charset name is lower case

            doc.OutputSettings.Charset = Encoding.ASCII;
            Assert.AreEqual(DocumentEscapeMode.Base, doc.OutputSettings.EscapeMode);
            Assert.AreEqual("<p title=\"&#x3c0;\">&#x3c0; &amp; &lt; &gt; </p>", doc.Body.Html);

            doc.OutputSettings.EscapeMode = DocumentEscapeMode.Extended;
            Assert.AreEqual("<p title=\"&pi;\">&pi; &amp; &lt; &gt; </p>", doc.Body.Html);
        }

        [Test]
        public void TestXhtmlReferences()
        {
            Document doc = Dcsoup.Parse("&lt; &gt; &amp; &quot; &apos; &times;");
            doc.OutputSettings.EscapeMode = DocumentEscapeMode.Xhtml;
            Assert.AreEqual("&lt; &gt; &amp; \" ' ×", doc.Body.Html);
        }

        [Test]
        public void TestNormalisesStructure()
        {
            Document doc = Dcsoup.Parse("<html><head><script>one</script><noscript><p>two</p></noscript></head><body><p>three</p></body><p>four</p></html>");
            Assert.AreEqual("<html><head><script>one</script><noscript></noscript></head><body><p>two</p><p>three</p><p>four</p></body></html>", TextUtil.StripNewlines(doc.Html));
        }

        [Test]
        public void TestClone()
        {
            Document doc = Dcsoup.Parse("<title>Hello</title> <p>One<p>Two");
            Document clone = (Document)doc.Clone();

            Assert.AreEqual("<html><head><title>Hello</title> </head><body><p>One</p><p>Two</p></body></html>", TextUtil.StripNewlines(clone.Html));
            clone.Title = "Hello there";
            var first = clone.Select("p").First;
            first.Text = "One more";
            first.Attr("id", "1");
            Assert.AreEqual("<html><head><title>Hello there</title> </head><body><p id=\"1\">One more</p><p>Two</p></body></html>", TextUtil.StripNewlines(clone.Html));
            Assert.AreEqual("<html><head><title>Hello</title> </head><body><p>One</p><p>Two</p></body></html>", TextUtil.StripNewlines(doc.Html));
        }

        [Test]
        public void TestClonesDeclarations()
        {
            Document doc = Dcsoup.Parse("<!DOCTYPE html><html><head><title>Doctype test");
            Document clone = (Document)doc.Clone();

            Assert.AreEqual(doc.Html, clone.Html);
            Assert.AreEqual("<!DOCTYPE html><html><head><title>Doctype test</title></head><body></body></html>",
                    TextUtil.StripNewlines(clone.Html));
        }

        [Test]
        public void TestLocation()
        {
            string @in = new ParseTest().GetFilePath("/htmltests/yahoo-jp.html");
            Document doc = Dcsoup.ParseFile(@in, "UTF-8", "http://www.yahoo.co.jp/index.html");
            string location = doc.Location;
            string baseUri = doc.BaseUri;
            Assert.AreEqual("http://www.yahoo.co.jp/index.html", location);
            Assert.AreEqual("http://www.yahoo.co.jp/_ylh=X3oDMTB0NWxnaGxsBF9TAzIwNzcyOTYyNjUEdGlkAzEyBHRtcGwDZ2Ex/", baseUri);
            @in = new ParseTest().GetFilePath("/htmltests/nyt-article-1.html");
            doc = Dcsoup.ParseFile(@in, null, "http://www.nytimes.com/2010/07/26/business/global/26bp.html?hp");
            location = doc.Location;
            baseUri = doc.BaseUri;
            Assert.AreEqual("http://www.nytimes.com/2010/07/26/business/global/26bp.html?hp", location);
            Assert.AreEqual("http://www.nytimes.com/2010/07/26/business/global/26bp.html?hp", baseUri);
        }

        [Test]
        public void TestHtmlAndXmlSyntax()
        {
            string h = "<!DOCTYPE html><body><img async checked='checked' src='&<>\"'>&lt;&gt;&amp;&quot;<foo />bar";
            Document doc = Dcsoup.Parse(h);

            doc.OutputSettings.Syntax = DocumentSyntax.Html;
            Assert.AreEqual("<!DOCTYPE html>\n" +
                    "<html>\n" +
                    " <head></head>\n" +
                    " <body>\n" +
                    "  <img async checked src=\"&amp;<>&quot;\">&lt;&gt;&amp;\"\n" +
                    "  <foo />bar\n" +
                    " </body>\n" +
                    "</html>", doc.Html);

            doc.OutputSettings.Syntax = DocumentSyntax.Xml;
            Assert.AreEqual("<!DOCTYPE html>\n" +
                    "<html>\n" +
                    " <head></head>\n" +
                    " <body>\n" +
                    "  <img async=\"\" checked=\"checked\" src=\"&amp;<>&quot;\" />&lt;&gt;&amp;\"\n" +
                    "  <foo />bar\n" +
                    " </body>\n" +
                    "</html>", doc.Html);
        }

        [Test]
        public void HtmlParseDefaultsToHtmlOutputSyntax()
        {
            Document doc = Dcsoup.Parse("x");
            Assert.AreEqual(DocumentSyntax.Html, doc.OutputSettings.Syntax);
        }


        [Test]
        public void TestOverflowClone()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 100000; i++)
            {
                builder.Insert(0, "<i>");
                builder.Append("</i>");
            }

            Document doc = Dcsoup.Parse(builder.ToString());
            doc.Clone();
        }
    }
}
