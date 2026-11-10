using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// A self-balancing binary search tree that keeps the heights of every node's two
    /// subtrees within one of each other, rotating after an insert when they drift
    /// further apart. Height therefore stays at about 1.44 log2(n), so lookups are
    /// O(log n) even when keys arrive in sorted order - which is exactly how reference
    /// numbers arrive, and would turn a plain BST into a linked list.
    /// </summary>
    public class AvlTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    {
        public AvlTree(IComparer<TKey> comparer = null)
            : base(comparer)
        {
        }

        protected override void OnNodeInserted(BinaryTreeNode<TKey, TValue> node)
        {
            // Walk back up from the new leaf, refreshing heights and fixing the first
            // node found out of balance. One fix (single or double rotation) is enough
            // for an insert, but carrying on to the root keeps every height correct.
            var current = node.Parent;
            while (current != null)
            {
                UpdateHeight(current);
                current = Rebalance(current).Parent;
            }
        }

        private BinaryTreeNode<TKey, TValue> Rebalance(BinaryTreeNode<TKey, TValue> node)
        {
            var balance = BalanceFactor(node);

            if (balance > 1)
            {
                // Left-right case: straighten the left child first.
                if (BalanceFactor(node.Left) < 0)
                {
                    RotateLeftAndUpdate(node.Left);
                }

                return RotateRightAndUpdate(node);
            }

            if (balance < -1)
            {
                // Right-left case: straighten the right child first.
                if (BalanceFactor(node.Right) > 0)
                {
                    RotateRightAndUpdate(node.Right);
                }

                return RotateLeftAndUpdate(node);
            }

            return node;
        }

        private BinaryTreeNode<TKey, TValue> RotateLeftAndUpdate(BinaryTreeNode<TKey, TValue> node)
        {
            var newRoot = RotateLeft(node);
            UpdateHeight(node);
            UpdateHeight(newRoot);
            return newRoot;
        }

        private BinaryTreeNode<TKey, TValue> RotateRightAndUpdate(BinaryTreeNode<TKey, TValue> node)
        {
            var newRoot = RotateRight(node);
            UpdateHeight(node);
            UpdateHeight(newRoot);
            return newRoot;
        }

        private static int NodeHeight(BinaryTreeNode<TKey, TValue> node)
        {
            return node?.Height ?? 0;
        }

        private static int BalanceFactor(BinaryTreeNode<TKey, TValue> node)
        {
            return NodeHeight(node.Left) - NodeHeight(node.Right);
        }

        private static void UpdateHeight(BinaryTreeNode<TKey, TValue> node)
        {
            node.Height = 1 + Math.Max(NodeHeight(node.Left), NodeHeight(node.Right));
        }
    }
}
