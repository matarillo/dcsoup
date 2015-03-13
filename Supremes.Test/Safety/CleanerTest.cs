using NUnit.Framework;
using Supremes.Nodes;
using Supremes.Safety;

namespace Supremes.Test.Safety
{
    [TestFixture]
    public class CleanerTest
    {
        [Test]
        public void SimpleBehaviourTest()
        {
            string h = "<div><p class=foo><a href='http://evil.com'>Hello <b id=bar>there</b>!</a></div>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.SimpleText());

            Assert.AreEqual("Hello <b>there</b>!", TextUtil.StripNewlines(cleanHtml));
        }

        [Test]
        public void SimpleBehaviourTest2()
        {
            string h = "Hello <b>there</b>!";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.SimpleText());

            Assert.AreEqual("Hello <b>there</b>!", TextUtil.StripNewlines(cleanHtml));
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: trailing slash to domain name
        /// </remarks>
        [Test]
        public void BasicBehaviourTest()
        {
            string h = "<div><p><a href='javascript:sendAllMoney()'>Dodgy</a> <A HREF='HTTP://nice.com'>Nice</a></p><blockquote>Hello</blockquote>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Basic());

            Assert.AreEqual("<p><a rel=\"nofollow\">Dodgy</a> <a href=\"http://nice.com/\" rel=\"nofollow\">Nice</a></p><blockquote>Hello</blockquote>",
                    TextUtil.StripNewlines(cleanHtml)); // trailing slash
        }

        [Test]
        public void BasicWithImagesTest()
        {
            string h = "<div><p><img src='http://example.com/' alt=Image></p><p><img src='ftp://ftp.example.com'></p></div>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.BasicWithImages());
            Assert.AreEqual("<p><img src=\"http://example.com/\" alt=\"Image\"></p><p><img></p>", TextUtil.StripNewlines(cleanHtml));
        }

        [Test]
        public void TestRelaxed()
        {
            string h = "<h1>Head</h1><table><tr><td>One<td>Two</td></tr></table>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Relaxed());
            Assert.AreEqual("<h1>Head</h1><table><tbody><tr><td>One</td><td>Two</td></tr></tbody></table>", TextUtil.StripNewlines(cleanHtml));
        }

        [Test]
        public void TestDropComments()
        {
            string h = "<p>Hello<!-- no --></p>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Relaxed());
            Assert.AreEqual("<p>Hello</p>", cleanHtml);
        }

        [Test]
        public void TestDropXmlProc()
        {
            string h = "<?import namespace=\"xss\"><p>Hello</p>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Relaxed());
            Assert.AreEqual("<p>Hello</p>", cleanHtml);
        }

        [Test]
        public void TestDropScript()
        {
            string h = "<SCRIPT SRC=//ha.ckers.org/.j><SCRIPT>alert(/XSS/.source)</SCRIPT>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Relaxed());
            Assert.AreEqual("", cleanHtml);
        }

        [Test]
        public void TestDropImageScript()
        {
            string h = "<IMG SRC=\"javascript:alert('XSS')\">";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Relaxed());
            Assert.AreEqual("<img>", cleanHtml);
        }

        [Test]
        public void TestCleanJavascriptHref()
        {
            string h = "<A HREF=\"javascript:document.location='http://www.google.com/'\">XSS</A>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Relaxed());
            Assert.AreEqual("<a>XSS</a>", cleanHtml);
        }

        [Test]
        public void TestDropsUnknownTags()
        {
            string h = "<p><custom foo=true>Test</custom></p>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.Relaxed());
            Assert.AreEqual("<p>Test</p>", cleanHtml);
        }

        [Test]
        public void TestHandlesEmptyAttributes()
        {
            string h = "<img alt=\"\" src= unknown=''>";
            string cleanHtml = Dcsoup.Clean(h, Whitelist.BasicWithImages());
            Assert.AreEqual("<img alt=\"\">", cleanHtml);
        }

        [Test]
        public void TestIsValid()
        {
            string ok = "<p>Test <b><a href='http://example.com/'>OK</a></b></p>";
            string nok1 = "<p><script></script>Not <b>OK</b></p>";
            string nok2 = "<p align=right>Test Not <b>OK</b></p>";
            string nok3 = "<!-- comment --><p>Not OK</p>"; // comments and the like will be cleaned
            Assert.IsTrue(Dcsoup.IsValid(ok, Whitelist.Basic()));
            Assert.IsFalse(Dcsoup.IsValid(nok1, Whitelist.Basic()));
            Assert.IsFalse(Dcsoup.IsValid(nok2, Whitelist.Basic()));
            Assert.IsFalse(Dcsoup.IsValid(nok3, Whitelist.Basic()));
        }

