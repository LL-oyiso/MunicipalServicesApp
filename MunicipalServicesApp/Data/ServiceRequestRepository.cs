using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly List<Issue> _requests = new List<Issue>();

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

            _requests.Add(issue);
        }

        public static Issue FindByReference(string referenceNumber)
        {
            if (string.IsNullOrWhiteSpace(referenceNumber))
            {
                return null;
            }

            var key = referenceNumber.Trim();
            return _requests.FirstOrDefault(r => string.Equals(r.ReferenceNumber, key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Moves a request to a new status and records the change in its history.</summary>
        public static void ChangeStatus(Issue issue, ServiceRequestStatus newStatus, string note)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            issue.Status = newStatus;
            issue.StatusHistory.Add(new StatusChange(newStatus, DateTime.Now, note));
        }

        /// <summary>Records staff's triage decisions: which ward, sub-type, crew and priority.</summary>
        public static void UpdateTriage(Issue issue, string area, string subCategory, string crew, RequestPriority priority)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            issue.Area = area;
            issue.SubCategory = subCategory;
            issue.AssignedCrew = crew;
            issue.Priority = priority;
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
                _requests.Add(issue);
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
