using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Single in-memory store for local events and municipal announcements, seeded on
    /// startup. Mirrors the shape of <see cref="IssueRepository"/>, but layers in the
    /// extra data structures the Local Events and Announcements feature needs:
    /// dictionaries keyed by id and by category, a sorted dictionary that keeps events
    /// chronologically ordered automatically, hash sets of unique categories/dates for
    /// filter UI, and a queue of resident-submitted announcement requests awaiting
    /// publish.
    /// </summary>
    public static class EventsRepository
    {
        private static readonly Dictionary<string, Event> _eventsById = new Dictionary<string, Event>();
        private static readonly Dictionary<string, List<Event>> _eventsByCategory =
            new Dictionary<string, List<Event>>(StringComparer.OrdinalIgnoreCase);
        private static readonly SortedDictionary<DateTime, List<Event>> _eventsByDate =
            new SortedDictionary<DateTime, List<Event>>();
        private static readonly HashSet<string> _categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<DateTime> _eventDates = new HashSet<DateTime>();

        private static readonly Queue<Announcement> _pendingSubmissions = new Queue<Announcement>();
        private static readonly List<Announcement> _publishedAnnouncements = new List<Announcement>();

        static EventsRepository()
        {
            SeedEvents();
            SeedAnnouncements();
        }

        /// <summary>Every known event category, as typed when events were added (unique, case-insensitive).</summary>
        public static IReadOnlyCollection<string> Categories => _categories;

        /// <summary>Every distinct date (time component stripped) on which at least one event happens.</summary>
        public static IReadOnlyCollection<DateTime> EventDates => _eventDates;

        /// <summary>Resident-submitted announcement requests still waiting to be published, oldest first.</summary>
        public static IReadOnlyCollection<Announcement> PendingAnnouncements => _pendingSubmissions.ToArray();

        /// <summary>Total number of events currently seeded.</summary>
        public static int EventCount => _eventsById.Count;

        /// <summary>Total number of announcements published so far (excludes pending submissions).</summary>
        public static int PublishedAnnouncementCount => _publishedAnnouncements.Count;

        /// <summary>
        /// Adds an event to every index (by id, by category, by date, plus the unique
        /// category/date sets) in one place, so the indexes can never drift apart.
        /// </summary>
        private static void AddEvent(Event evt)
        {
            _eventsById[evt.Id] = evt;

            if (!_eventsByCategory.TryGetValue(evt.Category, out var categoryList))
            {
                categoryList = new List<Event>();
                _eventsByCategory[evt.Category] = categoryList;
            }
            categoryList.Add(evt);

            var dateOnly = evt.EventDate.Date;
            if (!_eventsByDate.TryGetValue(dateOnly, out var dateList))
            {
                dateList = new List<Event>();
                _eventsByDate[dateOnly] = dateList;
            }
            dateList.Add(evt);

            _categories.Add(evt.Category);
            _eventDates.Add(dateOnly);
        }

        /// <summary>All events currently known, in chronological order (sourced from the date-sorted index).</summary>
        public static List<Event> GetAllEventsChronological()
        {
            return _eventsByDate.Values.SelectMany(list => list).ToList();
        }

        public static Event GetEventById(string id)
        {
            return !string.IsNullOrEmpty(id) && _eventsById.TryGetValue(id, out var evt) ? evt : null;
        }

        /// <summary>
        /// Filters events by category, date range and keyword. Any parameter left null
        /// (or blank, for category/keyword) is treated as "no restriction" on that field.
        /// </summary>
        public static List<Event> SearchEvents(string category, DateTime? fromDate, DateTime? toDate, string keyword)
        {
            IEnumerable<Event> candidates = GetAllEventsChronological();

            if (!string.IsNullOrWhiteSpace(category))
            {
                candidates = _eventsByCategory.TryGetValue(category, out var categoryList)
                    ? categoryList.OrderBy(e => e.EventDate)
                    : Enumerable.Empty<Event>();
            }

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                candidates = candidates.Where(e => e.EventDate.Date >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date;
                candidates = candidates.Where(e => e.EventDate.Date <= to);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                candidates = candidates.Where(e =>
                    (e.Title != null && e.Title.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (e.Description != null && e.Description.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (e.Location != null && e.Location.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return candidates.ToList();
        }

        /// <summary>Soonest upcoming events (today or later), used for the cold-start recommendation fallback.</summary>
        public static List<Event> GetUpcomingEvents(int count)
        {
            var today = DateTime.Now.Date;
            return _eventsByDate
                .Where(pair => pair.Key >= today)
                .SelectMany(pair => pair.Value)
                .Take(Math.Max(count, 0))
                .ToList();
        }

        /// <summary>Events in a given category, soonest first (used by the recommendation engine).</summary>
        public static List<Event> GetEventsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || !_eventsByCategory.TryGetValue(category, out var list))
            {
                return new List<Event>();
            }

            return list.OrderBy(e => e.EventDate).ToList();
        }

        /// <summary>Enqueues a resident's announcement request so it awaits municipal review before publishing.</summary>
        public static void SubmitAnnouncement(Announcement announcement)
        {
            if (announcement == null)
            {
                throw new ArgumentNullException(nameof(announcement));
            }

            announcement.IsResidentSubmitted = true;
            _pendingSubmissions.Enqueue(announcement);
        }

        /// <summary>
        /// Dequeues the oldest pending announcement request and moves it onto the
        /// public board. Returns null when there is nothing pending.
        /// </summary>
        public static Announcement PublishNextPendingAnnouncement()
        {
            if (_pendingSubmissions.Count == 0)
            {
                return null;
            }

            var next = _pendingSubmissions.Dequeue();
            _publishedAnnouncements.Add(next);
            return next;
        }

        /// <summary>
        /// Published announcements ranked with the shared <see cref="PriorityQueue{T}"/>:
        /// Urgent first, then newest first within the same priority.
        /// </summary>
        public static List<Announcement> GetRankedAnnouncements()
        {
            var comparer = Comparer<Announcement>.Create((a, b) =>
            {
                // Urgent must come out of the heap before Normal, so compare priority
                // "descending" (higher enum value = higher priority = dequeued first).
                var priorityComparison = b.Priority.CompareTo(a.Priority);
                if (priorityComparison != 0)
                {
                    return priorityComparison;
                }

                // Within the same priority, newest posted date comes out first.
                return b.DatePosted.CompareTo(a.DatePosted);
            });

            var queue = new PriorityQueue<Announcement>(comparer);
            foreach (var announcement in _publishedAnnouncements)
            {
                queue.Enqueue(announcement);
            }

            var ranked = new List<Announcement>(_publishedAnnouncements.Count);
            while (queue.Count > 0)
            {
                ranked.Add(queue.Dequeue());
            }

            return ranked;
        }

        private static void SeedEvents()
        {
            var today = DateTime.Now.Date;

            void Seed(string title, string category, string description, string location, int dayOffset, EventPriority priority)
            {
                AddEvent(new Event
                {
                    Title = title,
                    Category = category,
                    Description = description,
                    Location = location,
                    EventDate = today.AddDays(dayOffset),
                    Priority = priority
                });
            }

            // Community
            Seed("Ward 12 Community Clean-Up", "Community", "Join neighbours to clear litter and illegal dumping sites around the ward.", "Ward 12 Community Hall", 3, EventPriority.Medium);
            Seed("Neighbourhood Watch Meeting", "Community", "Monthly safety briefing and coordination with local law enforcement.", "Riverside Community Centre", 7, EventPriority.Medium);
            Seed("Spring Community Fair", "Community", "Stalls, food and activities celebrating the start of spring.", "Central Park Grounds", 14, EventPriority.High);
            Seed("Ward Budget Feedback Session", "Community", "Residents can review and comment on the proposed ward budget.", "Town Hall, Main Chamber", -10, EventPriority.Medium);

            // Sports and Recreation
            Seed("Municipal Fun Run 5km", "Sports and Recreation", "A family-friendly timed fun run through the botanical gardens.", "Botanical Gardens", 10, EventPriority.Medium);
            Seed("Public Pool Re-Opening", "Sports and Recreation", "The municipal swimming pool reopens for the summer season.", "Greenfield Public Pool", 20, EventPriority.High);
            Seed("Inter-Ward Soccer Tournament", "Sports and Recreation", "Local wards compete in a round-robin soccer tournament.", "Civic Sports Grounds", 25, EventPriority.Medium);

            // Arts and Culture
            Seed("Street Art Mural Unveiling", "Arts and Culture", "Unveiling of murals painted by local youth artists.", "Market Street Underpass", 5, EventPriority.Low);
            Seed("Municipal Theatre Night", "Arts and Culture", "An evening of short plays performed by the community theatre group.", "Civic Theatre", 18, EventPriority.Medium);
            Seed("Heritage Day Exhibition", "Arts and Culture", "Exhibition celebrating the area's cultural heritage and history.", "Municipal Museum", -3, EventPriority.Low);

            // Health and Safety
            Seed("Free Health Screening Day", "Health and Safety", "Free blood pressure, glucose and basic health screenings for residents.", "Community Clinic Forecourt", 2, EventPriority.High);
            Seed("Fire Safety Awareness Workshop", "Health and Safety", "Practical demonstration of fire safety equipment and evacuation planning.", "Fire Station 3", 9, EventPriority.High);
            Seed("Water Conservation Briefing", "Health and Safety", "Guidance on reducing household water usage during the dry season.", "Town Hall, Main Chamber", 12, EventPriority.Medium);

            // Municipal Notices
            Seed("Scheduled Water Maintenance", "Municipal Notices", "Planned maintenance will interrupt water supply to affected areas.", "Affected Suburbs (see notice)", 1, EventPriority.High);
            Seed("Refuse Collection Schedule Change", "Municipal Notices", "Updated refuse collection days ahead of the public holiday.", "Municipality-wide", 4, EventPriority.Medium);
            Seed("Electricity Load Testing", "Municipal Notices", "Scheduled testing of backup power infrastructure.", "Substation 7", -1, EventPriority.Low);

            // Youth and Education
            Seed("Back to School Supplies Drive", "Youth and Education", "Donate or collect school supplies ahead of the new term.", "Civic Centre Foyer", 6, EventPriority.Medium);
            Seed("Youth Coding Workshop", "Youth and Education", "Free introductory coding workshop for high school learners.", "Public Library Annex", 16, EventPriority.Medium);

            // Environment
            Seed("Tree Planting Initiative", "Environment", "Volunteers needed to plant indigenous trees along the riverbank.", "Riverbank Nature Reserve", 8, EventPriority.Medium);
            Seed("Recycling Awareness Drive", "Environment", "Learn about the municipality's expanded recycling programme.", "Central Park Grounds", 22, EventPriority.Low);
        }

        private static void SeedAnnouncements()
        {
            void Seed(string title, string message, string category, int daysAgo, AnnouncementPriority priority)
            {
                _publishedAnnouncements.Add(new Announcement
                {
                    Title = title,
                    Message = message,
                    Category = category,
                    DatePosted = DateTime.Now.AddDays(-daysAgo),
                    Priority = priority,
                    IsResidentSubmitted = false
                });
            }

            Seed("Water Supply Interruption Notice", "Water supply will be interrupted in affected areas during scheduled maintenance.", "Municipal Notices", 0, AnnouncementPriority.Urgent);
            Seed("Load Shedding Schedule Update", "An updated load shedding schedule is now in effect for this week.", "Municipal Notices", 1, AnnouncementPriority.Urgent);
            Seed("Municipal Offices Closed on Public Holiday", "All municipal offices will be closed for the upcoming public holiday.", "Municipal Notices", 2, AnnouncementPriority.Normal);
            Seed("New Pothole Reporting Channel Live", "Residents can now report potholes directly through the Municipal Services App.", "Community", 3, AnnouncementPriority.Normal);
            Seed("Refuse Collection Delays Expected", "Refuse collection may be delayed by a day in some wards this week.", "Municipal Notices", 4, AnnouncementPriority.Urgent);
            Seed("Public Pool Season Opening Soon", "The municipal public pool will open for the season in a few weeks.", "Sports and Recreation", 5, AnnouncementPriority.Normal);
            Seed("Community Clean-Up Volunteers Needed", "The municipality is calling for volunteers for the upcoming ward clean-up.", "Community", 6, AnnouncementPriority.Normal);
            Seed("Updated Recycling Guidelines Published", "New guidelines for household recycling are now available on the website.", "Environment", 7, AnnouncementPriority.Normal);
            Seed("Road Resurfacing on Main Avenue", "Expect delays as resurfacing work begins on Main Avenue next week.", "Municipal Notices", 8, AnnouncementPriority.Urgent);
            Seed("Library Extended Hours Trial", "The public library will trial extended evening hours for the next month.", "Youth and Education", 9, AnnouncementPriority.Normal);
            Seed("Fire Hydrant Testing Notice", "Routine fire hydrant testing will take place across several suburbs.", "Health and Safety", 10, AnnouncementPriority.Normal);
        }
    }
}
