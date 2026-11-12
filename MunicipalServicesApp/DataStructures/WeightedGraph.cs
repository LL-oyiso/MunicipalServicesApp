using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Common;

namespace MunicipalServicesApp.DataStructures
{
    /// <summary>
    /// An edge of a <see cref="WeightedGraph{T}"/>. <see cref="Path"/> lists every vertex
    /// travelled through from <see cref="From"/> to <see cref="To"/>: just the two ends
    /// for a direct edge, or the full shortest route for an edge between vertices that
    /// aren't directly connected.
    /// </summary>
    public sealed class WeightedEdge<T>
    {
        public T From { get; }
        public T To { get; }
        public double Weight { get; }
        public IReadOnlyList<T> Path { get; }

        public WeightedEdge(T from, T to, double weight, IReadOnlyList<T> path = null)
        {
            From = from;
            To = to;
            Weight = weight;
            Path = path ?? new[] { from, to };
        }
    }

    /// <summary>
    /// An undirected graph whose edges carry a weight (here, a distance in km), stored as
    /// an adjacency list. Supports breadth-first search by hop count, Dijkstra's shortest
    /// paths and Prim's minimum spanning tree; the last two use the app's binary heap
    /// <see cref="PriorityQueue{T}"/> to pick the cheapest next step in O(log n).
    /// </summary>
    public class WeightedGraph<T>
    {
        private readonly Dictionary<T, Dictionary<T, double>> _adjacency;
        private readonly IEqualityComparer<T> _comparer;

        public WeightedGraph(IEqualityComparer<T> comparer = null)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
            _adjacency = new Dictionary<T, Dictionary<T, double>>(_comparer);
        }

        public IEnumerable<T> Vertices => _adjacency.Keys;

        public int VertexCount => _adjacency.Count;

        public bool ContainsVertex(T vertex) => _adjacency.ContainsKey(vertex);

        public void AddVertex(T vertex)
        {
            if (!_adjacency.ContainsKey(vertex))
            {
                _adjacency[vertex] = new Dictionary<T, double>(_comparer);
            }
        }

