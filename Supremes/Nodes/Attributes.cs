using Supremes.Helper;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Supremes.Nodes
{
    /// <summary>
    /// The attributes of an Element.
    /// </summary>
    /// <remarks>
    /// Attributes are treated as a map: there can be only one value associated with an attribute key.
    /// <p/>
    /// Attribute key and value comparisons are done case insensitively, and keys are normalised to
    /// lower-case.
    /// </remarks>
    /// <author>Jonathan Hedley, jonathan@hedley.net</author>
    public sealed class Attributes : IEnumerable<Attribute>
    {
        internal const string dataPrefix = "data-";

        private LinkedHashMap<string, Attribute> attributes = null;
        // linked hash map to preserve insertion order.
        // null be default as so many elements have no attributes -- saves a good chunk of memory

        internal Attributes()
        {
        }

        /// <summary>
        /// Get an attribute value by key.
        /// Set a new attribute, or replace an existing one by key.
        /// </summary>
        /// <param name="key">the attribute key</param>
        /// <value>attribute value</value>
        /// <returns>the attribute value if set; or empty string if not set.</returns>
        /// <seealso cref="ContainsKey(string)">ContainsKey(string)</seealso>
        public string this[string key]
        {
            get
            {
                Validate.NotEmpty(key);
                if (attributes == null)
                {
                    return string.Empty;
                }
                Attribute attr = attributes[key.ToLower()];
                return attr != null ? attr.Value : string.Empty;
            }
            set
            {
                Attribute attr = new Attribute(key, value);
                Put(attr);
            }
        }

        /// <summary>
        /// Set a new attribute, or replace an existing one by key.
        /// </summary>
        /// <param name="attribute">attribute</param>
        public void Put(Attribute attribute)
        {
            Validate.NotNull(attribute);
            if (attributes == null)
            {
                attributes = new LinkedHashMap<string, Attribute>(2);
            }
            attributes[attribute.Key] = attribute;
        }

        /// <summary>
        /// Remove an attribute by key.
        /// </summary>
        /// <param name="key">attribute key to remove</param>
        public void Remove(string key)
        {
            Validate.NotEmpty(key);
            if (attributes == null)
            {
                return;
            }
            attributes.Remove(key.ToLower());
        }

        /// <summary>
        /// Tests if these attributes contain an attribute with this key.
        /// </summary>
        /// <param name="key">key to check for</param>
        /// <returns>true if key exists, false otherwise</returns>
        public bool ContainsKey(string key)
        {
            return attributes != null && attributes.ContainsKey(key.ToLower());
        }

        /// <summary>
        /// Get the number of attributes in this set.
        /// </summary>
        /// <returns>size</returns>
        public int Count
        {
            get
            {
                if (attributes == null)
                {
                    return 0;
                }
                return attributes.Count;
            }
        }

        /// <summary>
        /// Add all the attributes from the incoming set to this set.
        /// </summary>
        /// <param name="incoming">attributes to add to these attributes.</param>
        public void SetAll(Attributes incoming)
        {
            if (incoming == null || incoming.Count == 0)
            {
                return;
            }
            if (attributes == null)
            {
                attributes = new LinkedHashMap<string, Attribute>(incoming.Count);
            }
            foreach (var pair in incoming.attributes)
            {
                attributes[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="Attributes"/>.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Attribute> GetEnumerator()
        {
            if (attributes != null)
            {
                foreach (var pair in attributes) yield return pair.Value;
            }
        }

        /// <summary>
        /// Get the attributes as a List, for iteration.
        /// </summary>
        /// <remarks>
        /// Get the attributes as a List, for iteration. Do not modify the keys of the attributes via this view, as changes
        /// to keys will not be recognised in the containing set.
        /// </remarks>
        /// <returns>an view of the attributes as a List.</returns>
        public IReadOnlyList<Attribute> AsList()
        {
            if (attributes == null)
            {
                return new List<Attribute>(0).AsReadOnly();
            }
            return attributes.Values.ToList().AsReadOnly();
        }

        /// <summary>
        /// Retrieves a filtered view of attributes that are HTML5 custom data attributes; that is, attributes with keys
        /// starting with
        /// <c>data-</c>
        /// .
        /// </summary>
        /// <returns>map of custom data attributes.</returns>
        public IDictionary<string, string> Dataset
        {
            get { return new Attributes._Dataset(this); }
        }

        /// <summary>
        /// Get the HTML representation of these attributes.
        /// </summary>
        /// <returns>HTML</returns>
        public string Html
        {
            get
            {
                StringBuilder accum = new StringBuilder();
                AppendHtmlTo(accum, (new Document(string.Empty)).OutputSettings);
                // output settings a bit funky, but this html() seldom used
                return accum.ToString();
            }
        }

        internal void AppendHtmlTo(StringBuilder accum, DocumentOutputSettings @out)
        {
            if (attributes == null)
            {
                return;
            }
            foreach (KeyValuePair<string, Attribute> entry in attributes)
            {
                Attribute attribute = (Attribute)entry.Value;
                accum.Append(" ");
                attribute.AppendHtmlTo(accum, @out);
            }
        }

        /// <summary>
        /// Converts the value of this instance to a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Html;
        }

        /// <summary>
        /// Compares two <see cref="Attributes"/> instances for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            Attributes that = obj as Attributes;
            if (that == null)
            {
                return false;
            }
            if (attributes == null || that.attributes == null)
            {
                return (attributes == that.attributes);
            }
            return Enumerable.SequenceEqual(attributes, that.attributes);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return attributes != null ? attributes.GetHashCode() : 0;
        }

        internal Attributes Clone()
        {
            if (attributes == null)
            {
                return new Attributes();
            }
            Attributes clone;
            clone = (Attributes)this.MemberwiseClone();
            clone.attributes = new LinkedHashMap<string, Attribute>(attributes.Count);
            foreach (Attribute attribute in this)
            {
                clone.attributes[attribute.Key] = attribute.Clone();
            }
            return clone;
        }

        private class _Dataset : IDictionary<string, string>
        {
            private readonly LinkedHashMap<string, Attribute> enclosingAttributes;

            public _Dataset(Attributes enclosing)
            {
                if (enclosing.attributes == null)
                {
                    enclosing.attributes = new LinkedHashMap<string, Attribute>(2);
                }
                this.enclosingAttributes = enclosing.attributes;
            }

            public void Add(string key, string value)
            {
                string dataKey = Attributes.DataKey(key);
                Attribute attr = new Attribute(dataKey, value);
                enclosingAttributes.Add(dataKey, attr);
            }

            public bool ContainsKey(string key)
            {
                string dataKey = Attributes.DataKey(key);
                return enclosingAttributes.ContainsKey(dataKey);
            }

            public ICollection<string> Keys
            {
                get { return this.Select(a => a.Key).ToArray(); }
            }

            public bool Remove(string key)
            {
                string dataKey = Attributes.DataKey(key);
                return enclosingAttributes.Remove(dataKey);
            }

            public bool TryGetValue(string key, out string value)
            {
                string dataKey = Attributes.DataKey(key);
                Attribute attr = null;
                if (enclosingAttributes.TryGetValue(dataKey, out attr))
                {
                    value = attr.Value;
                    return true;
                }
                value = null;
                return false;
            }

            public ICollection<string> Values
            {
                get { return this.Select(a => a.Value).ToArray(); }
            }

            public string this[string key]
            {
                get
                {
                    string dataKey = Attributes.DataKey(key);
                    Attribute attr = enclosingAttributes[dataKey];
                    return attr.Value;
                }
                set
                {
                    string dataKey = Attributes.DataKey(key);
                    Attribute attr = new Attribute(dataKey, value);
                    enclosingAttributes[dataKey] = attr;
                }
            }

            public void Add(KeyValuePair<string, string> item)
            {
                this.Add(item.Key, item.Value);
            }

            public void Clear()
            {
                var dataAttrs = GetDataAttributes().ToList();
                foreach (var dataAttr in dataAttrs)
                {
                    enclosingAttributes.Remove(dataAttr.Key);
                }
            }

            private IEnumerable<Attribute> GetDataAttributes()
            {
                return enclosingAttributes
                    .Select(p => (Attribute)p.Value)
                    .Where(a => a.IsDataAttribute());
            }

            public bool Contains(KeyValuePair<string, string> item)
            {
                string value = null;
                return (this.TryGetValue(item.Key, out value) && (value == item.Value));
            }

            public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
            {
                foreach (var pair in this)
                {
                    array[arrayIndex++] = pair;
                }
            }

            public int Count
            {
                get { return GetDataAttributes().Count(); }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(KeyValuePair<string, string> item)
            {
                return this.Contains(item) && this.Remove(item.Key);
            }

            public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
            {
                return GetDataAttributes()
                    .Select(a => new KeyValuePair<string, string>(a.Key.Substring(dataPrefix.Length) /*substring*/, a.Value))
                    .GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private static string DataKey(string key)
        {
            return dataPrefix + key;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
