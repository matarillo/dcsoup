using NUnit.Framework;
using Supremes.Nodes;

namespace Supremes.Test.Nodes
{
    [TestFixture]
    public class AttributesTest
    {
        [Test]
        public void Html()
        {
            Attributes a = new Attributes();
            a["Tot"] = "a&p";
            a["Hello"] = "There";
            a["data-name"] = "Jsoup";

            Assert.AreEqual(3, a.Count);
            Assert.IsTrue(a.ContainsKey("tot"));
            Assert.IsTrue(a.ContainsKey("Hello"));
            Assert.IsTrue(a.ContainsKey("data-name"));
            Assert.AreEqual(1, a.Dataset.Count);
            Assert.AreEqual("Jsoup", a.Dataset["name"]);
            Assert.AreEqual("a&p", a["tot"]);

            Assert.AreEqual(" tot=\"a&amp;p\" hello=\"There\" data-name=\"Jsoup\"", a.Html());
            Assert.AreEqual(a.Html(), a.ToString());
        }
    }
}
