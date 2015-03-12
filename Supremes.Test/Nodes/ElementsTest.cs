using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Supremes.Nodes;
using Supremes.Select;

namespace Supremes.Test.Nodes
{
    [TestFixture]
    public class ElementsTest
    {
        [Test]
        public void Filter()
        {
            string h = "<p>Excl</p><div class=headline><p>Hello</p><p>There</p></div><div class=headline><h1>Headline</h1></div>";
            Document doc = Dcsoup.Parse(h);
            Elements els = doc.Select(".headline").Select("p");
            Assert.AreEqual(2, els.Count);
            Assert.AreEqual("Hello", els[0].Text());
            Assert.AreEqual("There", els[1].Text());
        }
        
        [Test]
        public void Attributes()
        {
            string h = "<p title=foo><p title=bar><p class=foo><p class=bar>";
            Document doc = Dcsoup.Parse(h);
            Elements withTitle = doc.Select("p[title]");
            Assert.AreEqual(2, withTitle.Count);
            Assert.IsTrue(withTitle.HasAttr("title"));
            Assert.IsFalse(withTitle.HasAttr("class"));
            Assert.AreEqual("foo", withTitle.Attr("title"));

            withTitle.RemoveAttr("title");
            Assert.AreEqual(2, withTitle.Count); // existing Elements are not reevaluated
            Assert.AreEqual(0, doc.Select("p[title]").Count);

            Elements ps = doc.Select("p").Attr("style", "classy");
            Assert.AreEqual(4, ps.Count);
            Assert.AreEqual("classy", ps.Last().Attr("style"));
            Assert.AreEqual("bar", ps.Last().Attr("class"));
        }
    
        [Test]
        public void HasAttr()
        {
            Document doc = Dcsoup.Parse("<p title=foo><p title=bar><p class=foo><p class=bar>");
            Elements ps = doc.Select("p");
            Assert.IsTrue(ps.HasAttr("class"));
            Assert.IsFalse(ps.HasAttr("style"));
        }
        
        [Test]
        public void HasAbsAttr()
        {
            Document doc = Dcsoup.Parse("<a id=1 href='/foo'>One</a> <a id=2 href='http://jsoup.org'>Two</a>");
            Elements one = doc.Select("#1");
            Elements two = doc.Select("#2");
            Elements both = doc.Select("a");
            Assert.IsFalse(one.HasAttr("abs:href"));
            Assert.IsTrue(two.HasAttr("abs:href"));
            Assert.IsTrue(both.HasAttr("abs:href")); // hits on #2
        }
        
        [Test]
        public void Attr()
        {
            Document doc = Dcsoup.Parse("<p title=foo><p title=bar><p class=foo><p class=bar>");
            string classVal = doc.Select("p").Attr("class");
            Assert.AreEqual("foo", classVal);
        }
        
        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: trailing slash to domain name
        /// </remarks>
        [Test]
        public void AbsAttr()
        {
            Document doc = Dcsoup.Parse("<a id=1 href='/foo'>One</a> <a id=2 href='http://jsoup.org'>Two</a>");
            Elements one = doc.Select("#1");
            Elements two = doc.Select("#2");
            Elements both = doc.Select("a");

            Assert.AreEqual("", one.Attr("abs:href"));
            Assert.AreEqual("http://jsoup.org/", two.Attr("abs:href")); // trailing slash
            Assert.AreEqual("http://jsoup.org/", both.Attr("abs:href")); // trailing slash
        }
        
        [Test]
        public void Classes()
        {
            Document doc = Dcsoup.Parse("<div><p class='mellow yellow'></p><p class='red green'></p>");

            Elements els = doc.Select("p");
            Assert.IsTrue(els.HasClass("red"));
            Assert.IsFalse(els.HasClass("blue"));
            els.AddClass("blue");
            els.RemoveClass("yellow");
            els.ToggleClass("mellow");

            Assert.AreEqual("blue", els[0].ClassName());
            Assert.AreEqual("red green blue mellow", els[1].ClassName());
        }
        
        [Test]
        public void Text()
        {
            string h = "<div><p>Hello<p>there<p>world</div>";
            Document doc = Dcsoup.Parse(h);
            Assert.AreEqual("Hello there world", doc.Select("div > *").Text());
        }
        
