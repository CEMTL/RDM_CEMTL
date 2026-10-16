using System;

namespace Solution.Models
{
    public class InstallationService
    {
        public int InstallationServiceId { get; set; }
        public int InstallationId { get; set; }
        public int ServiceId { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public bool EstActif { get; set; }

        public virtual Installation Installation { get; set; }
        public virtual Service Service { get; set; }
    }
}
