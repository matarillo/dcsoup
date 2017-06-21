using NUnit.Framework;
using Supremes.Nodes;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Nodes
#else
namespace Supremes.Test.net45.Nodes
#endif
{
    [TestFixture]
    public class TextNodeTest
    {
        [Test]
        public void TestBlank()
        {
            TextNode one = new TextNode("", "");
            TextNode two = new TextNode("     ", "");
            TextNode three = new TextNode("  \n\n   ", "");
            TextNode four = new TextNode("Hello", "");
            TextNode five = new TextNode("  \nHello ", "");

            Assert.IsTrue(one.IsBlank);
            Assert.IsTrue(two.IsBlank);
            Assert.IsTrue(three.IsBlank);
            Assert.IsFalse(four.IsBlank);
            Assert.IsFalse(five.IsBlank);
        }

        [Test]
        public void TestTextBean()
        {
            Document doc = Dcsoup.Parse("<p>One <span>two &amp;</span> three &amp;</p>");
            Element p = doc.Select("p").First;

            Element span = doc.Select("span").First;
            Assert.AreEqual("two &", span.Text);
            TextNode spanText = (TextNode)span.ChildNode(0);
            Assert.AreEqual("two &", spanText.Text);

            TextNode tn = (TextNode)p.ChildNode(2);
            Assert.AreEqual(" three &", tn.Text);

            tn.Text = " POW!";
            Assert.AreEqual("One <span>two &amp;</span> POW!", TextUtil.StripNewlines(p.Html));

            tn.Attr("text", "kablam &");
            Assert.AreEqual("kablam &", tn.Text);
            Assert.AreEqual("One <span>two &amp;</span>kablam &amp;", TextUtil.StripNewlines(p.Html));
        }

        [Test]
        public void TestSplitText()
        {
            Document doc = Dcsoup.Parse("<div>Hello there</div>");
            Element div = doc.Select("div").First;
            TextNode tn = (TextNode)div.ChildNode(0);
            TextNode tail = tn.SplitText(6);
            Assert.AreEqual("Hello ", tn.WholeText);
            Assert.AreEqual("there", tail.WholeText);
            tail.Text = "there!";
            Assert.AreEqual("Hello there!", div.Text);
            Assert.IsTrue(tn.Parent == tail.Parent);
        }

        [Test]
        public void TestSplitAnEmbolden()
        {
            Document doc = Dcsoup.Parse("<div>Hello there</div>");
            Element div = doc.Select("div").First;
            TextNode tn = (TextNode)div.ChildNode(0);
            TextNode tail = tn.SplitText(6);
            tail.Wrap("<b></b>");

            Assert.AreEqual("Hello <b>there</b>", TextUtil.StripNewlines(div.Html)); // not great that we get \n<b>there there... must correct
        }

        [Test]
        public void TestWithSupplementaryCharacter()
        {
            Document doc = Dcsoup.Parse(char.ConvertFromUtf32(135361));
            TextNode t = doc.Body.TextNodes[0];
            Assert.AreEqual(char.ConvertFromUtf32(135361), t.OuterHtml.Trim());
        }
    }
}
