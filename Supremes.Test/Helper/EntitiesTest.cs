using NUnit.Framework;
using Supremes.Helper;
using Supremes.Nodes;
using System.Text;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Helper
#else
namespace Supremes.Test.net45.Helper
#endif
{
    [TestFixture]
    public class EntitiesTest
    {
        [Test]
        public void Escape()
        {
            string text = "Hello &<> Å å π 新 there ¾ © »";
            string escapedAscii = Entities.Escape(text, Entities.EscapeMode.Base, Encoding.ASCII);
            string escapedAsciiFull = Entities.Escape(text, Entities.EscapeMode.Extended, Encoding.ASCII);
            string escapedAsciiXhtml = Entities.Escape(text, Entities.EscapeMode.Xhtml, Encoding.ASCII);
            string escapedUtfFull = Entities.Escape(text, Entities.EscapeMode.Base, Encoding.UTF8);
            string escapedUtfMin = Entities.Escape(text, Entities.EscapeMode.Xhtml, Encoding.UTF8);

            Assert.AreEqual("Hello &amp;&lt;&gt; &Aring; &aring; &#x3c0; &#x65b0; there &frac34; &copy; &raquo;", escapedAscii);
            Assert.AreEqual("Hello &amp;&lt;&gt; &angst; &aring; &pi; &#x65b0; there &frac34; &copy; &raquo;", escapedAsciiFull);
            Assert.AreEqual("Hello &amp;&lt;&gt; &#xc5; &#xe5; &#x3c0; &#x65b0; there &#xbe; &#xa9; &#xbb;", escapedAsciiXhtml);
            Assert.AreEqual("Hello &amp;&lt;&gt; Å å π 新 there ¾ © »", escapedUtfFull);
            Assert.AreEqual("Hello &amp;&lt;&gt; Å å π 新 there ¾ © »", escapedUtfMin);
            // odd that it's defined as aring in base but angst in full

            // round trip
            Assert.AreEqual(text, Entities.Unescape(escapedAscii));
            Assert.AreEqual(text, Entities.Unescape(escapedAsciiFull));
            Assert.AreEqual(text, Entities.Unescape(escapedAsciiXhtml));
            Assert.AreEqual(text, Entities.Unescape(escapedUtfFull));
            Assert.AreEqual(text, Entities.Unescape(escapedUtfMin));
        }

        [Test]
        public void EscapeSupplementaryCharacter()
        {
            string text = char.ConvertFromUtf32(135361);
            string escapedAscii = Entities.Escape(text, Entities.EscapeMode.Base, Encoding.ASCII);
            Assert.AreEqual("&#x210c1;", escapedAscii);
            string escapedUtf = Entities.Escape(text, Entities.EscapeMode.Base, Encoding.UTF8);
            Assert.AreEqual(text, escapedUtf);
        }

        [Test]
        public void Unescape()
        {
            string text = "Hello &amp;&LT&gt; &reg &angst; &angst &#960; &#960 &#x65B0; there &! &frac34; &copy; &COPY;";
            Assert.AreEqual("Hello &<> ® Å &angst π π 新 there &! ¾ © ©", Entities.Unescape(text));

            Assert.AreEqual("&0987654321; &unknown", Entities.Unescape("&0987654321; &unknown"));
        }

        [Test]
        public void StrictUnescape()
        { // for attributes, enforce strict unescaping (must look like &#xxx; , not just &#xxx)
            string text = "Hello &amp= &amp;";
            Assert.AreEqual("Hello &amp= &", Entities.Unescape(text, true));
            Assert.AreEqual("Hello &= &", Entities.Unescape(text));
            Assert.AreEqual("Hello &= &", Entities.Unescape(text, false));
        }


        [Test]
        public void CaseSensitive()
        {
            string unescaped = "Ü ü & &";
            Assert.AreEqual("&Uuml; &uuml; &amp; &amp;",
                    Entities.Escape(unescaped, Entities.EscapeMode.Extended, Encoding.ASCII));

            string escaped = "&Uuml; &uuml; &amp; &AMP";
            Assert.AreEqual("Ü ü & &", Entities.Unescape(escaped));
        }

        [Test]
        public void QuoteReplacements()
        {
            string escaped = "&#92; &#36;";
            string unescaped = "\\ $";

            Assert.AreEqual(unescaped, Entities.Unescape(escaped));
        }

        [Test]
        public void LetterDigitEntities()
        {
            string html = "<p>&sup1;&sup2;&sup3;&frac14;&frac12;&frac34;</p>";
            Document doc = Dcsoup.Parse(html);
            doc.OutputSettings.Charset = Encoding.ASCII;
            Element p = doc.Select("p").First;
            Assert.AreEqual("&sup1;&sup2;&sup3;&frac14;&frac12;&frac34;", p.Html);
            Assert.AreEqual("¹²³¼½¾", p.Text);
            doc.OutputSettings.Charset = Encoding.UTF8;
            Assert.AreEqual("¹²³¼½¾", p.Html);
        }

        [Test]
        public void noSpuriousDecodes()
        {
            string @string = "http://www.foo.com?a=1&num_rooms=1&children=0&int=VA&b=2";
            Assert.AreEqual(@string, Entities.Unescape(@string));
        }
    }
}
