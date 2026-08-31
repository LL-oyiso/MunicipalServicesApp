using System;
using System.Collections.Generic;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// Represents a single issue or service request reported by a resident.
    /// </summary>
    public class Issue
    {
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> AttachmentPaths { get; set; }
        public DateTime DateReported { get; set; }

        public Issue()
        {
            AttachmentPaths = new List<string>();
            DateReported = DateTime.Now;
        }
    }
}
