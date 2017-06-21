using NUnit.Framework;
using Supremes.Nodes;
using System;

#if (NETSTANDARD1_3)
namespace Supremes.Test.Nodes
#else
namespace Supremes.Test.net45.Nodes
#endif
{
    [TestFixture]
    public class DocumentTypeTest
    {
        [Test]
        public void ConstructorValidationOkWithBlankName()
        {
            DocumentType fail = new DocumentType("", "", "", "");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorValidationThrowsExceptionOnNulls()
        {
            DocumentType fail = new DocumentType("html", null, null, "");
        }

        [Test]
        public void ConstructorValidationOkWithBlankPublicAndSystemIds()
        {
            DocumentType fail = new DocumentType("html", "", "", "");
        }

        [Test]
        public void OuterHtmlGeneration()
        {
            DocumentType html5 = new DocumentType("html", "", "", "");
            Assert.AreEqual("<!DOCTYPE html>", html5.OuterHtml);

            DocumentType publicDocType = new DocumentType("html", "-//IETF//DTD HTML//", "", "");
            Assert.AreEqual("<!DOCTYPE html PUBLIC \"-//IETF//DTD HTML//\">", publicDocType.OuterHtml);

            DocumentType systemDocType = new DocumentType("html", "", "http://www.ibm.com/data/dtd/v11/ibmxhtml1-transitional.dtd", "");
            Assert.AreEqual("<!DOCTYPE html \"http://www.ibm.com/data/dtd/v11/ibmxhtml1-transitional.dtd\">", systemDocType.OuterHtml);

            DocumentType combo = new DocumentType("notHtml", "--public", "--system", "");
            Assert.AreEqual("<!DOCTYPE notHtml PUBLIC \"--public\" \"--system\">", combo.OuterHtml);
        }
    }
}
