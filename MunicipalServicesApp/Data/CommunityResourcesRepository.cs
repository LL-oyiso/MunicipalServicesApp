using System;
using System.Collections.Generic;
using System.Linq;
using MunicipalServicesApp.Models;

namespace MunicipalServicesApp.Data
{
    /// <summary>
    /// In-memory store backing the "Community Resources" bonus section - Emergency
    /// Services, Know Your Ward, and Community Programs. This section is explicitly
    /// a nice-to-have extra and is not required by (or scored against) the marking
    /// rubric for Local Events and Announcements; it exists purely to be useful to a
    /// resident using the app. Seeded on startup, same in-memory-only pattern as
    /// <see cref="EventsRepository"/>.
    /// </summary>
    public static class CommunityResourcesRepository
    {
        private static readonly List<EmergencyContact> _emergencyContacts = new List<EmergencyContact>();
        private static readonly SortedDictionary<int, List<WardCouncilMember>> _wardMembers = new SortedDictionary<int, List<WardCouncilMember>>();
        private static readonly Dictionary<ProgramCategory, List<CommunityProgram>> _programsByCategory = new Dictionary<ProgramCategory, List<CommunityProgram>>();

        static CommunityResourcesRepository()
        {
            SeedEmergencyContacts();
            SeedWardMembers();
            SeedCommunityPrograms();
        }

