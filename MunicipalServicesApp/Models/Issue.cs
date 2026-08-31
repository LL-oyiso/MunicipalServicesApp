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

        public Issue()
        {
            AttachmentPaths = new List<string>();
            DateReported = DateTime.Now;
        }
    }
}
