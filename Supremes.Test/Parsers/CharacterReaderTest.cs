using NUnit.Framework;
using Supremes.Parsers;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Parsers
#else
namespace Supremes.Test.net45.Parsers
#endif
{
    [TestFixture]
    public class CharacterReaderTest
    {
        [Test]
        public void Consume()
        {
            CharacterReader r = new CharacterReader("one");
            Assert.AreEqual(0, r.Pos());
            Assert.AreEqual('o', r.Current());
            Assert.AreEqual('o', r.Consume());
            Assert.AreEqual(1, r.Pos());
            Assert.AreEqual('n', r.Current());
            Assert.AreEqual(1, r.Pos());
            Assert.AreEqual('n', r.Consume());
            Assert.AreEqual('e', r.Consume());
            Assert.IsTrue(r.IsEmpty());
            Assert.AreEqual(CharacterReader.EOF, r.Consume());
            Assert.IsTrue(r.IsEmpty());
            Assert.AreEqual(CharacterReader.EOF, r.Consume());
        }

        [Test]
        public void Unconsume()
        {
            CharacterReader r = new CharacterReader("one");
            Assert.AreEqual('o', r.Consume());
            Assert.AreEqual('n', r.Current());
            r.Unconsume();
            Assert.AreEqual('o', r.Current());

            Assert.AreEqual('o', r.Consume());
            Assert.AreEqual('n', r.Consume());
            Assert.AreEqual('e', r.Consume());
            Assert.IsTrue(r.IsEmpty());
            r.Unconsume();
            Assert.IsFalse(r.IsEmpty());
            Assert.AreEqual('e', r.Current());
            Assert.AreEqual('e', r.Consume());
            Assert.IsTrue(r.IsEmpty());

            Assert.AreEqual(CharacterReader.EOF, r.Consume());
            r.Unconsume();
            Assert.IsTrue(r.IsEmpty());
            Assert.AreEqual(CharacterReader.EOF, r.Current());
        }

        [Test]
        public void Mark()
        {
            CharacterReader r = new CharacterReader("one");
            r.Consume();
            r.Mark();
            Assert.AreEqual('n', r.Consume());
            Assert.AreEqual('e', r.Consume());
            Assert.IsTrue(r.IsEmpty());
            r.RewindToMark();
            Assert.AreEqual('n', r.Consume());
        }

        [Test]
        public void ConsumeToEnd() {
        string @in = "one two three";
        CharacterReader r = new CharacterReader(@in);
        string toEnd = r.ConsumeToEnd();
        Assert.AreEqual(@in, toEnd);
        Assert.IsTrue(r.IsEmpty());
    }

        [Test]
        public void NextIndexOfChar() {
        string @in = "blah blah";
        CharacterReader r = new CharacterReader(@in);

        Assert.AreEqual(-1, r.NextIndexOf('x'));
        Assert.AreEqual(3, r.NextIndexOf('h'));
        string pull = r.ConsumeTo('h');
        Assert.AreEqual("bla", pull);
        r.Consume();
        Assert.AreEqual(2, r.NextIndexOf('l'));
        Assert.AreEqual(" blah", r.ConsumeToEnd());
        Assert.AreEqual(-1, r.NextIndexOf('x'));
    }

        [Test]
        public void NextIndexOfString() {
        string @in = "One Two something Two Three Four";
        CharacterReader r = new CharacterReader(@in);

        Assert.AreEqual(-1, r.NextIndexOf("Foo"));
        Assert.AreEqual(4, r.NextIndexOf("Two"));
        Assert.AreEqual("One Two ", r.ConsumeTo("something"));
        Assert.AreEqual(10, r.NextIndexOf("Two"));
        Assert.AreEqual("something Two Three Four", r.ConsumeToEnd());
        Assert.AreEqual(-1, r.NextIndexOf("Two"));
    }

        [Test]
        public void NextIndexOfUnmatched()
        {
            CharacterReader r = new CharacterReader("<[[one]]");
            Assert.AreEqual(-1, r.NextIndexOf("]]>"));
        }

        [Test]
        public void ConsumeToChar()
        {
            CharacterReader r = new CharacterReader("One Two Three");
            Assert.AreEqual("One ", r.ConsumeTo('T'));
            Assert.AreEqual("", r.ConsumeTo('T')); // on Two
            Assert.AreEqual('T', r.Consume());
            Assert.AreEqual("wo ", r.ConsumeTo('T'));
            Assert.AreEqual('T', r.Consume());
            Assert.AreEqual("hree", r.ConsumeTo('T')); // consume to end
        }

        [Test]
        public void ConsumeToString()
        {
            CharacterReader r = new CharacterReader("One Two Two Four");
            Assert.AreEqual("One ", r.ConsumeTo("Two"));
            Assert.AreEqual('T', r.Consume());
            Assert.AreEqual("wo ", r.ConsumeTo("Two"));
            Assert.AreEqual('T', r.Consume());
            Assert.AreEqual("wo Four", r.ConsumeTo("Qux"));
        }

        [Test]
        public void Advance()
        {
            CharacterReader r = new CharacterReader("One Two Three");
            Assert.AreEqual('O', r.Consume());
            r.Advance();
            Assert.AreEqual('e', r.Consume());
        }

        [Test]
        public void ConsumeToAny()
        {
            CharacterReader r = new CharacterReader("One &bar; qux");
            Assert.AreEqual("One ", r.ConsumeToAny('&', ';'));
            Assert.IsTrue(r.Matches('&'));
            Assert.IsTrue(r.Matches("&bar;"));
            Assert.AreEqual('&', r.Consume());
            Assert.AreEqual("bar", r.ConsumeToAny('&', ';'));
            Assert.AreEqual(';', r.Consume());
            Assert.AreEqual(" qux", r.ConsumeToAny('&', ';'));
        }

        [Test]
        public void ConsumeLetterSequence()
        {
            CharacterReader r = new CharacterReader("One &bar; qux");
            Assert.AreEqual("One", r.ConsumeLetterSequence());
            Assert.AreEqual(" &", r.ConsumeTo("bar;"));
            Assert.AreEqual("bar", r.ConsumeLetterSequence());
            Assert.AreEqual("; qux", r.ConsumeToEnd());
        }

        [Test]
        public void ConsumeLetterThenDigitSequence()
        {
            CharacterReader r = new CharacterReader("One12 Two &bar; qux");
            Assert.AreEqual("One12", r.ConsumeLetterThenDigitSequence());
            Assert.AreEqual(' ', r.Consume());
            Assert.AreEqual("Two", r.ConsumeLetterThenDigitSequence());
            Assert.AreEqual(" &bar; qux", r.ConsumeToEnd());
        }

        [Test]
        public void Matches()
        {
            CharacterReader r = new CharacterReader("One Two Three");
            Assert.IsTrue(r.Matches('O'));
            Assert.IsTrue(r.Matches("One Two Three"));
            Assert.IsTrue(r.Matches("One"));
            Assert.IsFalse(r.Matches("one"));
            Assert.AreEqual('O', r.Consume());
            Assert.IsFalse(r.Matches("One"));
            Assert.IsTrue(r.Matches("ne Two Three"));
            Assert.IsFalse(r.Matches("ne Two Three Four"));
            Assert.AreEqual("ne Two Three", r.ConsumeToEnd());
            Assert.IsFalse(r.Matches("ne"));
        }

        [Test]
        public void MatchesIgnoreCase()
        {
            CharacterReader r = new CharacterReader("One Two Three");
            Assert.IsTrue(r.MatchesIgnoreCase("O"));
            Assert.IsTrue(r.MatchesIgnoreCase("o"));
            Assert.IsTrue(r.Matches('O'));
            Assert.IsFalse(r.Matches('o'));
            Assert.IsTrue(r.MatchesIgnoreCase("One Two Three"));
            Assert.IsTrue(r.MatchesIgnoreCase("ONE two THREE"));
            Assert.IsTrue(r.MatchesIgnoreCase("One"));
            Assert.IsTrue(r.MatchesIgnoreCase("one"));
            Assert.AreEqual('O', r.Consume());
            Assert.IsFalse(r.MatchesIgnoreCase("One"));
            Assert.IsTrue(r.MatchesIgnoreCase("NE Two Three"));
            Assert.IsFalse(r.MatchesIgnoreCase("ne Two Three Four"));
            Assert.AreEqual("ne Two Three", r.ConsumeToEnd());
            Assert.IsFalse(r.MatchesIgnoreCase("ne"));
        }

        [Test]
        public void ContainsIgnoreCase()
        {
            CharacterReader r = new CharacterReader("One TWO three");
            Assert.IsTrue(r.ContainsIgnoreCase("two"));
            Assert.IsTrue(r.ContainsIgnoreCase("three"));
            // weird one: does not find one, because it scans for consistent case only
            Assert.IsFalse(r.ContainsIgnoreCase("one"));
        }

        [Test]
        public void MatchesAny()
        {
            char[] scan = { ' ', '\n', '\t' };
            CharacterReader r = new CharacterReader("One\nTwo\tThree");
            Assert.IsFalse(r.MatchesAny(scan));
            Assert.AreEqual("One", r.ConsumeToAny(scan));
            Assert.IsTrue(r.MatchesAny(scan));
            Assert.AreEqual('\n', r.Consume());
            Assert.IsFalse(r.MatchesAny(scan));
        }
    }
}
