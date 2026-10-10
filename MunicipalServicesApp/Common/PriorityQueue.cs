using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Common
{
    /// <summary>
    /// A minimal binary min-heap priority queue. .NET Framework 4.8 (unlike .NET 6+)
    /// has no built-in <c>PriorityQueue&lt;TElement, TPriority&gt;</c> type, so this
    /// hand-rolled implementation backs every priority-queue requirement in the
    /// Local Events and Announcements feature. The item the supplied
    /// <see cref="IComparer{T}"/> considers "smallest" - which callers define as
    /// "highest priority" - is always the one <see cref="Dequeue"/> returns first.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the queue.</typeparam>
    public class PriorityQueue<T>
    {
        private readonly List<T> _heap = new List<T>();
        private readonly IComparer<T> _comparer;

        public PriorityQueue(IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public int Count => _heap.Count;

        /// <summary>Adds an item to the queue, restoring the heap property with an upward bubble.</summary>
        public void Enqueue(T item)
        {
            _heap.Add(item);
            BubbleUp(_heap.Count - 1);
        }

        /// <summary>Removes and returns the highest-priority item (the current root of the heap).</summary>
        public T Dequeue()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            var top = _heap[0];
            var lastIndex = _heap.Count - 1;
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);

            if (_heap.Count > 0)
            {
                BubbleDown(0);
            }

            return top;
        }

        /// <summary>Returns the highest-priority item without removing it.</summary>
        public T Peek()
        {
            if (_heap.Count == 0)
            {
                throw new InvalidOperationException("The priority queue is empty.");
            }

            return _heap[0];
        }

        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                var parentIndex = (index - 1) / 2;
                if (_comparer.Compare(_heap[index], _heap[parentIndex]) >= 0)
                {
                    break;
                }

                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void BubbleDown(int index)
        {
            var count = _heap.Count;
            while (true)
            {
                var leftIndex = index * 2 + 1;
                var rightIndex = index * 2 + 2;
                var smallestIndex = index;

                if (leftIndex < count && _comparer.Compare(_heap[leftIndex], _heap[smallestIndex]) < 0)
                {
                    smallestIndex = leftIndex;
                }

                if (rightIndex < count && _comparer.Compare(_heap[rightIndex], _heap[smallestIndex]) < 0)
                {
                    smallestIndex = rightIndex;
                }

                if (smallestIndex == index)
                {
                    break;
                }

                Swap(index, smallestIndex);
                index = smallestIndex;
            }
        }

        private void Swap(int indexA, int indexB)
        {
            var temp = _heap[indexA];
            _heap[indexA] = _heap[indexB];
            _heap[indexB] = temp;
        }
    }
}
