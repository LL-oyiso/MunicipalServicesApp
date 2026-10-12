using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>Which support category a <see cref="CommunityProgram"/> falls under.</summary>
    public enum ProgramCategory
    {
        ComputerLiteracy,
        CvGuidelines,
        InterviewPrep
    }

    /// <summary>
    /// A community support programme (a class, workshop or drop-in session) residents
    /// can attend. Part of the "Community Resources" bonus section - not required by
    /// the marking rubric.
    /// </summary>
    public class CommunityProgram
    {
        public string Id { get; set; }
        public ProgramCategory Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Schedule { get; set; }
        public string Location { get; set; }
        public string ContactInfo { get; set; }

        public CommunityProgram()
        {
            Id = Guid.NewGuid().ToString("N");
        }
    }
}
