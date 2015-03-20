using NUnit.Framework;
using Supremes.Nodes;
using Supremes.Select;
using System.Collections.Generic;
using System.Text;

namespace Supremes.Test.Nodes
{
    [TestFixture]
    public class NodeTest
    {
        [Test]
        public void HandlesBaseUri()
        {
            Tag tag = Tag.ValueOf("a");
            Attributes attribs = new Attributes();
            attribs["relHref"] = "/foo";
            attribs["absHref"] = "http://bar/qux";

            Element noBase = new Element(tag, "", attribs);
            Assert.AreEqual("", noBase.AbsUrl("relHref")); // with no base, should NOT fallback to href attrib, whatever it is
            Assert.AreEqual("http://bar/qux", noBase.AbsUrl("absHref")); // no base but valid attrib, return attrib

            Element withBase = new Element(tag, "http://foo/", attribs);
            Assert.AreEqual("http://foo/foo", withBase.AbsUrl("relHref")); // construct abs from base + rel
            Assert.AreEqual("http://bar/qux", withBase.AbsUrl("absHref")); // href is abs, so returns that
            Assert.AreEqual("", withBase.AbsUrl("noval"));

            Element dodgyBase = new Element(tag, "wtf://no-such-protocol/", attribs);
            Assert.AreEqual("http://bar/qux", dodgyBase.AbsUrl("absHref")); // base fails, but href good, so get that
            Assert.AreEqual("", dodgyBase.AbsUrl("relHref")); // base fails, only rel href, so return nothing 
        }

        [Test]
        public void SetBaseUriIsRecursive()
        {
            Document doc = Dcsoup.Parse("<div><p></p></div>");
            string baseUri = "http://jsoup.org";
            doc.SetBaseUri(baseUri);

            Assert.AreEqual(baseUri, doc.BaseUri);
            Assert.AreEqual(baseUri, doc.Select("div").First.BaseUri);
            Assert.AreEqual(baseUri, doc.Select("p").First.BaseUri);
        }

        [Test]
        public void HandlesAbsPrefix()
        {
            Document doc = Dcsoup.Parse("<a href=/foo>Hello</a>", "http://jsoup.org/");
            Element a = doc.Select("a").First;
            Assert.AreEqual("/foo", a.Attr("href"));
            Assert.AreEqual("http://jsoup.org/foo", a.Attr("abs:href"));
            Assert.IsTrue(a.HasAttr("abs:href"));
        }

        [Test]
        public void HandlesAbsOnImage()
        {
            Document doc = Dcsoup.Parse("<p><img src=\"/rez/osi_logo.png\" /></p>", "http://jsoup.org/");
            Element img = doc.Select("img").First;
            Assert.AreEqual("http://jsoup.org/rez/osi_logo.png", img.Attr("abs:src"));
            Assert.AreEqual(img.AbsUrl("src"), img.Attr("abs:src"));
        }

        [Test]
        public void HandlesAbsPrefixOnHasAttr()
        {
            // 1: no abs url; 2: has abs url
            Document doc = Dcsoup.Parse("<a id=1 href='/foo'>One</a> <a id=2 href='http://jsoup.org/'>Two</a>");
            Element one = doc.Select("#1").First;
            Element two = doc.Select("#2").First;

            Assert.IsFalse(one.HasAttr("abs:href"));
            Assert.IsTrue(one.HasAttr("href"));
            Assert.AreEqual("", one.AbsUrl("href"));

            Assert.IsTrue(two.HasAttr("abs:href"));
            Assert.IsTrue(two.HasAttr("href"));
            Assert.AreEqual("http://jsoup.org/", two.AbsUrl("href"));
        }

