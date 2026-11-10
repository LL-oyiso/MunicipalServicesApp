using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// A binary search tree: every key in a node's left subtree is smaller than the
    /// node's key, and every key in its right subtree is larger, so a search discards
    /// one subtree at each step. Search, insert and range queries cost O(h), where h is
    /// the tree's height. This class does no rebalancing, so h depends on the order keys
    /// arrive in; <see cref="AvlTree{TKey, TValue}"/> and
    /// <see cref="RedBlackTree{TKey, TValue}"/> add balancing through
    /// <see cref="OnNodeInserted"/>.
    /// </summary>
    public class BinarySearchTree<TKey, TValue> : BinaryTree<TKey, TValue>
    {
        protected IComparer<TKey> Comparer { get; }

        public BinarySearchTree(IComparer<TKey> comparer = null)
        {
            Comparer = comparer ?? Comparer<TKey>.Default;
        }

        /// <summary>Adds a value under a key, creating the key's node if it is new.</summary>
        public void Insert(TKey key, TValue value)
        {
            BinaryTreeNode<TKey, TValue> parent = null;
            var current = Root;
            var comparison = 0;

            while (current != null)
            {
                comparison = Comparer.Compare(key, current.Key);
                if (comparison == 0)
                {
                    current.Values.Add(value);
                    Count++;
                    return;
                }

                parent = current;
                current = comparison < 0 ? current.Left : current.Right;
            }

            var node = new BinaryTreeNode<TKey, TValue>(key) { Parent = parent };
            node.Values.Add(value);

            if (parent == null)
            {
                Root = node;
            }
            else if (comparison < 0)
            {
                parent.Left = node;
            }
            else
            {
                parent.Right = node;
            }

            NodeCount++;
            Count++;
            OnNodeInserted(node);
        }

        /// <summary>
        /// Removes one value from a key's node. The node itself stays in place even when
        /// it becomes empty (lazy deletion), so the AVL and red-black trees never need a
        /// structural delete and their balance is never disturbed. Empty nodes are
        /// skipped by every lookup.
        /// </summary>
        public bool Remove(TKey key, TValue value)
        {
            var node = FindNode(key, out _);
            if (node == null || !node.Values.Remove(value))
            {
                return false;
            }

            Count--;
            return true;
        }

        /// <summary>The values stored under a key, or an empty list if there are none.</summary>
        public IReadOnlyList<TValue> Find(TKey key)
        {
            return Find(key, out _);
        }

        /// <summary>
        /// As <see cref="Find(TKey)"/>, also reporting how many nodes were compared on the
        /// way - the number the Service Request Status page shows to illustrate O(h) search.
        /// </summary>
        public IReadOnlyList<TValue> Find(TKey key, out int steps)
        {
            var node = FindNode(key, out steps);
            return node == null ? (IReadOnlyList<TValue>)new TValue[0] : node.Values.AsReadOnly();
        }

        /// <summary>Every value whose key lies between from and to (inclusive), in ascending order.</summary>
        public List<TValue> Range(TKey from, TKey to)
        {
            var results = new List<TValue>();
            CollectRange(Root, from, to, results);
            return results;
        }

        private void CollectRange(BinaryTreeNode<TKey, TValue> node, TKey from, TKey to, List<TValue> results)
        {
            if (node == null)
            {
                return;
            }

            var afterFrom = Comparer.Compare(node.Key, from) > 0;
            var beforeTo = Comparer.Compare(node.Key, to) < 0;

            // Only visit a subtree if it can contain keys inside the range.
            if (afterFrom)
            {
                CollectRange(node.Left, from, to, results);
            }

            if (Comparer.Compare(node.Key, from) >= 0 && Comparer.Compare(node.Key, to) <= 0)
            {
                results.AddRange(node.Values);
            }

            if (beforeTo)
            {
                CollectRange(node.Right, from, to, results);
            }
        }

        private BinaryTreeNode<TKey, TValue> FindNode(TKey key, out int steps)
        {
            steps = 0;
            var current = Root;
            while (current != null)
            {
                steps++;
                var comparison = Comparer.Compare(key, current.Key);
                if (comparison == 0)
                {
                    return current;
                }

                current = comparison < 0 ? current.Left : current.Right;
            }

            return null;
        }

        /// <summary>Called after a new node is linked in, so subclasses can rebalance.</summary>
        protected virtual void OnNodeInserted(BinaryTreeNode<TKey, TValue> node)
        {
        }

        /// <summary>
        /// Rotates x's right child up into x's place. In-order key order is unchanged;
        /// only the shape (and therefore the height) of the subtree changes.
        /// </summary>
        protected BinaryTreeNode<TKey, TValue> RotateLeft(BinaryTreeNode<TKey, TValue> x)
        {
            var y = x.Right ?? throw new InvalidOperationException("Cannot rotate left without a right child.");

            x.Right = y.Left;
            if (y.Left != null)
            {
                y.Left.Parent = x;
            }

            ReplaceInParent(x, y);
            y.Left = x;
            x.Parent = y;
            return y;
        }

        /// <summary>Mirror image of <see cref="RotateLeft"/>.</summary>
        protected BinaryTreeNode<TKey, TValue> RotateRight(BinaryTreeNode<TKey, TValue> x)
        {
            var y = x.Left ?? throw new InvalidOperationException("Cannot rotate right without a left child.");

            x.Left = y.Right;
            if (y.Right != null)
            {
                y.Right.Parent = x;
            }

            ReplaceInParent(x, y);
            y.Right = x;
            x.Parent = y;
            return y;
        }

        private void ReplaceInParent(BinaryTreeNode<TKey, TValue> oldChild, BinaryTreeNode<TKey, TValue> newChild)
        {
            var parent = oldChild.Parent;
            newChild.Parent = parent;

            if (parent == null)
            {
                Root = newChild;
            }
            else if (parent.Left == oldChild)
            {
                parent.Left = newChild;
            }
            else
            {
                parent.Right = newChild;
            }
        }
    }
}
