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

        /// <summary>Read-only view of every issue reported so far this session.</summary>
        public static IReadOnlyList<Issue> Issues => _issues.AsReadOnly();

        /// <summary>Number of issues reported so far this session.</summary>
        public static int Count => _issues.Count;

        public static void AddIssue(Issue issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            issue.ReferenceNumber = GenerateReferenceNumber();
            _issues.Add(issue);
        }

        /// <summary>
        /// Builds a human-friendly reference number, e.g. "REQ-2026-0001", combining
        /// the current year with a zero-padded, 1-based sequence number.
        /// </summary>
        private static string GenerateReferenceNumber()
        {
            var sequence = _issues.Count + 1;
            return $"REQ-{DateTime.Now.Year}-{sequence:D4}";
        }
    }
}
