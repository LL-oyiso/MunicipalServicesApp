using System;
using System.Collections.Generic;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Single in-memory store for all reported issues. Using one shared List&lt;Issue&gt;
    /// here (rather than arrays or ad hoc storage) keeps issue data consistent and
    /// ready to be read by later tasks (e.g. Service Request Status).
    /// </summary>
    public static class IssueRepository
    {
        private static readonly List<Issue> _issues = new List<Issue>();
        private static readonly List<Issue> _sessionReports = new List<Issue>();
        private static int _lastSequence;

        /// <summary>Read-only view of every issue held, including the sample requests.</summary>
        public static IReadOnlyList<Issue> Issues => _issues.AsReadOnly();

        /// <summary>The issues the resident reported this session, oldest first (sample requests excluded).</summary>
        public static IReadOnlyList<Issue> SessionReports => _sessionReports.AsReadOnly();

        /// <summary>Number of issues reported by the resident so far this session (sample requests excluded).</summary>
        public static int Count => _sessionReports.Count;

        /// <summary>The most recent issue the resident reported this session, or null if none yet.</summary>
        public static Issue LastReported => _sessionReports.Count == 0 ? null : _sessionReports[_sessionReports.Count - 1];

        public static void AddIssue(Issue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            // Load the sample requests first, so they take the earlier reference
            // numbers and the resident's own report is always the newest one.
            ServiceRequestRepository.EnsureInitialized();

            issue.ReferenceNumber = GenerateReferenceNumber();
            _issues.Add(issue);
            _sessionReports.Add(issue);

            ServiceRequestRepository.Register(issue);
        }

        /// <summary>
        /// Adds one of the sample requests the Service Request Status feature starts
        /// with. It shares the reference number sequence with real reports, so numbers
        /// never collide, but doesn't count as a report logged this session.
        /// </summary>
        internal static void AddSampleIssue(Issue issue)
        {
            issue.ReferenceNumber = GenerateReferenceNumber();
            _issues.Add(issue);
        }

        /// <summary>
        /// Builds a human-friendly reference number, e.g. "REQ-2026-0001", combining
        /// the current year with a zero-padded, 1-based sequence number.
        /// </summary>
        private static string GenerateReferenceNumber()
        {
            _lastSequence++;
            return $"REQ-{DateTime.Now.Year}-{_lastSequence:D4}";
        }
    }
}
