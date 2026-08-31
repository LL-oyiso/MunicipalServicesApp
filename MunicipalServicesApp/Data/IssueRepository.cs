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

            _issues.Add(issue);
        }
    }
}
