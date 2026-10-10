using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// How urgent or noteworthy an event is, used to rank it alongside others of the
    /// same date when space is limited (for example in a "don't miss these" strip).
    /// </summary>
    public enum EventPriority
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Represents a single local event or municipal notice that residents can browse,
    /// search and be recommended on the Local Events and Announcements page.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Unique identifier used for hash table lookups (<see cref="System.Collections.Generic.Dictionary{TKey,TValue}"/>
        /// keyed collections in the repository), independent of the event's title or date.
        /// </summary>
        public string Id { get; set; }

        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        public EventPriority Priority { get; set; }

        public Event()
        {
            Id = Guid.NewGuid().ToString("N");
            Priority = EventPriority.Medium;
        }
    }
}
