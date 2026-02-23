namespace StockTrack.Dto.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalCategories { get; set; } //Total kategori
        public int TotalProducts { get; set; }  // Ürün tablosundaki toplam ürün sayısı
        public int ActiveProducts { get; set; }  // Aktif ürün sayısı
        public int CategoryCount { get; set; }  // Kategori sayısı
        public int LocationCount { get; set; }  // Lokasyon sayısı
        public int DeletedProductsCount { get; set; }    // Silinmiş (IsDeleted) ürün sayısı
        public int ActiveUserCount { get; set; }  // Kullanıcı sayısı
        public int TotalLocationCount { get; set; } //Lokasyon sayısı
        public int TotalMainRepoCount { get; set; } //Depo sayısı
        public int TotalAwaitingApprovalCargo { get; set; } //Onay Bekleyen sayısı
        public int TotalPackingCargo { get; set; } //Paketli kargo sayısı
        public int TotalTransportationCargo { get; set; } //Paketli kargo sayısı
        public int TotalCompletedCargo { get; set; } //tamamlanmış kargı sayısı
        public int TotalCanceledCargo { get; set; } //iptal edilen kargo sayısı
        public int TotalInstillation { get; set; } //Kurulum sayısı
        public int TotalServices { get; set; } //servis sayısı
        public int TotalCargo { get; set; } //kargo sayısı

        public int TotalRequested { get; set; } // Tüm talep tipleri için "Talep Edildi" sayısı
        public int InstallationPending { get; set; } // Kurulum - Onay Bekliyor
        public int InstallationReady { get; set; } // Kurulum - Kuruluma Hazır
        public int InstallationCompleted { get; set; } // Kurulum - Tamamlananlar
        public int InstallationCancelled { get; set; } // Kurulum - Tamamlananlar
        public int ServiceCompleted { get; set; } // Servis - Tamamlananlar

    }


}
