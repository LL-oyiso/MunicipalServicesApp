using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// How the resident would prefer to be contacted about follow-ups on their report.
    /// </summary>
    public enum ContactMethod
    {
        Email,
        Mobile
    }

    /// <summary>
    /// Represents a single issue or service request reported by a resident.
    /// </summary>
    public class Issue
    {
        /// <summary>
        /// Human-friendly reference number assigned when the issue is added to the
        /// repository (e.g. "REQ-2026-0001"), so residents have something concrete
        /// to quote when following up on their report.
        /// </summary>
        public string ReferenceNumber { get; set; }

        public string Location { get; set; }
        public string AdditionalLocationInfo { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> AttachmentPaths { get; set; }
        public DateTime DateReported { get; set; }

        public string ContactFirstName { get; set; }
        public string ContactSurname { get; set; }
        public string ContactEmail { get; set; }
        public string ContactMobile { get; set; }
        public ContactMethod PreferredContactMethod { get; set; }

        /// <summary>
        /// The specific kind of problem within <see cref="Category"/> (e.g. "Street lights"
        /// within Electricity). Set by staff when triaging; null until then.
        /// </summary>
        public string SubCategory { get; set; }

        public ServiceRequestStatus Status { get; set; }
        public RequestPriority Priority { get; set; }

        /// <summary>The ward the request falls in, chosen by staff when triaging; null until then.</summary>
        public string Area { get; set; }

        public string AssignedCrew { get; set; }

        /// <summary>Every status the request has been in, oldest first.</summary>
        public List<StatusChange> StatusHistory { get; set; }

        /// <summary>The resident's 1-5 rating of the service, given once the request is closed.</summary>
        public int? Rating { get; set; }
        public string RatingComment { get; set; }

        /// <summary>True until the request is closed or rejected.</summary>
        public bool IsOpen => Status != ServiceRequestStatus.Closed && Status != ServiceRequestStatus.Rejected;

        public DateTime LastUpdated => StatusHistory.Count == 0
            ? DateReported
            : StatusHistory[StatusHistory.Count - 1].ChangedAt;

        public Issue()
        {
            AttachmentPaths = new List<string>();
            DateReported = DateTime.Now;
            Status = ServiceRequestStatus.Submitted;
            Priority = RequestPriority.Medium;
            StatusHistory = new List<StatusChange>();
        }
    }
}
