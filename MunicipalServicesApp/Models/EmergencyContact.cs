using System;

namespace MunicipalServicesApp.Models
{
    /// <summary>Which kind of emergency service a <see cref="EmergencyContact"/> represents.</summary>
    public enum EmergencyServiceType
    {
        FireBrigade,
        Ambulance,
        Police,
        MentalHealth
    }

    /// <summary>
    /// A single emergency service contact (a specific station/branch, or a general
    /// province-wide line) that residents can browse or search by area. Part of the
    /// "Community Resources" bonus section - not required by the marking rubric.
    /// </summary>
    public class EmergencyContact
    {
        public string Id { get; set; }
        public EmergencyServiceType ServiceType { get; set; }
        public string Name { get; set; }
        public string Area { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Hours { get; set; }

        public EmergencyContact()
        {
            Id = Guid.NewGuid().ToString("N");
        }
    }
}
