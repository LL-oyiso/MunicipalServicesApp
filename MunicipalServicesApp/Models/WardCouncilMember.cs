using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>
    /// A ward councillor or community caretaker, so residents can look up who
    /// represents their ward and how to reach them. Part of the "Community
    /// Resources" bonus section - not required by the marking rubric.
    /// </summary>
    public class WardCouncilMember
    {
        public string Id { get; set; }
        public int WardNumber { get; set; }
        public string Name { get; set; }

        /// <summary>E.g. "Ward Councillor" or "Community Caretaker".</summary>
        public string Role { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string OfficeAddress { get; set; }
        public string OfficeHours { get; set; }

        public WardCouncilMember()
        {
            Id = Guid.NewGuid().ToString("N");
        }
    }
}
