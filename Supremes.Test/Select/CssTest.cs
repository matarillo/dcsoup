using System;
using System.Text;
using NUnit.Framework;
using Supremes.Nodes;

namespace Supremes.Test.Select
{
    [TestFixture]
    public class CssTest
    {
        private Document html = null;
        private static string htmlString;

        [TestFixtureSetUp]
        public static void InitClass()
        {
            var sb = new StringBuilder("<html><head></head><body>");
            sb.Append("<div id='pseudo'>");
            for (int i = 1; i <= 10; i++)
            {
                sb.AppendFormat("<p>{0}</p>", i);
            }
            sb.Append("</div>");

            sb.Append("<div id='type'>");
            for (int i = 1; i <= 10; i++)
            {
                sb.AppendFormat("<p>{0}</p>", i);
                sb.AppendFormat("<span>{0}</span>", i);
                sb.AppendFormat("<em>{0}</em>", i);
                sb.AppendFormat("<svg>{0}</svg>", i);
            }
            sb.Append("</div>");

            sb.Append("<span id='onlySpan'><br /></span>");
            sb.Append("<p class='empty'><!-- Comment only is still empty! --></p>");
        
            sb.Append("<div id='only'>");
            sb.Append("Some text before the <em>only</em> child in this div");
            sb.Append("</div>");
        
            sb.Append("</body></html>");
            htmlString = sb.ToString();
        }
        
        [SetUp]
        public void Init()
        {
            html = Dcsoup.Parse(htmlString);
        }
        
        [Test]
        public void FirstChild()
        {
            Check(html.Select("#pseudo :first-child"), "1");
            Check(html.Select("html:first-child"));
        }
        
        [Test]
        public void LastChild()
        {
            Check(html.Select("#pseudo :last-child"), "10");
            Check(html.Select("html:last-child"));
        }
	
        [Test]
        public void NthChild_Simple()
        {
            for (int i = 1; i <= 10; i++)
            {
                Check(html.Select(string.Format("#pseudo :nth-child({0})", i)), i.ToString());
            }
        }

        [Test]
        public void NthOfType_UnknownTag()
        {
            for (int i = 1; i <= 10; i++)
            {
                Check(html.Select(string.Format("#type svg:nth-of-type({0})", i)), i.ToString());
            }
        }

        [Test]
        public void NthLastChild_Simple()
        {
            for (int i = 1; i <= 10; i++)
            {
                Check(html.Select(string.Format("#pseudo :nth-last-child({0})", i)), (11 - i).ToString());
            }
        }

        [Test]
        public void NthOfType_Simple()
        {
            for (int i = 1; i <= 10; i++)
            {
                Check(html.Select(string.Format("#type p:nth-of-type({0})", i)), i.ToString());
            }
        }
	
        [Test]
        public void NthLastOfType_Simple()
        {
            for (int i = 1; i <= 10; i++)
            {
                Check(html.Select(string.Format("#type :nth-last-of-type({0})", i)), (11 - i).ToString(), (11 - i).ToString(), (11 - i).ToString(), (11 - i).ToString());
            }
        }

        [Test]
        public void NthChild_Advanced()
        {
            Check(html.Select("#pseudo :nth-child(-5)"));
            Check(html.Select("#pseudo :nth-child(odd)"), "1", "3", "5", "7", "9");
            Check(html.Select("#pseudo :nth-child(2n-1)"), "1", "3", "5", "7", "9");
            Check(html.Select("#pseudo :nth-child(2n+1)"), "1", "3", "5", "7", "9");
            Check(html.Select("#pseudo :nth-child(2n+3)"), "3", "5", "7", "9");
            Check(html.Select("#pseudo :nth-child(even)"), "2", "4", "6", "8", "10");
            Check(html.Select("#pseudo :nth-child(2n)"), "2", "4", "6", "8", "10");
            Check(html.Select("#pseudo :nth-child(3n-1)"), "2", "5", "8");
            Check(html.Select("#pseudo :nth-child(-2n+5)"), "1", "3", "5");
            Check(html.Select("#pseudo :nth-child(+5)"), "5");
        }

