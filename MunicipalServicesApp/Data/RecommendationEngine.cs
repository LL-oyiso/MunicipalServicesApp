using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Common;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// Records the criteria used in a single events search, so it can be revisited
    /// later via the search-history stack (e.g. a "Previous search" button).
    /// </summary>
    public class SearchQuery
    {
        public string Category { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Keyword { get; set; }
        public DateTime SearchedAt { get; set; }

        /// <summary>Short, human-friendly description of this search, e.g. "Category: Community".</summary>
        public string Describe()
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(Category))
            {
                parts.Add($"Category: {Category}");
            }

            if (FromDate.HasValue || ToDate.HasValue)
            {
                var from = FromDate?.ToString("dd MMM yyyy") ?? "any";
                var to = ToDate?.ToString("dd MMM yyyy") ?? "any";
                parts.Add($"Dates: {from} - {to}");
            }

            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                parts.Add($"Keyword: \"{Keyword}\"");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "All events";
        }
    }

    /// <summary>An event recommended to the resident, paired with a plain-language reason it was chosen.</summary>
    public class RecommendedEvent
    {
        public Event Event { get; }
        public string Reason { get; }

        public RecommendedEvent(Event evt, string reason)
        {
            Event = evt;
            Reason = reason;
        }
    }

    /// <summary>
    /// Recommends upcoming events to a resident based on the categories they have
    /// searched for most often this session. Every search pushes its criteria onto a
    /// history stack and increments a per-category frequency count; recommendations
    /// are built by ranking categories by that frequency (via the shared
    /// <see cref="PriorityQueue{T}"/>, for consistency with the rest of the feature)
    /// and pulling the soonest upcoming events from the top-ranked categories. With
    /// no search history yet, it falls back to the soonest upcoming events across
    /// all categories (cold start).
    /// </summary>
    public class RecommendationEngine
    {
        private readonly Stack<SearchQuery> _searchHistory = new Stack<SearchQuery>();
        private readonly Dictionary<string, int> _categoryFrequency =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Number of searches recorded so far this session.</summary>
        public int SearchCount => _searchHistory.Count;

        /// <summary>
        /// Records a search's criteria: pushes it onto the history stack and, when a
        /// specific category was searched, increments that category's frequency.
        /// </summary>
        public void RecordSearch(SearchQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            query.SearchedAt = DateTime.Now;
            _searchHistory.Push(query);

            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                _categoryFrequency[query.Category] = _categoryFrequency.TryGetValue(query.Category, out var count)
                    ? count + 1
                    : 1;
            }
        }

        /// <summary>
        /// Pops the most recent search off the history stack (LIFO), so the UI can
        /// offer a "Previous search" button that re-applies it. Returns null when
        /// there is no history left.
        /// </summary>
        public SearchQuery PopLastSearch()
        {
            return _searchHistory.Count > 0 ? _searchHistory.Pop() : null;
        }

        /// <summary>Looks at the most recent search without removing it from the stack.</summary>
        public SearchQuery PeekLastSearch()
        {
            return _searchHistory.Count > 0 ? _searchHistory.Peek() : null;
        }

        /// <summary>
        /// Builds up to <paramref name="count"/> recommended events, ranking searched
        /// categories by frequency (most-searched first) and pulling the soonest
        /// upcoming events from them. Falls back to the soonest upcoming events
        /// overall when there is no search history yet, or to fill any remaining
        /// quota the searched categories could not satisfy.
        /// </summary>
        public List<RecommendedEvent> GetRecommendations(int count)
        {
            var recommendations = new List<RecommendedEvent>();
            var seenEventIds = new HashSet<string>();

            if (_categoryFrequency.Count == 0)
            {
                foreach (var evt in EventsRepository.GetUpcomingEvents(count))
                {
                    if (seenEventIds.Add(evt.Id))
                    {
                        recommendations.Add(new RecommendedEvent(evt, "Popular upcoming event"));
                    }
                }

                return recommendations;
            }

            var today = DateTime.Now.Date;
            foreach (var category in RankCategoriesByFrequency())
            {
                if (recommendations.Count >= count)
                {
                    break;
                }

                var upcomingInCategory = EventsRepository.GetEventsByCategory(category)
                    .Where(e => e.EventDate.Date >= today);

                foreach (var evt in upcomingInCategory)
                {
                    if (recommendations.Count >= count)
                    {
                        break;
                    }

                    if (seenEventIds.Add(evt.Id))
                    {
                        recommendations.Add(new RecommendedEvent(
                            evt,
                            $"Because you searched {category} {_categoryFrequency[category]} time(s)"));
                    }
                }
            }

            // Cold-start-style fallback for any quota the searched categories could not fill.
            if (recommendations.Count < count)
            {
                var remainingNeeded = count - recommendations.Count;
                var fallbackCandidates = EventsRepository.GetUpcomingEvents(seenEventIds.Count + remainingNeeded);

                foreach (var evt in fallbackCandidates)
                {
                    if (recommendations.Count >= count)
                    {
                        break;
                    }

                    if (seenEventIds.Add(evt.Id))
                    {
                        recommendations.Add(new RecommendedEvent(evt, "Popular upcoming event"));
                    }
                }
            }

            return recommendations;
        }

        /// <summary>
        /// Orders searched categories from most to least frequent, using the shared
        /// <see cref="PriorityQueue{T}"/> for consistency with the rest of the feature.
        /// </summary>
        private List<string> RankCategoriesByFrequency()
        {
            var comparer = Comparer<KeyValuePair<string, int>>.Create((a, b) => b.Value.CompareTo(a.Value));
            var queue = new PriorityQueue<KeyValuePair<string, int>>(comparer);

            foreach (var pair in _categoryFrequency)
            {
                queue.Enqueue(pair);
            }

            var ranked = new List<string>(_categoryFrequency.Count);
            while (queue.Count > 0)
            {
                ranked.Add(queue.Dequeue().Key);
            }

            return ranked;
        }
    }
}