        [Test]
        public void ResolvesRelativeLinks()
        {
            string html = "<a href='/foo'>Link</a><img src='/bar'>";
            string clean = Dcsoup.Clean(html, "http://example.com/", Whitelist.BasicWithImages());
            Assert.AreEqual("<a href=\"http://example.com/foo\" rel=\"nofollow\">Link</a>\n<img src=\"http://example.com/bar\">", clean);
        }

        [Test]
        public void PreservesRelativeLinksIfConfigured()
        {
            string html = "<a href='/foo'>Link</a><img src='/bar'> <img src='javascript:alert()'>";
            string clean = Dcsoup.Clean(html, "http://example.com/", Whitelist.BasicWithImages().PreserveRelativeLinks(true));
            Assert.AreEqual("<a href=\"/foo\" rel=\"nofollow\">Link</a>\n<img src=\"/bar\"> \n<img>", clean);
        }

        [Test]
        public void DropsUnresolvableRelativeLinks()
        {
            string html = "<a href='/foo'>Link</a>";
            string clean = Dcsoup.Clean(html, Whitelist.Basic());
            Assert.AreEqual("<a rel=\"nofollow\">Link</a>", clean);
        }

        [Test]
        public void HandlesCustomProtocols()
        {
            string html = "<img src='cid:12345' /> <img src='data:gzzt' />";
            string dropped = Dcsoup.Clean(html, Whitelist.BasicWithImages());
            Assert.AreEqual("<img> \n<img>", dropped);

            string preserved = Dcsoup.Clean(html, Whitelist.BasicWithImages().AddProtocols("img", "src", "cid", "data"));
            Assert.AreEqual("<img src=\"cid:12345\"> \n<img src=\"data:gzzt\">", preserved);
        }

        [Test]
        public void HandlesAllPseudoTag()
        {
            string html = "<p class='foo' src='bar'><a class='qux'>link</a></p>";
            Whitelist whitelist = new Whitelist()
                    .AddAttributes(":all", "class")
                    .AddAttributes("p", "style")
                    .AddTags("p", "a");

            string clean = Dcsoup.Clean(html, whitelist);
            Assert.AreEqual("<p class=\"foo\"><a class=\"qux\">link</a></p>", clean);
        }

        [Test]
        public void AddsTagOnAttributesIfNotSet()
        {
            string html = "<p class='foo' src='bar'>One</p>";
            Whitelist whitelist = new Whitelist()
                .AddAttributes("p", "class");
            // ^^ whitelist does not have explicit tag add for p, inferred from add attributes.
            string clean = Dcsoup.Clean(html, whitelist);
            Assert.AreEqual("<p class=\"foo\">One</p>", clean);
        }

        [Test]
        public void SupplyOutputSettings()
        {
            // test that one can override the default document output settings
            DocumentOutputSettings os = new DocumentOutputSettings();
            os.PrettyPrint(false);
            os.EscapeMode(DocumentEscapeMode.Extended);
            os.Charset("ascii");

            string html = "<div><p>&bernou;</p></div>";
            string customOut = Dcsoup.Clean(html, "http://foo.com/", Whitelist.Relaxed(), os);
            string defaultOut = Dcsoup.Clean(html, "http://foo.com/", Whitelist.Relaxed());
            Assert.AreNotEqual(defaultOut, customOut);

            Assert.AreEqual("<div><p>&bernou;</p></div>", customOut);
            Assert.AreEqual("<div>\n" +
                " <p>ℬ</p>\n" +
                "</div>", defaultOut);

            os.Charset("ASCII");
            os.EscapeMode(DocumentEscapeMode.Base);
            string customOut2 = Dcsoup.Clean(html, "http://foo.com/", Whitelist.Relaxed(), os);
            Assert.AreEqual("<div><p>&#x212c;</p></div>", customOut2);
        }

        [Test]
        public void HandlesFramesets()
        {
            string dirty = "<html><head><script></script><noscript></noscript></head><frameset><frame src=\"foo\" /><frame src=\"foo\" /></frameset></html>";
            string clean = Dcsoup.Clean(dirty, Whitelist.Basic());
            Assert.AreEqual("", clean); // nothing good can come out of that

            Document dirtyDoc = Dcsoup.Parse(dirty);
            Document cleanDoc = new Cleaner(Whitelist.Basic()).Clean(dirtyDoc);
            Assert.IsFalse(cleanDoc == null);
            Assert.AreEqual(0, cleanDoc.Body().ChildNodeSize());
        }

        [Test]
        public void CleansInternationalText()
        {
            Assert.AreEqual("привет", Dcsoup.Clean("привет", Whitelist.None()));
        }

        [Test]
        public void TestScriptTagInWhiteList()
        {
            Whitelist whitelist = Whitelist.Relaxed();
            whitelist.AddTags("script");
            Assert.IsTrue(Dcsoup.IsValid("Hello<script>alert('Doh')</script>World !", whitelist));
        }
    }
}
