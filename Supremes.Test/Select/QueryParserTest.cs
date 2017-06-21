using System;
using NUnit.Framework;
using Supremes.Select;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Select
#else
namespace Supremes.Test.net45.Select
#endif
{
    [TestFixture]
    public class QueryParserTest
    {
        [Test]
        public void TestOrGetsCorrectPrecedence()
        {
            // tests that a selector "a b, c d, e f" evals to (a AND b) OR (c AND d) OR (e AND f)"
            // top level or, three child ands
            Evaluator eval = QueryParser.Parse("a b, c d, e f");
            Assert.IsTrue(eval is CombiningEvaluator.Or);
            CombiningEvaluator.Or or = (CombiningEvaluator.Or)eval;
            Assert.AreEqual(3, or.evaluators.Count);
            foreach (Evaluator innerEval in or.evaluators)
            {
                Assert.IsTrue(innerEval is CombiningEvaluator.And);
                CombiningEvaluator.And and = (CombiningEvaluator.And)innerEval;
                Assert.AreEqual(2, and.evaluators.Count);
                Assert.IsTrue(and.evaluators[0] is Evaluator.Tag);
                Assert.IsTrue(and.evaluators[1] is StructuralEvaluator.Parent);
            }
        }
        
        [Test]
        public void TestParsesMultiCorrectly()
        {
            Evaluator eval = QueryParser.Parse(".foo > ol, ol > li + li");
            Assert.IsTrue(eval is CombiningEvaluator.Or);
            CombiningEvaluator.Or or = (CombiningEvaluator.Or)eval;
            Assert.AreEqual(2, or.evaluators.Count);

            CombiningEvaluator.And andLeft = (CombiningEvaluator.And)or.evaluators[0];
            CombiningEvaluator.And andRight = (CombiningEvaluator.And)or.evaluators[1];

            Assert.AreEqual("ol :ImmediateParent.foo", andLeft.ToString());
            Assert.AreEqual(2, andLeft.evaluators.Count);
            Assert.AreEqual("li :prevli :ImmediateParentol", andRight.ToString());
            Assert.AreEqual(2, andLeft.evaluators.Count);
        }
    }
}
