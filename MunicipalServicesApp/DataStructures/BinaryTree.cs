using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// One node of a binary tree. A node holds every value stored under its key, so
    /// several requests can share a key (e.g. the same ward). <see cref="Height"/> and
    /// <see cref="IsRed"/> are balancing data used only by the AVL and red-black trees.
    /// </summary>
    public sealed class BinaryTreeNode<TKey, TValue>
    {
        public TKey Key { get; }
        public List<TValue> Values { get; } = new List<TValue>();

        public BinaryTreeNode<TKey, TValue> Left { get; internal set; }
        public BinaryTreeNode<TKey, TValue> Right { get; internal set; }
        public BinaryTreeNode<TKey, TValue> Parent { get; internal set; }

        internal int Height { get; set; } = 1;
        internal bool IsRed { get; set; }

        public BinaryTreeNode(TKey key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Base for every binary tree in the app: a root, node and value counts, height,
    /// and the standard traversals. It knows nothing about ordering - that is added by
    /// <see cref="BinarySearchTree{TKey, TValue}"/>.
    /// </summary>
    public abstract class BinaryTree<TKey, TValue>
    {
        protected BinaryTreeNode<TKey, TValue> Root { get; set; }

        /// <summary>Number of distinct keys (nodes) in the tree.</summary>
        public int NodeCount { get; protected set; }

        /// <summary>Number of values stored across all nodes.</summary>
        public int Count { get; protected set; }

        /// <summary>Number of nodes on the longest root-to-leaf path; 0 for an empty tree.</summary>
        public int Height => HeightOf(Root);

        private static int HeightOf(BinaryTreeNode<TKey, TValue> node)
        {
            return node == null ? 0 : 1 + Math.Max(HeightOf(node.Left), HeightOf(node.Right));
        }

        /// <summary>Left subtree, node, right subtree: ascending key order in a search tree.</summary>
        public IEnumerable<BinaryTreeNode<TKey, TValue>> InOrder()
        {
            var stack = new Stack<BinaryTreeNode<TKey, TValue>>();
            var current = Root;
            while (current != null || stack.Count > 0)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();
                yield return current;
                current = current.Right;
            }
        }

        /// <summary>Right subtree, node, left subtree: descending key order in a search tree.</summary>
        public IEnumerable<BinaryTreeNode<TKey, TValue>> ReverseInOrder()
        {
            var stack = new Stack<BinaryTreeNode<TKey, TValue>>();
            var current = Root;
            while (current != null || stack.Count > 0)
            {
                while (current != null)
                {
                    stack.Push(current);
                    current = current.Right;
                }

                current = stack.Pop();
                yield return current;
                current = current.Left;
            }
        }

        /// <summary>Node before its subtrees: the order to copy or print a tree's shape.</summary>
        public IEnumerable<BinaryTreeNode<TKey, TValue>> PreOrder()
        {
            if (Root == null)
            {
                yield break;
            }

            var stack = new Stack<BinaryTreeNode<TKey, TValue>>();
            stack.Push(Root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;

                if (node.Right != null)
                {
                    stack.Push(node.Right);
                }

                if (node.Left != null)
                {
                    stack.Push(node.Left);
                }
            }
        }

        /// <summary>Subtrees before their node: the order to free or total up a tree bottom-up.</summary>
        public IEnumerable<BinaryTreeNode<TKey, TValue>> PostOrder()
        {
            if (Root == null)
            {
                yield break;
            }

            var input = new Stack<BinaryTreeNode<TKey, TValue>>();
            var output = new Stack<BinaryTreeNode<TKey, TValue>>();
            input.Push(Root);
            while (input.Count > 0)
            {
                var node = input.Pop();
                output.Push(node);

                if (node.Left != null)
                {
                    input.Push(node.Left);
                }

                if (node.Right != null)
                {
                    input.Push(node.Right);
                }
            }

            while (output.Count > 0)
            {
                yield return output.Pop();
            }
        }

        /// <summary>Every value in ascending key order.</summary>
        public IEnumerable<TValue> InOrderValues()
        {
            foreach (var node in InOrder())
            {
                foreach (var value in node.Values)
                {
                    yield return value;
                }
            }
        }

        /// <summary>Every value in descending key order.</summary>
        public IEnumerable<TValue> ReverseInOrderValues()
        {
            foreach (var node in ReverseInOrder())
            {
                for (var i = node.Values.Count - 1; i >= 0; i--)
                {
                    yield return node.Values[i];
                }
            }
        }
    }
}
