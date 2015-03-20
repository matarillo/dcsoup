using NUnit.Framework;
using Supremes.Nodes;
using System;

namespace Supremes.Test.Nodes
{
    [TestFixture]
    public class TagTest
    {
        [Test]
        public void IsCaseInsensitive()
        {
            Tag p1 = Tag.ValueOf("P");
            Tag p2 = Tag.ValueOf("p");
            Assert.AreEqual(p1, p2);
        }

        [Test]
        public void Trims()
        {
            Tag p1 = Tag.ValueOf("p");
            Tag p2 = Tag.ValueOf(" p ");
            Assert.AreEqual(p1, p2);
        }

        [Test]
        public void Equality()
        {
            Tag p1 = Tag.ValueOf("p");
            Tag p2 = Tag.ValueOf("p");
            Assert.IsTrue(p1.Equals(p2));
            Assert.IsTrue(p1 == p2);
        }

        [Test]
        public void DivSemantics()
        {
            Tag div = Tag.ValueOf("div");

            Assert.IsTrue(div.IsBlock);
            Assert.IsTrue(div.IsFormattedAsBlock);
        }

        [Test]
        public void PSemantics()
        {
            Tag p = Tag.ValueOf("p");

            Assert.IsTrue(p.IsBlock);
            Assert.IsFalse(p.IsFormattedAsBlock);
        }

        [Test]
        public void ImgSemantics()
        {
            Tag img = Tag.ValueOf("img");
            Assert.IsTrue(img.IsInline);
            Assert.IsTrue(img.IsSelfClosing);
            Assert.IsFalse(img.IsBlock);
        }

        [Test]
        public void DefaultSemantics()
        {
            Tag foo = Tag.ValueOf("foo"); // not defined
            Tag foo2 = Tag.ValueOf("FOO");

            Assert.AreEqual(foo, foo2);
            Assert.IsTrue(foo.IsInline);
            Assert.IsTrue(foo.IsFormattedAsBlock);
        }

        [Test]
        public void ValueOfChecksNotNull()
        {
            var ex = Assert.Throws(typeof(ArgumentException), () =>
            {
                Tag.ValueOf(null);
            });
            Assert.AreEqual("Object must not be null", ex.Message);
        }

        [Test]
        public void ValueOfChecksNotEmpty()
        {
            var ex = Assert.Throws(typeof(ArgumentException), () =>
            {
                Tag.ValueOf(" ");
            });
            Assert.AreEqual("String must not be empty", ex.Message);
        }
    }
}