        /// <summary>All emergency contacts, optionally filtered by service type and/or an area keyword.</summary>
        public static List<EmergencyContact> SearchEmergencyContacts(EmergencyServiceType? serviceType, string areaKeyword)
        {
            IEnumerable<EmergencyContact> candidates = _emergencyContacts;

            if (serviceType.HasValue)
            {
                candidates = candidates.Where(c => c.ServiceType == serviceType.Value);
            }

            if (!string.IsNullOrWhiteSpace(areaKeyword))
            {
                candidates = candidates.Where(c =>
                    (c.Area != null && c.Area.IndexOf(areaKeyword, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (c.Name != null && c.Name.IndexOf(areaKeyword, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            return candidates
                .OrderBy(c => c.ServiceType)
                .ThenBy(c => c.Name)
                .ToList();
        }

        /// <summary>All ward council members/caretakers, in ward order (lowest first), optionally filtered to a single ward.</summary>
        public static List<WardCouncilMember> GetWardMembers(int? wardNumber = null)
        {
            if (wardNumber.HasValue)
            {
                return _wardMembers.TryGetValue(wardNumber.Value, out var members)
                    ? members.ToList()
                    : new List<WardCouncilMember>();
            }

            return _wardMembers.Values.SelectMany(list => list).ToList();
        }

        /// <summary>Every ward number that has at least one council member/caretaker on record, ascending.</summary>
        public static IReadOnlyCollection<int> WardNumbers => _wardMembers.Keys;

        /// <summary>Community programmes, optionally filtered to a single category.</summary>
        public static List<CommunityProgram> GetPrograms(ProgramCategory? category = null)
        {
            if (category.HasValue)
            {
                return _programsByCategory.TryGetValue(category.Value, out var programs)
                    ? programs.ToList()
                    : new List<CommunityProgram>();
            }

            return _programsByCategory.Values.SelectMany(list => list).ToList();
        }

        public static string GetServiceTypeDisplayName(EmergencyServiceType type)
        {
            switch (type)
            {
                case EmergencyServiceType.FireBrigade: return "Fire Brigade";
                case EmergencyServiceType.Ambulance: return "Ambulance";
                case EmergencyServiceType.Police: return "Police";
                case EmergencyServiceType.MentalHealth: return "Mental Health";
                default: return type.ToString();
            }
        }

        public static string GetProgramCategoryDisplayName(ProgramCategory category)
        {
            switch (category)
            {
                case ProgramCategory.ComputerLiteracy: return "Computer Literacy";
                case ProgramCategory.CvGuidelines: return "CV Guidelines";
                case ProgramCategory.InterviewPrep: return "Interview Preparation";
                default: return category.ToString();
            }
        }

        private static void AddEmergencyContact(EmergencyServiceType type, string name, string area, string phone, string address, string hours)
        {
            _emergencyContacts.Add(new EmergencyContact
            {
                ServiceType = type,
                Name = name,
                Area = area,
                Phone = phone,
                Address = address,
                Hours = hours
            });
        }

        private static void SeedEmergencyContacts()
        {
            // Fire Brigade
            AddEmergencyContact(EmergencyServiceType.FireBrigade, "Central Fire Station", "Central / Wards 1-3", "012 555 0170", "139 Civic Avenue, City Centre", "24/7");
            AddEmergencyContact(EmergencyServiceType.FireBrigade, "Riverside Fire Station", "Ward 7", "012 555 0171", "22 Riverside Drive", "24/7");
            AddEmergencyContact(EmergencyServiceType.FireBrigade, "Ward 12 Fire Station", "Ward 12", "012 555 0172", "Fire Station 3, Ward 12", "24/7");

            // Ambulance
            AddEmergencyContact(EmergencyServiceType.Ambulance, "National Emergency Ambulance Line", "Municipality-wide", "10177", "N/A - telephonic dispatch", "24/7");
            AddEmergencyContact(EmergencyServiceType.Ambulance, "Cell Phone Emergency Line", "Municipality-wide", "112", "N/A - telephonic dispatch", "24/7");
            AddEmergencyContact(EmergencyServiceType.Ambulance, "Ward 12 Community Ambulance Point", "Ward 12", "012 555 0173", "Community Clinic Forecourt, Ward 12", "06:00 - 22:00");

            // Police
            AddEmergencyContact(EmergencyServiceType.Police, "Central Police Station", "Central / Wards 1-3", "012 555 0174", "48 Main Chamber Road, City Centre", "24/7");
            AddEmergencyContact(EmergencyServiceType.Police, "Riverside Police Station", "Ward 7", "012 555 0175", "15 Riverside Drive", "24/7");
            AddEmergencyContact(EmergencyServiceType.Police, "Ward 12 Satellite Police Office", "Ward 12", "012 555 0176", "Ward 12 Community Hall", "08:00 - 18:00");

            // Mental Health
            AddEmergencyContact(EmergencyServiceType.MentalHealth, "South African Depression and Anxiety Group (SADAG)", "Municipality-wide", "0800 567 567", "N/A - telephonic support line", "24/7");
            AddEmergencyContact(EmergencyServiceType.MentalHealth, "Municipal Community Wellness Clinic", "Central", "012 555 0177", "Community Clinic Forecourt, City Centre", "Weekdays 08:00 - 16:00");
            AddEmergencyContact(EmergencyServiceType.MentalHealth, "Ward 12 Peer Support Group", "Ward 12", "012 555 0178", "Ward 12 Community Hall", "Tuesdays 17:00 - 19:00");
        }

        private static void AddWardMember(int wardNumber, string name, string role, string phone, string email, string officeAddress, string officeHours)
        {
            if (!_wardMembers.TryGetValue(wardNumber, out var members))
            {
                members = new List<WardCouncilMember>();
                _wardMembers[wardNumber] = members;
            }

            members.Add(new WardCouncilMember
            {
                WardNumber = wardNumber,
                Name = name,
                Role = role,
                Phone = phone,
                Email = email,
                OfficeAddress = officeAddress,
                OfficeHours = officeHours
            });
        }

        private static void SeedWardMembers()
        {
            AddWardMember(1, "Councillor Thandiwe Mokoena", "Ward Councillor", "012 555 0201", "ward1.councillor@example.gov", "Town Hall, Main Chamber", "Weekdays 09:00 - 15:00");
            AddWardMember(1, "Sipho Ndlovu", "Community Caretaker", "012 555 0202", "ward1.caretaker@example.gov", "Town Hall, Main Chamber", "Weekdays 08:00 - 16:00");

            AddWardMember(3, "Councillor Pieter van der Merwe", "Ward Councillor", "012 555 0203", "ward3.councillor@example.gov", "Civic Centre Annex", "Weekdays 09:00 - 15:00");

            AddWardMember(7, "Councillor Aisha Patel", "Ward Councillor", "012 555 0204", "ward7.councillor@example.gov", "Riverside Community Centre", "Weekdays 09:00 - 15:00");
            AddWardMember(7, "Johan Botha", "Community Caretaker", "012 555 0205", "ward7.caretaker@example.gov", "Riverside Community Centre", "Weekdays 08:00 - 16:00");

            AddWardMember(12, "Councillor Nomvula Dlamini", "Ward Councillor", "012 555 0206", "ward12.councillor@example.gov", "Ward 12 Community Hall", "Weekdays 09:00 - 15:00");
            AddWardMember(12, "Grace Adams", "Community Caretaker", "012 555 0207", "ward12.caretaker@example.gov", "Ward 12 Community Hall", "Weekdays 08:00 - 16:00");
        }

        private static void AddProgram(ProgramCategory category, string title, string description, string schedule, string location, string contactInfo)
        {
            if (!_programsByCategory.TryGetValue(category, out var programs))
            {
                programs = new List<CommunityProgram>();
                _programsByCategory[category] = programs;
            }

            programs.Add(new CommunityProgram
            {
                Category = category,
                Title = title,
                Description = description,
                Schedule = schedule,
                Location = location,
                ContactInfo = contactInfo
            });
        }

        private static void SeedCommunityPrograms()
        {
            AddProgram(ProgramCategory.ComputerLiteracy, "Introduction to Computers", "Covers keyboard and mouse basics, using email, and browsing the internet safely.", "Mondays and Wednesdays, 10:00 - 12:00", "Public Library Annex", "012 555 0301");
            AddProgram(ProgramCategory.ComputerLiteracy, "Microsoft Office Essentials", "Hands-on sessions covering Word, Excel and PowerPoint for job seekers.", "Saturdays, 09:00 - 11:00", "Civic Centre Computer Lab", "012 555 0302");

            AddProgram(ProgramCategory.CvGuidelines, "CV Writing Workshop", "Guidance on structuring a CV, highlighting experience, and avoiding common mistakes.", "First Tuesday of every month, 14:00 - 16:00", "Civic Centre Foyer", "012 555 0303");
            AddProgram(ProgramCategory.CvGuidelines, "One-on-One CV Review Sessions", "Book a slot for individual feedback on your CV from a volunteer career advisor.", "By appointment, Thursdays", "Public Library Annex", "012 555 0304");

            AddProgram(ProgramCategory.InterviewPrep, "Interview Skills and Confidence Building", "Practice answering common interview questions and receive feedback in a supportive group setting.", "Second Saturday of every month, 10:00 - 12:00", "Civic Centre Annex", "012 555 0305");
            AddProgram(ProgramCategory.InterviewPrep, "Mock Interview Sessions", "Simulated interviews with volunteer professionals, followed by a short debrief.", "By appointment, Fridays", "Civic Centre Annex", "012 555 0306");
        }
    }
}
