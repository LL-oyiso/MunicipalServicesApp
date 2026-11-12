using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// An unweighted graph stored as an adjacency list: each vertex maps to the set of
    /// vertices it has an edge to. Directed graphs keep one-way edges; undirected
    /// graphs store each edge in both directions. Breadth-first and depth-first
    /// traversal each visit every reachable vertex once, in O(V + E).
    /// </summary>
    public class Graph<T>
    {
        private readonly Dictionary<T, List<T>> _adjacency;

        public bool IsDirected { get; }

        public Graph(bool isDirected, IEqualityComparer<T> comparer = null)
        {
            IsDirected = isDirected;
            _adjacency = new Dictionary<T, List<T>>(comparer ?? EqualityComparer<T>.Default);
        }

        public IEnumerable<T> Vertices => _adjacency.Keys;

        public int VertexCount => _adjacency.Count;

        public void AddVertex(T vertex)
        {
            if (!_adjacency.ContainsKey(vertex))
            {
                _adjacency[vertex] = new List<T>();
            }
        }

        public void AddEdge(T from, T to)
        {
            AddVertex(from);
            AddVertex(to);

            if (!_adjacency[from].Contains(to))
            {
                _adjacency[from].Add(to);
            }

            if (!IsDirected && !_adjacency[to].Contains(from))
            {
                _adjacency[to].Add(from);
            }
        }

        public bool HasEdge(T from, T to)
        {
            return _adjacency.TryGetValue(from, out var neighbours) && neighbours.Contains(to);
        }

        public IReadOnlyList<T> Neighbours(T vertex)
        {
            return _adjacency.TryGetValue(vertex, out var neighbours)
                ? neighbours.AsReadOnly()
                : (IReadOnlyList<T>)new T[0];
        }

        /// <summary>Vertices in the order a breadth-first search reaches them: nearest first.</summary>
        public List<T> BreadthFirst(T start)
        {
            var order = new List<T>();
            if (!_adjacency.ContainsKey(start))
            {
                return order;
            }

            var visited = new HashSet<T>(_adjacency.Comparer) { start };
            var queue = new Queue<T>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var vertex = queue.Dequeue();
                order.Add(vertex);

                foreach (var neighbour in _adjacency[vertex])
                {
                    if (visited.Add(neighbour))
                    {
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return order;
        }

        /// <summary>Vertices in the order a depth-first search reaches them: one branch fully before the next.</summary>
        public List<T> DepthFirst(T start)
        {
            var order = new List<T>();
            if (!_adjacency.ContainsKey(start))
            {
                return order;
            }

            var visited = new HashSet<T>(_adjacency.Comparer);
            var stack = new Stack<T>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var vertex = stack.Pop();
                if (!visited.Add(vertex))
                {
                    continue;
                }

                order.Add(vertex);

                var neighbours = _adjacency[vertex];
                for (var i = neighbours.Count - 1; i >= 0; i--)
                {
                    if (!visited.Contains(neighbours[i]))
                    {
                        stack.Push(neighbours[i]);
                    }
                }
            }

            return order;
        }

        /// <summary>
        /// The path with the fewest edges from start to goal, inclusive of both, found by
        /// breadth-first search. Empty if goal can't be reached.
        /// </summary>
        public List<T> ShortestPath(T start, T goal)
        {
            var path = new List<T>();
            if (!_adjacency.ContainsKey(start) || !_adjacency.ContainsKey(goal))
            {
                return path;
            }

            var cameFrom = new Dictionary<T, T>(_adjacency.Comparer);
            var visited = new HashSet<T>(_adjacency.Comparer) { start };
            var queue = new Queue<T>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var vertex = queue.Dequeue();
                if (_adjacency.Comparer.Equals(vertex, goal))
                {
                    for (var step = goal; ; step = cameFrom[step])
                    {
                        path.Insert(0, step);
                        if (_adjacency.Comparer.Equals(step, start))
                        {
                            break;
                        }
                    }

                    return path;
                }

                foreach (var neighbour in _adjacency[vertex])
                {
                    if (visited.Add(neighbour))
                    {
                        cameFrom[neighbour] = vertex;
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return path;
        }

        /// <summary>Every vertex reachable from start within maxHops edges, mapped to its hop count.</summary>
        public Dictionary<T, int> WithinHops(T start, int maxHops)
        {
            if (maxHops < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHops));
            }

            var hops = new Dictionary<T, int>(_adjacency.Comparer);
            if (!_adjacency.ContainsKey(start))
            {
                return hops;
            }

            hops[start] = 0;
            var queue = new Queue<T>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                var vertex = queue.Dequeue();
                if (hops[vertex] == maxHops)
                {
                    continue;
                }

                foreach (var neighbour in _adjacency[vertex])
                {
                    if (!hops.ContainsKey(neighbour))
                    {
                        hops[neighbour] = hops[vertex] + 1;
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return hops;
        }
    }
}