        [Test]
        public void LiteralAbsPrefix()
        {
            // if there is a literal attribute "abs:xxx", don't try and make absolute.
            Document doc = Dcsoup.Parse("<a abs:href='odd'>One</a>");
            Element el = doc.Select("a").First;
            Assert.IsTrue(el.HasAttr("abs:href"));
            Assert.AreEqual("odd", el.Attr("abs:href"));
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: in file URI, double slash // is required
        /// </remarks>
        [Test]
        public void HandleAbsOnFileUris()
        {
            Document doc = Dcsoup.Parse("<a href='password'>One/a><a href='/var/log/messages'>Two</a>", "file:///etc/"); // double slash // is required
            Element one = doc.Select("a").First;
            Assert.AreEqual("file:///etc/password", one.AbsUrl("href")); // double slash // is required
            Element two = doc.Select("a")[1];
            Assert.AreEqual("file:///var/log/messages", two.AbsUrl("href")); // double slash // is required
        }

        [Test]
        public void HandleAbsOnLocalhostFileUris()
        {
            Document doc = Dcsoup.Parse("<a href='password'>One/a><a href='/var/log/messages'>Two</a>", "file://localhost/etc/");
            Element one = doc.Select("a").First;
            Assert.AreEqual("file://localhost/etc/password", one.AbsUrl("href"));
        }

        [Test]
        public void HandlesAbsOnProtocolessAbsoluteUris()
        {
            Document doc1 = Dcsoup.Parse("<a href='//example.net/foo'>One</a>", "http://example.com/");
            Document doc2 = Dcsoup.Parse("<a href='//example.net/foo'>One</a>", "https://example.com/");

            Element one = doc1.Select("a").First;
            Element two = doc2.Select("a").First;

            Assert.AreEqual("http://example.net/foo", one.AbsUrl("href"));
            Assert.AreEqual("https://example.net/foo", two.AbsUrl("href"));

            Document doc3 = Dcsoup.Parse("<img src=//www.google.com/images/errors/logo_sm.gif alt=Google>", "https://google.com");
            Assert.AreEqual("https://www.google.com/images/errors/logo_sm.gif", doc3.Select("img").Attr("abs:src"));
        }

        /*
        Test for an issue with Java's abs URL handler.
         */

        [Test]
        public void AbsHandlesRelativeQuery()
        {
            Document doc = Dcsoup.Parse("<a href='?foo'>One</a> <a href='bar.html?foo'>Two</a>", "http://jsoup.org/path/file?bar");

            Element a1 = doc.Select("a").First;
            Assert.AreEqual("http://jsoup.org/path/file?foo", a1.AbsUrl("href"));

            Element a2 = doc.Select("a")[1];
            Assert.AreEqual("http://jsoup.org/path/bar.html?foo", a2.AbsUrl("href"));
        }

        [Test]
        public void TestRemove()
        {
            Document doc = Dcsoup.Parse("<p>One <span>two</span> three</p>");
            Element p = doc.Select("p").First;
            p.ChildNode(0).Remove();

            Assert.AreEqual("two three", p.Text);
            Assert.AreEqual("<span>two</span> three", TextUtil.StripNewlines(p.Html));
        }

        [Test]
        public void TestReplace()
        {
            Document doc = Dcsoup.Parse("<p>One <span>two</span> three</p>");
            Element p = doc.Select("p").First;
            Element insert = doc.CreateElement("em");
            insert.Text = "foo";
            p.ChildNode(1).ReplaceWith(insert);

            Assert.AreEqual("One <em>foo</em> three", p.Html);
        }

        [Test]
        public void OwnerDocument()
        {
            Document doc = Dcsoup.Parse("<p>Hello");
            Element p = doc.Select("p").First;
            Assert.IsTrue(p.OwnerDocument == doc);
            Assert.IsTrue(doc.OwnerDocument == doc);
            Assert.IsNull(doc.Parent);
        }

        [Test]
        public void Before()
        {
            Document doc = Dcsoup.Parse("<p>One <b>two</b> three</p>");
            Element newNode = new Element(Tag.ValueOf("em"), "");
            newNode.AppendText("four");

            doc.Select("b").First.Before(newNode);
            Assert.AreEqual("<p>One <em>four</em><b>two</b> three</p>", doc.Body.Html);

            doc.Select("b").First.Before("<i>five</i>");
            Assert.AreEqual("<p>One <em>four</em><i>five</i><b>two</b> three</p>", doc.Body.Html);
        }

        [Test]
        public void After()
        {
            Document doc = Dcsoup.Parse("<p>One <b>two</b> three</p>");
            Element newNode = new Element(Tag.ValueOf("em"), "");
            newNode.AppendText("four");

            doc.Select("b").First.After(newNode);
            Assert.AreEqual("<p>One <b>two</b><em>four</em> three</p>", doc.Body.Html);

            doc.Select("b").First.After("<i>five</i>");
            Assert.AreEqual("<p>One <b>two</b><i>five</i><em>four</em> three</p>", doc.Body.Html);
        }

        [Test]
        public void Unwrap()
        {
            Document doc = Dcsoup.Parse("<div>One <span>Two <b>Three</b></span> Four</div>");
            Element span = doc.Select("span").First;
            Node twoText = span.ChildNode(0);
            Node node = span.Unwrap();

            Assert.AreEqual("<div>One Two <b>Three</b> Four</div>", TextUtil.StripNewlines(doc.Body.Html));
            Assert.IsTrue(node is TextNode);
            Assert.AreEqual("Two ", ((TextNode)node).Text);
            Assert.AreEqual(node, twoText);
            Assert.AreEqual(node.Parent, doc.Select("div").First);
        }

        [Test]
        public void UnwrapNoChildren()
        {
            Document doc = Dcsoup.Parse("<div>One <span></span> Two</div>");
            Element span = doc.Select("span").First;
            Node node = span.Unwrap();
            Assert.AreEqual("<div>One  Two</div>", TextUtil.StripNewlines(doc.Body.Html));
            Assert.IsTrue(node == null);
        }

        [Test]
        public void Traverse()
        {
            Document doc = Dcsoup.Parse("<div><p>Hello</p></div><div>There</div>");
            StringBuilder accum = new StringBuilder();
            doc.Select("div").First.Traverse(new TraverseTestVisitor(accum));
            Assert.AreEqual("<div><p><#text></#text></p></div>", accum.ToString());
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
        public void OrphanNodeReturnsNullForSiblingElements()
        {
            Node node = new Element(Tag.ValueOf("p"), "");
            Element el = new Element(Tag.ValueOf("p"), "");

            Assert.AreEqual(0, node.SiblingIndex);
            Assert.AreEqual(0, node.SiblingNodes.Count);

            Assert.IsNull(node.PreviousSibling);
            Assert.IsNull(node.NextSibling);

            Assert.AreEqual(0, el.SiblingElements.Count);
            Assert.IsNull(el.PreviousElementSibling);
            Assert.IsNull(el.NextElementSibling);
        }

        [Test]
        public void NodeIsNotASiblingOfItself()
        {
            Document doc = Dcsoup.Parse("<div><p>One<p>Two<p>Three</div>");
            Element p2 = doc.Select("p")[1];

            Assert.AreEqual("Two", p2.Text);
            IReadOnlyList<Node> nodes = p2.SiblingNodes;
            Assert.AreEqual(2, nodes.Count);
            Assert.AreEqual("<p>One</p>", nodes[0].OuterHtml);
            Assert.AreEqual("<p>Three</p>", nodes[1].OuterHtml);
        }

        [Test]
        public void ChildNodesCopy()
        {
            Document doc = Dcsoup.Parse("<div id=1>Text 1 <p>One</p> Text 2 <p>Two<p>Three</div><div id=2>");
            Element div1 = doc.Select("#1").First;
            Element div2 = doc.Select("#2").First;
            IList<Node> divChildren = div1.ChildNodesCopy();
            Assert.AreEqual(5, divChildren.Count);
            TextNode tn1 = (TextNode)div1.ChildNode(0);
            TextNode tn2 = (TextNode)divChildren[0];
            tn2.Text = "Text 1 updated";
            Assert.AreEqual("Text 1 ", tn1.Text);
            div2.InsertChildren(-1, divChildren);
            Assert.AreEqual("<div id=\"1\">Text 1 <p>One</p> Text 2 <p>Two</p><p>Three</p></div><div id=\"2\">Text 1 updated"
                + "<p>One</p> Text 2 <p>Two</p><p>Three</p></div>", TextUtil.StripNewlines(doc.Body.Html));
        }
    }
}

