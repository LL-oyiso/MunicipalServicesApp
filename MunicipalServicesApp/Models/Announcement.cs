using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// How urgent an announcement is. Urgent announcements are surfaced ahead of
    /// normal ones when the announcements board is ranked by the priority queue.
    /// </summary>
    public enum AnnouncementPriority
    {
        Normal,
        Urgent
    }

    /// <summary>
    /// A municipal notice or community announcement. Most are published directly by
    /// the municipality (<see cref="IsResidentSubmitted"/> false), but residents can
    /// also submit their own announcement requests, which enter a review queue
    /// before appearing on the public board.
    /// </summary>
    public class Announcement
    {
        /// <summary>Unique identifier used for hash table lookups in the repository.</summary>
        public string Id { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Category { get; set; }
        public DateTime DatePosted { get; set; }
        public AnnouncementPriority Priority { get; set; }

        /// <summary>
        /// True for announcements a resident requested through the "Submit an
        /// announcement" form, rather than one the municipality posted directly.
        /// </summary>
        public bool IsResidentSubmitted { get; set; }

        public Announcement()
        {
            Id = Guid.NewGuid().ToString("N");
            DatePosted = DateTime.Now;
            Priority = AnnouncementPriority.Normal;
        }
    }
}
