using NUnit.Framework;
using Supremes.Nodes;
using System;
using System.Collections.Generic;

namespace Supremes.Test.Nodes
{
    [TestFixture]
    public class FormElementTest
    {
        [Test]
        public void HasAssociatedControls()
        {
            //"button", "fieldset", "input", "keygen", "object", "output", "select", "textarea"
            string html = "<form id=1><button id=1><fieldset id=2 /><input id=3><keygen id=4><object id=5><output id=6>" +
                    "<select id=7><option></select><textarea id=8><p id=9>";
            Document doc = Dcsoup.Parse(html);

            FormElement form = (FormElement)doc.Select("form").First();
            Assert.AreEqual(8, form.Elements().Count);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: string expression of FormData is different.
        /// (because HTTP client class is different from Java) 
        /// </remarks>
        [Test]
        public void CreatesFormData()
        {
            string html = "<form><input name='one' value='two'><select name='three'><option value='not'>" +
                    "<option value='four' selected><option value='five' selected><textarea name=six>seven</textarea></form>";
            Document doc = Dcsoup.Parse(html);
            FormElement form = (FormElement)doc.Select("form").First();
            IList<KeyValuePair<string, string>> data = form.FormData();

            Assert.AreEqual(4, data.Count);
            Assert.AreEqual("[one, two]", data[0].ToString()); // string expression is different
            Assert.AreEqual("[three, four]", data[1].ToString()); // string expression is different
            Assert.AreEqual("[three, five]", data[2].ToString()); // string expression is different
            Assert.AreEqual("[six, seven]", data[3].ToString()); // string expression is different
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: request url has a query parameter string.
        /// (because HTTP client class is different from Java)
        /// </remarks>
        [Test]
        public void CreatesSubmitableConnection()
        {
            string html = "<form action='/search'><input name='q'></form>";
            Document doc = Dcsoup.Parse(html, "http://example.com/");
            doc.Select("[name=q]").Attr("value", "jsoup");

            FormElement form = ((FormElement)doc.Select("form").First());
            var client = new System.Net.Http.HttpClient();
            var message = form.SubmitAsync(client).Result;
            Assert.AreEqual(System.Net.Http.HttpMethod.Get, message.RequestMessage.Method);
            Assert.AreEqual("http://example.com/search?q=jsoup", message.RequestMessage.RequestUri.ToString()); // request url has a query parameter string.

            doc.Select("form").Attr("method", "post");
            var message2 = form.SubmitAsync(client).Result;
            Assert.AreEqual(System.Net.Http.HttpMethod.Post, message2.RequestMessage.Method);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: request url has a query parameter string.
        /// (because HTTP client class is different from Java)
        /// </remarks>
        [Test]
        public void ActionWithNoValue()
        {
            string html = "<form><input name='q'></form>";
            Document doc = Dcsoup.Parse(html, "http://example.com/");
            FormElement form = ((FormElement)doc.Select("form").First());

            var client = new System.Net.Http.HttpClient();
            var message = form.SubmitAsync(client).Result;
            Assert.AreEqual("http://example.com/?q=", message.RequestMessage.RequestUri.ToString()); // request url has a query parameter string.
        }

        [Test]
        public void ActionWithNoBaseUri()
        {
            string html = "<form><input name='q'></form>";
            Document doc = Dcsoup.Parse(html);
            FormElement form = ((FormElement)doc.Select("form").First());

            var ex = Assert.Throws(typeof(ArgumentException), () =>
            {
                var client = new System.Net.Http.HttpClient();
                var message = form.SubmitAsync(client).Result;
            });
            Assert.AreEqual("Could not determine a form action URL for submit. Ensure you set a base URI when parsing.", ex.Message);
        }

        [Test]
        public void FormsAddedAfterParseAreFormElements()
        {
            Document doc = Dcsoup.Parse("<body />");
            doc.Body().Html("<form action='http://example.com/search'><input name='q' value='search'>");
            Element formEl = doc.Select("form").First();
            Assert.IsTrue(formEl is FormElement);

            FormElement form = (FormElement)formEl;
            Assert.AreEqual(1, form.Elements().Count);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// different behavior from jsoup: string expression of FormData is different.
        /// (because HTTP client class is different from Java) 
        /// </remarks>
        [Test]
        public void ControlsAddedAfterParseAreLinkedWithForms()
        {
            Document doc = Dcsoup.Parse("<body />");
            doc.Body().Html("<form />");

            Element formEl = doc.Select("form").First();
            formEl.Append("<input name=foo value=bar>");

            Assert.IsTrue(formEl is FormElement);
            FormElement form = (FormElement)formEl;
            Assert.AreEqual(1, form.Elements().Count);

            IList<KeyValuePair<string, string>> data = form.FormData();
            Assert.AreEqual("[foo, bar]", data[0].ToString()); // string expression is different
        }
    }
}
