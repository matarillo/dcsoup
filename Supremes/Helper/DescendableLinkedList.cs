/*
 * This code is derived from MyJavaLibrary (http://somelinktomycoollibrary)
 * 
 * If this is an open source Java library, include the proper license and copyright attributions here!
 */

using System;
using System.Collections.Generic;

namespace Supremes.Helper
{
    /// <summary>
    /// Provides a descending iterator and other 1.6 methods to allow support on the 1.5 JRE.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DescendableLinkedList<T> : LinkedList<T>, IList<T>
        where T : class
    {
        /// <summary>
        /// Create a new DescendableLinkedList.
        /// </summary>
        public DescendableLinkedList()
        {
        }

        /// <summary>
        /// Add a new element to the start of the list.
        /// </summary>
        /// <param name="e">element to add</param>
        public void Push(T e)
        {
            AddFirst(e);
        }

        /// <summary>
        /// Look at the last element, if there is one.
        /// </summary>
        /// <returns>the last element, or null</returns>
        public T PeekLast()
        {
            return Count == 0 ? null : Last.Value;
        }

        /// <summary>
        /// Remove and return the last element, if there is one
        /// </summary>
        /// <returns>the last element, or null</returns>
        public T PollLast()
        {
            if (Count == 0) return null;
            var last = Last.Value;
            RemoveLast();
            return last;
        }

        /// <summary>
        /// Get an iterator that starts and the end of the list and works towards the start.
        /// </summary>
        /// <returns>
        /// an iterator that starts and the end of the list and works towards the start.
        /// </returns>
        public DescendingEnumerator GetDescendingEnumerator()
        {
            return new DescendingEnumerator(Last);
        }

        public class DescendingEnumerator : IEnumerator<T>
        {
            private LinkedListNode<T> current;
            private bool started;
            private bool removeAfterMoveNext;

            public DescendingEnumerator(LinkedListNode<T> last)
            {
                current = last;
                started = false;
            }

            public T Current
            {
                get { return (current == null) ? null : current.Value; }
            }

            public void Dispose()
            {
                current = null;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (started)
                {
                    if (current != null)
                    {
                        current = current.Previous;
                        if (removeAfterMoveNext)
                        {
                            current.List.Remove(current.Next);
                            removeAfterMoveNext = false;
                        }
                    }
                }
                else
                {
                    started = true;
                }
                return (current != null);
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            #region for compat to Java

            public void Remove()
            {
                if (!started || current == null) return;
                removeAfterMoveNext = true;
            }

            public bool HasNext()
            {
                return started
                    ? (current != null && current.Previous != null)
                    : (current != null);
            }

            #endregion
        }

        #region IList<T> for compat to Java

        public void Add(T item)
        {
            this.AddLast(item);
        }

        public bool IsReadOnly
        {
            get { return  false; }
        }

        public int IndexOf(T item)
        {
            const int NOTFOUND = -1;
            var i = 0;
            for (var n = this.First; n != null; n = n.Next)
            {
                if (n.Value == null)
                {
                    if (item == null) return i;
                }
                else
                {
                    if (n.Value.Equals(item)) return i;
                }
                i++;
            }
            return NOTFOUND;
        }

        private LinkedListNode<T> NodeAt(int index)
        {
            var i = 0;
            for (var n = this.First; n != null; n = n.Next)
            {
                if (i == index) return n;
                i++;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        public void Insert(int index, T item)
        {
            if (index == Count)
            {
                AddLast(item);
                return;
            }
            var node = NodeAt(index);
            AddBefore(node, item);
        }

        public void RemoveAt(int index)
        {
            var node = NodeAt(index);
            Remove(node);
        }

        public T this[int index]
        {
            get
            {
                var node = NodeAt(index);
                return node.Value;
            }
            set
            {
                var node = NodeAt(index);
                var newNode = AddAfter(node, value);
                Remove(node);
            }
        }

        #endregion
    }
}
