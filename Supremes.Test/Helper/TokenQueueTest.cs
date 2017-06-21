using NUnit.Framework;
using Supremes.Helper;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Helper
#else
namespace Supremes.Test.net45.Helper
#endif
{
    [TestFixture]
    public class TokenQueueTest
    {
        [Test]
        public void ChompBalanced()
        {
            TokenQueue tq = new TokenQueue(":contains(one (two) three) four");
            string pre = tq.ConsumeTo("(");
            string guts = tq.ChompBalanced('(', ')');
            string remainder = tq.Remainder();

            Assert.AreEqual(":contains", pre);
            Assert.AreEqual("one (two) three", guts);
            Assert.AreEqual(" four", remainder);
        }

        [Test]
        public void ChompEscapedBalanced()
        {
            TokenQueue tq = new TokenQueue(":contains(one (two) \\( \\) \\) three) four");
            string pre = tq.ConsumeTo("(");
            string guts = tq.ChompBalanced('(', ')');
            string remainder = tq.Remainder();

            Assert.AreEqual(":contains", pre);
            Assert.AreEqual("one (two) \\( \\) \\) three", guts);
            Assert.AreEqual("one (two) ( ) ) three", TokenQueue.Unescape(guts));
            Assert.AreEqual(" four", remainder);
        }

        [Test]
        public void ChompBalancedMatchesAsMuchAsPossible()
        {
            TokenQueue tq = new TokenQueue("unbalanced(something(or another");
            tq.ConsumeTo("(");
            string match = tq.ChompBalanced('(', ')');
            Assert.AreEqual("something(or another", match);
        }

        [Test]
        public void Unescape()
        {
            Assert.AreEqual("one ( ) \\", TokenQueue.Unescape("one \\( \\) \\\\"));
        }

        [Test]
        public void ChompToIgnoreCase()
        {
            string t = "<textarea>one < two </TEXTarea>";
            TokenQueue tq = new TokenQueue(t);
            string data = tq.ChompToIgnoreCase("</textarea");
            Assert.AreEqual("<textarea>one < two ", data);

            tq = new TokenQueue("<textarea> one two < three </oops>");
            data = tq.ChompToIgnoreCase("</textarea");
            Assert.AreEqual("<textarea> one two < three </oops>", data);
        }

        [Test]
        public void AddFirst()
        {
            TokenQueue tq = new TokenQueue("One Two");
            tq.ConsumeWord();
            tq.AddFirst("Three");
            Assert.AreEqual("Three Two", tq.Remainder());
        }
    }
}