        [Test]
        public void hasText()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello</p></div><div><p></p></div>");
            Elements divs = doc.Select("div");
            Assert.IsTrue(divs.HasText());
            Assert.IsFalse(doc.Select("div + div").HasText());
        }
        
        [Test]
        public void Html()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello</p></div><div><p>There</p></div>");
            Elements divs = doc.Select("div");
            Assert.AreEqual("<p>Hello</p>\n<p>There</p>", divs.Html());
        }
        
        [Test]
        public void OuterHtml()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello</p></div><div><p>There</p></div>");
            Elements divs = doc.Select("div");
            Assert.AreEqual("<div><p>Hello</p></div><div><p>There</p></div>", TextUtil.StripNewlines(divs.OuterHtml()));
        }
        
        [Test]
        public void SetHtml()
        {
            Document doc = Dcsoup.Parse("<p>One</p><p>Two</p><p>Three</p>");
            Elements ps = doc.Select("p");
        
            ps.Prepend("<b>Bold</b>").Append("<i>Ital</i>");
            Assert.AreEqual("<p><b>Bold</b>Two<i>Ital</i></p>", TextUtil.StripNewlines(ps[1].OuterHtml()));
        
            ps.Html("<span>Gone</span>");
            Assert.AreEqual("<p><span>Gone</span></p>", TextUtil.StripNewlines(ps[1].OuterHtml()));
        }
        
        [Test]
        public void Val()
        {
            Document doc = Dcsoup.Parse("<input value='one' /><textarea>two</textarea>");
            Elements els = doc.Select("input, textarea");
            Assert.AreEqual(2, els.Count);
            Assert.AreEqual("one", els.Val());
            Assert.AreEqual("two", els.Last().Val());
        
            els.Val("three");
            Assert.AreEqual("three", els.First().Val());
            Assert.AreEqual("three", els.Last().Val());
            Assert.AreEqual("<textarea>three</textarea>", els.Last().OuterHtml());
        }

        [Test]
        public void Before()
        {
            Document doc = Dcsoup.Parse("<p>This <a>is</a> <a>jsoup</a>.</p>");
            doc.Select("a").Before("<span>foo</span>");
            Assert.AreEqual("<p>This <span>foo</span><a>is</a> <span>foo</span><a>jsoup</a>.</p>", TextUtil.StripNewlines(doc.Body().Html()));
        }
    
        [Test]
        public void After()
        {
            Document doc = Dcsoup.Parse("<p>This <a>is</a> <a>jsoup</a>.</p>");
            doc.Select("a").After("<span>foo</span>");
            Assert.AreEqual("<p>This <a>is</a><span>foo</span> <a>jsoup</a><span>foo</span>.</p>", TextUtil.StripNewlines(doc.Body().Html()));
        }

        [Test]
        public void Wrap()
        {
            string h = "<p><b>This</b> is <b>jsoup</b></p>";
            Document doc = Dcsoup.Parse(h);
            doc.Select("b").Wrap("<i></i>");
            Assert.AreEqual("<p><i><b>This</b></i> is <i><b>jsoup</b></i></p>", doc.Body().Html());
        }

        [Test]
        public void WrapDiv()
        {
            string h = "<p><b>This</b> is <b>jsoup</b>.</p> <p>How do you like it?</p>";
            Document doc = Dcsoup.Parse(h);
            doc.Select("p").Wrap("<div></div>");
            Assert.AreEqual("<div><p><b>This</b> is <b>jsoup</b>.</p></div> <div><p>How do you like it?</p></div>",
                TextUtil.StripNewlines(doc.Body().Html()));
        }

        [Test]
        public void Unwrap()
        {
            string h = "<div><font>One</font> <font><a href=\"/\">Two</a></font></div";
            Document doc = Dcsoup.Parse(h);
            doc.Select("font").Unwrap();
            Assert.AreEqual("<div>One <a href=\"/\">Two</a></div>", TextUtil.StripNewlines(doc.Body().Html()));
        }

        [Test]
        public void UnwrapP()
        {
            string h = "<p><a>One</a> Two</p> Three <i>Four</i> <p>Fix <i>Six</i></p>";
            Document doc = Dcsoup.Parse(h);
            doc.Select("p").Unwrap();
            Assert.AreEqual("<a>One</a> Two Three <i>Four</i> Fix <i>Six</i>", TextUtil.StripNewlines(doc.Body().Html()));
        }

        [Test]
        public void Empty()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello <b>there</b></p> <p>now!</p></div>");
            doc.OutputSettings().PrettyPrint(false);

            doc.Select("p").Empty();
            Assert.AreEqual("<div><p></p> <p></p></div>", doc.Body().Html());
        }

        [Test]
        public void Remove()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello <b>there</b></p> jsoup <p>now!</p></div>");
            doc.OutputSettings().PrettyPrint(false);
        
            doc.Select("p").Remove();
            Assert.AreEqual("<div> jsoup </div>", doc.Body().Html());
        }
    
        [Test]
        public void Eq()
        {
            string h = "<p>Hello<p>there<p>world";
            Document doc = Dcsoup.Parse(h);
            Assert.AreEqual("there", doc.Select("p").Eq(1).Text());
            Assert.AreEqual("there", doc.Select("p")[1].Text());
        }
    
        [Test]
        public void Is()
        {
            string h = "<p>Hello<p title=foo>there<p>world";
            Document doc = Dcsoup.Parse(h);
            Elements ps = doc.Select("p");
            Assert.IsTrue(ps.Is("[title=foo]"));
            Assert.IsFalse(ps.Is("[title=bar]"));
        }

        [Test]
        public void Parents()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello</p></div><p>There</p>");
            Elements parents = doc.Select("p").Parents();

            Assert.AreEqual(3, parents.Count);
            Assert.AreEqual("div", parents[0].TagName());
            Assert.AreEqual("body", parents[1].TagName());
            Assert.AreEqual("html", parents[2].TagName());
        }

        [Test]
        public void Not()
        {
            Document doc = Dcsoup.Parse("<div id=1><p>One</p></div> <div id=2><p><span>Two</span></p></div>");

            Elements div1 = doc.Select("div").Not(":has(p > span)");
            Assert.AreEqual(1, div1.Count);
            Assert.AreEqual("1", div1.First().Id());

            Elements div2 = doc.Select("div").Not("#1");
            Assert.AreEqual(1, div2.Count);
            Assert.AreEqual("2", div2.First().Id());
        }

        [Test]
        public void TagNameSet()
        {
            Document doc = Dcsoup.Parse("<p>Hello <i>there</i> <i>now</i></p>");
            doc.Select("i").TagName("em");

            Assert.AreEqual("<p>Hello <em>there</em> <em>now</em></p>", doc.Body().Html());
        }

        [Test]
        public void Traverse()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello</p></div><div>There</div>");
            StringBuilder accum = new StringBuilder();
            doc.Select("div").Traverse(new TraverseTestVisitor(accum));
            Assert.AreEqual("<div><p><#text></#text></p></div><div><#text></#text></div>", accum.ToString());
        }
        
        private class TraverseTestVisitor : INodeVisitor
        {
            private readonly StringBuilder accum;

            public TraverseTestVisitor(StringBuilder accum)
            {
                this.accum = accum;
            }
            
            public void Head(Node node, int depth)
            {
                accum.Append("<" + node.NodeName + ">");
            }

            public void Tail(Node node, int depth)
            {
                accum.Append("</" + node.NodeName + ">");
            }
        }

        [Test]
        public void Forms()
        {
            Document doc = Dcsoup.Parse("<form id=1><input name=q></form><div /><form id=2><input name=f></form>");
            Elements els = doc.Select("*");
            Assert.AreEqual(9, els.Count);

            IReadOnlyList<FormElement> forms = els.Forms();
            Assert.AreEqual(2, forms.Count);
            Assert.IsTrue(forms[0] != null);
            Assert.IsTrue(forms[1] != null);
            Assert.AreEqual("1", forms[0].Id());
            Assert.AreEqual("2", forms[1].Id());
        }

        [Test]
        public void ClassWithHyphen()
        {
            Document doc = Dcsoup.Parse("<p class='tab-nav'>Check</p>");
            Elements els = doc.GetElementsByClass("tab-nav");
            Assert.AreEqual(1, els.Count);
            Assert.AreEqual("Check", els.Text());
        }
    }
}