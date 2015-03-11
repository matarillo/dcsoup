using System;
using System.Collections.Generic;
using System.Linq;

namespace Supremes.Helper
{
    internal class LinkedHashMap<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dict;
        private readonly LinkedList<KeyValuePair<TKey, TValue>> list;

        #region constructor

        public LinkedHashMap()
        {
            dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public LinkedHashMap(IEqualityComparer<TKey> comparer)
        {
            dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public LinkedHashMap(int capacity)
        {
            dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity);
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public LinkedHashMap(int capacity, IEqualityComparer<TKey> comparer)
        {
            dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity, comparer);
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public LinkedHashMap(IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            var countable = source as System.Collections.ICollection;
            if (countable != null)
            {
                dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(countable.Count);
            }
            else
            {
                dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
            }
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
            foreach (var pair in source)
            {
                this[pair.Key] = pair.Value;
            }
        }

        public LinkedHashMap(IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            var countable = source as System.Collections.ICollection;
            if (countable != null)
            {
                dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(countable.Count, comparer);
            }
            else
            {
                dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
            }
            list = new LinkedList<KeyValuePair<TKey, TValue>>();
            foreach (var pair in source)
            {
                this[pair.Key] = pair.Value;
            }
        }

        #endregion

        #region IDictionary implementation

        public bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            DoAdd(key, value);
        }

        private void DoAdd(TKey key, TValue value)
        {
            var pair = new KeyValuePair<TKey, TValue>(key, value);
            var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(pair);
            dict.Add(key, node);
            list.AddLast(node);
        }

        public bool Remove(TKey key)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> n;
            if (!dict.TryGetValue(key, out n))
            {
                return false;
            }
            DoRemove(n);
            return true;
        }

        private void DoRemove(LinkedListNode<KeyValuePair<TKey, TValue>> node)
        {
            dict.Remove(node.Value.Key);
            list.Remove(node);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> n;
            if (dict.TryGetValue(key, out n))
            {
                value = n.Value.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        private bool TryGetNode(TKey key, TValue value, out LinkedListNode<KeyValuePair<TKey, TValue>> node)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> n;
            if (dict.TryGetValue(key, out n) && EqualityComparer<TValue>.Default.Equals(value, n.Value.Value))
            {
                node = n;
                return true;
            }
            node = null;
            return false;
        }

        public TValue this[TKey key]
        {
            get { return dict[key].Value.Value; }
            set
            {
                LinkedListNode<KeyValuePair<TKey, TValue>> n;
                if (!dict.TryGetValue(key, out n))
                {
                    DoAdd(key, value);
                    return;
                }
                DoSet(n, key, value);
            }
        }

        private void DoSet(LinkedListNode<KeyValuePair<TKey, TValue>> node, TKey key, TValue value)
        {
            var pair = new KeyValuePair<TKey, TValue>(key, value);
            var newNode = new LinkedListNode<KeyValuePair<TKey, TValue>>(pair);
            dict[key] = newNode;
            list.AddAfter(node, newNode);
            list.Remove(node);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return list.Select(p => p.Key).ToArray();
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return list.Select(p => p.Value).ToArray();
            }
        }

        #endregion

        #region ICollection implementation

        public void Clear()
        {
            dict.Clear();
            list.Clear();
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable implementation

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region explicit ICollection implementation

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> pair;
            return TryGetNode(item.Key, item.Value, out pair);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (!TryGetNode(item.Key, item.Value, out node))
            {
                return false;
            }
            DoRemove(node);
            return true;
        }

        #endregion
    }
}
