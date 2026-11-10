using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// The stages a service request moves through, from the resident submitting it
    /// to the municipality closing it. Which stage may follow which is defined by the
    /// status workflow graph, not by the order of these values.
    /// </summary>
    public enum ServiceRequestStatus
    {
        Submitted,
        Assigned,
        InProgress,
        OnHold,
        Resolved,
        Closed,
        Rejected
    }

    /// <summary>
    /// How urgently a request needs attention. Lower values are more urgent, so the
    /// staff work queue (a min-heap) can compare priorities directly.
    /// </summary>
    public enum RequestPriority
    {
        Critical = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    /// <summary>One entry in a request's status history: what it changed to, when, and why.</summary>
    public class StatusChange
    {
        public ServiceRequestStatus Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Note { get; set; }

        public StatusChange(ServiceRequestStatus status, DateTime changedAt, string note)
        {
            Status = status;
            ChangedAt = changedAt;
            Note = note ?? string.Empty;
        }
    }
}