        public void AddEdge(T a, T b, double weight)
        {
            if (weight < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), "Edge weights can't be negative.");
            }

            AddVertex(a);
            AddVertex(b);
            _adjacency[a][b] = weight;
            _adjacency[b][a] = weight;
        }

        public IEnumerable<WeightedEdge<T>> Edges
        {
            get
            {
                var seen = new HashSet<T>(_comparer);
                foreach (var pair in _adjacency)
                {
                    seen.Add(pair.Key);
                    foreach (var neighbour in pair.Value)
                    {
                        if (!seen.Contains(neighbour.Key))
                        {
                            yield return new WeightedEdge<T>(pair.Key, neighbour.Key, neighbour.Value);
                        }
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<T, double>> Neighbours(T vertex)
        {
            return _adjacency.TryGetValue(vertex, out var neighbours)
                ? neighbours
                : Enumerable.Empty<KeyValuePair<T, double>>();
        }

        /// <summary>
        /// Breadth-first search ignoring weights: every vertex within maxHops edges of
        /// start, mapped to its hop count (0 for start itself).
        /// </summary>
        public Dictionary<T, int> WithinHops(T start, int maxHops)
        {
            var hops = new Dictionary<T, int>(_comparer);
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
                if (hops[vertex] >= maxHops)
                {
                    continue;
                }

                foreach (var neighbour in _adjacency[vertex].Keys)
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

        /// <summary>
        /// Dijkstra's algorithm: the shortest distance from start to every reachable
        /// vertex, plus each vertex's predecessor on that shortest route.
        /// </summary>
        public Dictionary<T, double> ShortestDistances(T start, out Dictionary<T, T> previous)
        {
            var distances = new Dictionary<T, double>(_comparer);
            previous = new Dictionary<T, T>(_comparer);
            if (!_adjacency.ContainsKey(start))
            {
                return distances;
            }

            var settled = new HashSet<T>(_comparer);
            var queue = new PriorityQueue<KeyValuePair<T, double>>(
                Comparer<KeyValuePair<T, double>>.Create((a, b) => a.Value.CompareTo(b.Value)));

            distances[start] = 0;
            queue.Enqueue(new KeyValuePair<T, double>(start, 0));

            while (queue.Count > 0)
            {
                var vertex = queue.Dequeue().Key;
                if (!settled.Add(vertex))
                {
                    continue;
                }

                foreach (var neighbour in _adjacency[vertex])
                {
                    var candidate = distances[vertex] + neighbour.Value;
                    if (!distances.TryGetValue(neighbour.Key, out var known) || candidate < known)
                    {
                        distances[neighbour.Key] = candidate;
                        previous[neighbour.Key] = vertex;
                        queue.Enqueue(new KeyValuePair<T, double>(neighbour.Key, candidate));
                    }
                }
            }

            return distances;
        }

        /// <summary>The shortest route between two vertices (inclusive) and its total weight; empty if unreachable.</summary>
        public List<T> ShortestPath(T from, T to, out double distance)
        {
            var distances = ShortestDistances(from, out var previous);
            if (!distances.TryGetValue(to, out distance))
            {
                distance = double.PositiveInfinity;
                return new List<T>();
            }

            return BuildPath(previous, from, to);
        }

        private List<T> BuildPath(Dictionary<T, T> previous, T from, T to)
        {
            var path = new List<T>();
            for (var step = to; ; step = previous[step])
            {
                path.Insert(0, step);
                if (_comparer.Equals(step, from))
                {
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// Prim's algorithm over the whole graph: the set of edges that connects every
        /// vertex reachable from the first one at the lowest possible total weight.
        /// </summary>
        public List<WeightedEdge<T>> MinimumSpanningTree()
        {
            return Prim(this);
        }

        /// <summary>
        /// The cheapest network linking just the given vertices. Builds a smaller complete
        /// graph whose edge weights are the Dijkstra shortest distances between each pair
        /// (so two wards with no direct road are still linked through the wards between
        /// them), then runs Prim's algorithm on it. Each returned edge's
        /// <see cref="WeightedEdge{T}.Path"/> holds the actual route through the full graph.
        /// </summary>
        public List<WeightedEdge<T>> MinimumSpanningTree(IEnumerable<T> requiredVertices)
        {
            var required = requiredVertices.Where(ContainsVertex).Distinct(_comparer).ToList();
            if (required.Count < 2)
            {
                return new List<WeightedEdge<T>>();
            }

            var closure = new WeightedGraph<T>(_comparer);
            var routes = new Dictionary<T, Dictionary<T, List<T>>>(_comparer);

            foreach (var from in required)
            {
                closure.AddVertex(from);
                routes[from] = new Dictionary<T, List<T>>(_comparer);
                var distances = ShortestDistances(from, out var previous);

                foreach (var to in required)
                {
                    if (!_comparer.Equals(from, to) && distances.TryGetValue(to, out var distance))
                    {
                        closure.AddEdge(from, to, distance);
                        routes[from][to] = BuildPath(previous, from, to);
                    }
                }
            }

            return Prim(closure)
                .Select(e => new WeightedEdge<T>(e.From, e.To, e.Weight, routes[e.From][e.To]))
                .ToList();
        }

        private static List<WeightedEdge<T>> Prim(WeightedGraph<T> graph)
        {
            var tree = new List<WeightedEdge<T>>();
            var start = graph.Vertices.FirstOrDefault();
            if (graph.VertexCount < 2)
            {
                return tree;
            }

            var inTree = new HashSet<T>(graph._comparer) { start };
            var candidates = new PriorityQueue<WeightedEdge<T>>(
                Comparer<WeightedEdge<T>>.Create((a, b) => a.Weight.CompareTo(b.Weight)));

            foreach (var neighbour in graph.Neighbours(start))
            {
                candidates.Enqueue(new WeightedEdge<T>(start, neighbour.Key, neighbour.Value));
            }

            // Repeatedly take the cheapest edge leaving the tree; skip any that now lead
            // back inside it, since adding those would create a cycle.
            while (candidates.Count > 0 && inTree.Count < graph.VertexCount)
            {
                var edge = candidates.Dequeue();
                if (!inTree.Add(edge.To))
                {
                    continue;
                }

                tree.Add(edge);

                foreach (var neighbour in graph.Neighbours(edge.To))
                {
                    if (!inTree.Contains(neighbour.Key))
                    {
                        candidates.Enqueue(new WeightedEdge<T>(edge.To, neighbour.Key, neighbour.Value));
                    }
                }
            }

            return tree;
        }
    }
}
