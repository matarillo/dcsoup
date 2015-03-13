using NUnit.Framework;
using System;

namespace Supremes.Test.Nodes
{
    [TestFixture]
    public class AttributeTest
    {
        [Test]
        public void Html()
        {
            Supremes.Nodes.Attribute attr = new Supremes.Nodes.Attribute("key", "value &");
            Assert.AreEqual("key=\"value &amp;\"", attr.Html());
            Assert.AreEqual(attr.Html(), attr.ToString());
        }

        [Test]
        public void TestWithSupplementaryCharacterInAttributeKeyAndValue()
        {
            String s = char.ConvertFromUtf32(135361);
            Supremes.Nodes.Attribute attr = new Supremes.Nodes.Attribute(s, "A" + s + "B");
            Assert.AreEqual(s + "=\"A" + s + "B\"", attr.Html());
            Assert.AreEqual(attr.Html(), attr.ToString());
        }
    }
}
