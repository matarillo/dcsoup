/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using Supremes.Helper;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Supremes.Nodes
{
    /// <summary>
    /// A HTML Form Element provides ready access to the form fields/controls that are associated with it.
    /// </summary>
    /// <remarks>
    /// It also allows a form to easily be submitted.
    /// </remarks>
    public sealed class FormElement : Element
    {
        private readonly Elements elements = new Elements();

        /// <summary>
        /// Create a new, standalone form element.
        /// </summary>
        /// <param name="tag">tag of this element</param>
        /// <param name="baseUri">the base URI</param>
        /// <param name="attributes">initial attributes</param>
        internal FormElement(Tag tag, string baseUri, Attributes attributes) : base(tag, baseUri, attributes)
        {
        }

        /// <summary>
        /// Get the list of form control elements associated with this form.
        /// </summary>
        /// <returns>form controls associated with this element.</returns>
        public Elements Elements()
        {
            return elements;
        }

        /// <summary>
        /// Add a form control element to this form.
        /// </summary>
        /// <param name="element">form control to add</param>
        /// <returns>this form element, for chaining</returns>
        public FormElement AddElement(Element element)
        {
            elements.Add(element);
            return this;
        }

        /// <summary>
        /// submit this form, using a specified HttpClient.
        /// </summary>
        /// <remarks>
        /// the request will be set up from the form values.
        /// You can set up other options (like user-agent, timeout, cookies) before executing.
        /// </remarks>
        /// <returns>an async task.</returns>
        /// <exception cref="System.ArgumentException">
        /// if the form's absolute action URL cannot be determined.
        /// Make sure you pass the document's base URI when parsing.
        /// </exception>
        public Task<HttpResponseMessage> SubmitAsync(HttpClient client)
        {
            string action = HasAttr("action") ? AbsUrl("action") : BaseUri();
            Validate.NotEmpty(action, "Could not determine a form action URL for submit. Ensure you set a base URI when parsing.");
            var data = new FormUrlEncodedContent(this.FormData());
            if (string.Equals(Attr("method"), "POST", StringComparison.OrdinalIgnoreCase))
            {
                // POST
                return client.PostAsync(action, data);
            }
            else
            {
                // GET
                var actionUrl = new UriBuilder(action);
                actionUrl.Query = data.ReadAsStringAsync().Result;
                return client.GetAsync(actionUrl.Uri);
            }
        }

        /// <summary>
        /// submit this form, using a specified HttpClient.
        /// </summary>
        /// <remarks>
        /// the request will be set up from the form values.
        /// You can set up other options (like user-agent, timeout, cookies) before executing.
        /// </remarks>
        /// <returns>an async task.</returns>
        /// <exception cref="System.ArgumentException">
        /// if the form's absolute action URL cannot be determined.
        /// Make sure you pass the document's base URI when parsing.
        /// </exception>
        public Task<HttpResponseMessage> SubmitAsync(HttpClient client, CancellationToken cancellationToken)
        {
            string action = HasAttr("action") ? AbsUrl("action") : BaseUri();
            Validate.NotEmpty(action, "Could not determine a form action URL for submit. Ensure you set a base URI when parsing.");
            var data = new FormUrlEncodedContent(this.FormData());
            if (string.Equals(Attr("method"), "POST", StringComparison.OrdinalIgnoreCase))
            {
                // POST
                return client.PostAsync(action, data, cancellationToken);
            }
            else
            {
                // GET
                var actionUrl = new UriBuilder(action);
                actionUrl.Query = data.ReadAsStringAsync().Result;
                return client.GetAsync(actionUrl.Uri, cancellationToken);
            }
        }

        /// <summary>
        /// Get the data that this form submits.
        /// </summary>
        /// <remarks>
        /// The returned list is a copy of the data, and changes to the contents of the
        /// list will not be reflected in the DOM.
        /// </remarks>
        /// <returns>a list of key vals</returns>
        public IList<KeyValuePair<string, string>> FormData()
        {
            List<KeyValuePair<string, string>> data = new List<KeyValuePair<string, string>>();
            // iterate the form control elements and accumulate their values
            foreach (Element el in elements)
            {
                if (!el.Tag().IsFormSubmittable())
                {
                    continue;
                }
                // contents are form listable, superset of submitable
                string name = el.Attr("name");
                if (name.Length == 0)
                {
                    continue;
                }
                if ("select".Equals(el.TagName()))
                {
                    Supremes.Nodes.Elements options = el.Select("option[selected]");
                    foreach (Element option in options)
                    {
                        data.Add(new KeyValuePair<string, string>(name, option.Val()));
                    }
                }
                else
                {
                    data.Add(new KeyValuePair<string, string>(name, el.Val()));
                }
            }
            return data;
        }
    }
}
