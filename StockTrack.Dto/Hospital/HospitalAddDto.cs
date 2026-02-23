using System;

namespace StockTrack.Dto.Hospital
{
    public class HospitalAddDto
    {
        public string Name { get; set; }
        public string Branch { get; set; }
        public string City { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? WebSite { get; set; }
        public string? Email { get; set; }

        // PDF'teki Yeni Alanlar
        public string? HbysName { get; set; }
        public string? HbysVersion { get; set; }
        public DateTime? InstallationDate { get; set; }
        public string? IntegrationUrl { get; set; }
        public string? IntegrationUsername { get; set; }
        public string? IntegrationPassword { get; set; }
    }
}