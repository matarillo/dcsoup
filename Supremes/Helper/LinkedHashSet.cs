using System.Collections.Generic;

namespace Supremes.Helper
{
    internal class LinkedHashSet<T> : ICollection<T>
    {
        private readonly Dictionary<T, LinkedListNode<T>> dict;
        private readonly LinkedList<T> list;

        public LinkedHashSet()
        {
            dict = new Dictionary<T, LinkedListNode<T>>();
            list = new LinkedList<T>();
        }

        public LinkedHashSet(IEnumerable<T> collection)
        {
            var col = collection as ICollection<T>;
            if (col != null)
            {
                dict = new Dictionary<T, LinkedListNode<T>>(col.Count);
                list = new LinkedList<T>();
            }
            else
            {
                dict = new Dictionary<T, LinkedListNode<T>>();
                list = new LinkedList<T>();
            }
            AddRange(collection);
        }

        public void Add(T item)
        {
            if (!dict.ContainsKey(item))
            {
                var node = list.AddLast(item);
                dict[item] = node;
            }
        }

        public void Clear()
        {
            dict.Clear();
            list.Clear();
        }

        public bool Contains(T item)
        {
            return dict.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            if (dict.TryGetValue(item, out node))
            {
                dict.Remove(item);
                list.Remove(node);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            if (collection == null) return;
            foreach(var item in collection)
            {
                Add(item);
            }
        }
    }
}
