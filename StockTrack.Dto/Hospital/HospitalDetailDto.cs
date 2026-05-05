namespace StockTrack.Dto.Hospital
{
    public class HospitalDetailDto
    {
        public string Name { get; set; }
        public string Branch { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string? WebSite { get; set; }
        public string? Email { get; set; }
        public string Phone { get; set; }
        public string? HbysName { get; set; }
        public string? HbysVersion { get; set; }
        public string? InstallationDate { get; set; } // JavaScript'te kolay göstermek için string yaptık
        public string? IntegrationUrl { get; set; }
        public string? IntegrationUsername { get; set; }
        public string? IntegrationPassword { get; set; }
        public string? SnUsername { get; set; }
        public string? SnPassword { get; set; }
    }
}