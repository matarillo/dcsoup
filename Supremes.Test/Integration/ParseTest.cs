using NUnit.Framework;
using Supremes.Nodes;
using System;
using System.IO;
using System.Text;

namespace Supremes.Test.Integration
{
    [TestFixture]
    public class ParseTest
    {
        [Test]
        public void TestSmhBizArticle()
        {
            string @in = GetFilePath("/htmltests/smh-biz-article-1.html");
            Document doc = Dcsoup.ParseFile(@in, "UTF-8",
                "http://www.smh.com.au/business/the-boards-next-fear-the-female-quota-20100106-lteq.html");
            Assert.AreEqual("The board’s next fear: the female quota",
                    doc.Title()); // note that the apos in the source is a literal ’ (8217), not escaped or '
            Assert.AreEqual("en", doc.Select("html").Attr("xml:lang"));

            Elements articleBody = doc.Select(".articleBody > *");
            Assert.AreEqual(17, articleBody.Count);

            // todo: more tests!
        }

        [Test]
        public void TestNewsHomepage()
        {
            string @in = GetFilePath("/htmltests/news-com-au-home.html");
            Document doc = Dcsoup.ParseFile(@in, "UTF-8", "http://www.news.com.au/");
            Assert.AreEqual("News.com.au | News from Australia and around the world online | NewsComAu", doc.Title());
            Assert.AreEqual("Brace yourself for Metro meltdown", doc.Select(".id1225817868581 h4").Text().Trim());

            Element a = doc.Select("a[href=/entertainment/horoscopes]").First();
            Assert.AreEqual("/entertainment/horoscopes", a.Attr("href"));
            Assert.AreEqual("http://www.news.com.au/entertainment/horoscopes", a.Attr("abs:href"));

            Element hs = doc.Select("a[href*=naughty-corners-are-a-bad-idea]").First();
            Assert.AreEqual(
                    "http://www.heraldsun.com.au/news/naughty-corners-are-a-bad-idea-for-kids/story-e6frf7jo-1225817899003",
                    hs.Attr("href"));
            Assert.AreEqual(hs.Attr("href"), hs.Attr("abs:href"));
        }

        [Test]
        public void TestGoogleSearchIpod()
        {
            string @in = GetFilePath("/htmltests/google-ipod.html");
            Document doc = Dcsoup.ParseFile(@in, "UTF-8", "http://www.google.com/search?hl=en&q=ipod&aq=f&oq=&aqi=g10");
            Assert.AreEqual("ipod - Google Search", doc.Title());
            Elements results = doc.Select("h3.r > a");
            Assert.AreEqual(12, results.Count);
            Assert.AreEqual(
                    "http://news.google.com/news?hl=en&q=ipod&um=1&ie=UTF-8&ei=uYlKS4SbBoGg6gPf-5XXCw&sa=X&oi=news_group&ct=title&resnum=1&ved=0CCIQsQQwAA",
                    results[0].Attr("href"));
            Assert.AreEqual("http://www.apple.com/itunes/",
                    results[1].Attr("href"));
        }

        [Test]
        public void TestBinary()
        {
            string @in = GetFilePath("/htmltests/thumb.jpg");
            Document doc = Dcsoup.ParseFile(@in, "UTF-8");
            // nothing useful, but did not blow up
            Assert.IsTrue(doc.Text().Contains("gd-jpeg"));
        }

