using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.DataStructures;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// The municipality's wards as a weighted, undirected graph: each ward is a vertex
    /// and each road between neighbouring wards is an edge weighted by its distance in
    /// km. The distances are fictional sample values.
    /// </summary>
    public static class WardMap
    {
        public static WeightedGraph<string> Graph { get; } = Build();

        private static WeightedGraph<string> Build()
        {
            var a = ServiceRequestRepository.Areas;
            var graph = new WeightedGraph<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var area in a)
            {
                graph.AddVertex(area);
            }

            // a[0] City Centre, a[1] Civic Park, a[2] Industrial Park, a[3] Riverside,
            // a[4] Greenfield, a[5] Hillcrest, a[6] Lakeside
            graph.AddEdge(a[0], a[1], 3.2);
            graph.AddEdge(a[0], a[3], 4.5);
            graph.AddEdge(a[0], a[2], 6.0);
            graph.AddEdge(a[1], a[5], 5.1);
            graph.AddEdge(a[1], a[4], 4.0);
            graph.AddEdge(a[2], a[3], 5.5);
            graph.AddEdge(a[2], a[4], 7.4);
            graph.AddEdge(a[3], a[6], 3.8);
            graph.AddEdge(a[3], a[5], 2.9);
            graph.AddEdge(a[5], a[6], 6.3);
            graph.AddEdge(a[4], a[6], 8.1);

            return graph;
        }

        /// <summary>One open request found near a ward, with how many wards away it is.</summary>
        public sealed class NearbyRequest
        {
            public Issue Request { get; set; }
            public int WardsAway { get; set; }
        }

        /// <summary>
        /// Open requests in the given ward and in wards up to maxHops roads away, found by
        /// breadth-first search, nearest wards first. Helps staff spot related faults,
        /// e.g. street lights out across neighbouring wards.
        /// </summary>
        public static List<NearbyRequest> FindOpenRequestsNear(string area, int maxHops)
        {
            return Graph.WithinHops(area, maxHops)
                .OrderBy(pair => pair.Value)
                .SelectMany(pair => ServiceRequestRepository.GetByArea(pair.Key)
                    .Where(r => r.IsOpen)
                    .Select(r => new NearbyRequest { Request = r, WardsAway = pair.Value }))
                .ToList();
        }

        /// <summary>
        /// The wards that currently have at least one open request, optionally only
        /// counting requests assigned to one crew (null means every open request).
        /// </summary>
        public static List<string> AreasWithOpenRequests(string crew = null)
        {
            return ServiceRequestRepository.Areas
                .Where(area => ServiceRequestRepository.GetByArea(area).Any(r => r.IsOpen
                    && (crew == null || string.Equals(r.AssignedCrew, crew, StringComparison.OrdinalIgnoreCase))))
                .ToList();
        }

        /// <summary>
        /// The shortest set of roads connecting every ward with open requests (for one
        /// crew, or all when crew is null): a minimum spanning tree (Prim's algorithm)
        /// over those wards, using shortest-path distances where two wards aren't
        /// directly connected.
        /// </summary>
        public static List<WeightedEdge<string>> PlanCrewRoute(string crew = null)
        {
            return Graph.MinimumSpanningTree(AreasWithOpenRequests(crew));
        }

        /// <summary>
        /// The individual roads a planned route uses, each once, even when two legs of
        /// the route share a road.
        /// </summary>
        public static List<WeightedEdge<string>> RoadsOnRoute(IEnumerable<WeightedEdge<string>> route)
        {
            var roads = new List<WeightedEdge<string>>();
            foreach (var leg in route)
            {
                for (var i = 0; i < leg.Path.Count - 1; i++)
                {
                    var from = leg.Path[i];
                    var to = leg.Path[i + 1];
                    if (roads.Any(r => IsSameRoad(r, from, to)))
                    {
                        continue;
                    }

                    var road = Graph.Neighbours(from).First(n => string.Equals(n.Key, to, StringComparison.OrdinalIgnoreCase));
                    roads.Add(new WeightedEdge<string>(from, to, road.Value));
                }
            }

            return roads;
        }

        public static bool IsSameRoad(WeightedEdge<string> road, string a, string b)
        {
            return (string.Equals(road.From, a, StringComparison.OrdinalIgnoreCase) && string.Equals(road.To, b, StringComparison.OrdinalIgnoreCase))
                || (string.Equals(road.From, b, StringComparison.OrdinalIgnoreCase) && string.Equals(road.To, a, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Total km of every road in the map, to compare against the planned route.</summary>
        public static double TotalRoadLength()
        {
            return Graph.Edges.Sum(e => e.Weight);
        }
    }
}
