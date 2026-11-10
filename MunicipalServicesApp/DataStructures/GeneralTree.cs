using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>A node in a <see cref="GeneralTree{T}"/>, which may have any number of children.</summary>
    public sealed class GeneralTreeNode<T>
    {
        private readonly List<GeneralTreeNode<T>> _children = new List<GeneralTreeNode<T>>();

        public T Value { get; }
        public GeneralTreeNode<T> Parent { get; private set; }
        public IReadOnlyList<GeneralTreeNode<T>> Children => _children.AsReadOnly();

        /// <summary>0 for the root, 1 for its children, and so on.</summary>
        public int Depth => Parent == null ? 0 : Parent.Depth + 1;

        public GeneralTreeNode(T value)
        {
            Value = value;
        }

        public GeneralTreeNode<T> AddChild(T value)
        {
            var child = new GeneralTreeNode<T>(value) { Parent = this };
            _children.Add(child);
            return child;
        }
    }

    /// <summary>
    /// A basic (general) tree: one root, and nodes with any number of children. Suited
    /// to hierarchies such as department, category and sub-type, where a binary tree's
    /// two-child limit doesn't match the data.
    /// </summary>
    public class GeneralTree<T>
    {
        public GeneralTreeNode<T> Root { get; }

        public GeneralTree(T rootValue)
        {
            Root = new GeneralTreeNode<T>(rootValue);
        }

        /// <summary>Every node, each before its children (depth-first pre-order).</summary>
        public IEnumerable<GeneralTreeNode<T>> PreOrder()
        {
            var stack = new Stack<GeneralTreeNode<T>>();
            stack.Push(Root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;

                for (var i = node.Children.Count - 1; i >= 0; i--)
                {
                    stack.Push(node.Children[i]);
                }
            }
        }

        /// <summary>The first node, in pre-order, that matches the predicate; null if none does.</summary>
        public GeneralTreeNode<T> Find(Func<T, bool> predicate)
        {
            foreach (var node in PreOrder())
            {
                if (predicate(node.Value))
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>A node and every node below it.</summary>
        public static IEnumerable<GeneralTreeNode<T>> Subtree(GeneralTreeNode<T> node)
        {
            yield return node;
            foreach (var child in node.Children)
            {
                foreach (var descendant in Subtree(child))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>The nodes from the root down to the given node, inclusive.</summary>
        public static List<GeneralTreeNode<T>> PathFromRoot(GeneralTreeNode<T> node)
        {
            var path = new List<GeneralTreeNode<T>>();
            for (var current = node; current != null; current = current.Parent)
            {
                path.Insert(0, current);
            }

            return path;
        }
    }
}