        [Test]
        public void TestYahooJp()
        {
            string @in = GetFilePath("/htmltests/yahoo-jp.html");
            Document doc = Dcsoup.ParseFile(@in, "UTF-8", "http://www.yahoo.co.jp/index.html"); // http charset is utf-8.
            Assert.AreEqual("Yahoo! JAPAN", doc.Title());
            Element a = doc.Select("a[href=t/2322m2]").First();
            Assert.AreEqual("http://www.yahoo.co.jp/_ylh=X3oDMTB0NWxnaGxsBF9TAzIwNzcyOTYyNjUEdGlkAzEyBHRtcGwDZ2Ex/t/2322m2",
                    a.Attr("abs:href")); // session put into <base>
            Assert.AreEqual("全国、人気の駅ランキング", a.Text());
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: trailing slash to domain name
        /// different behavior from jsoup: charset name is lower case
        /// </remarks>
        [Test]
        public void TestBaidu()
        {
            // tests <meta http-equiv="Content-Type" content="text/html;charset=gb2312">
            string @in = GetFilePath("/htmltests/baidu-cn-home.html");
            Document doc = Dcsoup.ParseFile(@in, null,
                    "http://www.baidu.com/"); // http charset is gb2312, but NOT specifying it, to test http-equiv parse
            Element submit = doc.Select("#su").First();
            Assert.AreEqual("百度一下", submit.Attr("value"));

            // test from attribute match
            submit = doc.Select("input[value=百度一下]").First();
            Assert.AreEqual("su", submit.Id());
            Element newsLink = doc.Select("a:Contains(新)").First();
            Assert.AreEqual("http://news.baidu.com/", newsLink.AbsUrl("href")); // trailing slash to domain name

            // check auto-detect from meta
            Assert.AreEqual("gb2312", doc.OutputSettings().Charset().WebName); // charset name is lower case
            Assert.AreEqual("<title>百度一下，你就知道      </title>", doc.Select("title").OuterHtml());

            doc.OutputSettings().Charset("ascii");
            Assert.AreEqual("<title>&#x767e;&#x5ea6;&#x4e00;&#x4e0b;&#xff0c;&#x4f60;&#x5c31;&#x77e5;&#x9053;      </title>",
                    doc.Select("title").OuterHtml());
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: charset name is lower case
        /// </remarks>
        [Test]
        public void TestBaiduVariant()
        {
            // tests <meta charset> when preceded by another <meta>
            string @in = GetFilePath("/htmltests/baidu-variant.html");
            Document doc = Dcsoup.ParseFile(@in, null,
                    "http://www.baidu.com/"); // http charset is gb2312, but NOT specifying it, to test http-equiv parse
            // check auto-detect from meta
            Assert.AreEqual("gb2312", doc.OutputSettings().Charset().WebName); // charset name is lower case
            Assert.AreEqual("<title>百度一下，你就知道</title>", doc.Select("title").OuterHtml());
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: charset name is lower case
        /// </remarks>
        [Test]
        public void TestHtml5Charset()
        {
            // test that <meta charset="gb2312"> works
            string @in = GetFilePath("/htmltests/meta-charset-1.html");
            Document doc = Dcsoup.ParseFile(@in, null, "http://example.com/"); //gb2312, has html5 <meta charset>
            Assert.AreEqual("新", doc.Text());
            Assert.AreEqual("gb2312", doc.OutputSettings().Charset().WebName); // charset name is lower case

            // double check, no charset, falls back to utf8 which is incorrect
            @in = GetFilePath("/htmltests/meta-charset-2.html"); //
            doc = Dcsoup.ParseFile(@in, null, "http://example.com"); // gb2312, no charset
            Assert.AreEqual("utf-8", doc.OutputSettings().Charset().WebName); // charset name is lower case
            Assert.IsFalse("新".Equals(doc.Text()));

            // confirm fallback to utf8
            @in = GetFilePath("/htmltests/meta-charset-3.html");
            doc = Dcsoup.ParseFile(@in, null, "http://example.com/"); // utf8, no charset
            Assert.AreEqual("utf-8", doc.OutputSettings().Charset().WebName); // charset name is lower case
            Assert.AreEqual("新", doc.Text());
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: charset name is lower case
        /// </remarks>
        [Test]
        public void TestBrokenHtml5CharsetWithASingleDoubleQuote()
        {
            Stream @in = GetStreamFrom("<html>\n" +
                    "<head><meta charset=UTF-8\"></head>\n" +
                    "<body></body>\n" +
                    "</html>");
            Document doc = Dcsoup.Parse(@in, null, "http://example.com/");
            Assert.AreEqual("utf-8", doc.OutputSettings().Charset().WebName); // charset name is lower case
        }

        [Test]
        public void TestNytArticle()
        {
            // has tags like <nyt_text>
            string @in = GetFilePath("/htmltests/nyt-article-1.html");
            Document doc = Dcsoup.ParseFile(@in, null, "http://www.nytimes.com/2010/07/26/business/global/26bp.html?hp");

            Element headline = doc.Select("nyt_headline[version=1.0]").First();
            Assert.AreEqual("As BP Lays Out Future, It Will Not Include Hayward", headline.Text());
        }

        [Test]
        public void TestYahooArticle()
        {
            string @in = GetFilePath("/htmltests/yahoo-article-1.html");
            Document doc = Dcsoup.ParseFile(@in, "UTF-8", "http://news.yahoo.com/s/nm/20100831/bs_nm/us_gm_china");
            Element p = doc.Select("p:Contains(Volt will be sold in the United States").First();
            Assert.AreEqual("In July, GM said its electric Chevrolet Volt will be sold in the United States at $41,000 -- $8,000 more than its nearest competitor, the Nissan Leaf.", p.Text());
        }


        public string GetFilePath(string resourceName)
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "." + resourceName));
        }

        private Stream GetStreamFrom(string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }
    }
}