        [Test]
        public void NthOfType_Advanced()
        {
            Check(html.Select("#type :nth-of-type(-5)"));
            Check(html.Select("#type p:nth-of-type(odd)"), "1", "3", "5", "7", "9");
            Check(html.Select("#type em:nth-of-type(2n-1)"), "1", "3", "5", "7", "9");
            Check(html.Select("#type p:nth-of-type(2n+1)"), "1", "3", "5", "7", "9");
            Check(html.Select("#type span:nth-of-type(2n+3)"), "3", "5", "7", "9");
            Check(html.Select("#type p:nth-of-type(even)"), "2", "4", "6", "8", "10");
            Check(html.Select("#type p:nth-of-type(2n)"), "2", "4", "6", "8", "10");
            Check(html.Select("#type p:nth-of-type(3n-1)"), "2", "5", "8");
            Check(html.Select("#type p:nth-of-type(-2n+5)"), "1", "3", "5");
            Check(html.Select("#type :nth-of-type(+5)"), "5", "5", "5", "5");
        }

	
        [Test]
        public void NthLastChild_Advanced()
        {
            Check(html.Select("#pseudo :nth-last-child(-5)"));
            Check(html.Select("#pseudo :nth-last-child(odd)"), "2", "4", "6", "8", "10");
            Check(html.Select("#pseudo :nth-last-child(2n-1)"), "2", "4", "6", "8", "10");
            Check(html.Select("#pseudo :nth-last-child(2n+1)"), "2", "4", "6", "8", "10");
            Check(html.Select("#pseudo :nth-last-child(2n+3)"), "2", "4", "6", "8");
            Check(html.Select("#pseudo :nth-last-child(even)"), "1", "3", "5", "7", "9");
            Check(html.Select("#pseudo :nth-last-child(2n)"), "1", "3", "5", "7", "9");
            Check(html.Select("#pseudo :nth-last-child(3n-1)"), "3", "6", "9");

            Check(html.Select("#pseudo :nth-last-child(-2n+5)"), "6", "8", "10");
            Check(html.Select("#pseudo :nth-last-child(+5)"), "6");
        }

        [Test]
        public void NthLastOfType_Advanced()
        {
            Check(html.Select("#type :nth-last-of-type(-5)"));
            Check(html.Select("#type p:nth-last-of-type(odd)"), "2", "4", "6", "8", "10");
            Check(html.Select("#type em:nth-last-of-type(2n-1)"), "2", "4", "6", "8", "10");
            Check(html.Select("#type p:nth-last-of-type(2n+1)"), "2", "4", "6", "8", "10");
            Check(html.Select("#type span:nth-last-of-type(2n+3)"), "2", "4", "6", "8");
            Check(html.Select("#type p:nth-last-of-type(even)"), "1", "3", "5", "7", "9");
            Check(html.Select("#type p:nth-last-of-type(2n)"), "1", "3", "5", "7", "9");
            Check(html.Select("#type p:nth-last-of-type(3n-1)"), "3", "6", "9");

            Check(html.Select("#type span:nth-last-of-type(-2n+5)"), "6", "8", "10");
            Check(html.Select("#type :nth-last-of-type(+5)"), "6", "6", "6", "6");
        }
	
        [Test]
        public void FirstOfType()
        {
            Check(html.Select("div:not(#only) :first-of-type"), "1", "1", "1", "1", "1");
        }

        [Test]
        public void LastOfType()
        {
            Check(html.Select("div:not(#only) :last-of-type"), "10", "10", "10", "10", "10");
        }

        [Test]
        public void Empty()
        {
            Elements sel = html.Select(":empty");
            Assert.AreEqual(3, sel.Count);
            Assert.AreEqual("head", sel[0].TagName());
            Assert.AreEqual("br", sel[1].TagName());
            Assert.AreEqual("p", sel[2].TagName());
        }
	
        [Test]
        public void OnlyChild()
        {
            Elements sel = html.Select("span :only-child");
            Assert.AreEqual(1, sel.Count);
            Assert.AreEqual("br", sel[0].TagName());
		
            Check(html.Select("#only :only-child"), "only");
        }
	
        [Test]
        public void OnlyOfType()
        {
            Elements sel = html.Select(":only-of-type");
            Assert.AreEqual(6, sel.Count);
            Assert.AreEqual("head", sel[0].TagName());
            Assert.AreEqual("body", sel[1].TagName());
            Assert.AreEqual("span", sel[2].TagName());
            Assert.AreEqual("br", sel[3].TagName());
            Assert.AreEqual("p", sel[4].TagName());
            Assert.IsTrue(sel[4].HasClass("empty"));
            Assert.AreEqual("em", sel[5].TagName());
        }
        
        protected void Check(Elements result, params string[] expectedContent)
        {
            Assert.AreEqual(expectedContent.Length, result.Count, "Number of elements");
            for (int i = 0; i < expectedContent.Length; i++)
            {
                Assert.NotNull(result[i]);
                Assert.AreEqual(expectedContent[i], result[i].OwnText(), "Expected element");
            }
        }
    }
}
