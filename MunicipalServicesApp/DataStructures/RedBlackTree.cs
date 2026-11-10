using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// A self-balancing binary search tree that colours each node red or black and
    /// keeps two rules: a red node never has a red child, and every path from a node
    /// down to a missing child passes the same number of black nodes. Together these
    /// cap the height at 2 log2(n + 1). It balances less strictly than an AVL tree, so
    /// inserts need fewer rotations - a good fit for an index that grows with every
    /// new report, like the one ordering requests by date.
    /// </summary>
    public class RedBlackTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    {
        public RedBlackTree(IComparer<TKey> comparer = null)
            : base(comparer)
        {
        }

        /// <summary>Number of black nodes on any path from the root to a missing child.</summary>
        public int BlackHeight
        {
            get
            {
                var count = 0;
                for (var node = Root; node != null; node = node.Left)
                {
                    if (!node.IsRed)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        protected override void OnNodeInserted(BinaryTreeNode<TKey, TValue> node)
        {
            node.IsRed = true;

            while (node.Parent != null && node.Parent.IsRed)
            {
                var parent = node.Parent;
                var grandparent = parent.Parent;

                if (parent == grandparent.Left)
                {
                    var uncle = grandparent.Right;
                    if (IsRed(uncle))
                    {
                        // Red uncle: recolour and push the problem two levels up.
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandparent.IsRed = true;
                        node = grandparent;
                    }
                    else
                    {
                        if (node == parent.Right)
                        {
                            node = parent;
                            RotateLeft(node);
                            parent = node.Parent;
                        }

                        parent.IsRed = false;
                        grandparent.IsRed = true;
                        RotateRight(grandparent);
                    }
                }
                else
                {
                    var uncle = grandparent.Left;
                    if (IsRed(uncle))
                    {
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandparent.IsRed = true;
                        node = grandparent;
                    }
                    else
                    {
                        if (node == parent.Left)
                        {
                            node = parent;
                            RotateRight(node);
                            parent = node.Parent;
                        }

                        parent.IsRed = false;
                        grandparent.IsRed = true;
                        RotateLeft(grandparent);
                    }
                }
            }

            Root.IsRed = false;
        }

        private static bool IsRed(BinaryTreeNode<TKey, TValue> node)
        {
            return node != null && node.IsRed;
        }
    }
}
