namespace StockTrack.Entity.Enitities
{
    public class Hospital : EntityBase
    {
        public string Name { get; set; }  //Kurum Adı
        public string Branch { get; set; } //Sube
        public string City { get; set; }
        public string? Address { get; set; }
        public string? WebSite { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        // Yeni gelen istekler...

        public string? HbysName { get; set; }  //Hbys
        public string? HbysVersion { get; set; } //Versiyon
        public DateTime? InstallationDate { get; set; } //Kurulum tarihi

        // Entegrasyon Bilgileri
        public string? IntegrationUrl { get; set; }
        public string? IntegrationUsername { get; set; }
        public string? IntegrationPassword { get; set; }
    }
}
