/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using Supremes.Nodes;

namespace Supremes.Select
{
    /// <summary>
    /// Base structural evaluator.
    /// </summary>
    internal abstract class StructuralEvaluator : Evaluator
    {
        internal Evaluator evaluator;

        internal class Root : Evaluator
        {
            public override bool Matches(Element root, Element element)
            {
                return root == element;
            }
        }

        internal class Has : StructuralEvaluator
        {
            public Has(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public override bool Matches(Element root, Element element)
            {
                foreach (Element e in element.GetAllElements())
                {
                    if (e != element && evaluator.Matches(root, e))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string ToString()
            {
                return string.Format(":has({0})", evaluator);
            }
        }

        internal class Not : StructuralEvaluator
        {
            public Not(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public override bool Matches(Element root, Element element)
            {
                return !evaluator.Matches(root, element);
            }

            public override string ToString()
            {
                return string.Format(":not{0}", evaluator);
            }
        }

        internal class Parent : StructuralEvaluator
        {
            public Parent(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public override bool Matches(Element root, Element element)
            {
                if (root == element)
                {
                    return false;
                }
                Element parent = element.ParentElement();
                while (parent != root)
                {
                    if (evaluator.Matches(root, parent))
                    {
                        return true;
                    }
                    parent = parent.ParentElement();
                }
                return false;
            }

            public override string ToString()
            {
                return string.Format(":parent{0}", evaluator);
            }
        }

        internal class ImmediateParent : StructuralEvaluator
        {
            public ImmediateParent(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public override bool Matches(Element root, Element element)
            {
                if (root == element)
                {
                    return false;
                }
                Element parent = element.ParentElement();
                return parent != null && evaluator.Matches(root, parent);
            }

            public override string ToString()
            {
                return string.Format(":ImmediateParent{0}", evaluator);
            }
        }

        internal class PreviousSibling : StructuralEvaluator
        {
            public PreviousSibling(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public override bool Matches(Element root, Element element)
            {
                if (root == element)
                {
                    return false;
                }
                Element prev = element.PreviousElementSibling();
                while (prev != null)
                {
                    if (evaluator.Matches(root, prev))
                    {
                        return true;
                    }
                    prev = prev.PreviousElementSibling();
                }
                return false;
            }

            public override string ToString()
            {
                return string.Format(":prev*{0}", evaluator);
            }
        }

        internal class ImmediatePreviousSibling : StructuralEvaluator
        {
            public ImmediatePreviousSibling(Evaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public override bool Matches(Element root, Element element)
            {
                if (root == element)
                {
                    return false;
                }
                Element prev = element.PreviousElementSibling();
                return prev != null && evaluator.Matches(root, prev);
            }

            public override string ToString()
            {
                return string.Format(":prev{0}", evaluator);
            }
        }
    }
}
