using Supremes.Nodes;
using System.Collections.Generic;

namespace Supremes.Select
{
    /// <summary>
    /// Base combining (and, or) evaluator.
    /// </summary>
    internal abstract class CombiningEvaluator : Evaluator
    {
        internal readonly List<Evaluator> evaluators;

        internal int num = 0;

        public CombiningEvaluator() : base()
        {
            evaluators = new List<Evaluator>();
        }

        internal CombiningEvaluator(ICollection<Evaluator> evaluators) : this()
        {
            this.evaluators.AddRange(evaluators);
            UpdateNumEvaluators();
        }

        internal Evaluator RightMostEvaluator()
        {
            return num > 0 ? evaluators[num - 1] : null;
        }

        internal void ReplaceRightMostEvaluator(Evaluator replacement)
        {
            evaluators[num - 1] = replacement;
        }

        internal void UpdateNumEvaluators()
        {
            // used so we don't need to bash on size() for every match test
            num = evaluators.Count;
        }

        internal sealed class And : CombiningEvaluator
        {
            internal And(ICollection<Evaluator> evaluators) : base(evaluators)
            {
            }

            internal And(params Evaluator[] evaluators) : base(evaluators)
            {
            }

            public override bool Matches(Element root, Element element)
            {
                for (int i = 0; i < num; i++)
                {
                    Evaluator s = evaluators[i];
                    if (!s.Matches(root, element))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override string ToString()
            {
                return string.Join(" ", evaluators);
            }
        }

        internal sealed class Or : CombiningEvaluator
        {
            /// <summary>
            /// Create a new Or evaluator.
            /// </summary>
            /// <remarks>
            /// The initial evaluators are ANDed together and used as the first clause of the OR.
            /// </remarks>
            /// <param name="evaluators">initial OR clause (these are wrapped into an AND evaluator).</param>
            internal Or(ICollection<Evaluator> evaluators) : base()
            {
                if (num > 1)
                {
                    this.evaluators.Add(new CombiningEvaluator.And(evaluators));
                }
                else
                {
                    // 0 or 1
                    this.evaluators.AddRange(evaluators);
                }
                UpdateNumEvaluators();
            }

            public Or() : base()
            {
            }

            public void Add(Evaluator e)
            {
                evaluators.Add(e);
                UpdateNumEvaluators();
            }

            public override bool Matches(Element root, Element element)
            {
                for (int i = 0; i < num; i++)
                {
                    Evaluator s = evaluators[i];
                    if (s.Matches(root, element))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return string.Format(":or{0}", evaluators);
            }
        }
    }
}
