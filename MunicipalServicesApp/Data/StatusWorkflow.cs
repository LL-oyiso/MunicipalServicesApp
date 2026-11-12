using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.DataStructures;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// The service request lifecycle as a directed graph: each status is a vertex and
    /// each allowed change is an edge. Staff can only move a request along an edge, and
    /// a breadth-first search from the current status gives the fewest steps left to
    /// close it. Changing the workflow means changing edges here, not code elsewhere.
    /// </summary>
    public static class StatusWorkflow
    {
        public static Graph<ServiceRequestStatus> Graph { get; } = Build();

        private static Graph<ServiceRequestStatus> Build()
        {
            var graph = new Graph<ServiceRequestStatus>(isDirected: true);

            graph.AddEdge(ServiceRequestStatus.Submitted, ServiceRequestStatus.Assigned);
            graph.AddEdge(ServiceRequestStatus.Submitted, ServiceRequestStatus.Rejected);
            graph.AddEdge(ServiceRequestStatus.Assigned, ServiceRequestStatus.InProgress);
            graph.AddEdge(ServiceRequestStatus.InProgress, ServiceRequestStatus.OnHold);
            graph.AddEdge(ServiceRequestStatus.InProgress, ServiceRequestStatus.Resolved);
            graph.AddEdge(ServiceRequestStatus.OnHold, ServiceRequestStatus.InProgress);
            graph.AddEdge(ServiceRequestStatus.Resolved, ServiceRequestStatus.Closed);
            graph.AddEdge(ServiceRequestStatus.Resolved, ServiceRequestStatus.InProgress);

            return graph;
        }

        /// <summary>The statuses a request can move to next from its current one.</summary>
        public static IReadOnlyList<ServiceRequestStatus> NextStatuses(ServiceRequestStatus status)
        {
            return Graph.Neighbours(status);
        }

        public static bool CanMove(ServiceRequestStatus from, ServiceRequestStatus to)
        {
            return Graph.HasEdge(from, to);
        }

        /// <summary>
        /// The quickest remaining route to Closed, starting with the current status.
        /// Empty when Closed can't be reached (a rejected request).
        /// </summary>
        public static List<ServiceRequestStatus> PathToClosed(ServiceRequestStatus status)
        {
            return Graph.ShortestPath(status, ServiceRequestStatus.Closed);
        }

        /// <summary>Fewest status changes left before the request closes; -1 if it never will.</summary>
        public static int StepsToClosed(ServiceRequestStatus status)
        {
            var path = PathToClosed(status);
            return path.Count == 0 ? -1 : path.Count - 1;
        }

        /// <summary>
        /// The normal happy path, Submitted to Closed, found by breadth-first search. The
        /// page draws it as the request's progress timeline.
        /// </summary>
        public static List<ServiceRequestStatus> MainPath()
        {
            return Graph.ShortestPath(ServiceRequestStatus.Submitted, ServiceRequestStatus.Closed);
        }

        /// <summary>
        /// Every status a request can still end up in from its current one, found by
        /// depth-first search (one branch of the workflow followed to its end before the
        /// next). The current status itself is left out. Empty for a final status.
        /// </summary>
        public static List<ServiceRequestStatus> ReachableStatuses(ServiceRequestStatus status)
        {
            var reachable = Graph.DepthFirst(status);
            reachable.Remove(status);
            return reachable;
        }

        /// <summary>Every status reachable from Submitted, nearest first (breadth-first order).</summary>
        public static List<ServiceRequestStatus> AllStatusesInWorkflowOrder()
        {
            return Graph.BreadthFirst(ServiceRequestStatus.Submitted);
        }

        public static string DisplayName(ServiceRequestStatus status)
        {
            switch (status)
            {
                case ServiceRequestStatus.InProgress: return "In Progress";
                case ServiceRequestStatus.OnHold: return "On Hold";
                default: return status.ToString();
            }
        }

        public static string[] DisplayNames(IEnumerable<ServiceRequestStatus> statuses)
        {
            return statuses.Select(DisplayName).ToArray();
        }
    }
}
