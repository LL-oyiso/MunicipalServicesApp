using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.DataStructures;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Holds every service request the Service Request Status feature can show:
    /// sample requests at mixed stages, plus anything the resident reports through
    /// Report Issues this session. All data is in memory only, like the rest of the app.
    /// </summary>
    public static class ServiceRequestRepository
    {
        /// <summary>Wards staff can place a request in when triaging it.</summary>
        public static readonly string[] Areas =
        {
            "Ward 1 - City Centre",
            "Ward 3 - Civic Park",
            "Ward 5 - Industrial Park",
            "Ward 7 - Riverside",
            "Ward 9 - Greenfield",
            "Ward 12 - Hillcrest",
            "Ward 15 - Lakeside"
        };

        /// <summary>Field crews staff can assign a request to.</summary>
        public static readonly string[] Crews =
        {
            "Electrical Crew A",
            "Roads Crew B",
            "Water Crew C",
            "Sanitation Crew D",
            "General Maintenance Crew E"
        };

        /// <summary>Label used for requests staff haven't placed in a ward yet.</summary>
        public const string UnassignedArea = "Not yet assigned";

        private static readonly List<Issue> _requests = new List<Issue>();

        // Main tracking index: reference number -> request. Balanced, because reference
        // numbers arrive in ascending order and would degrade a plain BST to a list.
        private static readonly AvlTree<string, Issue> _byReference =
            new AvlTree<string, Issue>(StringComparer.OrdinalIgnoreCase);

        // The same keys in an unbalanced BST, kept only to compare its height and search
        // steps against the AVL tree on the Service Request Status page.
        private static readonly BinarySearchTree<string, Issue> _byReferenceUnbalanced =
            new BinarySearchTree<string, Issue>(StringComparer.OrdinalIgnoreCase);

        // Ward -> requests in that ward. Ward names arrive in no particular order, so a
        // plain BST stays reasonably shallow here.
        private static readonly BinarySearchTree<string, Issue> _byArea =
            new BinarySearchTree<string, Issue>(StringComparer.OrdinalIgnoreCase);

        // Date reported -> requests, for newest-first lists and date-range filters.
        private static readonly RedBlackTree<DateTime, Issue> _byDate = new RedBlackTree<DateTime, Issue>();

        // Staff work queue, kept as one live heap. A heap can't cheaply find and move an
        // item whose priority changed, so a fresh entry is pushed instead and the old one
        // is marked stale by its version; stale entries are discarded when they reach the root.
        private static readonly PriorityQueue<WorkQueueEntry> _workQueue =
            new PriorityQueue<WorkQueueEntry>(Comparer<WorkQueueEntry>.Create((a, b) =>
            {
                // Compare the priority saved in the entry: the request's own priority may
                // have changed since it was pushed, and the heap order must not shift under it.
                var byPriority = a.Priority.CompareTo(b.Priority);
                return byPriority != 0 ? byPriority : a.Request.DateReported.CompareTo(b.Request.DateReported);
            }));

        private static readonly Dictionary<Issue, int> _workQueueVersions = new Dictionary<Issue, int>();

        private struct WorkQueueEntry
        {
            public Issue Request;
            public RequestPriority Priority;
            public int Version;
        }

        static ServiceRequestRepository()
        {
            SeedSampleRequests();
        }

        /// <summary>Forces the sample requests to load. Has no other effect.</summary>
        internal static void EnsureInitialized()
        {
        }

        public static IReadOnlyList<Issue> All => _requests.AsReadOnly();

        public static int Count => _requests.Count;

        /// <summary>Starts tracking a newly reported request.</summary>
        public static void Register(Issue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (issue.StatusHistory.Count == 0)
            {
                issue.StatusHistory.Add(new StatusChange(ServiceRequestStatus.Submitted, issue.DateReported, "Request received."));
            }

            AddToIndexes(issue);
        }

        private static void AddToIndexes(Issue issue)
        {
            _requests.Add(issue);
            _byReference.Insert(issue.ReferenceNumber, issue);
            _byReferenceUnbalanced.Insert(issue.ReferenceNumber, issue);
            _byArea.Insert(AreaKey(issue.Area), issue);
            _byDate.Insert(issue.DateReported, issue);

            if (issue.IsOpen)
            {
                PushToWorkQueue(issue);
            }
        }

        /// <summary>Adds a current entry for the request to the work queue heap, in O(log n).</summary>
        private static void PushToWorkQueue(Issue issue)
        {
            _workQueueVersions.TryGetValue(issue, out var version);
            version++;
            _workQueueVersions[issue] = version;
            _workQueue.Enqueue(new WorkQueueEntry { Request = issue, Priority = issue.Priority, Version = version });
        }

        private static bool IsStale(WorkQueueEntry entry)
        {
            return !entry.Request.IsOpen || entry.Version != _workQueueVersions[entry.Request];
        }

        private static string AreaKey(string area)
        {
            return string.IsNullOrWhiteSpace(area) ? UnassignedArea : area;
        }

        /// <summary>Looks a request up by reference number in the AVL tree, in O(log n).</summary>
        public static Issue FindByReference(string referenceNumber)
        {
            return FindByReference(referenceNumber, out _, out _);
        }

        /// <summary>
        /// As <see cref="FindByReference(string)"/>, also reporting how many nodes the AVL
        /// tree and the unbalanced BST each compared to reach the same request.
        /// </summary>
        public static Issue FindByReference(string referenceNumber, out int avlSteps, out int bstSteps)
        {
            avlSteps = 0;
            bstSteps = 0;
            if (string.IsNullOrWhiteSpace(referenceNumber))
            {
                return null;
            }

            var key = referenceNumber.Trim();
            var matches = _byReference.Find(key, out avlSteps);
            _byReferenceUnbalanced.Find(key, out bstSteps);
            return matches.Count == 0 ? null : matches[0];
        }

        /// <summary>Every request, newest report first (reverse in-order walk of the red-black tree).</summary>
        public static List<Issue> GetNewestFirst()
        {
            return _byDate.ReverseInOrderValues().ToList();
        }

        /// <summary>Requests reported between two dates inclusive, newest first (red-black tree range query).</summary>
        public static List<Issue> GetReportedBetween(DateTime from, DateTime to)
        {
            var results = _byDate.Range(from, to);
            results.Reverse();
            return results;
        }

        /// <summary>Requests placed in a ward (BST lookup); pass <see cref="UnassignedArea"/> for untriaged ones.</summary>
        public static IReadOnlyList<Issue> GetByArea(string area)
        {
            return _byArea.Find(AreaKey(area));
        }

        /// <summary>
        /// Every open request in the order staff should handle them: most urgent priority
        /// first, then oldest first. Listing all of them in order is a heap sort - push
        /// each open request onto a binary min-heap and pop it empty - O(n log n), the
        /// same as any sort. For just the next job, use <see cref="PeekNextJob"/>.
        /// </summary>
        public static List<Issue> GetWorkQueue()
        {
            var heap = new PriorityQueue<Issue>(WorkQueueOrder);
            foreach (var request in _requests)
            {
                if (request.IsOpen)
                {
                    heap.Enqueue(request);
                }
            }

            var ordered = new List<Issue>(heap.Count);
            while (heap.Count > 0)
            {
                ordered.Add(heap.Dequeue());
            }

            return ordered;
        }

        /// <summary>
        /// The most urgent open request, read from the root of the live work queue heap
        /// in O(1). Stale entries (closed requests, or ones whose priority has since
        /// changed) are discarded first, each in O(log n). Null if nothing is open.
        /// </summary>
        public static Issue PeekNextJob()
        {
            while (_workQueue.Count > 0 && IsStale(_workQueue.Peek()))
            {
                _workQueue.Dequeue();
            }

            return _workQueue.Count == 0 ? null : _workQueue.Peek().Request;
        }

        private static readonly IComparer<Issue> WorkQueueOrder = Comparer<Issue>.Create((a, b) =>
        {
            var byPriority = a.Priority.CompareTo(b.Priority);
            return byPriority != 0 ? byPriority : a.DateReported.CompareTo(b.DateReported);
        });

        /// <summary>Heights and sizes of the request indexes, shown on the page to compare the trees.</summary>
        public static IndexStatistics GetIndexStatistics()
        {
            return new IndexStatistics
            {
                RequestCount = _requests.Count,
                AvlHeight = _byReference.Height,
                UnbalancedBstHeight = _byReferenceUnbalanced.Height,
                RedBlackHeight = _byDate.Height,
                RedBlackBlackHeight = _byDate.BlackHeight,
                AreaBstHeight = _byArea.Height,
                AreaCount = _byArea.InOrder().Count(n => n.Values.Count > 0)
            };
        }

        /// <summary>
        /// Moves a request to a new status and records the change in its history. Only
        /// moves along an edge of the <see cref="StatusWorkflow"/> graph are allowed.
        /// </summary>
        public static void ChangeStatus(Issue issue, ServiceRequestStatus newStatus, string note)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (!StatusWorkflow.CanMove(issue.Status, newStatus))
            {
                throw new InvalidOperationException(
                    $"A request can't move from {StatusWorkflow.DisplayName(issue.Status)} to {StatusWorkflow.DisplayName(newStatus)}.");
            }

            var wasOpen = issue.IsOpen;
            issue.Status = newStatus;
            issue.StatusHistory.Add(new StatusChange(newStatus, DateTime.Now, note));

            if (!wasOpen && issue.IsOpen)
            {
                PushToWorkQueue(issue);
            }
        }

        /// <summary>Records staff's triage decisions: which ward, sub-type, crew and priority.</summary>
        public static void UpdateTriage(Issue issue, string area, string subCategory, string crew, RequestPriority priority)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (!string.Equals(AreaKey(issue.Area), AreaKey(area), StringComparison.OrdinalIgnoreCase))
            {
                _byArea.Remove(AreaKey(issue.Area), issue);
                _byArea.Insert(AreaKey(area), issue);
            }

            issue.Area = area;
            issue.SubCategory = subCategory;
            issue.AssignedCrew = crew;

            if (issue.Priority != priority)
            {
                issue.Priority = priority;
                if (issue.IsOpen)
                {
                    PushToWorkQueue(issue);
                }
            }
        }

        /// <summary>Saves the resident's 1-5 rating of a closed request.</summary>
        public static void RateService(Issue issue, int rating, string comment)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (issue.Status != ServiceRequestStatus.Closed)
            {
                throw new InvalidOperationException("Only closed requests can be rated.");
            }

            if (rating < 1 || rating > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(rating), "Ratings run from 1 to 5.");
            }

            issue.Rating = rating;
            issue.RatingComment = comment?.Trim();
        }

        /// <summary>Snapshot of the request indexes' sizes and heights.</summary>
        public sealed class IndexStatistics
        {
            public int RequestCount { get; set; }
            public int AvlHeight { get; set; }
            public int UnbalancedBstHeight { get; set; }
            public int RedBlackHeight { get; set; }
            public int RedBlackBlackHeight { get; set; }
            public int AreaBstHeight { get; set; }
            public int AreaCount { get; set; }
        }

        #region Sample data

        private sealed class SampleRequest
        {
            public int DaysAgo;
            public string Category;
            public string SubCategory;
            public string Description;
            public string Location;
            public string Area;
            public RequestPriority Priority;
            public ServiceRequestStatus FinalStatus;
            public string Crew;
            public int? Rating;
        }

        /// <summary>
        /// Fictional requests spread over the last seven weeks and every stage of the
        /// workflow, so all of the feature's views have something meaningful to show.
        /// Contact details are obviously fake placeholders.
        /// </summary>
        private static void SeedSampleRequests()
        {
            var samples = new[]
            {
                Sample(48, "Electricity", "Street lights", "Three street lights out along the main road.", "14 Fake Avenue", Areas[0], RequestPriority.High, ServiceRequestStatus.Closed, Crews[0], 5),
                Sample(45, "Water", "Burst pipe", "Water pouring from a burst pipe on the pavement.", "8 Sample Road", Areas[3], RequestPriority.Critical, ServiceRequestStatus.Closed, Crews[2], 4),
                Sample(42, "Roads", "Pothole", "Deep pothole in the left lane causing tyre damage.", "22 Test Street", Areas[1], RequestPriority.High, ServiceRequestStatus.Closed, Crews[1], 3),
                Sample(40, "Sanitation", "Missed refuse collection", "Bins on this street were not collected this week.", "5 Placeholder Lane", Areas[5], RequestPriority.Medium, ServiceRequestStatus.Closed, Crews[3], null),
                Sample(37, "Other", "Graffiti", "Graffiti on the wall of the public library.", "1 Library Square", Areas[0], RequestPriority.Low, ServiceRequestStatus.Rejected, null, null),
                Sample(35, "Electricity", "Power outage", "No electricity in the block since last night.", "30 Demo Crescent", Areas[2], RequestPriority.Critical, ServiceRequestStatus.Closed, Crews[0], 4),
                Sample(32, "Water", "Leaking meter", "Water meter leaking continuously at the gate.", "17 Fixture Drive", Areas[4], RequestPriority.Medium, ServiceRequestStatus.Resolved, Crews[2], null),
                Sample(30, "Roads", "Damaged road sign", "Stop sign knocked over at the intersection.", "Corner of Fake Avenue and Mock Street", Areas[6], RequestPriority.High, ServiceRequestStatus.Resolved, Crews[1], null),
                Sample(27, "Utilities", "Meter fault", "Prepaid electricity meter shows an error code.", "9 Sample Road", Areas[3], RequestPriority.Medium, ServiceRequestStatus.OnHold, Crews[0], null),
                Sample(25, "Sanitation", "Illegal dumping", "Building rubble dumped on the open field.", "Open field behind 40 Test Street", Areas[2], RequestPriority.Medium, ServiceRequestStatus.InProgress, Crews[3], null),
                Sample(22, "Electricity", "Street lights", "Street light flickering outside the clinic.", "3 Clinic Road", Areas[5], RequestPriority.Medium, ServiceRequestStatus.InProgress, Crews[0], null),
                Sample(20, "Water", "No water", "No water supply to the whole street.", "12 Mock Street", Areas[1], RequestPriority.Critical, ServiceRequestStatus.InProgress, Crews[2], null),
                Sample(18, "Roads", "Blocked stormwater drain", "Drain blocked with leaves; road floods when it rains.", "27 Fixture Drive", Areas[4], RequestPriority.High, ServiceRequestStatus.OnHold, Crews[1], null),
                Sample(15, "Other", "Park maintenance", "Broken swing and overgrown grass in the play park.", "Fake Park, Lakeside", Areas[6], RequestPriority.Low, ServiceRequestStatus.Assigned, Crews[4], null),
                Sample(13, "Sanitation", "Sewage overflow", "Manhole overflowing with sewage near the school.", "6 School Lane", Areas[3], RequestPriority.Critical, ServiceRequestStatus.Assigned, Crews[3], null),
                Sample(11, "Electricity", "Exposed wiring", "Exposed cables hanging from a street light pole.", "19 Demo Crescent", Areas[2], RequestPriority.Critical, ServiceRequestStatus.Assigned, Crews[0], null),
                Sample(9, "Water", "Low pressure", "Very low water pressure every morning.", "44 Placeholder Lane", Areas[5], RequestPriority.Low, ServiceRequestStatus.Assigned, Crews[2], null),
                Sample(8, "Roads", "Pothole", "Several potholes along the bus route.", "Bus route, Mock Street", Areas[0], RequestPriority.High, ServiceRequestStatus.Assigned, Crews[1], null),
                Sample(6, "Utilities", "Meter fault", "Water meter reading has not changed in two months.", "2 Sample Road", Areas[1], RequestPriority.Low, ServiceRequestStatus.Submitted, null, null),
                Sample(5, "Electricity", "Street lights", "Whole row of street lights off near the taxi rank.", "Taxi rank, Fake Avenue", Areas[6], RequestPriority.High, ServiceRequestStatus.Submitted, null, null),
                Sample(4, "Sanitation", "Missed refuse collection", "Refuse not collected for two weeks.", "11 Test Street", Areas[4], RequestPriority.Medium, ServiceRequestStatus.Submitted, null, null),
                Sample(3, "Water", "Burst pipe", "Water bubbling up through the road surface.", "33 Mock Street", null, RequestPriority.High, ServiceRequestStatus.Submitted, null, null),
                Sample(2, "Roads", "Damaged road sign", "Street name sign missing at the corner.", "Corner of Demo Crescent", null, RequestPriority.Low, ServiceRequestStatus.Submitted, null, null),
                Sample(1, "Other", "Park maintenance", "Park benches vandalised over the weekend.", "Fake Park, City Centre", null, RequestPriority.Low, ServiceRequestStatus.Submitted, null, null),
                Sample(0, "Electricity", "Power outage", "Power keeps tripping in the area every evening.", "50 Fixture Drive", Areas[3], RequestPriority.High, ServiceRequestStatus.Submitted, null, null)
            };

            var now = DateTime.Now;
            var sampleNumber = 0;

            foreach (var sample in samples)
            {
                sampleNumber++;
                var reported = now.Date.AddDays(-sample.DaysAgo).AddHours(8 + sampleNumber % 9).AddMinutes(sampleNumber * 7 % 60);
                if (reported > now)
                {
                    reported = now.AddMinutes(-30);
                }

                var issue = new Issue
                {
                    Category = sample.Category,
                    SubCategory = sample.Area == null ? null : sample.SubCategory,
                    Description = sample.Description,
                    Location = sample.Location,
                    AdditionalLocationInfo = string.Empty,
                    DateReported = reported,
                    ContactFirstName = "Sample",
                    ContactSurname = "Resident " + sampleNumber.ToString("D2"),
                    ContactEmail = "sample.resident" + sampleNumber.ToString("D2") + "@example.com",
                    ContactMobile = "08255501" + sampleNumber.ToString("D2"),
                    PreferredContactMethod = sampleNumber % 2 == 0 ? ContactMethod.Email : ContactMethod.Mobile,
                    Area = sample.Area,
                    AssignedCrew = sample.Crew,
                    Priority = sample.Priority,
                    Status = sample.FinalStatus
                };

                BuildSampleHistory(issue, sample.FinalStatus, sample.Crew, now);

                if (sample.Rating.HasValue)
                {
                    issue.Rating = sample.Rating;
                    issue.RatingComment = sample.Rating.Value >= 4 ? "Quick and friendly service." : "Fixed, but it took a while.";
                }

                IssueRepository.AddSampleIssue(issue);
                AddToIndexes(issue);
            }
        }

        private static SampleRequest Sample(int daysAgo, string category, string subCategory, string description,
            string location, string area, RequestPriority priority, ServiceRequestStatus finalStatus, string crew, int? rating)
        {
            return new SampleRequest
            {
                DaysAgo = daysAgo,
                Category = category,
                SubCategory = subCategory,
                Description = description,
                Location = location,
                Area = area,
                Priority = priority,
                FinalStatus = finalStatus,
                Crew = crew,
                Rating = rating
            };
        }

        /// <summary>
        /// Fills in a realistic history for a sample request: each status on the usual
        /// path to its current one, spaced out between the report date and now.
        /// </summary>
        private static void BuildSampleHistory(Issue issue, ServiceRequestStatus finalStatus, string crew, DateTime now)
        {
            var path = new List<ServiceRequestStatus> { ServiceRequestStatus.Submitted };
            switch (finalStatus)
            {
                case ServiceRequestStatus.Rejected:
                    path.Add(ServiceRequestStatus.Rejected);
                    break;
                case ServiceRequestStatus.Assigned:
                    path.Add(ServiceRequestStatus.Assigned);
                    break;
                case ServiceRequestStatus.InProgress:
                    path.AddRange(new[] { ServiceRequestStatus.Assigned, ServiceRequestStatus.InProgress });
                    break;
                case ServiceRequestStatus.OnHold:
                    path.AddRange(new[] { ServiceRequestStatus.Assigned, ServiceRequestStatus.InProgress, ServiceRequestStatus.OnHold });
                    break;
                case ServiceRequestStatus.Resolved:
                    path.AddRange(new[] { ServiceRequestStatus.Assigned, ServiceRequestStatus.InProgress, ServiceRequestStatus.Resolved });
                    break;
                case ServiceRequestStatus.Closed:
                    path.AddRange(new[] { ServiceRequestStatus.Assigned, ServiceRequestStatus.InProgress, ServiceRequestStatus.Resolved, ServiceRequestStatus.Closed });
                    break;
            }

            var span = now - issue.DateReported;
            var step = TimeSpan.FromTicks(span.Ticks / (path.Count + 1));

            for (var i = 0; i < path.Count; i++)
            {
                var changedAt = issue.DateReported + TimeSpan.FromTicks(step.Ticks * i);
                issue.StatusHistory.Add(new StatusChange(path[i], changedAt, SampleNote(path[i], crew)));
            }
        }

        private static string SampleNote(ServiceRequestStatus status, string crew)
        {
            switch (status)
            {
                case ServiceRequestStatus.Submitted: return "Request received.";
                case ServiceRequestStatus.Assigned: return "Assigned to " + crew + ".";
                case ServiceRequestStatus.InProgress: return "Crew on site and work has started.";
                case ServiceRequestStatus.OnHold: return "Waiting for replacement parts.";
                case ServiceRequestStatus.Resolved: return "Work completed.";
                case ServiceRequestStatus.Closed: return "Request closed.";
                case ServiceRequestStatus.Rejected: return "Not a municipal responsibility; referred to the property owner.";
                default: return string.Empty;
            }
        }

        #endregion
    }
}
